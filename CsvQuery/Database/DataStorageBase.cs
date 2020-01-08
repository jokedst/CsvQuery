using System.Data;
using System.Linq;
using CsvQuery.Tools;

namespace CsvQuery.Database
{
    using System;
    using System.Collections.Generic;
    using Csv;

    public abstract class DataStorageBase : IDataStorage
    {
        protected readonly Dictionary<IntPtr, string> CreatedTables = new Dictionary<IntPtr, string>();
        public readonly Dictionary<IntPtr, Dictionary<string,string>> UnsafeColumnNames = new Dictionary<IntPtr, Dictionary<string, string>>();
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
            if (this.CreatedTables.ContainsKey(bufferId))
            {
                this.ExecuteNonQuery(this.QueryDropViewThisIfExists);
                this.ExecuteNonQuery(string.Format(this.QueryCreateViewThisForTable, this.CreatedTables[bufferId]));
                this.CurrentActiveBufferId = bufferId;
            }
        }

        public abstract string SaveData(IntPtr bufferId, List<string[]> data, CsvColumnTypes columnTypes);
        public abstract void ExecuteNonQuery(string query);

        public virtual void TestConnection()
        {
            this.ExecuteNonQuery("SELECT 2*3");
        }

        public IReadOnlyDictionary<string, string> GetUnsafeColumnMaps(IntPtr bufferId)
        {
            return this.UnsafeColumnNames.GetValueOrDefault(bufferId);
        }

        public abstract List<string[]> ExecuteQuery(string query, bool includeColumnNames);

        public DataTable ExecuteQueryToDataTable(string query, IntPtr bufferId)
        {
            var data = this.ExecuteQuery(query, true);
            if (data == null || data.Count == 0) return null;

            var table = new DataTable();
            table.ExtendedProperties.Add("query", query);
            table.ExtendedProperties.Add("bufferId", bufferId);

            // Create columns
            foreach (var header in data[0])
            // Column names in a DataGridView can't contain commas it seems
                table.Columns.Add(header.Replace(",", string.Empty));
            
            foreach (var row in data.Skip(1))
                table.Rows.Add(row);

            return table;
        }

        protected string GetOrAllocateTableName(IntPtr bufferId)
        {
            string tableName;
            if (this.CreatedTables.ContainsKey(bufferId))
            {
                tableName = this.CreatedTables[bufferId];
            }
            else
            {
                tableName = "T" + ++this.LastCreatedTableName;
                this.CreatedTables.Add(bufferId, tableName);
            }

            this.ExecuteNonQuery(string.Format(this.QueryDropTableIfExists, tableName));
            return tableName;
        }

        public void SetLastCreatedTableName(int tableNumber)
        {
            this.LastCreatedTableName = tableNumber;
        }

        protected void SaveUnsafeColumnNames(IntPtr bufferId, CsvColumnTypes columnTypes)
        {
            this.UnsafeColumnNames[bufferId] = columnTypes.ColumnUnsafeNames;
        }

        public abstract void SaveMore(IntPtr bufferId, IEnumerable<string[]> data);

        public virtual string CreateLimitedSelect(int linesToSelect)
        {
            return $"SELECT * FROM THIS LIMIT {linesToSelect}";
        }

        public abstract void SaveDone(IntPtr bufferId);

        public abstract string LastError();
    }
}