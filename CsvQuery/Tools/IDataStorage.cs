namespace CsvQuery
{
    using System;
    using System.Collections.Generic;
    using Csv;

    public interface IDataStorage
    {
        void SetActiveTab(IntPtr bufferId);
        string SaveData(IntPtr bufferId, List<string[]> data, CsvColumnTypes columnTypes);

        /// <summary>
        /// Executes an SQL query and returns the results
        /// </summary>
        /// <param name="query"> SQL Query </param>
        /// <param name="includeColumnNames"> If true, the first row will contain the column names </param>
        /// <returns></returns>
        List<string[]> ExecuteQuery(string query, bool includeColumnNames);
        void ExecuteNonQuery(string query);
        void TestConnection();
    }

    public enum DataStorageProvider
    {
        SQLite,
        MSSQL
    }
}