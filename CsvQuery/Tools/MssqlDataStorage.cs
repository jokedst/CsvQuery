using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Community.CsharpSqlite;
using CsvQuery.Csv;

namespace CsvQuery
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;

    public class MssqlDataStorage : IDataStorage
    {
        private readonly string _connectionString;
        private IntPtr _currentActiveBufferId;
        private readonly Dictionary<IntPtr, string> _createdTables = new Dictionary<IntPtr, string>();
        private int _lastCreatedTableName;

        public MssqlDataStorage(string database)
        {
            Trace.TraceInformation($"Creating MssqlDataStorage for db {database}");

            var builder = new SqlConnectionStringBuilder
            {
                DataSource = "(local)",
                InitialCatalog = database,
                IntegratedSecurity = true
            };
            _connectionString = builder.ConnectionString;

        }

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

        public string SaveData(IntPtr bufferId, List<string[]> data, bool? hasHeader)
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
            ExecuteNonQuery($"IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{tableName}' AND TABLE_SCHEMA = 'dbo') DROP TABLE dbo.{tableName}");

            var columnTypes = CsvAnalyzer.DetectColumnTypes(data, hasHeader);

            // Create SQL by string concat - look out for SQL injection! (although rather harmless since it's all your own data)
            var createQuery = new StringBuilder("CREATE TABLE " + tableName + "(");
            var colnames = new List<string>();
            bool first = true;
            if (columnTypes.HasHeader)
            {
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
                    createQuery.AppendFormat("[{0}] NVARCHAR(MAX)", columnNameClean);
                    i++;
                }
            }
            else
            {
                // Just create Col1, Col2, Col3 etc
                for (int index = 0; index < data[0].Length; index++)
                {
                    if (first) first = false;
                    else createQuery.Append(", ");
                    createQuery.AppendFormat("{0} NVARCHAR(MAX)", "Col" + index);
                    colnames.Add("Col" + index);
                }
            }
            createQuery.Append(")");
            ExecuteNonQuery(createQuery.ToString());

            try
            {
                // Convert to datatable
                var table = new DataTable();
                table.Columns.AddRange(colnames.Select(r => new DataColumn(r)).ToArray());
                var datalist = columnTypes.HasHeader ? data.Skip(1) : data;
                int i = 0;
                foreach (var row in datalist)
                {
                    i++;
                    if (row.Length != colnames.Count)
                    {
                        Trace.TraceInformation($"Row {i} has {row.Length} columns, but should have {colnames.Count}");
                    }
                    var objects = row.Cast<object>().ToArray();
                    table.Rows.Add(objects);
                }
                Trace.TraceInformation($"Converted {table.Rows.Count} to data table");

                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    SqlTransaction transaction = connection.BeginTransaction();

                    using (var bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction))
                    {
                        bulkCopy.BatchSize = 100;
                        bulkCopy.DestinationTableName = tableName;
                        try
                        {
                            bulkCopy.WriteToServer(table);
                        }
                        catch (Exception)
                        {
                            transaction.Rollback();
                            connection.Close();
                        }
                    }

                    transaction.Commit();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error when writing to database");
            }
            return tableName;
        }

        public List<string[]> ExecuteQuery(string query)
        {
            var result = new List<string[]>();
            using (var con = new SqlConnection(_connectionString))
            {
                con.Open();
                using (var command = new SqlCommand(query, con))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var columns = reader.FieldCount;
                        var data = new List<string>();
                        for (int i = 0; i < columns; i++)
                        {
                            data.Add(reader.GetString(i));
                        }
                        result.Add(data.ToArray());
                    }
                }
            }
            return result;
        }

        public List<string[]> ExecuteQueryWithColumnNames(string query)
        {
            var result = new List<string[]>();
            using (var con = new SqlConnection(_connectionString))
            {
                con.Open();
                using (var command = new SqlCommand(query, con))
                using (var reader = command.ExecuteReader())
                {
                    bool first=true;
                    while (reader.Read())
                    {
                        var columns = reader.FieldCount;
                        if (first)
                        {
                            var cols = new List<string>();
                            for (int i = 0; i < columns; i++)
                            {
                                cols.Add(reader.GetName(i));
                            }
                            result.Add(cols.ToArray());
                            first = false;
                        }

                        var data = new List<string>();
                        for (int i = 0; i < columns; i++)
                        {
                            data.Add(reader.GetString(i));
                        }
                        result.Add(data.ToArray());
                    }
                }
            }
            return result;
        }

        public void ExecuteNonQuery(string query)
        {
            using (var con = new SqlConnection(_connectionString))
            {
                con.Open();
                using (var command = new SqlCommand(query, con))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        public void TestConnection()
        {
            // Test connection
            ExecuteNonQuery("BEGIN tran;CREATE TABLE [bnfkwencvwrjk]([X] int NULL);ROLLBACK tran");
        }
    }
}