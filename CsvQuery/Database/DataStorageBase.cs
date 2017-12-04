namespace CsvQuery.Database
{
    using System;
    using System.Collections.Generic;
    using Csv;

    public abstract class DataStorageBase : IDataStorage
    {
        protected readonly Dictionary<IntPtr, string> CreatedTables = new Dictionary<IntPtr, string>();
        protected IntPtr CurrentActiveBufferId;
        protected int LastCreatedTableName;

        /// <summary>
        /// Query to drop a table safely, i.e. if it doesn't exist no error should occur. Table name is inserted in parameter '{0}'
        /// </summary>
        public abstract string QueryDropTableIfExists { get; }

        /// <summary>
        /// Query to drop the view 'this' if it exists
        /// </summary>
        public abstract string QueryDropViewThisIfExists { get; }

        /// <summary>
        /// Query that creates the view 'this' as 'SELECT * FROM {0}', where {0} is table name
        /// </summary>
        public virtual string QueryCreateViewThisForTable => "CREATE VIEW this AS SELECT * FROM [{0}]";

        public void SetActiveTab(IntPtr bufferId)
        {
            if (CurrentActiveBufferId != bufferId && CreatedTables.ContainsKey(bufferId))
            {
                ExecuteNonQuery(QueryDropViewThisIfExists);
                ExecuteNonQuery(string.Format(QueryCreateViewThisForTable, CreatedTables[bufferId]));
                CurrentActiveBufferId = bufferId;
            }
        }

        public abstract string SaveData(IntPtr bufferId, List<string[]> data, CsvColumnTypes columnTypes);
        public abstract void ExecuteNonQuery(string query);

        public virtual void TestConnection()
        {
            ExecuteNonQuery("SELECT 2*3");
        }

        public abstract List<string[]> ExecuteQuery(string query, bool includeColumnNames);

        protected string GetOrAllocateTableName(IntPtr bufferId)
        {
            string tableName;
            if (CreatedTables.ContainsKey(bufferId))
            {
                tableName = CreatedTables[bufferId];
            }
            else
            {
                tableName = "T" + ++LastCreatedTableName;
                CreatedTables.Add(bufferId, tableName);
            }
            ExecuteNonQuery(string.Format(QueryDropTableIfExists, tableName));
            return tableName;
        }

        public void SetLastCreatedTableName(int tableNumber)
        {
            LastCreatedTableName = tableNumber;
        }
    }
}