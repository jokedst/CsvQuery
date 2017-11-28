namespace CsvGen
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;

    public enum CsvColumnType
    {
        Integer=0,
        ShortString,
        LongString,
        Char,
        Date,
        DateAndTime,
        Decimal=6
    } 

    class Program
    {
        static void Main(string[] args)
        {
            var r = new Random();
            var rowsToCreate = 100000;
            var columns = 10;
            var filename = @"bigCsv.csv";

                var columnTypes = Enumerable.Range(0, columns).Select(x => x < 7 ? x : r.Next(7)).Cast<CsvColumnType>()
                    .ToList();
            using (var fs = new StreamWriter (filename))
            {
                // Headers
                foreach (var str in columnTypes.Select(x => r.RandomString(5) + x.ToString()).Interspace(","))
                {
                    fs.Write(str);
                }
                fs.WriteLine();

                // Rows
                for (int l = 0; l < rowsToCreate; l++)
                {
                    foreach (var str in columnTypes.Select(x => r.GenColumn(x)).Interspace(","))
                    {
                        fs.Write(str);
                    }
                    fs.WriteLine();
                }
            }
        }

    }

    public static class Helpers
    {
            const string Alphabet = "abcdefghijklmnopqrstuvwxyz";

        public static IEnumerable<T> Interspace<T>(this IEnumerable<T> enumerable, T separator)
        {
            var first = true;
            foreach (var item in enumerable)
            {
                if (first) first = false;
                else yield return separator;

                yield return item;
            }
        }

        public static string RandomString(this Random r, int length)
        {
            var chars = new char[length];
            for (int i = 0; i < length; i++)
            {
                chars[i] = Alphabet[r.Next(Alphabet.Length)];
            }
            chars[0] = char.ToUpperInvariant(chars[0]);
            return new string(chars);
        }

        public static char Char(this Random r) => Alphabet[r.Next(Alphabet.Length)];

        public static string GenColumn(this Random r, CsvColumnType type)
        {
            switch (type)
            {
                case CsvColumnType.Integer:
                    return r.Next().ToString();
                case CsvColumnType.ShortString:
                    return r.RandomString(1 + r.Next(4));
                case CsvColumnType.LongString:
                    return r.RandomString(20 + r.Next(10));
                case CsvColumnType.Char:
                  return  r.Char().ToString();
                case CsvColumnType.Date:
                    return DateTime.Today.AddDays(r.Next(10000) - 5000).ToShortDateString();
                case CsvColumnType.DateAndTime:
                    return DateTime.Now.AddSeconds(-1 * r.Next(60 * 60 * 24 * 365 * 4)).ToString();
                case CsvColumnType.Decimal:
                    return (r.NextDouble() * (double) 1000).ToString(CultureInfo.InvariantCulture);
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
    }
}
