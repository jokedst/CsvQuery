using System.Diagnostics;

namespace CsvQuery
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using Community.CsharpSqlite;
    using Csv;

    public class SQLiteDataStorage : DataStorageBase
    {
        private readonly SQLiteDatabase Db;
        private static readonly string[] PRAGMA_Commands =
            {
                "PRAGMA synchronous =  OFF",
                "PRAGMA temp_store =  MEMORY",
                "PRAGMA journal_mode = OFF",
                "PRAGMA locking_mode = EXCLUSIVE",
                "PRAGMA main.page_size = 4096",
                "PRAGMA main.cache_size = 10000"
            };

        public SQLiteDataStorage(string database = ":memory:")
        {
            Db = new SQLiteDatabase(database);
            foreach (string command in PRAGMA_Commands)
                Db.ExecuteNonQuery(command);
        }

        /// <summary>
        /// Saves parsed data into SQLite database.
        /// This function currently does a little too much, it detects headers and column types (which it then pretty much ignore)
        /// </summary>
        /// <param name="bufferId"></param>
        /// <param name="data"></param>
        /// <param name="hasHeader"></param>
        /// <returns></returns>
        public override string SaveData(IntPtr bufferId, List<string[]> data, CsvColumnTypes columnTypes)
        {
            string tableName = GetOrAllocateTableName(bufferId);
            
            // Create SQL by string concat - look out for SQL injection! (although rather harmless since it's all your own data)
            var createQuery = new StringBuilder("CREATE TABLE [" + tableName + "] (");

            bool first=true;
            foreach (var column in columnTypes.ColumnNames)
            {
                if (first) first = false;
                else createQuery.Append(", ");
                createQuery.AppendFormat("[{0}] CHAR", column);
            }
           
            createQuery.Append(")");
            ExecuteNonQuery("BEGIN EXCLUSIVE");
            ExecuteNonQuery(createQuery.ToString());
            ExecuteNonQuery("END");


            var insertQuery = new StringBuilder("INSERT INTO ");
            insertQuery.Append(tableName);
            insertQuery.Append(" VALUES (?");
            for (int i = 1; i < columnTypes.Columns.Count; i++)
                insertQuery.Append(",?");
            insertQuery.Append(")");

            ExecuteNonQuery("BEGIN EXCLUSIVE");
            var stmt = new SQLiteVdbe(Db, insertQuery.ToString());
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
                    stmt.BindText(++index,s);
                }
                stmt.ExecuteStep();
            }
            stmt.Close();
            ExecuteNonQuery("END");

            return tableName;
        }
        
        /// <summary>
        /// Executes the query. The first row in the results will be the column names
        /// </summary>
        /// <param name="query">SQL query to execute</param>
        /// <param name="includeColumnNames"> If true first row in results are the clumn names</param>
        /// <returns>Query results</returns>
        public override List<string[]> ExecuteQuery(string query, bool includeColumnNames)
        {
            Trace.TraceInformation($"SQLite ExecuteQuery '{query}'");
            var result = new List<string[]>();
            var c1 = new SQLiteVdbe(Db, query);
            int columns = 0;
            while (c1.ExecuteStep() == Sqlite3.SQLITE_ROW)
            {
                columns = c1.ResultColumnCount();
                var data = new List<string>();
                for (int i = 0; i < columns; i++)
                {
                    data.Add(c1.Result_Text(i));
                }
                result.Add(data.ToArray());
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
            return result;
        }

        public override void ExecuteNonQuery(string query)
        {
            Trace.TraceInformation($"SQLite ExecuteNonQuery '{query}'");
            Db.ExecuteNonQuery(query);
        }

        public override string QueryDropTableIfExists => "DROP TABLE IF EXISTS [{0}]";
        public override string QueryDropViewThisIfExists => "DROP VIEW IF EXISTS [this]";
    }
}
