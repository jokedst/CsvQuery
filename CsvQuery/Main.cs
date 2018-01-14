using System.Reflection;

namespace CsvQuery
{
    using System;
    using System.Diagnostics;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Runtime.InteropServices;
    using System.Threading.Tasks;
    using System.Windows.Forms;
    using Community.CsharpSqlite;
    using Csv;
    using CsvQuery.PluginInfrastructure;
    using CsvQuery.Forms;
    using CsvQuery.Tools;
    using Database;
    using Properties;

    public class Main
    {
        public const string PluginName = "CsvQuery";
        public static Settings Settings = new Settings();

        public static QueryWindow QueryWindow;
        public static int MenuToggleId = -1;
        public static IDataStorage DataStorage;

        static Main()
        {
            CreateDataStorage(Settings);
            Settings.ValidateChanges += OnValidateChanges;
            Settings.SettingsChanged += (sender, args) => CreateDataStorage(args.NewSettings);
        }
        
        private static void CreateDataStorage(Settings settings)
        {
            try
            {
                switch (settings.StorageProvider)
                {
                    case DataStorageProvider.SQLite:
                        DataStorage = new SQLiteDataStorage(settings.Database);
                        break;
                    case DataStorageProvider.MSSQL:
                        DataStorage = new MssqlDataStorage(settings.Database);
                        break;
                }
            }
            catch (Exception e)
            {
                var msg = $"Error configuring the {settings.StorageProvider} database '{settings.Database}': {e.Message}\nFalling back to in-memory SQLite";
                Trace.TraceError(msg + Environment.NewLine + e.StackTrace);
                MessageBox.Show(msg, Resources.Title_CSV_Query_Error);
                DataStorage = new SQLiteDataStorage();
            }
        }

        private static void OnValidateChanges(object sender, SettingsChangedEventArgs e)
        {
            Trace.TraceInformation("Main.OnValidateChanges fired");
            if (!e.Changed.Contains(nameof(Settings.StorageProvider)) && !e.Changed.Contains(nameof(Settings.Database)))
                return;
            Trace.TraceInformation($"Main.OnValidateChanges relevant! type={e.NewSettings.StorageProvider}, db={e.NewSettings.Database}");
            try
            {
                IDataStorage newStorage;
                switch (e.NewSettings.StorageProvider)
                {
                    case DataStorageProvider.SQLite:
                        newStorage = new SQLiteDataStorage(e.NewSettings.Database);
                        break;
                    case DataStorageProvider.MSSQL:
                        newStorage = new MssqlDataStorage(e.NewSettings.Database);
                        break;
                    default:
                        throw new Exception("Unknown enum value "+ Settings.StorageProvider);
                }
                newStorage.TestConnection();
            }
            catch (Exception ex)
            {
                var msg = $"Error validating the {e.NewSettings.StorageProvider} database '{e.NewSettings.Database}': {ex.Message}";
                Trace.TraceError(msg + Environment.NewLine + ex.StackTrace);
                MessageBox.Show(msg, Resources.Title_CSV_Query_Error);
                e.Cancel = true;
            }
        }

        public static void OnNotification(ScNotification notification)
        {
            if (notification.Header.EventType == NppEventType.NPPN_WORDSTYLESUPDATED)
            {
                if (QueryWindow != null && Settings.UseNppStyling)
                {
                    QueryWindow.ApplyStyling(true);
                }
            }
            // This method is invoked whenever something is happening in notepad++. Use as:
            // if (notification.Header.Code == (uint)NppMsg.NPPN_xxx) {...}
            // (or SciMsg.SCNxxx)
            //Trace.TraceInformation($"Npp notification received: {notification.Header.EventType}");
        }

        public static void CommandMenuInit()
        {
            MenuToggleId = PluginBase.AddMenuItem("Toggle query window", ToggleQueryWindow, false, new ShortcutKey(true, true, false, Keys.C));
            PluginBase.AddMenuItem("Manual parse settings", ParseWithManualSettings);
            PluginBase.AddMenuItem("List parsed files", ListSqliteTables);
            PluginBase.AddMenuItem("---", null);
            PluginBase.AddMenuItem("&Settings", Settings.ShowDialog);
            PluginBase.AddMenuItem("&About", AboutCsvQuery);
        }

        private static void ParseWithManualSettings()
        {
            try
            {
                var askUserDialog = new ParseSettings
                {
                    MainLabel = {Text = "Manually enter values for parsing CSV\n\nUse this if detection fails"},
                    txbQuoteChar = {Text = Main.Settings.DefaultQuoteChar.ToString()},
                    txbSep = {Text = Main.Settings.DefaultSeparator}
                };
                if (askUserDialog.ShowDialog() != DialogResult.OK)
                    return;

                var csvSettings = askUserDialog.Settings;
                QueryWindowVisible(true, true);
                QueryWindow.StartParse(csvSettings);
            }
            catch (Exception e)
            {
                Trace.WriteLine("Error: "+e.Message);
            }
        }

