namespace CsvQuery.PluginInfrastructure
{
    public enum NppEventType : uint
    {
        /// <summary>
        /// To notify plugins that all the procedures of launchment of notepad++ are done.
        ///scnNotification->nmhdr.code = NPPN_READY;
        ///scnNotification->nmhdr.hwndFrom = hwndNpp;
        ///scnNotification->nmhdr.idFrom = 0;
        /// </summary>
        NPPN_FIRST = 1000,
        /// <summary>
        ///scnNotification->nmhdr.code = NPPN_READY;
        ///scnNotification->nmhdr.hwndFrom = hwndNpp;
        ///scnNotification->nmhdr.idFrom = 0;
        /// </summary>
        NPPN_READY = 1000 + 1,

        /// <summary>
        ///scnNotification->nmhdr.code = NPPN_TB_MODIFICATION;
        ///scnNotification->nmhdr.hwndFrom = hwndNpp;
        ///scnNotification->nmhdr.idFrom = 0;
        /// </summary>
        NPPN_TBMODIFICATION = 1000 + 2,

        /// <summary>
        ///scnNotification->nmhdr.code = NPPN_FILEBEFORECLOSE;
        ///scnNotification->nmhdr.hwndFrom = hwndNpp;
        ///scnNotification->nmhdr.idFrom = BufferID;
        /// </summary>
        NPPN_FILEBEFORECLOSE = 1000 + 3,

        /// <summary>
        ///scnNotification->nmhdr.code = NPPN_FILEOPENED;
        ///scnNotification->nmhdr.hwndFrom = hwndNpp;
        ///scnNotification->nmhdr.idFrom = BufferID;
        /// </summary>
        NPPN_FILEOPENED = 1000 + 4,

        /// <summary>
        ///scnNotification->nmhdr.code = NPPN_FILECLOSED;
        ///scnNotification->nmhdr.hwndFrom = hwndNpp;
        ///scnNotification->nmhdr.idFrom = BufferID;
        /// </summary>
        NPPN_FILECLOSED = 1000 + 5,

        /// <summary>
        ///scnNotification->nmhdr.code = NPPN_FILEBEFOREOPEN;
        ///scnNotification->nmhdr.hwndFrom = hwndNpp;
        ///scnNotification->nmhdr.idFrom = BufferID;
        /// </summary>
        NPPN_FILEBEFOREOPEN = 1000 + 6,

        /// <summary>
        ///scnNotification->nmhdr.code = NPPN_FILEBEFOREOPEN;
        ///scnNotification->nmhdr.hwndFrom = hwndNpp;
        ///scnNotification->nmhdr.idFrom = BufferID;
        /// </summary>
        NPPN_FILEBEFORESAVE = 1000 + 7,

        /// <summary>
        ///scnNotification->nmhdr.code = NPPN_FILESAVED;
        ///scnNotification->nmhdr.hwndFrom = hwndNpp;
        ///scnNotification->nmhdr.idFrom = BufferID;
        /// </summary>
        NPPN_FILESAVED = 1000 + 8,

        /// <summary>
        ///scnNotification->nmhdr.code = NPPN_SHUTDOWN;
        ///scnNotification->nmhdr.hwndFrom = hwndNpp;
        ///scnNotification->nmhdr.idFrom = 0;
        /// </summary>
        NPPN_SHUTDOWN = 1000 + 9,

        /// <summary>
        ///scnNotification->nmhdr.code = NPPN_BUFFERACTIVATED;
        ///scnNotification->nmhdr.hwndFrom = hwndNpp;
        ///scnNotification->nmhdr.idFrom = activatedBufferID;
        /// </summary>
        NPPN_BUFFERACTIVATED = 1000 + 10,

        /// <summary>
        ///scnNotification->nmhdr.code = NPPN_LANGCHANGED;
        ///scnNotification->nmhdr.hwndFrom = hwndNpp;
        ///scnNotification->nmhdr.idFrom = currentBufferID;
        /// </summary>
        NPPN_LANGCHANGED = 1000 + 11,

        /// <summary>
        ///scnNotification->nmhdr.code = NPPN_WORDSTYLESUPDATED;
        ///scnNotification->nmhdr.hwndFrom = hwndNpp;
        ///scnNotification->nmhdr.idFrom = currentBufferID;
        /// </summary>
        NPPN_WORDSTYLESUPDATED = 1000 + 12,

