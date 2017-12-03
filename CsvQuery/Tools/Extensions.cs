namespace CsvQuery.Tools
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Windows.Forms;
    using Properties;

    /// <summary>
    /// Mixed bag of helper extensions for this project
    /// </summary>
    public static class Extensions
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
            if (escaped == "\\t")
                return '\t';
            return escaped.FirstOrDefault();
        }

        /// <summary>
        /// Run something on the main thread of the control, e.g. UI updates
        /// </summary>
        public static void UiThread(this Control control, Action code)
        {
            if (control.InvokeRequired)
                control.Invoke(code);
            else
                code.Invoke();
        }

        /// <summary>
        /// Display a message in a message box on the main thread
        /// </summary>
        /// <param name="control"></param>
        /// <param name="message"> The text to display </param>
        public static void Message(this Control control, string message)
            => UiThread(control, () => MessageBox.Show(message, Resources.Title_MessagBox));

        /// <summary>
        /// Display a message in a message box on the main thread
        /// </summary>
        /// <param name="control"></param>
        /// <param name="message"> The text to display </param>
        /// <param name="title"> Text to show in the title bar of the message box </param>
        public static void Message(this Control control, string message, string title)
            => UiThread(control, () => MessageBox.Show(message, title));

        /// <summary>
        /// Displays an error message in a message box on the main thread
        /// </summary>
        public static void ErrorMessage(this Control control, string message)
            => UiThread(control, () => MessageBox.Show(message, Resources.Title_CSV_Query_Error));

        public static string CommonPrefix(string a, string b)
        {
            if (a == null || b == null) return string.Empty;

            var min = Math.Min(a.Length, b.Length);
            if (min == 0) return string.Empty;

            int common;
            for (common = 0; common < min && a[common] == b[common]; common++) ;

            return a.Substring(0, common);
        }

        public static string CommonSuffix(string a, string b)
        {
            if (a == null || b == null) return string.Empty;

            int apos = a.Length - 1;
            int diff = b.Length - a.Length;

            while (apos>=0 && ((apos+diff)>=0) &&  a[apos] == b[apos + diff])
            {
                apos--;
            }
            apos++;
            return a.Substring(apos);
        }

        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            if (key == null || dictionary == null) return default(TValue);
            // Ignore return value since at "false" it sets the default value
            dictionary.TryGetValue(key, out var ret);
            return ret;
        }

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

        public static IEnumerable<TSource> Append<TSource>(this IEnumerable<TSource> first, TSource item)
        {
            foreach (TSource element in first) yield return element;
            yield return item;
        }
        public static IEnumerable<TSource> Prepend<TSource>(this IEnumerable<TSource> last, TSource item)
        {
            yield return item;
            foreach (TSource element in last) yield return element;
        }

        public static string JoinStrings(this IEnumerable<string> values, string separator = null)
        {
            return string.Join(separator, values);
        }

        public static string JoinStrings(this IEnumerable<string> values, string separator, string prefix, string suffix)
        {
            var sb = new StringBuilder();
            if (prefix != null) sb.Append(prefix);
            foreach (var str in values.Interspace(separator))
            {
                sb.Append(str);
            }
            if (suffix != null) sb.Append(suffix);
            return sb.ToString();
            //if (prefix != null) values = values.Prepend(prefix);
            //if (suffix != null) values = values.Append(suffix);
            //return string.Join(separator, values);
        }

        public static void AppendMany(this StringBuilder sb, params string[] strings)
        {
            foreach (var s in strings)
            {
                sb.Append(s);
            }
        }
    }
}