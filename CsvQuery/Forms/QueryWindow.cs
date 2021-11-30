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
    using Database;
    using PluginInfrastructure;
    using Properties;
    using Tools;
    
    /// <summary>
    /// The query window that shows the current query and the results in a grid
    /// </summary><inheritdoc />
    public partial class QueryWindow : Form
    {
        /// <summary> Background worker </summary>
        private Task _worker = Task.CompletedTask;
        private Color[] _winColors = null;
        private CsvSettings _lastGenerateSettings = null;
        private (IntPtr bufferId, string query) _lastRunQuery = (IntPtr.Zero, null);

        public QueryWindow()
        {
            this.InitializeComponent();

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

                this.txbQuery.AutoCompleteCustomSource.AddRange(lines);
            }

            if (Main.Settings.UseNppStyling) this.ApplyStyling(true);
            
            Main.Settings.SettingsChanged += this.OnSettingsChanged;
        }

        private void OnSettingsChanged(object sender, SettingsChangedEventArgs e)
        {
            if (!e.Changed.Contains(nameof(Settings.UseNppStyling)))
                return;
            this.ApplyStyling(e.NewSettings.UseNppStyling);
        }

        /// <summary>
        /// Applies NPP colors to window
        /// </summary>
        public void ApplyStyling(bool active)
        {
            if (this._winColors == null) this._winColors = new[] {this.dataGrid.ForeColor, this.dataGrid.BackgroundColor, this.dataGrid.BackColor};
            if (active)
            {
                // Get NPP colors 
                var backgroundColor = PluginBase.GetDefaultBackgroundColor();
                var foreColor = PluginBase.GetDefaultForegroundColor();
                var inBetween = Color.FromArgb((foreColor.R + backgroundColor.R*3) / 4, (foreColor.G + backgroundColor.G*3) / 4, (foreColor.B + backgroundColor.B*3) / 4);

                this.ApplyColors(foreColor, backgroundColor, inBetween);
                this.dataGrid.EnableHeadersVisualStyles = false;
            }
            else
            {
                // Disable styling
                this.ApplyColors(this._winColors[0], this._winColors[2], this._winColors[1]);
                this.dataGrid.EnableHeadersVisualStyles = true;
            }
        }

        private void ApplyColors(Color foreColor, Color backgroundColor, Color inBetween)
        {
            Trace.TraceInformation($"FG {foreColor}, BG {backgroundColor}, inBetween {inBetween}");
            this.BackColor = backgroundColor;
            this.dataGrid.BackColor = backgroundColor;
            this.dataGrid.BackgroundColor = inBetween;
            this.dataGrid.ForeColor = foreColor;
            this.dataGrid.ColumnHeadersDefaultCellStyle.BackColor = backgroundColor;
            this.dataGrid.ColumnHeadersDefaultCellStyle.ForeColor = foreColor;
            this.dataGrid.EnableHeadersVisualStyles = false;

            this.txbQuery.BackColor = backgroundColor;
            this.txbQuery.ForeColor = foreColor;

            this.btnAnalyze.ForeColor = foreColor;
            this.btnAnalyze.BackColor = backgroundColor;
            this.btnExec.ForeColor = foreColor;
            this.btnExec.BackColor = backgroundColor;

            this.dataGrid.DefaultCellStyle.BackColor = backgroundColor;
        }

        /// <summary>
        ///     Executes given query and shows the result in this window
        /// </summary>
        /// <param name="query"> SQL query to run </param>
        public void ExecuteQuery(string query)
        {
            this.txbQuery.Text = query;
            this.btnExec.PerformClick();
        }

        private void OnClickAnalyzeButton(object sender, EventArgs e)
        {
            this.StartAnalysis(false);
        }

        private void StartSomething(Action someAction)
        {
            this.UiThread(() => this.UiEnabled(false));

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
                    this.UiThread(() => this.UiEnabled(true));
                }
            }

            var busy = false;
            lock (this._worker)
            {
                if (this._worker.IsCompleted)
                    this._worker = Task.Factory.StartNew(SafeAction);
                else busy = true;
            }
            if (busy)
            {
                this.Message("CSV Query is busy", Resources.Title_CSV_Query_Error);
                this.UiThread(() => this.UiEnabled(true));
            }
        }

        private void UiEnabled(bool enabled)
        {
            this.txbQuery.Enabled = enabled;
            this.btnAnalyze.Enabled = enabled;
            this.btnExec.Enabled = enabled;
        }

        public void StartAnalysis(bool silent)
        {
            this.StartSomething(() => this.Analyze(silent));
        }

        private void Analyze(bool silent)
        {
            var watch = new DiagnosticTimer();
            var bufferId = NotepadPPGateway.GetCurrentBufferId();

            var textLength = PluginBase.CurrentScintillaGateway.GetTextLength();
            var text = PluginBase.CurrentScintillaGateway.GetTextRange(0, Math.Min(100000, textLength));

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

            using (var sr = ScintillaStreams.StreamAllText())
            {
                this.Parse(csvSettings, watch, sr, bufferId);
            }
        }

        private void Parse(CsvSettings csvSettings, DiagnosticTimer watch, TextReader text, IntPtr bufferId)
        {
            IEnumerable<string[]> data;
            try
            {
                data = csvSettings.Parse(text);
            }
            catch (Exception e)
            {
                this.ErrorMessage("Error when parsing text:\n" + e.Message);
                return;
            }
            watch.Checkpoint("Parse");

            var count = 0;
            try
            {
                var first = true;
                const int partitionSize = DataSettings.ChunkSize;
                foreach (var chunk in data.Partition(partitionSize))
                {
                    if (first)
                    {
                        var wholeChunk = chunk.ToList();
                        var columnTypes = new CsvColumnTypes(wholeChunk, csvSettings);
                        Main.DataStorage.SaveData(bufferId, wholeChunk, columnTypes);
                        first = false;
                        watch.Checkpoint("Saved first chunk to DB");
                    }
                    else
                    {
                        count += partitionSize;
                        var msg = $"Read lines: {count}";
                        this.UiThread(() => this.txbQuery.Text = msg);
                        Main.DataStorage.SaveMore(bufferId, chunk);
                    }
                }
            }
            catch (Exception ex)
            {
                this.ErrorMessage("Error when saving data to database:\n" + ex.Message);
                return;
            }
            watch.Checkpoint("Saved to DB");
            // I this is a refresh, i.e. parsing the same file as was there before, don't replace the existing query
            if (!(this.dataGrid.DataSource is DataTable table)
                || !table.ExtendedProperties.ContainsKey("bufferId")
                || !(table.ExtendedProperties["bufferId"] is IntPtr previousBufferId)
                || previousBufferId != bufferId
                || string.IsNullOrWhiteSpace(this.txbQuery.Text)
                || this._lastRunQuery.query == null
                || this._lastRunQuery.bufferId != previousBufferId)
            {
                var selectQuery = "SELECT * FROM THIS";
                if (count > 10000) selectQuery = Main.DataStorage.CreateLimitedSelect(10000);
                this.UiThread(() => this.txbQuery.Text = selectQuery);
            }
            else if (this._lastRunQuery.bufferId == previousBufferId && this._lastRunQuery.query != null)
            {
                this.UiThread(() => this.txbQuery.Text = this._lastRunQuery.query);
            }

            this.Execute(bufferId, watch);

            var diagnostic = watch.LastCheckpoint("Resize");
            Trace.TraceInformation(diagnostic);
            if (Main.Settings.DebugMode)
                this.Message(diagnostic);
        }

        public void StartParse(CsvSettings settings)
        {
            this.StartSomething(() =>
            {
                using (var sr = ScintillaStreams.StreamAllText())
                {
                    this.Parse(settings,
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

            var query = this.txbQuery.Text;
            List<string[]> toshow;
            try
            {
                toshow = Main.DataStorage.ExecuteQuery(query, true);
            }
            catch(DataStorageException e)
            {
                this.Message("Could not execute query:\n" + e.Message, Resources.Title_CSV_Query_Error);
                return;
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
                var safeColumnName = s.Replace(",", string.Empty);
                if (table.Columns.Contains(safeColumnName))
                {
                    var safePrefix = safeColumnName;
                    if (safePrefix[safePrefix.Length - 1] >= '0' && safePrefix[safePrefix.Length - 1] <= '9') safePrefix = safePrefix + "_";
                    var c = 2;
                    do safeColumnName = safePrefix + c++; while (table.Columns.Contains(safeColumnName));
                }
                table.Columns.Add(safeColumnName);
            }

            // Insert rows
            foreach (var row in toshow.Skip(1))
                table.Rows.Add(row);
            watch.Checkpoint("Create DataTable");

            this.UiThread(() =>
            {
                this.dataGrid.DataSource = table;
                // Enforce correct column order
                foreach (DataGridViewColumn col in this.dataGrid.Columns)
                    col.DisplayIndex = col.Index;
            });
            watch.Checkpoint("Display");

            // Store query in history
            this._lastRunQuery = (bufferId, query);
            if (!this.txbQuery.AutoCompleteCustomSource.Contains(query))
            {
                this.UiThread(() => this.txbQuery.AutoCompleteCustomSource.Add(query));
                if (Main.Settings.SaveQueryCache)
                    using (var writer = File.AppendText(PluginStorage.QueryCachePath))
                    {
                        writer.WriteLine(query);
                    }
            }
        }

        private void OnClickExecButton(object sender, EventArgs e)
        {
            this.StartSomething(() =>
            {
                var watch = new DiagnosticTimer();
                var bufferId = NotepadPPGateway.GetCurrentBufferId();

                this.Execute(bufferId, watch);

                var diagnosticMessage = watch.LastCheckpoint("Save query in history");
                Trace.TraceInformation(diagnosticMessage);
                if (Main.Settings.DebugMode)
                    this.Message(diagnosticMessage);
            });
        }

        private void OnQueryTextboxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return) this.btnExec.PerformClick();
        }

        private void OnMenuClickCreateNewCSV(object sender, EventArgs e)
        {
            if (!(this.dataGrid.DataSource is DataTable table ))
            {
                MessageBox.Show("No results available to convert");
                return;
            }

            var initialSettings = this._lastGenerateSettings 
                                  ?? new CsvSettings(Main.Settings.DefaultSeparator.Unescape(), true, '\0', true);
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
                    var producer = stream.StartProducer(s => settings.GenerateToStream(this.dataGrid.DataSource as DataTable, s, headerLookup));
                    var consumer = stream.StartConsumer(s => PluginBase.CurrentScintillaGateway.AddText(s));
                    Task.WaitAll(producer, consumer);
                }
            }
            catch (Exception ex)
            {
                Trace.TraceInformation("CSV gen: Exception: " + ex.GetType().Name + " - " + ex.Message);
            }

            this._lastGenerateSettings = settings;
            Trace.TraceInformation(watch.LastCheckpoint("CSV Done"));
        }

        public void CopyDataToClipboard(DataGridViewClipboardCopyMode mode)
        {
            if (this.dataGrid.GetCellCount(DataGridViewElementStates.Selected) == 0) return;
            var previousValue = this.dataGrid.ClipboardCopyMode;
            this.dataGrid.ClipboardCopyMode = mode;
            var clipboardContent = this.dataGrid.GetClipboardContent();
            this.dataGrid.ClipboardCopyMode = previousValue;
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
            => this.CopyDataToClipboard(DataGridViewClipboardCopyMode.EnableWithoutHeaderText);

        private void OnContextmenuCopyWithHeaders(object sender, EventArgs e) 
            => this.CopyDataToClipboard(DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText);

        private void OnContextmenuSelectAll(object sender, EventArgs e) 
            => this.dataGrid.SelectAll();

        private void OnContextmenuShowRowNumbers(object sender, EventArgs e)
        {
            this.contextmenuShowRowNumbers.Checked = this.dataGrid.RowHeadersVisible = !this.dataGrid.RowHeadersVisible;
            this.FormatDataGrid();
        }

        private void OnDataBindingComplete(object grid, DataGridViewBindingCompleteEventArgs args) => this.FormatDataGrid();

        private void FormatDataGrid()
        {
            if (this.dataGrid.RowHeadersVisible)
            {
                for (var i = 0; i < this.dataGrid.Rows.Count; i++) this.dataGrid.Rows[i].HeaderCell.Value = (i + 1).ToString();
                this.dataGrid.AutoResizeRowHeadersWidth(DataGridViewRowHeadersWidthSizeMode.AutoSizeToDisplayedHeaders);
            }

            this.dataGrid.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.DisplayedCells);
        }
    }
}