namespace CsvGen
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using CsvQuery.Csv;
    using CsvQuery.Tools;

    public enum CsvColumnType
    {
        Integer = 0,
        ShortString,
        LongString,
        Char,
        Date,
        DateAndTime,
        Decimal = 6
    }

    /// <summary>
    ///     CsvGen - a CSV generator
    /// </summary>
    internal class Program
    {
        private static void Main()
        {
            if (Param.Flag('P'))
            {
                PerformanceTest();
                return;
            }
            var r = new Random();
            var rowsToCreate = Param.Get('n', 100000);
            var columns = Param.Get('c', 10);
            var filename = Param.FirstOr($"random{rowsToCreate}x{columns}.csv");

            var columnTypes = Enumerable.Range(0, columns).Select(x => x < 7 ? x : r.Next(7)).Cast<CsvColumnType>()
                .ToList();
            using (var fs = new StreamWriter(filename))
            {
                // Headers
                foreach (var str in columnTypes.Select(x => r.RandomString(5) + x.ToString()).Interspace(","))
                    fs.Write(str);
                fs.WriteLine();

                // Rows
                for (var l = 0; l < rowsToCreate; l++)
                {
                    foreach (var str in columnTypes.Select(x => r.GenColumn(x)).Interspace(","))
                        fs.Write(str);
                    fs.WriteLine();
                }
            }
        }

        static void PerformanceTest()
        {
            var timer = new DiagnosticTimer();
            var data = new List<string[]>();

            var row = new[] { "12,34", "string", "321.23" };
            for (int i = 0; i < 2000000; i++)
            {
                data.Add(row);
            }
            timer.Checkpoint("data creation");

            var result = new CsvColumnTypes(data, null);
            Console.WriteLine(timer.LastCheckpoint("Anlyzed"));
            Console.WriteLine(result);
            Console.WriteLine("Column 1: " + result.Columns[0].DataType.ToString());
        }
    }
}