        /// <summary>
        ///scnNotification->nmhdr.code = NPPN_SHORTCUTSREMAPPED;
        ///scnNotification->nmhdr.hwndFrom = ShortcutKeyStructurePointer;
        ///scnNotification->nmhdr.idFrom = cmdID;
        ///where ShortcutKeyStructurePointer is pointer of struct ShortcutKey:
        ///struct ShortcutKey {
        ///	bool _isCtrl;
        ///	bool _isAlt;
        ///	bool _isShift;
        ///	UCHAR _key;
        ///};
        /// </summary>
        NPPN_SHORTCUTREMAPPED = 1000 + 13,

        /// <summary>
        ///scnNotification->nmhdr.code = NPPN_FILEBEFOREOPEN;
        ///scnNotification->nmhdr.hwndFrom = hwndNpp;
        ///scnNotification->nmhdr.idFrom = NULL;
        /// </summary>
        NPPN_FILEBEFORELOAD = 1000 + 14,

        /// <summary>
        ///scnNotification->nmhdr.code = NPPN_FILEOPENFAILED;
        ///scnNotification->nmhdr.hwndFrom = hwndNpp;
        ///scnNotification->nmhdr.idFrom = BufferID;
        /// </summary>
        NPPN_FILELOADFAILED = 1000 + 15,

        /// <summary>
        ///scnNotification->nmhdr.code = NPPN_READONLYCHANGED;
        ///scnNotification->nmhdr.hwndFrom = bufferID;
        ///scnNotification->nmhdr.idFrom = docStatus;
        /// where bufferID is BufferID
        ///       docStatus can be combined by DOCSTAUS_READONLY and DOCSTAUS_BUFFERDIRTY
        /// </summary>
        NPPN_READONLYCHANGED = 1000 + 16,

        /// <summary>
        ///scnNotification->nmhdr.code = NPPN_DOCORDERCHANGED;
        ///scnNotification->nmhdr.hwndFrom = newIndex;
        ///scnNotification->nmhdr.idFrom = BufferID;
        /// </summary>
        NPPN_DOCORDERCHANGED = 1000 + 17,

        /// <summary>
        ///scnNotification->nmhdr.code = NPPN_SNAPSHOTDIRTYFILELOADED;
        ///scnNotification->nmhdr.hwndFrom = NULL;
        ///scnNotification->nmhdr.idFrom = BufferID;
        /// </summary>
        NPPN_SNAPSHOTDIRTYFILELOADED = 1000 + 18,

        /// <summary>
        ///scnNotification->nmhdr.code = NPPN_BEFORESHUTDOWN;
        ///scnNotification->nmhdr.hwndFrom = hwndNpp;
        ///scnNotification->nmhdr.idFrom = 0;
        /// </summary>
        NPPN_BEFORESHUTDOWN = 1000 + 19,

        /// <summary>
        ///scnNotification->nmhdr.code = NPPN_CANCELSHUTDOWN;
        ///scnNotification->nmhdr.hwndFrom = hwndNpp;
        ///scnNotification->nmhdr.idFrom = 0;
        /// </summary>
        NPPN_CANCELSHUTDOWN = 1000 + 20,

        /// <summary>
        ///scnNotification->nmhdr.code = NPPN_FILEBEFORERENAME;
        ///scnNotification->nmhdr.hwndFrom = hwndNpp;
        ///scnNotification->nmhdr.idFrom = BufferID;
        /// </summary>
        NPPN_FILEBEFORERENAME = 1000 + 21,

        /// <summary>
        ///scnNotification->nmhdr.code = NPPN_FILERENAMECANCEL;
        ///scnNotification->nmhdr.hwndFrom = hwndNpp;
        ///scnNotification->nmhdr.idFrom = BufferID;
        /// </summary>
        NPPN_FILERENAMECANCEL = 1000 + 22,

        /// <summary>
        ///scnNotification->nmhdr.code = NPPN_FILERENAMED;
        ///scnNotification->nmhdr.hwndFrom = hwndNpp;
        ///scnNotification->nmhdr.idFrom = BufferID;
        /// </summary>
        NPPN_FILERENAMED = 1000 + 23,

