namespace CsvQuery
{
    using System;
    using System.Collections.Generic;
    using Csv;

    public interface IDataStorage
    {
        void SetActiveTab(IntPtr bufferId);
        string SaveData(IntPtr bufferId, List<string[]> data, CsvColumnTypes columnTypes);
        List<string[]> ExecuteQuery(string query, bool includeColumnNames);
        void ExecuteNonQuery(string query);
        void TestConnection();

        /// <summary>
        /// Query to drop a table safely, i.e. if it doesn't exist no error should occur. Table name is inserted in parameter '{0}'
        /// </summary>
        string QueryDropTableIfExists { get; }
        /// <summary>
        /// Query to drop the view 'this' if it exists
        /// </summary>
        string QueryDropViewThisIfExists { get; }
    }

    public enum DataStorageProvider
    {
        SQLite,
        MSSQL
    }
}