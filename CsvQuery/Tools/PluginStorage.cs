namespace CsvQuery.Tools
{
    using System.IO;
    using System.Text;
    using PluginInfrastructure;

    /// <summary>
    /// Helper for accessing the plugin-specific directory
    /// </summary>
    internal class PluginStorage
    {
        /// <summary> Storage location for query cache </summary>
        public static string QueryCachePath => GetFullPath("querycache.txt");

        protected static string StorageDirPath;

        static PluginStorage()
        {
            var sbIniFilePath = new StringBuilder(Win32.MAX_PATH);
            Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)NppMsg.NPPM_GETPLUGINSCONFIGDIR, Win32.MAX_PATH, sbIniFilePath);
            var configDirectory = sbIniFilePath.ToString();
            StorageDirPath = Path.Combine(configDirectory, Main.PluginName);
        }

        /// <summary>
        /// Gets a complete path to the plugin storage file, and creates the directory if needed
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static string GetFullPath(string filename)
        {
            if (!Directory.Exists(StorageDirPath))
                Directory.CreateDirectory(StorageDirPath);
            return Path.Combine(StorageDirPath, filename);
        }
    }
}
