using System.Data;

namespace CsvQuery.Database
{
    using System;
    using System.Collections.Generic;
    using Csv;

    public interface IDataStorage
    {
        /// <summary>
        /// Ensures the view "this" points to the current document (if it has been parsed)
        /// </summary>
        /// <param name="bufferId"> Id of curent document </param>
        void SetActiveTab(IntPtr bufferId);

        /// <summary>
        /// Save data to database
        /// </summary>
        /// <param name="bufferId"> Document that has been parsed </param>
        /// <param name="data"> Parsed data </param>
        /// <param name="columnTypes"> Column types </param>
        /// <returns> Name of table data was stored to </returns>
        string SaveData(IntPtr bufferId, List<string[]> data, CsvColumnTypes columnTypes);

        /// <summary>
        /// Executes an SQL query and returns the results
        /// </summary>
        /// <param name="query"> SQL Query </param>
        /// <param name="includeColumnNames"> If true, the first row will contain the column names </param>
        /// <returns></returns>
        List<string[]> ExecuteQuery(string query, bool includeColumnNames);

        /// <summary>
        /// Executes an SQL query without results
        /// </summary>
        /// <param name="query"> SQL Query </param>
        void ExecuteNonQuery(string query);

        /// <summary>
        /// Tests the database connection. Should throw an exception if it doesn't work
        /// </summary>
        void TestConnection();
       
        /// <summary>
        /// Returns the column headers as it was before made safe
        /// </summary>
        IReadOnlyDictionary<string, string> GetUnsafeColumnMaps(IntPtr bufferId);

        DataTable ExecuteQueryToDataTable(string query, IntPtr bufferId);

        /// <summary>
        /// Continue saving more data after a call to SaveDate
        /// </summary>
        /// <param name="bufferId"></param>
        /// <param name="data"></param>
        void SaveMore(IntPtr bufferId, IEnumerable<string[]> data);

        /// <summary>
        /// Returns a SQL query that only selects a limited number of rows (e.g. SELECT TOP 10 * FROM THIS)
        /// </summary>
        /// <param name="linesToSelect"></param>
        /// <returns></returns>
        string CreateLimitedSelect(int linesToSelect);
    }

    public enum DataStorageProvider
    {
        SQLite,
        MSSQL
    }
}