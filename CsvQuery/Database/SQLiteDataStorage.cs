namespace CsvQuery.Database
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text;
    using Community.CsharpSqlite;
    using Csv;
    using Tools;

    public class SQLiteDataStorage : DataStorageBase
    {
        private readonly SQLiteDatabase _db;
        private static readonly string[] PragmaCommands =
            {
                "PRAGMA synchronous =  OFF",
                "PRAGMA temp_store =  MEMORY",
                "PRAGMA journal_mode = OFF",
                "PRAGMA locking_mode = EXCLUSIVE",
                "PRAGMA main.page_size = 4096",
                "PRAGMA main.cache_size = 10000"
            };

        private static string _lastError = null;

        static SQLiteDataStorage()
        {
            Sqlite3.sqlite3_initialize();
            Sqlite3.sqlite3GlobalConfig.xLog = LogError;
        }

        public SQLiteDataStorage(string database = ":memory:")
        {
            this._db = new SQLiteDatabase(database);
            foreach (string command in PragmaCommands) this._db.ExecuteNonQuery(command);
        }

        private readonly Dictionary<IntPtr, (string, CsvColumnTypes)> _lastWriteSettings = new Dictionary<IntPtr, (string, CsvColumnTypes)>();
      
        /// <summary>
        /// Saves parsed data into SQLite database.
        /// This function currently does a little too much, it detects headers and column types (which it then pretty much ignore)
        /// </summary>
        /// <param name="bufferId"></param>
        /// <param name="data"></param>
        /// <param name="columnTypes"></param>
        /// <returns></returns>
        public override string SaveData(IntPtr bufferId, List<string[]> data, CsvColumnTypes columnTypes)
        {
            _lastError = null;
            string tableName = this.GetOrAllocateTableName(bufferId);
            
            // Create SQL by string concat - look out for SQL injection! (although rather harmless since it's all your own data)
            var createQuery = new StringBuilder("CREATE TABLE [" + tableName + "] (");

            bool first=true;
            foreach (var column in columnTypes.Columns)
            {
                if (first) first = false;
                else createQuery.Append(", ");

                if (Main.Settings.DetectDbColumnTypes)
                {
                    createQuery.Append('[').Append(column.Name).Append("] ");
                    switch (column.DataType)
                    {
                        case ColumnType.Empty:
                            createQuery.Append(" CHAR");
                            break;
                        case ColumnType.Integer:
                            createQuery.Append(" INT");
                            break;
                        case ColumnType.Decimal:
                            createQuery.Append(" FLOAT");
                            break;
                        case ColumnType.String:
                            createQuery.Append(" CHAR");
                            break;
                    }
                    createQuery.Append(column.Nullable ? " NULL" : " NOT NULL");
                }
                else
                    createQuery.AppendFormat("[{0}] CHAR", column.Name);
            }

            createQuery.Append(")");
            this.ExecuteNonQuery("BEGIN EXCLUSIVE");
            this.ExecuteNonQuery(createQuery.ToString());
            this.ExecuteNonQuery("END");

            var columns = columnTypes.Columns.Count;
            var insertQuery = new StringBuilder("INSERT INTO ");
            insertQuery.Append(tableName);
            insertQuery.Append(" VALUES (?");
            for (int i = 1; i < columns; i++)
                insertQuery.Append(",?");
            insertQuery.Append(")");

            this.ExecuteNonQuery("BEGIN EXCLUSIVE");
            var stmt = new SQLiteVdbe(this._db, insertQuery.ToString());
            first = true;
            foreach (var stringse in data)
            {
                if (first)
                {
                    first = false;
                    if(columnTypes.HasHeader)
                        continue;
                }
                stmt.Reset();
                int index = 0;
                foreach (var s in stringse)
                {
                    stmt.BindText(++index, s);
                }
                while (index < columns)
                    stmt.BindText(++index, null);
                stmt.ExecuteStep();
            }
            stmt.Close();
            this.ExecuteNonQuery("END");

            this.SaveUnsafeColumnNames(bufferId, columnTypes);

            this._lastWriteSettings[bufferId] = (insertQuery.ToString(), columnTypes);
           
            return tableName;
        }

        public override void SaveMore(IntPtr bufferId, IEnumerable<string[]> data)
        {
            if(!this._lastWriteSettings.ContainsKey(bufferId))
                throw new CsvQueryException("Can not save more data - no settings for this file saved");

            var (insertQuery, columnTypes) = this._lastWriteSettings[bufferId];
            var columns = columnTypes.Columns.Count;

            this.ExecuteNonQuery("BEGIN EXCLUSIVE");
            var stmt = new SQLiteVdbe(this._db, insertQuery);
            foreach (var stringse in data)
            {
                stmt.Reset();
                int index = 0;
                foreach (var s in stringse)
                {
                    stmt.BindText(++index, s);
                }
                while (index < columns)
                    stmt.BindText(++index, null);
                stmt.ExecuteStep();
            }
            stmt.Close();
            this.ExecuteNonQuery("END");
        }

        public override void SaveDone(IntPtr bufferId)
        {
            this._lastWriteSettings.Remove(bufferId);
        }

        private static void LogError(object plogarg, int i, string msg)
        {
            _lastError = msg;
        }

        /// <summary>
        /// Executes the query. The first row in the results will be the column names
        /// </summary>
        /// <param name="query">SQL query to execute</param>
        /// <param name="includeColumnNames"> If true first row in results are the clumn names</param>
        /// <returns>Query results</returns>
        public override List<string[]> ExecuteQuery(string query, bool includeColumnNames)
        {
            _lastError = null;
            Trace.TraceInformation($"SQLite ExecuteQuery '{query}'");
            var result = new List<string[]>();
            try
            {
                var c1 = new SQLiteVdbe(this._db, query);
                int columns = 0;
                while (c1.ExecuteStep() == Sqlite3.SQLITE_ROW)
                {
                    columns = c1.ResultColumnCount();
                    var data = new string[columns];
                    for (int i = 0; i < columns; i++)
                    {
                        data[i] = c1.Result_Text(i);
                    }
                    result.Add(data);
                }

                if (includeColumnNames)
                {
                    var columnNames = new List<string>();
                    for (int i = 0; i < columns; i++)
                    {
                        columnNames.Add(c1.ColumnName(i));
                    }
                    result.Insert(0, columnNames.ToArray());
                }

                c1.Close();
            }
            catch (Exception e)
            {
                if(_lastError != null)
                    throw new DataStorageException(_lastError, e);
                throw;
            }
            return result;
        }

        public override void ExecuteNonQuery(string query)
        {
            _lastError = null;
            Trace.TraceInformation($"SQLite ExecuteNonQuery '{query}'");
            this._db.ExecuteNonQuery(query);
        }

        public override string QueryDropTableIfExists => "DROP TABLE IF EXISTS [{0}]";
        public override string QueryDropViewThisIfExists => "DROP VIEW IF EXISTS [this]";
    }
}
