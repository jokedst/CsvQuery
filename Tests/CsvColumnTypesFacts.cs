namespace Tests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using CsvQuery.Csv;
    using CsvQuery.Tools;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class CsvColumnTypesFacts
    {
        [TestMethod]
        public void CanReadFiles()
        {
            var data = CsvSettings.Semicolon.Parse(File.ReadAllText(@"TestFiles\Headers.csv"));

            var result = new CsvColumnTypes(data, null);

            Assert.AreEqual(true, result.HasHeader);
        }

        [TestMethod]
        public void CanDetectColumnTypes()
        {
            var data = CsvSettings.Comma.Parse(File.ReadAllText(@"TestFiles\random100x10.csv"));

            var result = new CsvColumnTypes(data, null);

            // AaoboInteger,LebhxShortString,GftghLongString,VnsbqChar,NzgubDate,TuyceDateAndTime,6VrnpkDecimal,AnpcfDecimal,LbncsShortString,QofayDate

            Assert.AreEqual(true, result.HasHeader);
            Assert.IsTrue(result.Columns[0].DataType == ColumnType.Integer);
            Assert.IsTrue(result.Columns[1].DataType == ColumnType.String);
            Assert.IsTrue(result.Columns[6].DataType == ColumnType.Decimal);
        }

        [TestMethod]
        public void CanDetectNoHeader()
        {
            var data = CsvSettings.Comma.Parse(File.ReadAllText(@"TestFiles\random100x10.csv"));
            data.RemoveAt(0);

            var result = new CsvColumnTypes(data, null);

            Assert.AreEqual(false, result.HasHeader);
            Assert.IsTrue(result.Columns[0].DataType == ColumnType.Integer);
            Assert.IsTrue(result.Columns[1].DataType == ColumnType.String);
            Assert.IsTrue(result.Columns[6].DataType == ColumnType.Decimal);
        }

        [TestMethod]
        public void CanDetectLocalCurrency()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("sv-se");

            var x = new CsvColumnAnalyzer("12,34 kr");

            Assert.IsTrue(x.DataType == ColumnType.Decimal);
        }

        [TestMethod]
        public void CanDetectDecimalWithZeros()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("sv-se");
            var result = new CsvColumnAnalyzer("1,000");

            Assert.IsTrue(result.DataType == ColumnType.Decimal);
        }

        [TestMethod]
        public void PerformanceTest()
        {
            var timer = new DiagnosticTimer();
            var data = new List<string[]>();

            var row = new[] {"12,34", "string"};
            for (int i = 0; i < 1000000; i++)
            {
                data.Add(row );
            }
            timer.Checkpoint("data creation");

            var result = new CsvColumnTypes(data, null);
            Console.WriteLine(timer.LastCheckpoint("Anlyzed"));

            Assert.AreEqual(false,result.HasHeader);
            Assert.AreEqual(ColumnType.Decimal, result.Columns[0].DataType);
            Assert.AreEqual(ColumnType.String, result.Columns[1].DataType);
        }
    }
}
