using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using CsvQuery.PluginInfrastructure;

namespace CsvQuery
{
    using System.Diagnostics;
    using Community.CsharpSqlite;

    class Main
    {
        internal const string PluginName = "CsvQuery";
        static string iniFilePath = null;
        static bool someSetting = false;
        static frmMyDlg frmMyDlg = null;
        static int idMyDlg = -1;
        static Bitmap tbBmp_cq = Properties.Resources.cq;
        static Icon tbIcon = null;

        public static void OnNotification(ScNotification notification)
        {
            // This method is invoked whenever something is happening in notepad++
            // use eg. as
            // if (notification.Header.Code == (uint)NppMsg.NPPN_xxx)
            // { ... }
            // or
            //
            // if (notification.Header.Code == (uint)SciMsg.SCNxxx)
            // { ... }
        }

        internal static void CommandMenuInit()
        {
            StringBuilder sbIniFilePath = new StringBuilder(Win32.MAX_PATH);
            Win32.SendMessage(PluginBase.nppData._nppHandle, (uint) NppMsg.NPPM_GETPLUGINSCONFIGDIR, Win32.MAX_PATH, sbIniFilePath);
            iniFilePath = sbIniFilePath.ToString();
            if (!Directory.Exists(iniFilePath)) Directory.CreateDirectory(iniFilePath);
            iniFilePath = Path.Combine(iniFilePath, PluginName + ".ini");
            someSetting = (Win32.GetPrivateProfileInt("SomeSection", "SomeKey", 0, iniFilePath) != 0);

            PluginBase.SetCommand(0, "Popup test", myMenuFunction, new ShortcutKey(false, false, false, Keys.None));
            PluginBase.SetCommand(1, "Toggle query window", myDockableDialog); idMyDlg = 1;
            PluginBase.SetCommand(2, "Test Scintilla", ScinTest);
        }

        internal static void SetToolBarIcon()
        {
            toolbarIcons tbIcons = new toolbarIcons();
            tbIcons.hToolbarBmp = tbBmp_cq.GetHbitmap();
            IntPtr pTbIcons = Marshal.AllocHGlobal(Marshal.SizeOf(tbIcons));
            Marshal.StructureToPtr(tbIcons, pTbIcons, false);
            Win32.SendMessage(PluginBase.nppData._nppHandle, (uint) NppMsg.NPPM_ADDTOOLBARICON, PluginBase._funcItems.Items[idMyDlg]._cmdID, pTbIcons);
            Marshal.FreeHGlobal(pTbIcons);
        }

        internal static void PluginCleanUp()
        {
            Win32.WritePrivateProfileString("SomeSection", "SomeKey", someSetting ? "1" : "0", iniFilePath);
        }

        internal static void ScinTest()
        {
            var npp = new NotepadPPGateway();
            var gateway = PluginBase.GetGatewayFactory()();
            var length = gateway.GetLength();
            var codepage = gateway.GetCodePage();
            var bufferId = npp.GetCurrentBufferId();
            var text = gateway.GetAllText();
            MessageBox.Show($"Length: {length}, got: {text.Length}\nBuffer: {bufferId}, codepage: {codepage} \n{text.Substring(0, Math.Min(100, text.Length))}");
        }

