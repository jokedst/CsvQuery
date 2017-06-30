using System.Diagnostics;

namespace CsvQuery
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using Community.CsharpSqlite;

    public class SQLiteDataStorage : IDataStorage
    {
        // For now only in-memory DB, perhaps later we could have a config setting for saving to disk
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

        private readonly Dictionary<IntPtr, string> _createdTables = new Dictionary<IntPtr, string>();
        private IntPtr _currentActiveBufferId;
        private int _lastCreatedTableName;

        public SQLiteDataStorage(string database = ":memory:")
        {
            Db = new SQLiteDatabase(database);
            foreach (string command in PRAGMA_Commands)
                Db.ExecuteNonQuery(command);
        }

        /// <summary>
        /// Set current document as active, i.e. "this" should refer to this document if it has been parsed
        /// </summary>
        /// <param name="bufferId"></param>
        public void SetActiveTab(IntPtr bufferId)
        {
            if (_currentActiveBufferId != bufferId && _createdTables.ContainsKey(bufferId))
            {
                if (_currentActiveBufferId != default(IntPtr))
                {
                    Db.ExecuteNonQuery("DROP VIEW this");
                }
                Db.ExecuteNonQuery("CREATE VIEW this AS SELECT * FROM " + _createdTables[bufferId]);
                _currentActiveBufferId = bufferId;
            }
        }

        /// <summary>
        /// Saves parsed data into SQLite database.
        /// This function currently does a little too much, it detects headers and column types (which it then pretty much ignore)
        /// </summary>
        /// <param name="bufferId"></param>
        /// <param name="data"></param>
        /// <param name="hasHeader"></param>
        /// <returns></returns>
        public string SaveData(IntPtr bufferId, List<string[]> data, bool? hasHeader)
        {
            string tableName;
            if (_createdTables.ContainsKey(bufferId))
            {
                tableName = _createdTables[bufferId];
                Db.ExecuteNonQuery("DROP TABLE IF EXISTS " + tableName);
            }
            else
            {
                tableName = "T" + ++_lastCreatedTableName;
                _createdTables.Add(bufferId, tableName);
            }

            int columns = data[0].Length;
            // Figure out column types. For now just double/string
            var types = new List<bool>();
            var headerTypes = new List<bool>();
            bool first = true, allStrings = true;
            foreach (var cols in data)
            {
                if (first && (!hasHeader.HasValue || hasHeader.Value))
                {
                    // Save to headerTypes
                    foreach (var col in cols)
                    {
                        var isDouble = double.TryParse(col, out double d);
                        headerTypes.Add(isDouble);
                        if (isDouble) allStrings = false;
                    }
                }
                else
                {
                    // Save to types
                    int i = 0;
                    foreach (var col in cols)
                    {
                        double d;
                        var isDouble = string.IsNullOrWhiteSpace(col) || double.TryParse(col, out d);
                        if (types.Count <= i) types.Add(isDouble);
                        else if (types[i] && !isDouble) types[i] = false;
                        i++;
                    }
                }

                if (first)
                    first = false;
            }

            // If the first row is all strings, but the data rows have numbers, it's probably a header
            if (!hasHeader.HasValue && allStrings)
            {
                var dataAllStrings = !types.Any(x => x);
                if (!dataAllStrings) hasHeader = true;
            }
            Trace.TraceInformation($"Header row analysis: \n\tFirst row all strings:{allStrings}\n\tData columns strings: {types.Count(x=>x)}/{types.Count}\n\rHeader row: {hasHeader}");

            // Create SQL by string concat - look out for SQL injection! (although rather harmless since it's all your own data)
            var createQuery = new StringBuilder("CREATE TABLE " + tableName + "(");
            if (hasHeader ?? false)
            {
                var colnames = new List<string>();
                first = true;
                int i = 0;
                foreach (var colName in data[0])
                {
                    var columnNameClean = Regex.Replace(colName, @"[^\w_]", "");
                    //if(Sqlite3.yy)
                    if (string.IsNullOrEmpty(columnNameClean)) columnNameClean = "Col" + i;
                    if (colnames.Contains(columnNameClean))
                    {
                        var c = 2;
                        var fixedName = columnNameClean + c;
                        while (colnames.Contains(fixedName))
                            fixedName = columnNameClean + ++c;
                        columnNameClean = fixedName;
                    }
                    if (first) first = false;
                    else createQuery.Append(", ");
                    colnames.Add(columnNameClean);
                    createQuery.AppendFormat("[{0}] CHAR", columnNameClean);
                    i++;
                }
            }
            else
            {
                // Just create Col1, Col2, Col3 etc
                first = true;
                for (int index = 0; index < data[0].Length; index++)
                {
                    if (first) first = false;
                    else createQuery.Append(", ");
                    createQuery.AppendFormat("{0} CHAR", "Col" + index);
                }
            }
            createQuery.Append(")");
            Db.ExecuteNonQuery("BEGIN EXCLUSIVE");
            Db.ExecuteNonQuery(createQuery.ToString());
            Db.ExecuteNonQuery("END");


            var insertQuery = new StringBuilder("INSERT INTO ");
            insertQuery.Append(tableName);
            insertQuery.Append(" VALUES (?");
            for (int i = 1; i < columns; i++)
                insertQuery.Append(",?");
            insertQuery.Append(")");

            Db.ExecuteNonQuery("BEGIN EXCLUSIVE");
            var stmt = new SQLiteVdbe(Db, insertQuery.ToString());
            first = true;
            foreach (var stringse in data)
            {
                if (first)
                {
                    first = false;
                    if(hasHeader ?? false)
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
            Db.ExecuteNonQuery("END");

            return tableName;
        }

        public List<string[]> ExecuteQuery(string query)
        {
            var result = new List<string[]>();
            var c1 = new SQLiteVdbe(Db, query);
            while (c1.ExecuteStep() != Sqlite3.SQLITE_DONE)
            {
                var columns = c1.ResultColumnCount();
                var data = new List<string>();
                for (int i = 0; i < columns; i++)
                {
                    data.Add(c1.Result_Text(i));
                }
                result.Add(data.ToArray());
            }

            c1.Close();
            return result;
        }

        /// <summary>
        /// Executes the query. The first row in the results will be the column names
        /// </summary>
        /// <param name="query">SQL query to execute</param>
        /// <returns>Query results</returns>
        public List<string[]> ExecuteQueryWithColumnNames(string query)
        {
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

            if(c1.LastResult != 0){}


            var columnNames = new List<string>();
            for (int i = 0; i < columns; i++)
            {
                columnNames.Add(c1.ColumnName(i));
            }
            result.Insert(0, columnNames.ToArray());

            c1.Close();
            return result;
        }

        public void ExecuteNonQuery(string query)
        {
            Db.ExecuteNonQuery(query);
        }
    }
}
