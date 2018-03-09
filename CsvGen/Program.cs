namespace CsvGen
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
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
            }
            else if (Param.Flag('p'))
            {
                PerfTestCsvParsers();
            }
            else
            {
                var rowsToCreate = Param.Get('n', 100000);
                var columns = Param.Get('c', 10);
                var filename = Param.FirstOr($"random{rowsToCreate}x{columns}.csv");

                using (var fs = new StreamWriter(filename))
                    GenerateCsv(columns, rowsToCreate, fs);
            }
        }

        private static void GenerateCsv(int columns, int rowsToCreate, TextWriter fs, string separator=",")
        {
            var r = new Random();
            var columnTypes = Enumerable.Range(0, columns).Select(x => x < 7 ? x : r.Next(7)).Cast<CsvColumnType>().ToList();

            // Headers
            foreach (var str in columnTypes.Select(x => r.RandomString(5) + x.ToString()).Interspace(separator))
                fs.Write(str);
            fs.WriteLine();

            // Rows
            for (var l = 0; l < rowsToCreate; l++)
            {
                foreach (var str in columnTypes.Select(x => r.GenColumn(x)).Interspace(separator))
                    fs.Write(str);
                fs.WriteLine();
            }
        }

        private static void PerformanceTest()
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
            Console.WriteLine("Column 1: " + result.Columns[0].DataType);
        }

        private static void PerfTestCsvParsers()
        {
            // Prepare huge csv
            var mm = new MemoryStream();
            var sw = new StreamWriter(mm);

            GenerateCsv(20,200000,sw);

            mm.Position = 0;
            var sr = new StreamReader(mm, Encoding.UTF8, false, 1024*16, true);
            var count = 0;

            CsvSettings csvSettings  = new CsvSettings(',');

            var timer = new DiagnosticTimer();
            //foreach (var line in csvSettings.ParseVB(sr))
            //    count++;
            //timer.Checkpoint($"VB ({count})");

            mm.Position = 0;
            count = 0;
            timer.Checkpoint("NOP");
            foreach (var line in csvSettings.ParseStandard(sr))
                count++;
            timer.Checkpoint($"Standard ({count})");
            
            mm.Position = 0;
            count = 0;
            timer.Checkpoint("NOP");
            foreach (var line in csvSettings.ParseRaw(sr))
                count++;
            timer.Checkpoint($"ParseRaw ({count})");

            mm.Position = 0;
            count = 0;
            timer.Checkpoint("NOP");
            foreach (var line in csvSettings.ParseRawBuffered(sr))
                count++;
            var result = timer.LastCheckpoint($"ParseRawBuffered ({count})");

            Console.WriteLine(result);
        }
    }
}