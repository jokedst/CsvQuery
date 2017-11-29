// NPP plugin platform for .Net v0.93.96 by Kasper B. Graversen etc.
using System;

namespace CsvQuery.PluginInfrastructure
{
    class PluginBase
    {
        internal static NppData nppData;
        internal static FuncItems _funcItems = new FuncItems();
        protected static ScintillaGateway[] scintillaGateways = new ScintillaGateway[2];

        /// <summary>
        /// Adds an entry in the Notepad++ Plugin menu.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="functionPointer"></param>
        /// <param name="checkOnInit"></param>
        /// <param name="shortcut"></param>
        /// <returns></returns>
        public static int AddMenuItem(string text, NppFuncItemDelegate functionPointer, bool checkOnInit = false, ShortcutKey shortcut = new ShortcutKey())
        {
            var funcItem = new FuncItem { _itemName = text };
            if (functionPointer != null)
                funcItem._pFunc = functionPointer;
            if (shortcut._key != 0)
                funcItem._pShKey = shortcut;
            funcItem._init2Check = checkOnInit;
            return _funcItems.Add(funcItem);
        }

        [Obsolete("The 'index' parameter is not used! Use 'AddMenuItem' instead!")]
        internal static void SetCommand(int index, string commandName, NppFuncItemDelegate functionPointer)
        {
            AddMenuItem(commandName, functionPointer);
        }

        [Obsolete("The 'index' parameter is not used! Use 'AddMenuItem' instead!")]
        internal static void SetCommand(int index, string commandName, NppFuncItemDelegate functionPointer, ShortcutKey shortcut)
        {
            AddMenuItem(commandName, functionPointer, false, shortcut);
        }

        [Obsolete("The 'index' parameter is not used! Use 'AddMenuItem' instead!")]
        internal static void SetCommand(int index, string commandName, NppFuncItemDelegate functionPointer, bool checkOnInit)
        {
            AddMenuItem(commandName, functionPointer, checkOnInit);
        }

        [Obsolete("The 'index' parameter is not used! Use 'AddMenuItem' instead!")]
        internal static void SetCommand(int index, string commandName, NppFuncItemDelegate functionPointer, ShortcutKey shortcut, bool checkOnInit)
        {
            AddMenuItem(commandName, functionPointer, checkOnInit, shortcut);
        }

        internal static IntPtr GetCurrentScintilla()
        {
            int curScintilla;
            Win32.SendMessage(nppData._nppHandle, (uint) NppMsg.NPPM_GETCURRENTSCINTILLA, 0, out curScintilla);
            return (curScintilla == 0) ? nppData._scintillaMainHandle : nppData._scintillaSecondHandle;
        }

        static readonly Func<IScintillaGateway> gatewayFactory = () => new ScintillaGateway(GetCurrentScintilla());

        public static Func<IScintillaGateway> GetGatewayFactory()
        {
            return gatewayFactory;
        }

        /// <summary> Get gateway to currently active scintilla  </summary>
        /// <remarks>
        /// Notepad++ has two instances of Scintilla - the main one, and a second one that is only used when you show two documents side-by-side.
        /// Since a document can be moved between these at any time, we need to check current scintilla constantly (or listen to events, but meh)
        /// </remarks>
        public static ScintillaGateway CurrentScintillaGateway
        {
            get
            {
                Win32.SendMessage(nppData._nppHandle, (uint)NppMsg.NPPM_GETCURRENTSCINTILLA, 0, out int curScintilla);
                return scintillaGateways[curScintilla] ?? (scintillaGateways[curScintilla] = new ScintillaGateway(
                           curScintilla == 0
                               ? nppData._scintillaMainHandle
                               : nppData._scintillaSecondHandle));
            }
        }

        public static int GetDefaultForegroundColor()
        {
            return (int)Win32.SendMessage(nppData._nppHandle, (uint)NppMsg.NPPM_GETEDITORDEFAULTFOREGROUNDCOLOR, 0, 0);
        }

        public static int GetDefaultBackgroundColor()
        {
            return (int)Win32.SendMessage(nppData._nppHandle, (uint)NppMsg.NPPM_GETEDITORDEFAULTBACKGROUNDCOLOR, 0, 0);
        }
    }
}
