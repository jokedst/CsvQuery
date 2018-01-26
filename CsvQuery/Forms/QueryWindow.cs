using System.Text;

namespace CsvQuery.Forms
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows.Forms;
    using Csv;
    using PluginInfrastructure;
    using Properties;
    using Tools;

    /// <summary>
    ///     The query window that whos the current query and the results in a grid
    /// </summary>
    public partial class QueryWindow : Form
    {
        /// <summary> Background worker </summary>
        private Task _worker = Task.CompletedTask;

        private Color[] _winColors = null;
        private CsvSettings _lastGenerateSettings = null;

        public QueryWindow()
        {
            InitializeComponent();

            // Import query cache
            if (Main.Settings.SaveQueryCache && File.Exists(PluginStorage.QueryCachePath))
            {
                var lines = File.ReadAllLines(PluginStorage.QueryCachePath);
                // Arbitrary limit of 1000 cached queries. Reduces them to 900 to avoid rewrite every time
                if (lines.Length > 1000)
                {
                    var newLines = new string[900];
                    Array.Copy(lines, lines.Length - 900, newLines, 0, 900);
                    lines = newLines;
                    File.WriteAllLines(PluginStorage.QueryCachePath, lines);
                }
                txbQuery.AutoCompleteCustomSource.AddRange(lines);
            }

            if (Main.Settings.UseNppStyling)
                ApplyStyling(true);
            
            Main.Settings.SettingsChanged += OnSettingsChanged;
        }

        private void OnSettingsChanged(object sender, SettingsChangedEventArgs e)
        {
            if (!e.Changed.Contains(nameof(Settings.UseNppStyling)))
                return;
            ApplyStyling(e.NewSettings.UseNppStyling);
        }

        /// <summary>
        /// Applies NPP colors to window
        /// </summary>
        public void ApplyStyling(bool active)
        {
            if (_winColors == null)
                _winColors = new[] {dataGrid.ForeColor, dataGrid.BackgroundColor, dataGrid.BackColor};
            if (active)
            {
                // Get NPP colors 
                var backgroundColor = PluginBase.GetDefaultBackgroundColor();
                var foreColor = PluginBase.GetDefaultForegroundColor();
                var inBetween = Color.FromArgb((foreColor.R + backgroundColor.R*3) / 4, (foreColor.G + backgroundColor.G*3) / 4, (foreColor.B + backgroundColor.B*3) / 4);

                ApplyColors(foreColor, backgroundColor, inBetween);
                dataGrid.EnableHeadersVisualStyles = false;
            }
            else
            {
                // Disable styling
                ApplyColors(_winColors[0], _winColors[2], _winColors[1]);
                dataGrid.EnableHeadersVisualStyles = true;
            }
        }

        private void ApplyColors(Color foreColor, Color backgroundColor, Color inBetween)
        {
            Trace.TraceInformation($"FG {foreColor}, BG {backgroundColor}, inBetween {inBetween}");
            BackColor = backgroundColor;
            dataGrid.BackColor = backgroundColor;
            dataGrid.BackgroundColor = inBetween;
            dataGrid.ForeColor = foreColor;
            dataGrid.ColumnHeadersDefaultCellStyle.BackColor = backgroundColor;
            dataGrid.ColumnHeadersDefaultCellStyle.ForeColor = foreColor;
            dataGrid.EnableHeadersVisualStyles = false;

            txbQuery.BackColor = backgroundColor;
            txbQuery.ForeColor = foreColor;

            btnAnalyze.ForeColor = foreColor;
            btnAnalyze.BackColor = backgroundColor;
            btnExec.ForeColor = foreColor;
            btnExec.BackColor = backgroundColor;

            dataGrid.DefaultCellStyle.BackColor = backgroundColor;
        }

        /// <summary>
        ///     Executes given query and shows the result in this window
        /// </summary>
        /// <param name="query"> SQL query to run </param>
        public void ExecuteQuery(string query)
        {
            txbQuery.Text = query;
            btnExec.PerformClick();
        }

        private void btnAnalyze_Click(object sender, EventArgs e)
        {
            StartAnalysis(false);
        }

        private void StartSomething(Action someAction)
        {
            this.UiThread(() => UiEnabled(false));

            void SafeAction()
            {
                try
                {
                    someAction();
                }
                catch (Exception e)
                {
                    Trace.TraceError("CSV Action failed: {0}", e.Message);
                    this.Message("Error when executing an action: " + e.Message, Resources.Title_CSV_Query_Error);
                }
                finally
                {
                    this.UiThread(() => UiEnabled(true));
                }
            }

            var busy = false;
            lock (_worker)
            {
                if (_worker.IsCompleted)
                    _worker = Task.Factory.StartNew(SafeAction);
                else busy = true;
            }
            if (busy)
            {
                this.Message("CSV Query is busy", Resources.Title_CSV_Query_Error);
                this.UiThread(() => UiEnabled(true));
            }
        }

        private void UiEnabled(bool enabled)
        {
            txbQuery.Enabled = enabled;
            btnAnalyze.Enabled = enabled;
            btnExec.Enabled = enabled;
        }

        public void StartAnalysis(bool silent)
        {
            StartSomething(() => Analyze(silent));
        }

        private void Analyze(bool silent)
        {
            var watch = new DiagnosticTimer();
            var bufferId = NotepadPPGateway.GetCurrentBufferId();

            var textLength = PluginBase.CurrentScintillaGateway.GetTextLength();
            var text = PluginBase.CurrentScintillaGateway.GetTextRange(0, Math.Min(100000, textLength));
           // var text = PluginBase.CurrentScintillaGateway.GetAllText();

            //using (var sr = new StreamReader(ScintillaStreams.StreamAllText(), Encoding.UTF8))
            //{
            //    text = sr.ReadToEnd();
            //}
            

            watch.Checkpoint("GetText");

            var csvSettings = CsvAnalyzer.Analyze(text);
            if (csvSettings.Separator == '\0' && csvSettings.FieldWidths == null)
            {
                if (silent) return;

                var askUserDialog = new ParseSettings(csvSettings);
                this.UiThread(() => askUserDialog.ShowDialog());
                if (askUserDialog.DialogResult != DialogResult.OK)
                    return;
                csvSettings = askUserDialog.Settings;
            }
            watch.Checkpoint("Analyze");

            using (var sr = new StreamReader(ScintillaStreams.StreamAllText(), Encoding.UTF8))
            {
                Parse(csvSettings, watch, sr, bufferId);
            }
        }

        private void Parse(CsvSettings csvSettings, DiagnosticTimer watch, TextReader text, IntPtr bufferId)
        {
            var data = csvSettings.Parse(text);
            watch.Checkpoint("Parse");

            var columnTypes = new CsvColumnTypes(data, csvSettings);
            try
            {
                Main.DataStorage.SaveData(bufferId, data, columnTypes);
            }
            catch (Exception ex)
            {
                this.ErrorMessage("Error when saving data to database:\n" + ex.Message);
                return;
            }
            watch.Checkpoint("Saved to DB");
            this.UiThread(() => txbQuery.Text = "SELECT * FROM THIS");
            Execute(bufferId, watch);

            var diagnostic = watch.LastCheckpoint("Resize");
            Trace.TraceInformation(diagnostic);
            if (Main.Settings.DebugMode)
                this.Message(diagnostic);
        }

        public void StartParse(CsvSettings settings)
        {
            StartSomething(() =>
            {
                using (var sr = new StreamReader(ScintillaStreams.StreamAllText(), Encoding.UTF8))
                {
                    Parse(settings,
                        new DiagnosticTimer(),
                        sr,
                        NotepadPPGateway.GetCurrentBufferId());
                }
            });
        }

        private void Execute(IntPtr bufferId, DiagnosticTimer watch)
        {
            Main.DataStorage.SetActiveTab(bufferId);
            watch.Checkpoint("Switch buffer");

            var query = txbQuery.Text;
            List<string[]> toshow;
            try
            {
                toshow = Main.DataStorage.ExecuteQuery(query, true);
            }
            catch (Exception)
            {
                this.Message("Could not execute query", Resources.Title_CSV_Query_Error);
                return;
            }
            watch.Checkpoint("Execute query");

            if (toshow == null || toshow.Count==0)
            {
                this.Message("Query returned no data", Resources.Title_CSV_Query_Error);
                return;
            }

            var table = new DataTable();
            table.ExtendedProperties.Add("query",query);
            table.ExtendedProperties.Add("bufferId", bufferId);

            // Create columns
            foreach (var s in toshow[0])
            {
                // Column names in a DataGridView can't contain commas it seems
                table.Columns.Add(s.Replace(",", string.Empty));
            }

            // Insert rows
            foreach (var row in toshow.Skip(1))
                table.Rows.Add(row);
            watch.Checkpoint("Create DataTable");

            this.UiThread(() =>
            {
                dataGrid.DataSource = table;
            });
            watch.Checkpoint("Display");

            // Store query in history
            if (!txbQuery.AutoCompleteCustomSource.Contains(query))
            {
                this.UiThread(() => txbQuery.AutoCompleteCustomSource.Add(query));
                if (Main.Settings.SaveQueryCache)
                    using (var writer = File.AppendText(PluginStorage.QueryCachePath))
                    {
                        writer.WriteLine(query);
                    }
            }
        }

        private void btnExec_Click(object sender, EventArgs e)
        {
            StartSomething(() =>
            {
                var watch = new DiagnosticTimer();
                var bufferId = NotepadPPGateway.GetCurrentBufferId();

                Execute(bufferId, watch);

                var diagnosticMessage = watch.LastCheckpoint("Save query in history");
                Trace.TraceInformation(diagnosticMessage);
                if (Main.Settings.DebugMode)
                    this.Message(diagnosticMessage);
            });
        }

        private void txbQuery_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
                btnExec.PerformClick();
        }

        private void createNewCSVToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!(dataGrid.DataSource is DataTable table ))
            {
                MessageBox.Show("No results available to convert");
                return;
            }

            var initialSettings = _lastGenerateSettings ?? new CsvSettings(Main.Settings.DefaultSeparator.Unescape(),
                                      Main.Settings.DefaultQuoteChar, '\0', true);
            var settingsDialog = new ParseSettings(initialSettings)
            {
                btnReparse = {Text = "&Ok"},
                MainLabel = {Text = "How should the CSV be generated?"},
                hasHeaderCheckbox = { ThreeState = false , Text = "Create header row"},
                txbCommentChar = {Visible = false},
                CommentCharLabel = { Visible = false },
            };

            if (settingsDialog.ShowDialog() == DialogResult.Cancel)
                return;

            var settings = settingsDialog.Settings; 

            var watch = new DiagnosticTimer();
            try
            {
                // Create new tab for results
                Win32.SendMessage(PluginBase.nppData._nppHandle, (uint) NppMsg.NPPM_MENUCOMMAND, 0, NppMenuCmd.IDM_FILE_NEW);
                watch.Checkpoint("New document created");
                var headerLookup = Main.Settings.UseOriginalColumnHeadersOnGeneratedCsv
                                   && table.ExtendedProperties.ContainsKey("bufferId")
                                   && table.ExtendedProperties["bufferId"] is IntPtr bufferId
                    ? Main.DataStorage.GetUnsafeColumnMaps(bufferId)
                    : null;

                using (var stream = new BlockingStream(10))
                {
                    var producer = Task.Factory.StartNew(s =>
                    {
                        settings.GenerateToStream(dataGrid.DataSource as DataTable, (Stream) s, headerLookup);
                        ((BlockingStream) s).CompleteWriting();
                    }, stream);

                    var consumer = Task.Factory.StartNew(
                        s => { PluginBase.CurrentScintillaGateway.AddText((Stream) s); }, stream);

                    producer.Wait();
                    consumer.Wait();
                }
            }
            catch (Exception ex)
            {
                Trace.TraceInformation("CSV gen: Exception: " + ex.GetType().Name + " - " + ex.Message);
            }
            _lastGenerateSettings = settings;
            Trace.TraceInformation(watch.LastCheckpoint("CSV Done"));
        }

        public void CopyDataToClipboard(DataGridViewClipboardCopyMode mode)
        {
            if (dataGrid.GetCellCount(DataGridViewElementStates.Selected) == 0) return;
            var previousValue = dataGrid.ClipboardCopyMode;
            dataGrid.ClipboardCopyMode = mode;
            var clipboardContent = dataGrid.GetClipboardContent();
            dataGrid.ClipboardCopyMode = previousValue;
            if (clipboardContent == null) return;
            try
            {
                Clipboard.SetDataObject(clipboardContent);
            }
            catch (Exception exception)
            {
                this.ErrorMessage("Could not copy to clipboard: " + exception.Message);
            }
        }

        private void OnContextmenuCopy(object sender, EventArgs eventArgs) 
            => CopyDataToClipboard(DataGridViewClipboardCopyMode.EnableWithoutHeaderText);

        private void OnContextmenuCopyWithHeaders(object sender, EventArgs e) 
            => CopyDataToClipboard(DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText);

        private void OnContextmenuSelectAll(object sender, EventArgs e) 
            => dataGrid.SelectAll();

        private void OnContextmenuShowRowNumbers(object sender, EventArgs e)
        {
            contextmenuShowRowNumbers.Checked = dataGrid.RowHeadersVisible = !dataGrid.RowHeadersVisible;
            FormatDataGrid();
        }

        private void OnDataBindingComplete(object grid, DataGridViewBindingCompleteEventArgs args) => FormatDataGrid();

        private void FormatDataGrid()
        {
            if (dataGrid.RowHeadersVisible)
            {
                for (var i = 0; i < dataGrid.Rows.Count; i++)
                    dataGrid.Rows[i].HeaderCell.Value = (i + 1).ToString();
                dataGrid.AutoResizeRowHeadersWidth(DataGridViewRowHeadersWidthSizeMode.AutoSizeToDisplayedHeaders);
            }

            dataGrid.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.DisplayedCells);
        }
    }
}