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
    using Tools;

    public class MssqlDataStorage : DataStorageBase
    {
        private readonly string _connectionString;
        private readonly Dictionary<IntPtr, (string,DataTable, CsvColumnTypes)> _lastWriteSettings = new Dictionary<IntPtr, (string, DataTable, CsvColumnTypes)>();

        public MssqlDataStorage(string database)
        {
            Trace.TraceInformation($"Creating MssqlDataStorage for db {database}");

            var builder = new SqlConnectionStringBuilder
            {
                DataSource = "(local)",
                InitialCatalog = database,
                IntegratedSecurity = true
            };
            this._connectionString = builder.ConnectionString;
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
                        //// For some reason SQL Server doesn't like strings as a bit (works with int) so screw bit.
                        //if (column.MaxInteger <= 1)
                        //{
                        //    return "bit";
                        //}
                        if (column.MaxInteger < 256)
                        {
                            return "tinyint";
                        }
                    }
                    if (column.MinInteger >= -32768 && column.MaxInteger <= 32767)
                        return "smallint";
                    else if (column.MinInteger >= -2147483648 && column.MaxInteger <= 2147483647)
                        return "int";
                    else
                        return "bigint";
                case ColumnType.Decimal:
                    return "float";
                case ColumnType.String:
                    var maxLength = column.MaxSize <= 2000 ? column.MaxSize.ToString() : "MAX";
                    return "NVARCHAR("+ maxLength + ")";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private string MssqlType(CsvColumnAnalyzer column)
        {
            return ToLocalType(column) + (column.Nullable ? " NULL" : " NOT NULL");
        }

        public override string SaveData(IntPtr bufferId, List<string[]> data, CsvColumnTypes columnTypes)
        {
            var tableName = this.GetOrAllocateTableName(bufferId);

            // Create SQL by string concat - look out for SQL injection! (although rather harmless since it's all your own data)
            var createQuery = new StringBuilder("CREATE TABLE [" + tableName + "] (");
            var first = true;
            var columns = columnTypes.Columns.Count;
            foreach (var column in columnTypes.Columns)
            {
                if (first) first = false;
                else createQuery.Append(", ");

                if (!Main.Settings.DetectDbColumnTypes)
                {
                    // we'll just save everything as VARCHAR(N) NULL
                    column.DataType = ColumnType.String;
                    column.Nullable = true;
                    column.MaxSize = Math.Max(1, column.MaxSize);
                }

                createQuery.Append('[').Append(column.Name).Append("] ");
                createQuery.Append(this.MssqlType(column));
            }

            createQuery.Append(")");
            this.ExecuteNonQuery(createQuery.ToString());

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

            using (var connection = new SqlConnection(this._connectionString))
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
            table.Clear();
            this._lastWriteSettings[bufferId] = (tableName, table, columnTypes);

            this.SaveUnsafeColumnNames(bufferId, columnTypes);
            return tableName;
        }

        public override void SaveMore(IntPtr bufferId, IEnumerable<string[]> data)
        {
            if (!this._lastWriteSettings.ContainsKey(bufferId))
                throw new CsvQueryException("Can not save more data - no settings for this file saved");
            var(tableName, table, columnTypes) = this._lastWriteSettings[bufferId];

            // We need to check if this data will fit in the data types we guessed in the first chunk
            var oldTypes = columnTypes.Columns.Select(c => c.Clone()).ToList();

            foreach (var row in data)
            {
                var objects = new object[columnTypes.Columns.Count];
                for (var j = 0; j < row.Length; j++)
                {
                    columnTypes.Columns[j].Update(row[j]);
                    objects[j] = columnTypes.Columns[j].Parse(row[j]);
                }

                table.Rows.Add(objects);
            }

            var alterations = new List<string>();
            for (var c = 0; c < columnTypes.Columns.Count; c++)
            {
                var column = columnTypes.Columns[c];
                if (column.FitsIn(oldTypes[c])) continue;
                var query =
                    $"ALTER TABLE [{tableName}] ALTER COLUMN [{column.Name}] {this.MssqlType(column)}";
                alterations.Add(query);
                Trace.TraceInformation($"Column doesn\'t fit new data - altering table column from '{this.ToLocalType(oldTypes[c])}': {query}");
            }

            using (var connection = new SqlConnection(this._connectionString))
            {
                connection.Open();

                if (alterations.Any())
                {
                    var alterTransaction = connection.BeginTransaction();
                    foreach (var alteration in alterations)
                    {
                        using (var command = new SqlCommand(alteration, connection, alterTransaction))
                        {
                            command.ExecuteNonQuery();
                        }
                    }
                    alterTransaction.Commit();
                }

                var transaction = connection.BeginTransaction();

                using (var bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction))
                {
                    bulkCopy.BatchSize = 1000;
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
            table.Clear();
        }

        public override void SaveDone(IntPtr bufferId)
        {
            var (_, table, _) = this._lastWriteSettings[bufferId];
            table.Dispose();
            this._lastWriteSettings.Remove(bufferId);
        }

        public override List<string[]> ExecuteQuery(string query, bool includeColumnNames)
        {
            var result = new List<string[]>();
            try
            {
                using (var con = new SqlConnection(this._connectionString))
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
            }
            catch (SqlException e)
            {
                throw new DataStorageException(e.Message, e);
            }

            return result;
        }

        public override void ExecuteNonQuery(string query)
        {
            using (var con = new SqlConnection(this._connectionString))
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
            this.ExecuteNonQuery("BEGIN tran;CREATE TABLE [bnfkwencvwrjk]([X] int NULL);ROLLBACK tran");
        }

        public override string CreateLimitedSelect(int linesToSelect)
        {
            return $"SELECT TOP {linesToSelect} * FROM THIS";
        }
    }
}