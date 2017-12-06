namespace CsvQuery.Database
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using Csv;

    public class MssqlDataStorage : DataStorageBase
    {
        private readonly string _connectionString;

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

        public override string QueryDropTableIfExists =>
            "IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{0}' AND TABLE_SCHEMA = 'dbo') DROP TABLE dbo.[{0}]";

        public override string QueryDropViewThisIfExists =>
            "IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'this' AND TABLE_SCHEMA = 'dbo' AND TABLE_TYPE='VIEW') DROP VIEW dbo.this";

        private string ToLocalType(CsvColumnAnalyzer column)
        {
            switch (column.DataType)
            {
                case ColumnType.Empty:
                    return "bit";
                case ColumnType.Integer:
                    if (column.MinInteger >= 0)
                    {
                        if (column.MaxInteger <= 1)
                        {
                            return "bit";
                        }
                        if (column.MaxInteger < 256)
                        {
                            return "tinyint";
                        }
                    }
                    if (column.MinInteger >= -32768 && column.MaxInteger <= 32767)
                        return "tinyint";
                    else if (column.MinInteger >= -2147483648 && column.MaxInteger <= 2147483647)
                        return "int";
                    else
                        return "bigint";
                case ColumnType.Decimal:
                    return "float";
                case ColumnType.String:
                    return "NVARCHAR("+column.MaxSize+")";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override string SaveData(IntPtr bufferId, List<string[]> data, CsvColumnTypes columnTypes)
        {
            var tableName = GetOrAllocateTableName(bufferId);

            // Create SQL by string concat - look out for SQL injection! (although rather harmless since it's all your own data)
            var createQuery = new StringBuilder("CREATE TABLE [" + tableName + "] (");
            var first = true;
            var columns = columnTypes.Columns.Count;
            foreach (var column in columnTypes.Columns)
            {
                if (first) first = false;
                else createQuery.Append(", ");
    
                if (Main.Settings.DetectDbColumnTypes)
                {
                    createQuery.Append('[').Append(column.Name).Append("] ");
                    createQuery.Append(ToLocalType(column));
                    createQuery.Append(column.Nullable ? " NULL" : " NOT NULL");
                }
                else
                {
                    var stringSize = Math.Max(1, column.MaxSize);
                    createQuery.AppendFormat("[{0}] NVARCHAR({1})", column.Name, stringSize>2000?"MAX":stringSize.ToString());
                }
            }

            createQuery.Append(")");
            ExecuteNonQuery(createQuery.ToString());

            // Convert to datatable - bulk insert via datatable is fast
            var table = new DataTable();
            table.Columns.AddRange(columnTypes.Columns.Select(r => new DataColumn(r.Name)).ToArray());
            var datalist = columnTypes.HasHeader ? data.Skip(1) : data;
            foreach (var row in datalist)
            {
                var objects = new object[columns];
                for (var j = 0; j < row.Length; j++)
                    objects[j] = columnTypes.Columns[j].Parse(row[j]);
                table.Rows.Add(objects);
            }
            Trace.TraceInformation($"Converted {table.Rows.Count} rows to data table");

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var transaction = connection.BeginTransaction();

                using (var bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction))
                {
                    bulkCopy.BatchSize = 100;
                    bulkCopy.DestinationTableName = tableName;
                    try
                    {
                        bulkCopy.WriteToServer(table);
                    }
                    catch (Exception ex)
                    {
                        Trace.TraceError($"Error in MSSQL bulk copy: {ex.Message}");
                        transaction.Rollback();
                        connection.Close();
                        throw;
                    }
                }

                transaction.Commit();
            }
            return tableName;
        }

        public override List<string[]> ExecuteQuery(string query, bool includeColumnNames)
        {
            var result = new List<string[]>();
            using (var con = new SqlConnection(_connectionString))
            {
                Trace.TraceInformation($"MSSQL ExecuteQueryWithColumnNames '{query}'");
                con.Open();
                using (var command = new SqlCommand(query, con))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var columns = reader.FieldCount;
                        if (includeColumnNames)
                        {
                            var cols = new string[columns];
                            for (var i = 0; i < columns; i++)
                                cols[i] = reader.GetName(i);
                            result.Add(cols);
                            includeColumnNames = false;
                        }

                        var data = new string[columns];
                        for (var i = 0; i < columns; i++)
                            if (reader.IsDBNull(i))
                            {
                                data[i] = null;
                            }
                            else
                            {
                                var o = reader.GetValue(i);
                                data[i] = o.ToString();
                            }
                        result.Add(data);
                    }
                }
            }
            return result;
        }

        public override void ExecuteNonQuery(string query)
        {
            using (var con = new SqlConnection(_connectionString))
            {
                con.Open();
                using (var command = new SqlCommand(query, con))
                {
                    Trace.TraceInformation($"MSSQL ExecuteNonQuery '{query}'");
                    command.ExecuteNonQuery();
                }
            }
        }

        public override void TestConnection()
        {
            // Test connection and permission to create tables
            ExecuteNonQuery("BEGIN tran;CREATE TABLE [bnfkwencvwrjk]([X] int NULL);ROLLBACK tran");
        }
    }
}