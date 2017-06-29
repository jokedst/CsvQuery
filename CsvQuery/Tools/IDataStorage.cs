using System;
using System.Collections.Generic;

namespace CsvQuery
{
    public interface IDataStorage
    {
        void SetActiveTab(IntPtr bufferId);
        string SaveData(IntPtr bufferId, List<string[]> data, bool? hasHeader);
        List<string[]> ExecuteQuery(string query);
        List<string[]> ExecuteQueryWithColumnNames(string query);
        void ExecuteNonQuery(string query);
    }

    public enum DataStorageProvider
    {
        SQLite,
        MSSQL
    }
}