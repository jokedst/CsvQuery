namespace CsvQuery
{
    using System;
    using System.Collections.Generic;

    public abstract class DataStorageBase : IDataStorage
    {
        protected readonly Dictionary<IntPtr, string> _createdTables = new Dictionary<IntPtr, string>();
        protected IntPtr _currentActiveBufferId;
        protected int _lastCreatedTableName;

        public void SetActiveTab(IntPtr bufferId)
        {
            if (_currentActiveBufferId != bufferId && _createdTables.ContainsKey(bufferId))
            {
                if (_currentActiveBufferId != default(IntPtr))
                {
                    ExecuteNonQuery("DROP VIEW this");
                }
                ExecuteNonQuery("CREATE VIEW this AS SELECT * FROM " + _createdTables[bufferId]);
                _currentActiveBufferId = bufferId;
            }
        }
        public abstract string SaveData(IntPtr bufferId, List<string[]> data, bool? hasHeader);
        public List<string[]> ExecuteQuery(string query) => ExecuteQuery(query, false);
        public List<string[]> ExecuteQueryWithColumnNames(string query) => ExecuteQuery(query, true);
        public abstract void ExecuteNonQuery(string query);
        public void TestConnection()
        {
            ExecuteNonQuery("SELECT 2*3");
        }

        protected abstract List<string[]> ExecuteQuery(string query, bool includeColumnNames);

        protected string GetOrAllocateTableName(IntPtr bufferId)
        {
            string tableName;
            if (_createdTables.ContainsKey(bufferId))
            {
                tableName = _createdTables[bufferId];
                ExecuteNonQuery("DROP TABLE IF EXISTS " + tableName);
            }
            else
            {
                tableName = "T" + ++_lastCreatedTableName;
                _createdTables.Add(bufferId, tableName);
            }
            return tableName;
        }
    }
}