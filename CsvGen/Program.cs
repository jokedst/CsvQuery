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
        private static void Write(string text) => Console.Write(text);
        private static void WriteLine(string text) => Console.WriteLine(text);

        private static void Main()
        {


            string lastDefine = null;
            List<string> subDefines = new List<string>(), commentRows = new List<string>();
            foreach (var line in File.ReadLines(@"C:\Projects\Leech\notepad-plus-plus\PowerEditor\src\MISC\PluginsManager\Notepad_plus_msgs.h").Concat(new string[] { "" }))
            {
                if (lastDefine == null && !line.Trim().StartsWith("#define")) continue;
                if (string.IsNullOrWhiteSpace(line))  // Blank line ends a block
                {
                    if (subDefines.Count == 0) continue; // Skip empty blocks

                    var mainParts = subDefines[0].Split(new[] { ' ', '\t' }, 3, StringSplitOptions.RemoveEmptyEntries);

                    if (mainParts[1].StartsWith("NPPM_") && commentRows.Count > 1)
                    {
                        WriteLine("    /// <summary>");
                        WriteLine($"    /// {commentRows[1]}");
                        WriteLine("    /// </summary><remarks>");
                        WriteLine($"    /// {commentRows[0]} <br/>");
                        foreach (var comment in commentRows.Skip(2))
                            WriteLine($"    /// {comment} <br/>");
                        WriteLine("    /// </summary>");
                    }
                    else if (commentRows.Count > 0)
                    {
                        WriteLine("    /// <summary>");
                        foreach (var comment in commentRows)
                            WriteLine($"    /// {comment}");
                        WriteLine("    /// </summary>");
                    }

                    foreach (var define in subDefines)
                    {
                        var parts = define.Split(new[] { ' ', '\t' }, 3, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length < 2) continue; // Skip malformed defines
                        WriteLine($"    {parts[1]} = {parts[2].Trim()},");
                    }

                    WriteLine("");
                    subDefines.Clear();
                    commentRows.Clear();

                }
                else if (line.Trim().StartsWith("#define"))
                {
                    lastDefine = line;
                    if (line.Contains("//"))
                    {
                        // Split the line into define and comment parts
                        var parts = line.Split(new[] { "//" }, 2, StringSplitOptions.RemoveEmptyEntries);
                        subDefines.Add(parts[0]);
                        commentRows.Add(parts[1].Trim());
                    }
                    else
                        subDefines.Add(line);
                }
                else if (line.Trim().StartsWith("//"))
                {
                    commentRows.Add(line.Trim().Substring(2).Trim());
                }
            }

             

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
                var newline = System.Text.RegularExpressions.Regex.Unescape(Param.Get('N', "\\r\\n"));
                var sep = Param.Get('s', ",");
                var filename = Param.FirstOr($"random{rowsToCreate}x{columns}.csv");

                using (var fs = new StreamWriter(filename))
                    GenerateCsv(columns, rowsToCreate, fs, sep, newline);
            }
        }

        private static void GenerateCsv(int columns, int rowsToCreate, TextWriter fs, string separator = ",", string newline = "\r\n")
        {
            var r = new Random();
            var columnTypes = Enumerable.Range(0, columns).Select(x => x < 7 ? x : r.Next(7)).Cast<CsvColumnType>().ToList();

            // Headers
            foreach (var str in columnTypes.Select(x => r.RandomString(5) + x.ToString()).Interspace(separator))
                fs.Write(str);
            fs.Write(newline);

            // Rows
            for (var l = 0; l < rowsToCreate; l++)
            {
                foreach (var str in columnTypes.Select(x => r.GenColumn(x)).Interspace(separator))
                    fs.Write(str);
                fs.Write(newline);
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

        private static void PerfTestCsvParsers(int loops = 5)
        {
            // Prepare huge csv
            var mm = new MemoryStream();
            var sw = new StreamWriter(mm);

            GenerateCsv(20, 200000, sw);

            mm.Position = 0;
            var sr = new StreamReader(mm, Encoding.UTF8, false, 1024 * 16, true);
            var count = 0;

            CsvSettings csvSettings = new CsvSettings(',');
            var timer = new LoopDiagnosticTimer();

            // Compare results of different parsers
            if (Param.Flag('C'))
            {
                var vb = csvSettings.ParseVB(sr).ToList();
                timer.Checkpoint($"VB.ToList()");
                mm.Position = 0;
                sr = new StreamReader(mm, Encoding.UTF8, false, 1024 * 16, true);
                var custom = csvSettings.ParseCustom(sr).ToList();
                timer.Checkpoint($"ParseCustom.ToList()");

                if (custom.Count != vb.Count) Console.Error.WriteLine($"Error: VB rows={vb.Count} != Std rows={custom.Count}");

                for (int i = 0; i < custom.Count; i++)
                {
                    if (custom[i].Length != vb[i].Length) Console.Error.WriteLine($"Error (line {i}): VB columns={vb[i].Length} != Std columns={custom[i].Length}");

                    for (int c = 0; c < custom[i].Length; c++)
                    {
                        if (custom[i][c] != vb[i][c]) Console.Error.WriteLine($"Error (line {i}, column {c}): VB column='{vb[i][c]}' != Std column='{custom[i][c]}'");
                    }
                }

                timer.Checkpoint("Comparison");
            }

            for (int i = 0; i < loops; i++)
            {
                // VB is ten times slower
                mm.Position = 0;
                count = 0;
                foreach (var line in csvSettings.ParseVB(sr))
                    count++;
                timer.Checkpoint($"VB");

                mm.Position = 0;
                sr = new StreamReader(mm, Encoding.UTF8, false, 1024 * 16, true);
                count = 0;
                foreach (var line in csvSettings.ParseCustom(sr))
                    count++;
                timer.Checkpoint($"ParseCustom");
            }

            Console.WriteLine(timer.LastCheckpoint($"End"));
        }
    }
}