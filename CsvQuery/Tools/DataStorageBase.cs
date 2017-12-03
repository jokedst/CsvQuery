namespace CsvQuery
{
    using System;
    using System.Collections.Generic;
    using Csv;

    public abstract class DataStorageBase : IDataStorage
    {
        protected readonly Dictionary<IntPtr, string> _createdTables = new Dictionary<IntPtr, string>();
        protected IntPtr _currentActiveBufferId;
        protected int _lastCreatedTableName;

        public void SetActiveTab(IntPtr bufferId)
        {
            if (_currentActiveBufferId != bufferId && _createdTables.ContainsKey(bufferId))
            {
                ExecuteNonQuery(QueryDropViewThisIfExists);
                ExecuteNonQuery("CREATE VIEW this AS SELECT * FROM " + _createdTables[bufferId]);
                _currentActiveBufferId = bufferId;
            }
        }

        public abstract string SaveData(IntPtr bufferId, List<string[]> data, CsvColumnTypes columnTypes);
        public abstract void ExecuteNonQuery(string query);
        public void TestConnection()
        {
            ExecuteNonQuery("SELECT 2*3");
        }

        public abstract string QueryDropTableIfExists { get; }
        public abstract string QueryDropViewThisIfExists { get; }

        public abstract List<string[]> ExecuteQuery(string query, bool includeColumnNames);

        protected string GetOrAllocateTableName(IntPtr bufferId)
        {
            string tableName;
            if (_createdTables.ContainsKey(bufferId))
            {
                tableName = _createdTables[bufferId];
            }
            else
            {
                tableName = "T" + ++_lastCreatedTableName;
                _createdTables.Add(bufferId, tableName);
            }
            ExecuteNonQuery("DROP TABLE IF EXISTS " + tableName);
            return tableName;
        }
    }
}