// NPP plugin platform for .Net v0.93.96 by Kasper B. Graversen etc.
using System;
using System.Linq;

namespace CsvQuery.PluginInfrastructure
{
    using System.Drawing;

    class PluginBase
    {
        internal static NppData nppData;
        internal static FuncItems _funcItems = new FuncItems();
        protected static ScintillaGateway[] scintillaGateways = new ScintillaGateway[2];

        /// <summary>
        /// Adds an entry in the Notepad++ Plugin menu.
        /// </summary>
        /// <param name="text"> Menu item text. Prefix a character with "&" to hotkey it </param>
        /// <param name="onMenuClickedHandler"> Method to call when item is clicked </param>
        /// <param name="checkOnInit"> If true the menu item will have a checkmark initially</param>
        /// <param name="shortcut"> Keyboard chortcut </param>
        /// <returns> Index this menu item was given </returns>
        public static int AddMenuItem(string text, NppFuncItemDelegate onMenuClickedHandler, bool checkOnInit = false, ShortcutKey shortcut = new ShortcutKey())
        {
            var funcItem = new FuncItem { _itemName = text };
            if (onMenuClickedHandler != null)
                funcItem._pFunc = onMenuClickedHandler;
            if (shortcut._key != 0)
                funcItem._pShKey = shortcut;
            funcItem._init2Check = checkOnInit;
            return _funcItems.Add(funcItem);
        }

        public static int GetMenuItemId(string menuItemText)
        {
            try
            {
                return _funcItems.Items.First(item => item._itemName == menuItemText)._cmdID;
            }
            catch (InvalidOperationException e)
            {
                throw new ArgumentException($"Unknown menu item '{menuItemText}'", nameof(menuItemText), e);
            }

            //  ?? throw new ArgumentException("Unknown ´menu item '"+menuItemText+"'", nameof(menuItemText))
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

        public static Color GetDefaultForegroundColor()
        {
            var rawColor = (int)Win32.SendMessage(nppData._nppHandle, (uint)NppMsg.NPPM_GETEDITORDEFAULTFOREGROUNDCOLOR, 0, 0);
            return Color.FromArgb(rawColor & 0xff, (rawColor >> 8) & 0xff, (rawColor >> 16) & 0xff);
        }

        public static Color GetDefaultBackgroundColor()
        {
            var rawColor = (int)Win32.SendMessage(nppData._nppHandle, (uint)NppMsg.NPPM_GETEDITORDEFAULTBACKGROUNDCOLOR, 0, 0);
            return Color.FromArgb(rawColor & 0xff, (rawColor >> 8) & 0xff, (rawColor >> 16) & 0xff);
        }
    }
}
