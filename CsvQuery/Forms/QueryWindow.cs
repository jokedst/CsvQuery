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

    public partial class QueryWindow : Form
    {
        public QueryWindow()
        {
            InitializeComponent();
        }

        public void ExecuteQuery(string query)
        {
            txbQuery.Text = query;
            btnExec.PerformClick();
        }

        private void btnAnalyze_Click(object sender, EventArgs e)
        {
            var watch = new DiagnosticTimer();
            var sci = PluginBase.GetCurrentScintilla();
            var length = (int)Win32.SendMessage(sci, SciMsg.SCI_GETLENGTH, 0, 0);
            var codepage = (int)Win32.SendMessage(sci, SciMsg.SCI_GETCODEPAGE, 0, 0);
            var bufferId = Win32.SendMessage(PluginBase.nppData._nppHandle,(uint) NppMsg.NPPM_GETCURRENTBUFFERID, 0, 0);
            string text;
            using (var tr = new TextRange(0, length))
            //using (Sci_TextRange tr = new Sci_TextRange(0, length, length + 1))
            {
                Win32.SendMessage(sci, SciMsg.SCI_GETTEXTRANGE, 0, tr.NativePointer);

                switch (codepage)
                {
                    case (int)SciMsg.SC_CP_UTF8:
                        text = tr.GetFromUtf8();
                        break;
                    case (int)SciMsg.SC_CP_DBCS: // Double Byte Character Set, like unicode (utf16) - this never seems to happen, when the text is utc-2 we get SC_CP_UTF8 although that is a lie!
                        text = tr.GetFromUnicode();
                        break;
                    case 0: // ansi?
                        text = tr.lpstrText;
                        break;
                    default:
                        text = tr.lpstrText;
                        break;
                }

                //MessageBox.Show("Length: " + length + ", got: " + text.Length + " \n" + text.Substring(0, Math.Min(100, text.Length)));
            }
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
