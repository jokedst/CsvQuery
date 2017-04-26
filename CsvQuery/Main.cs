namespace CsvQuery
{
    using System;
    using System.Diagnostics;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;
    using Community.CsharpSqlite;
    using CsvQuery.PluginInfrastructure;
    using CsvQuery.Forms;
    using CsvQuery.Tools;

    internal class Main
    {
        public const string PluginName = "CsvQuery";
        public static Settings Settings = new Settings();
        public static QueryWindow QueryWindow;
        public static int MenuToggleId = -1;

        public static void OnNotification(ScNotification notification)
        {
            // This method is invoked whenever something is happening in notepad++. Use as:
            // if (notification.Header.Code == (uint)NppMsg.NPPN_xxx) {...}
            // (or SciMsg.SCNxxx)
        }

        public static void CommandMenuInit()
        {
            MenuToggleId = PluginBase.AddMenuItem("Toggle query window", ToggleQueryWindow, true, new ShortcutKey(true, true, false, Keys.C));
            PluginBase.AddMenuItem("List parsed files", ListSqliteTables);
            PluginBase.AddMenuItem("---", null);
            PluginBase.AddMenuItem("&Settings", Settings.ShowDialog);
            PluginBase.AddMenuItem("Settings file", Settings.OpenFile);
            PluginBase.AddMenuItem("&About", AboutCsvQuery);
        }

        public static void SetToolBarIcon()
        {
            toolbarIcons tbIcons = new toolbarIcons();
            tbIcons.hToolbarBmp = Properties.Resources.cq.GetHbitmap();
            IntPtr pTbIcons = Marshal.AllocHGlobal(Marshal.SizeOf(tbIcons));
            Marshal.StructureToPtr(tbIcons, pTbIcons, false);
            Win32.SendMessage(PluginBase.nppData._nppHandle, (uint) NppMsg.NPPM_ADDTOOLBARICON, PluginBase._funcItems.Items[MenuToggleId]._cmdID, pTbIcons);
            Marshal.FreeHGlobal(pTbIcons);
        }

        public static void PluginCleanUp()
        {
            Settings.SaveToIniFile();
        }

        public static void ListSqliteTables()
        {
            QueryWindowVisible(true);
            QueryWindow.ExecuteQuery("SELECT * FROM sqlite_master");
        }

        public static void AboutCsvQuery()
        {
            const int xsize = 300, ysize = 180;

            var dialog = new Form
            {
                Text = "About CSV Query",
                ClientSize = new Size(xsize, ysize),
                SizeGripStyle = SizeGripStyle.Hide,
                ShowIcon = false,
                FormBorderStyle = FormBorderStyle.FixedToolWindow,
                ShowInTaskbar = false,
                Controls =
                {
                    new Button
                    {
                        Name = "Ok",
                        Text = "&Ok",
                        Size = new Size(75, 23),
                        Location = new Point(xsize - 75 - 13, ysize - 23 - 13),
                        UseVisualStyleBackColor = true
                    },
                    new Label
                    {
                        Location = new Point(13,13),
                        Size = new Size(xsize-13-13,ysize-13-13-23-6),
                        Text = "CSV Query\r\n\r\nAllows SQL queries against CSV files.\r\n\r\nThe SQL syntax is the same as SQLite.\r\nThe table \"THIS\" represents the current file.\r\n\r\nBy jokedst",
                        TextAlign = ContentAlignment.MiddleCenter,
                        Font = new Font("Consolas", 8.25F)
                    }
                }
            };
            dialog.Controls["Ok"].Click += (a, b) => dialog.Close();

            if (Settings.DebugMode)
            {
                var testbutton = new Button
                {
                    Text = "Test",
                    Size = new Size(75, 23),
                    Location = new Point(13, ysize - 23 - 13),
                    UseVisualStyleBackColor = true
                };
                testbutton.Click += (a, b) => TestDatabase();
                dialog.Controls.Add(testbutton);
            }

            dialog.ShowDialog();
        }

        public static void TestDatabase()
        {
            // This tests the SQLite in-memory DB by creating some shit and selecting it
            var watch = new DiagnosticTimer();
            var db = new SQLiteDatabase(":memory:");
            watch.Checkpoint("Create DB");

            db.ExecuteNonQuery("CREATE TABLE Root (intIndex INTEGER PRIMARY KEY, strIndex TEXT, nr REAL)");
            watch.Checkpoint("Create table 1");
            db.ExecuteNonQuery("CREATE TABLE This (intIndex INTEGER PRIMARY KEY, strIndex TEXT, nr REAL)");
            watch.Checkpoint("Create table 2");
            db.ExecuteNonQuery("CREATE INDEX RootStrIndex ON Root (strIndex)");

            string INSERT_Command = "INSERT INTO Root VALUES (?,?,?)";
            int i;
            var stmt = new SQLiteVdbe(db, INSERT_Command);
            long key = 1999;
            for (i = 0; i < 10000; i++)
            {
                key = (3141592621L * key + 2718281829L) % 1000000007L;
                stmt.Reset();
                stmt.BindLong(1, key);
                stmt.BindText(2, key.ToString());
                stmt.BindDouble(3, 12.34);
                stmt.ExecuteStep();
            }
            stmt.Close();
            watch.Checkpoint("Insert 10000 rows");
            
            i = 0;
            var c1 = new SQLiteVdbe(db, "SELECT * FROM Root ORDER BY intIndex LIMIT 5000");
            while (c1.ExecuteStep() != Sqlite3.SQLITE_DONE)
            {
                long intKey = c1.Result_Long(0);
                key = intKey;
                i += 1;
            }
            c1.Close();
            var diagnostic = watch.LastCheckpoint("Select 5000 sorted rows");
            MessageBox.Show(diagnostic);
        }

        private static void ToggleQueryWindow()
        {
            QueryWindowVisible();
        }

        internal static void QueryWindowVisible(bool? show = null)
        {
            if (QueryWindow == null)
            {
                QueryWindow = new QueryWindow();
                Icon tbIcon;

                using (Bitmap newBmp = new Bitmap(16, 16))
                {
                    Graphics g = Graphics.FromImage(newBmp);
                    ColorMap[] colorMap = new ColorMap[1];
                    colorMap[0] = new ColorMap();
                    colorMap[0].OldColor = Color.Fuchsia;
                    colorMap[0].NewColor = Color.FromKnownColor(KnownColor.ButtonFace);
                    ImageAttributes attr = new ImageAttributes();
                    attr.SetRemapTable(colorMap);
                    g.DrawImage(Properties.Resources.cq, new Rectangle(0, 0, 16, 16), 0, 0, 16, 16, GraphicsUnit.Pixel, attr);
                    tbIcon = Icon.FromHandle(newBmp.GetHicon());
                }

                NppTbData _nppTbData = new NppTbData();
                _nppTbData.hClient = QueryWindow.Handle;
                _nppTbData.pszName = "CSV Query";
                _nppTbData.dlgID = MenuToggleId;
                _nppTbData.uMask = NppTbMsg.DWS_DF_CONT_BOTTOM | NppTbMsg.DWS_ICONTAB | NppTbMsg.DWS_ICONBAR;
                _nppTbData.hIconTab = (uint)tbIcon.Handle;
                _nppTbData.pszModuleName = PluginName;
                IntPtr _ptrNppTbData = Marshal.AllocHGlobal(Marshal.SizeOf(_nppTbData));
                Marshal.StructureToPtr(_nppTbData, _ptrNppTbData, false);

                Win32.SendMessage(PluginBase.nppData._nppHandle, (uint) NppMsg.NPPM_DMMREGASDCKDLG, 0, _ptrNppTbData);
            }
            else
            {
                if (show ?? !QueryWindow.Visible)
                {
                    Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)NppMsg.NPPM_DMMSHOW, 0, QueryWindow.Handle);
                    Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)NppMsg.NPPM_SETMENUITEMCHECK, PluginBase._funcItems.Items[MenuToggleId]._cmdID, 1);
                }
                else
                {
                    Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)NppMsg.NPPM_DMMHIDE, 0, QueryWindow.Handle);
                    Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)NppMsg.NPPM_SETMENUITEMCHECK, PluginBase._funcItems.Items[MenuToggleId]._cmdID, 0);
                }
            }
        }
    }
}