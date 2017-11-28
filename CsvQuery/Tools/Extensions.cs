namespace CsvQuery.Tools
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Forms;

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
            => UiThread(control, () => MessageBox.Show(message));

        /// <summary>
        /// Display a message in a message box on the main thread
        /// </summary>
        /// <param name="control"></param>
        /// <param name="message"> The text to display </param>
        /// <param name="title"> Text to show in the title bar of the message box </param>
        public static void Message(this Control control, string message, string title)
            => UiThread(control, () => MessageBox.Show(message, title));
    }
}