        public static void SetToolBarIcon()
        {
            var icons = new toolbarIcons { hToolbarBmp = Resources.cq.GetHbitmap() };
            var iconPointer = Marshal.AllocHGlobal(Marshal.SizeOf(icons));
            Marshal.StructureToPtr(icons, iconPointer, false);
            Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)NppMsg.NPPM_ADDTOOLBARICON, PluginBase._funcItems.Items[MenuToggleId]._cmdID, iconPointer);
            Marshal.FreeHGlobal(iconPointer);
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

        /// <summary>
        /// Shows the "About" dialog window
        /// </summary>
        public static void AboutCsvQuery()
        {
            const int xsize = 300, ysize = 180;
            
            var gitVersionInformationType = Assembly.GetExecutingAssembly().GetType("CsvQuery.GitVersionInformation");
            var semVer = (string)gitVersionInformationType.GetField("SemVer").GetValue(null);

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
                        Text = $"CSV Query v{semVer}\r\n\r\nAllows SQL queries against CSV files.\r\n\r\nThe SQL syntax is the same as SQLite.\r\nThe table \"THIS\" represents the current file.\r\n\r\nBy jokedst@gmail.com\r\nLicense: GPL v3",
                        TextAlign = ContentAlignment.MiddleCenter,
                        Font = new Font("Consolas", 8.25F)
                    }
                }
            };
            dialog.Controls["Ok"].Click += (a, b) => dialog.Close();

            // In debug mode we add two buttons to the About dialog
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

                var settingsButton = new Button
                {
                    Text = "&Settings",
                    Size = new Size(75, 23),
                    Location = new Point(xsize - 75 - 13 - 81, ysize - 23 - 13),
                    UseVisualStyleBackColor = true
                };
                settingsButton.Click += (a, b) => Settings.OpenFile();
                dialog.Controls.Add(settingsButton);
            }

            dialog.ShowDialog();
        }

        /// <summary>
        /// This tests the SQLite in-memory DB by creating some data and then selecting it
        /// </summary>
        public static void TestDatabase()
        {
            var watch = new DiagnosticTimer();
            var db = new SQLiteDatabase(":memory:");
            watch.Checkpoint("Create DB");

            db.ExecuteNonQuery("CREATE TABLE Root (intIndex INTEGER PRIMARY KEY, strIndex TEXT, nr REAL)");
            watch.Checkpoint("Create table 1");
            db.ExecuteNonQuery("CREATE TABLE This (intIndex INTEGER PRIMARY KEY, strIndex TEXT, nr REAL)");
            watch.Checkpoint("Create table 2");
            db.ExecuteNonQuery("CREATE INDEX RootStrIndex ON Root (strIndex)");

            const string insertCommand = "INSERT INTO Root VALUES (?,?,?)";
            int i;
            var stmt = new SQLiteVdbe(db, insertCommand);
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

        public static void QueryWindowVisible(bool? show = null, bool supressAnalysis = false)
        {
            if (QueryWindow == null)
            {
                QueryWindow = new QueryWindow();
                Icon queryWindowIcon;

                using (var newBmp = new Bitmap(16, 16))
                {
                    var g = Graphics.FromImage(newBmp);
                    ColorMap[] colorMap = { new ColorMap { OldColor = Color.Fuchsia, NewColor = Color.FromKnownColor(KnownColor.ButtonFace) } };
                    var attr = new ImageAttributes();
                    attr.SetRemapTable(colorMap);
                    g.DrawImage(Properties.Resources.cq, new Rectangle(0, 0, 16, 16), 0, 0, 16, 16, GraphicsUnit.Pixel, attr);
                    queryWindowIcon = Icon.FromHandle(newBmp.GetHicon());
                }

                var queryWindowData = new NppTbData
                {
                    hClient = QueryWindow.Handle,
                    pszName = "CSV Query",
                    dlgID = MenuToggleId,
                    uMask = NppTbMsg.DWS_DF_CONT_BOTTOM | NppTbMsg.DWS_ICONTAB | NppTbMsg.DWS_ICONBAR,
                    hIconTab = (uint) queryWindowIcon.Handle,
                    pszModuleName = PluginName
                };
                var queryWindowPointer = Marshal.AllocHGlobal(Marshal.SizeOf(queryWindowData));
                Marshal.StructureToPtr(queryWindowData, queryWindowPointer, false);

                Win32.SendMessage(PluginBase.nppData._nppHandle, (uint) NppMsg.NPPM_DMMREGASDCKDLG, 0, queryWindowPointer);

                // Analyze current file
                if(!supressAnalysis)
                    QueryWindow.StartAnalysis(true);
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