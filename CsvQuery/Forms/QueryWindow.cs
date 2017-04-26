namespace CsvQuery.Forms
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics;
    using System.Linq;
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
            if(csvSettings.Separator == '\0')
            {
                MessageBox.Show("Could not figure out separator");
                return;
            }
            watch.Checkpoint("Analyze");

            dataGrid.DataSource = null;
            dataGrid.Rows.Clear();
            dataGrid.Columns.Clear();

            var data = csvSettings.Parse(text);
            watch.Checkpoint("Parse");

            DataStorage.SaveData(bufferId, data, null);
            watch.Checkpoint("Saved to DB");

            txbQuery.Text = "SELECT * FROM THIS";
            Execute(bufferId, watch);

            var diagnostic = watch.LastCheckpoint("Resize");
            Trace.TraceInformation(diagnostic);
            if(Main.Settings.DebugMode) MessageBox.Show(diagnostic);
        }

        private void Execute(IntPtr bufferId, DiagnosticTimer watch)
        {
            DataStorage.SetActiveTab(bufferId);
            watch.Checkpoint("Switch buffer");

            var query = txbQuery.Text;
            List<string[]> toshow;
            try
            {
                toshow = DataStorage.ExecuteQueryWithColumnNames(query);
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
            queryAutoComplete.Add(query);
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
    }
}
