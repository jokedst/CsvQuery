namespace CsvQuery.Forms
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Windows.Forms;
    using Csv;
    using PluginInfrastructure;
    using Tools;

    /// <summary>
    /// The query window that whos the current query and the results in a grid
    /// </summary>
    public partial class QueryWindow : Form
    {
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
        }

        /// <summary>
        /// Executes given query and shows the result in this window
        /// </summary>
        /// <param name="query"> SQL query to run </param>
        public void ExecuteQuery(string query)
        {
            txbQuery.Text = query;
            btnExec.PerformClick();
        }

        private void btnAnalyze_Click(object sender, EventArgs e)
        {
            var watch = new DiagnosticTimer();
            var bufferId = NotepadPPGateway.GetCurrentBufferId();
            string text = PluginBase.CurrentScintillaGateway.GetAllText();
            watch.Checkpoint("GetText");

            var csvSettings = CsvAnalyzer.Analyze(text);
            if (csvSettings.Separator == '\0' && csvSettings.FieldWidths == null)
            {
                var askUserDialog = new ParseSettings();
                var userChoice = askUserDialog.ShowDialog();
                if (userChoice != DialogResult.OK)
                    return;
                csvSettings.Separator = askUserDialog.txbSep.Text.Unescape();
                csvSettings.TextQualifier = askUserDialog.txbQuoteChar.Text.Unescape();
            }
            watch.Checkpoint("Analyze");

            dataGrid.DataSource = null;
            dataGrid.Rows.Clear();
            dataGrid.Columns.Clear();

            var data = csvSettings.Parse(text);
            watch.Checkpoint("Parse");

            Main.DataStorage.SaveData(bufferId, data, null);
            watch.Checkpoint("Saved to DB");

            txbQuery.Text = "SELECT * FROM THIS";
            Execute(bufferId, watch);

            var diagnostic = watch.LastCheckpoint("Resize");
            Trace.TraceInformation(diagnostic);
            if(Main.Settings.DebugMode) MessageBox.Show(diagnostic);
        }

        public void ParseBuffer(CsvSettings settings, IntPtr bufferId = default(IntPtr))
        {
            if (bufferId == default(IntPtr))
                bufferId = NotepadPPGateway.GetCurrentBufferId();
            
            string text = PluginBase.CurrentScintillaGateway.GetAllText();
            var data = settings.Parse(text);
            Main.DataStorage.SaveData(bufferId, data, null);
            ExecuteQuery("SELECT * FROM THIS");
        }

        private void Execute(IntPtr bufferId, DiagnosticTimer watch)
        {
            Main.DataStorage.SetActiveTab(bufferId);
            watch.Checkpoint("Switch buffer");

            var query = txbQuery.Text;
            List<string[]> toshow;
            try
            {
                toshow = Main.DataStorage.ExecuteQueryWithColumnNames(query);
            }
            catch (Exception)
            {
                MessageBox.Show("Could not execute query", "Error in query");
                return;
            }
            watch.Checkpoint("Execute query");

            var table = new DataTable();
            // Create columns
            foreach (var s in toshow[0])
            {
                table.Columns.Add(s);
            }

            // Insert rows
            foreach (var row in toshow.Skip(1))
            {
                table.Rows.Add(row);
            }
            watch.Checkpoint("Create DataTable");

            dataGrid.DataSource = table;
            dataGrid.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.DisplayedCells);
            watch.Checkpoint("Display");

            // Store query in history
            if (!txbQuery.AutoCompleteCustomSource.Contains(query))
            {
                txbQuery.AutoCompleteCustomSource.Add(query);
                if (Main.Settings.SaveQueryCache)
                {
                    using (var writer = File.AppendText(PluginStorage.QueryCachePath))
                    {
                        writer.WriteLine(query);
                    }
                }
            }
        }

        private void btnExec_Click(object sender, EventArgs e)
        {
            var watch = new DiagnosticTimer();
            var bufferId = Win32.SendMessage(PluginBase.nppData._nppHandle,(uint) NppMsg.NPPM_GETCURRENTBUFFERID, 0, 0);
           
            Execute(bufferId, watch);

            var diagnosticMessage = watch.LastCheckpoint("Save query in history");
            Trace.TraceInformation(diagnosticMessage);
            if (Main.Settings.DebugMode) MessageBox.Show(diagnosticMessage);
        }

        private void txbQuery_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                btnExec.PerformClick();
            }
        }

        private void createNewCSVToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataGrid.DataSource == null)
            {
                MessageBox.Show("No results available to convert");
                return;
            }
            var settingsDialog = new ParseSettings
            {
                btnReparse = {Text = "&Ok"},
                MainLabel = {Text = "How should the CSV be generated?"},
                txbSep = {Text = Main.Settings.DefaultSeparator},
                txbQuoteChar = {Text = Main.Settings.DefaultQuoteChar.ToString()}
            };

            if (settingsDialog.ShowDialog() == DialogResult.Cancel) return;

            var settings = new CsvSettings
            {
                Separator = settingsDialog.txbSep.Text.Unescape(),
                TextQualifier = settingsDialog.txbQuoteChar.Text.Unescape()
            };
            var csvData = settings.Generate(dataGrid);


            // Create new tab for results
            Win32.SendMessage(PluginBase.nppData._nppHandle, (uint) NppMsg.NPPM_MENUCOMMAND, 0, NppMenuCmd.IDM_FILE_NEW);
            PluginBase.CurrentScintillaGateway.AddText(csvData.Length, csvData.ToString());
        }
    }
}