        /// <summary>
        ///scnNotification->nmhdr.code = NPPN_FILEBEFOREDELETE;
        ///scnNotification->nmhdr.hwndFrom = hwndNpp;
        ///scnNotification->nmhdr.idFrom = BufferID;
        /// </summary>
        NPPN_FILEBEFOREDELETE = 1000 + 24,

        /// <summary>
        ///scnNotification->nmhdr.code = NPPN_FILEDELETEFAILED;
        ///scnNotification->nmhdr.hwndFrom = hwndNpp;
        ///scnNotification->nmhdr.idFrom = BufferID;
        /// </summary>
        NPPN_FILEDELETEFAILED = 1000 + 25,

        /// <summary>
        ///scnNotification->nmhdr.code = NPPN_FILEDELETED;
        ///scnNotification->nmhdr.hwndFrom = hwndNpp;
        ///scnNotification->nmhdr.idFrom = BufferID;
        /// </summary>
        NPPN_FILEDELETED = 1000 + 26,

        // Scintilla ID's below

        /// Events
        SCN_STYLENEEDED = 2000,

        /// Events
        SCN_CHARADDED = 2001,

        /// Events
        SCN_SAVEPOINTREACHED = 2002,

        /// Events
        SCN_SAVEPOINTLEFT = 2003,

        /// Events
        SCN_MODIFYATTEMPTRO = 2004,

        /// GTK+ Specific to work around focus and accelerator problems:
        SCN_KEY = 2005,

        /// GTK+ Specific to work around focus and accelerator problems:
        SCN_DOUBLECLICK = 2006,

        /// GTK+ Specific to work around focus and accelerator problems:
        SCN_UPDATEUI = 2007,

        /// GTK+ Specific to work around focus and accelerator problems:
        SCN_MODIFIED = 2008,

        /// GTK+ Specific to work around focus and accelerator problems:
        SCN_MACRORECORD = 2009,

        /// GTK+ Specific to work around focus and accelerator problems:
        SCN_MARGINCLICK = 2010,

        /// GTK+ Specific to work around focus and accelerator problems:
        SCN_NEEDSHOWN = 2011,

        /// GTK+ Specific to work around focus and accelerator problems:
        SCN_PAINTED = 2013,

        /// GTK+ Specific to work around focus and accelerator problems:
        SCN_USERLISTSELECTION = 2014,

        /// GTK+ Specific to work around focus and accelerator problems:
        SCN_URIDROPPED = 2015,

        /// GTK+ Specific to work around focus and accelerator problems:
        SCN_DWELLSTART = 2016,

        /// GTK+ Specific to work around focus and accelerator problems:
        SCN_DWELLEND = 2017,

        /// GTK+ Specific to work around focus and accelerator problems:
        SCN_ZOOM = 2018,

        /// GTK+ Specific to work around focus and accelerator problems:
        SCN_HOTSPOTCLICK = 2019,

        /// GTK+ Specific to work around focus and accelerator problems:
        SCN_HOTSPOTDOUBLECLICK = 2020,

        /// GTK+ Specific to work around focus and accelerator problems:
        SCN_CALLTIPCLICK = 2021,

        /// GTK+ Specific to work around focus and accelerator problems:
        SCN_AUTOCSELECTION = 2022,

        /// GTK+ Specific to work around focus and accelerator problems:
        SCN_INDICATORCLICK = 2023,

        /// GTK+ Specific to work around focus and accelerator problems:
        SCN_INDICATORRELEASE = 2024,

        /// GTK+ Specific to work around focus and accelerator problems:
        SCN_AUTOCCANCELLED = 2025,

        /// GTK+ Specific to work around focus and accelerator problems:
        SCN_AUTOCCHARDELETED = 2026,

        /// GTK+ Specific to work around focus and accelerator problems:
        SCN_HOTSPOTRELEASECLICK = 2027,

        /// GTK+ Specific to work around focus and accelerator problems:
        SCN_FOCUSIN = 2028,

        /// GTK+ Specific to work around focus and accelerator problems:
        SCN_FOCUSOUT = 2029,

    }
}