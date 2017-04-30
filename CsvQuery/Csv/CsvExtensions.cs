namespace CsvQuery.Csv
{
    using System.Collections.Generic;

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
    }
}