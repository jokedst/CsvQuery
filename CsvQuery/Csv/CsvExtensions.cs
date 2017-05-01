namespace CsvQuery.Csv
{
    using System.Collections.Generic;
    using System.Linq;

    static class CsvExtensions
    {
        /// <summary>
        /// Increase a counter (creates the entry if missing)
        /// </summary>
        /// <param name="counts"></param>
        /// <param name="c"></param>
        public static void Increase(this Dictionary<char, int> counts, char c)
        {
            if (!counts.ContainsKey(c))
                counts.Add(c, 1);
            else
                counts[c]++;
        }
        public static void Incr<T>(this Dictionary<T, int> counts, T c)
        {
            if (!counts.ContainsKey(c))
                counts.Add(c, 1);
            else
                counts[c]++;
        }

        public static char Unescape(this string escaped)
        {
            if (escaped == "\\t") return '\t';
            return escaped.FirstOrDefault();
        }
    }
}