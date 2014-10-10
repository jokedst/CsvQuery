namespace CsvQuery
{
    using System;
    using System.IO;
    using System.Text.RegularExpressions;

    // From http://stackoverflow.com/questions/769621/dealing-with-commas-in-a-csv-file
    public sealed class CsvReader : IDisposable
    {
        public CsvReader(string fileName)
            : this(new FileStream(fileName, FileMode.Open, FileAccess.Read))
        {
        }

        public CsvReader(Stream stream)
        {
            this.reader = new StreamReader(stream);
        }

        public System.Collections.IEnumerable RowEnumerator
        {
            get
            {
                if (null == this.reader)
                    throw new ApplicationException("I can't start reading without CSV input.");

                this.rowNumber = 0;
                string sLine;

                while (null != (sLine = this.reader.ReadLine()))
                {
                    string sNextLine;
                    while (RunOnLineRegex.IsMatch(sLine) && null != (sNextLine = this.reader.ReadLine()))
                        sLine += "\n" + sNextLine;

                    this.rowNumber++;
                    string[] values = CsvSplitterRegex.Split(sLine);

                    for (int i = 0; i < values.Length; i++)
                        values[i] = Csv.Unescape(values[i]);

                    yield return values;
                }

                this.reader.Close();
            }
        }

        public long RowIndex { get { return this.rowNumber; } }

        public void Dispose()
        {
            if (null != this.reader) this.reader.Dispose();
        }

        //============================================


        private long rowNumber;
        private readonly TextReader reader;
        private static readonly Regex CsvSplitterRegex = new Regex(@",(?=(?:[^""]*""[^""]*"")*(?![^""]*""))");
        private static readonly Regex RunOnLineRegex = new Regex(@"^[^""]*(?:""[^""]*""[^""]*)*""[^""]*$");
    }

    public static class Csv
    {
        public static string Escape(string s)
        {
            if (s.Contains(QUOTE))
                s = s.Replace(QUOTE, ESCAPED_QUOTE);

            if (s.IndexOfAny(CHARACTERS_THAT_MUST_BE_QUOTED) > -1)
                s = QUOTE + s + QUOTE;

            return s;
        }

        public static string Unescape(string s)
        {
            if (s.StartsWith(QUOTE) && s.EndsWith(QUOTE))
            {
                s = s.Substring(1, s.Length - 2);

                if (s.Contains(ESCAPED_QUOTE))
                    s = s.Replace(ESCAPED_QUOTE, QUOTE);
            }

            return s;
        }


        private const string QUOTE = "\"";
        private const string ESCAPED_QUOTE = "\"\"";
        private static readonly char[] CHARACTERS_THAT_MUST_BE_QUOTED = { ',', '"', '\n' };
    }

    public class CsvTest
    {
        public static void Main()
        {
            using (var reader = new CsvReader("data.csv"))
            {
                foreach (string[] values in reader.RowEnumerator)
                {
                    Console.WriteLine("Row {0} has {1} values.", reader.RowIndex, values.Length);
                }
            }
            Console.ReadLine();
        }
    }
}