        internal static void myMenuFunction()
        {
            // This tests the SQLite in-memory DB by creating some shit and selecting it
            MessageBox.Show("Hello N++!");
            var watch = new Stopwatch();
            watch.Start();
            var db = new SQLiteDatabase(":memory:");
            var t1 = watch.ElapsedMilliseconds; watch.Restart();
            db.ExecuteNonQuery("SELECT 1");
            watch.Restart();

            db.ExecuteNonQuery("CREATE TABLE Root (intIndex INTEGER PRIMARY KEY, strIndex TEXT, nr REAL)");
            var t2a = watch.ElapsedMilliseconds; watch.Restart();
            db.ExecuteNonQuery("CREATE TABLE This (intIndex INTEGER PRIMARY KEY, strIndex TEXT, nr REAL)");
            var t2 = watch.ElapsedMilliseconds; watch.Restart();
            db.ExecuteNonQuery("CREATE INDEX RootStrIndex ON Root (strIndex)");

            string INSERT_Command = "INSERT INTO Root VALUES (?,?,?)";
            int i;
            var stmt = new SQLiteVdbe(db, INSERT_Command);
            long start = DateTime.Now.Ticks;
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
            var t3 = watch.ElapsedMilliseconds; watch.Restart();

            key = Int64.MinValue;
            i = 0;
            var c1 = new SQLiteVdbe(db, "SELECT * FROM Root ORDER BY intIndex LIMIT 10");
            while (c1.ExecuteStep() != Sqlite3.SQLITE_DONE)
            {
                long intKey = (long)c1.Result_Long(0);
                //MessageBox.Show(intKey + ":" + c1.Result_Text(1) + ":" + c1.Result_Double(2));
                key = intKey;
                i += 1;
            }
            c1.Close();
            var t4 = watch.ElapsedMilliseconds; watch.Restart();

            MessageBox.Show("Times: \nCreate DB: " + t1 + "ms\nCreate table 1: " + t2a + "ms\nCreate table 2: " + t2 + "ms\nInsert: " + t3 + "ms\nSelect: " + t4 + "ms");
        }

        internal static void myDockableDialog()
        {
            if (frmMyDlg == null)
            {
                frmMyDlg = new frmMyDlg();

                using (Bitmap newBmp = new Bitmap(16, 16))
                {
                    Graphics g = Graphics.FromImage(newBmp);
                    ColorMap[] colorMap = new ColorMap[1];
                    colorMap[0] = new ColorMap();
                    colorMap[0].OldColor = Color.Fuchsia;
                    colorMap[0].NewColor = Color.FromKnownColor(KnownColor.ButtonFace);
                    ImageAttributes attr = new ImageAttributes();
                    attr.SetRemapTable(colorMap);
                    g.DrawImage(tbBmp_cq, new Rectangle(0, 0, 16, 16), 0, 0, 16, 16, GraphicsUnit.Pixel, attr);
                    tbIcon = Icon.FromHandle(newBmp.GetHicon());
                }

                NppTbData _nppTbData = new NppTbData();
                _nppTbData.hClient = frmMyDlg.Handle;
                _nppTbData.pszName = "CSV Query";
                _nppTbData.dlgID = idMyDlg;
                _nppTbData.uMask = NppTbMsg.DWS_DF_CONT_BOTTOM | NppTbMsg.DWS_ICONTAB | NppTbMsg.DWS_ICONBAR;
                _nppTbData.hIconTab = (uint)tbIcon.Handle;
                _nppTbData.pszModuleName = PluginName;
                IntPtr _ptrNppTbData = Marshal.AllocHGlobal(Marshal.SizeOf(_nppTbData));
                Marshal.StructureToPtr(_nppTbData, _ptrNppTbData, false);

                Win32.SendMessage(PluginBase.nppData._nppHandle, (uint) NppMsg.NPPM_DMMREGASDCKDLG, 0, _ptrNppTbData);
            }
            else
            {
                if (!frmMyDlg.Visible)
                {
                    Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)NppMsg.NPPM_DMMSHOW, 0, frmMyDlg.Handle);
                    Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)NppMsg.NPPM_SETMENUITEMCHECK, PluginBase._funcItems.Items[idMyDlg]._cmdID, 1);
                }
                else
                {
                    Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)NppMsg.NPPM_DMMHIDE, 0, frmMyDlg.Handle);
                    Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)NppMsg.NPPM_SETMENUITEMCHECK, PluginBase._funcItems.Items[idMyDlg]._cmdID, 0);
                }
            }
        }
    }
}