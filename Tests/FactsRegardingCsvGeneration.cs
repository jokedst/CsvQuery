namespace Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using CsvQuery;
    using CsvQuery.Csv;
    using CsvQuery.Database;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class FactsRegardingCsvGeneration
    {
        [TestMethod]
        public void Can_use_unsafe_headers_in_generated_csv()
        {
            var dataStorage = new SQLiteDataStorage();
            Main.Settings = new Settings{UseOriginalColumnHeadersOnGeneratedCsv = true};

            var data = new List<string[]>
            {
                new[] {"A number", "another number here", "#¤%i"},
                new[] {"1", "2", "3"},
                new[] {"2", "12", "14"},
                new[] {"3", "12", "13"},
                new[] {"4", "2", "3"}
            };
            var csvSettings = new CsvSettings(',') { HasHeader = true };
            var ctypes = new CsvColumnTypes(data, csvSettings);

            var tableName = dataStorage.SaveData(new IntPtr(10), data, ctypes);

            var result = dataStorage.ExecuteQueryToDataTable("SELECT * FROM " + tableName, new IntPtr(10));
            Assert.AreEqual("Anumber", result.Columns[0].ColumnName);

            string csv;
            using (var memStream = new MemoryStream())
            {
                var lookup = dataStorage.GetUnsafeColumnMaps(new IntPtr(10));
                csvSettings.GenerateToStream(result, memStream, lookup);
                csv = Encoding.UTF8.GetString(memStream.GetBuffer());
            }
            Assert.IsTrue(csv.StartsWith("A number,another number here,#¤%i"));
        }

        [TestMethod]
        public void Generated_csv_get_safe_headers_when_setting_off()
        {
            var dataStorage = new SQLiteDataStorage();
            Main.Settings = new Settings { UseOriginalColumnHeadersOnGeneratedCsv = false };

            var data = new List<string[]>
            {
                new[] {"A number", "another number here", "#¤%i"},
                new[] {"1", "2", "3"},
                new[] {"2", "12", "14"},
                new[] {"3", "12", "13"},
                new[] {"4", "2", "3"}
            };
            var csvSettings = new CsvSettings(',') { HasHeader = true };
            var ctypes = new CsvColumnTypes(data, csvSettings);

            var tableName = dataStorage.SaveData(new IntPtr(10), data, ctypes);

            var result = dataStorage.ExecuteQueryToDataTable("SELECT * FROM " + tableName, new IntPtr(10));
            Assert.AreEqual("Anumber", result.Columns[0].ColumnName);

            string csv;
            using (var memStream = new MemoryStream())
            {
                csvSettings.GenerateToStream(result, memStream);
                csv = Encoding.UTF8.GetString(memStream.GetBuffer());
            }
            Assert.IsTrue(csv.StartsWith("Anumber,anothernumberhere,i"));
        }
    }
}
