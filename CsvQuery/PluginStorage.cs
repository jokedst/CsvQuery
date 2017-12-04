namespace CsvQuery
{
    using PluginInfrastructure;

    /// <summary>
    /// Helper for accessing the plugin-specific directory
    /// </summary>
    internal class PluginStorage : PluginStorageBase
    {
        /// <summary> Storage location for query cache </summary>
        public static string QueryCachePath => GetFullPath("querycache.txt");
    }
}
