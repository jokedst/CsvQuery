namespace CsvGen
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    public static class Helpers
    {
        private const string Alphabet = "abcdefghijklmnopqrstuvwxyz";

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
            for (var i = 0; i < length; i++)
                chars[i] = Alphabet[r.Next(Alphabet.Length)];
            chars[0] = char.ToUpperInvariant(chars[0]);
            return new string(chars);
        }

        public static char Char(this Random r)
        {
            return Alphabet[r.Next(Alphabet.Length)];
        }

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
                    return r.Char().ToString();
                case CsvColumnType.Date:
                    return DateTime.Today.AddDays(r.Next(10000) - 5000).ToShortDateString();
                case CsvColumnType.DateAndTime:
                    return DateTime.Now.AddSeconds(-1 * r.Next(60 * 60 * 24 * 365 * 4)).ToString();
                case CsvColumnType.Decimal:
                    return (r.NextDouble() * 1000).ToString(CultureInfo.InvariantCulture);
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
    }
}