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

        public override string SaveData(IntPtr bufferId, List<string[]> data, CsvColumnTypes columnTypes)
        {
            string tableName = GetOrAllocateTableName(bufferId);

            // Create SQL by string concat - look out for SQL injection! (although rather harmless since it's all your own data)
            var createQuery = new StringBuilder("CREATE TABLE [" + tableName + "] (");
            bool first = true;
            foreach (var column in columnTypes.Columns)
            {
                if (first) first = false;
                else createQuery.Append(", ");

                if (Main.Settings.GuessDbColumnTypes)
                {
                    createQuery.Append('[').Append(column.Name).Append("] ");

                    switch (column.DataType)
                    {
                        case ColumnType.Empty:
                            createQuery.Append("bit");
                            break;
                        case ColumnType.Integer:
                            if (column.MinInteger >= 0)
                            {
                                if (column.MaxInteger <= 1)
                                {
                                    createQuery.Append("bit");
                                    break;
                                }
                                if (column.MaxInteger < 256)
                                {
                                    createQuery.Append("tinyint");
                                    break;
                                }
                            }
                            if (column.MinInteger >= -32768 && column.MaxInteger <= 32767)
                                createQuery.Append("tinyint");
                            else if (column.MinInteger >= -2147483648 && column.MaxInteger <= 2147483647)
                                createQuery.Append("int");
                            else
                                createQuery.Append("bigint");
                            break;
                        case ColumnType.Decimal:
                            createQuery.Append("float");
                            break;
                        case ColumnType.String:
                            createQuery.Append("NVARCHAR(").Append(column.MaxSize).Append(")");
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    createQuery.Append(column.Nullable ? " NULL" : " NOT NULL");
                }
                else
                    createQuery.AppendFormat("[{0}] NVARCHAR(MAX)", column.Name);
            }

            createQuery.Append(")");
            ExecuteNonQuery(createQuery.ToString());

            // Convert to datatable
            var table = new DataTable();
            table.Columns.AddRange(columnTypes.Columns.Select(r => new DataColumn(r.Name)).ToArray());
            var datalist = columnTypes.HasHeader ? data.Skip(1) : data;
            int i = 0;
            foreach (var row in datalist)
            {
                i++;
                if (row.Length != columnTypes.Columns.Count)
                {
                    Trace.TraceInformation($"Row {i} has {row.Length} columns, but should have {columnTypes.Columns.Count}");
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
                            for (int i = 0; i < columns; i++)
                            {
                                cols[i] = reader.GetName(i);
                            }
                            result.Add(cols);
                            includeColumnNames = false;
                        }

                        var data = new string[columns];
                        for (int i = 0; i < columns; i++)
                        {
                            //data[i] = reader.IsDBNull(i) ? null : reader.GetString(i);
                            data[i] = reader.IsDBNull(i) ? null : reader.GetValue(i).ToString();
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

        public override string QueryDropTableIfExists => "IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{0}' AND TABLE_SCHEMA = 'dbo') DROP TABLE dbo.[{0}]";

        public override string QueryDropViewThisIfExists => "IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'this' AND TABLE_SCHEMA = 'dbo' AND TABLE_TYPE='VIEW') DROP VIEW dbo.this";
    }
}