namespace CsvQuery.Tools
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Mixed bag of helper extensions for this project
    /// </summary>
    internal static class Extensions
    {
        /// <summary>
        /// Safely increase a count (when using a dictionary to count stuff)
        /// </summary>
        /// <typeparam name="T">Type being counted</typeparam>
        /// <param name="counts">Dictionary containging counters</param>
        /// <param name="c">Occurance that should be counted</param>
        public static void Increase<T>(this Dictionary<T, int> counts, T c)
        {
            if (!counts.ContainsKey(c))
                counts.Add(c, 1);
            else
                counts[c]++;
        }

        /// <summary>
        /// Translates strings like "\n" to an actual newline character
        /// </summary>
        /// <param name="escaped">Escaped string</param>
        /// <returns>First character of string, after unescaping</returns>
        public static char Unescape(this string escaped)
        {
            if (escaped == "\\t") return '\t';
            return escaped.FirstOrDefault();
        }

        public static object Valid(object oldValue, object newValue)
        {
            return newValue;
        }
    }
}