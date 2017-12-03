namespace CsvGen
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;

    /// <summary>
    /// Helper for parsing command line parameters
    /// </summary>
    internal class Param
    {
        private static readonly List<string> Args;
        private static readonly string ExeName;
        private static readonly HashSet<int> Claimed = new HashSet<int>();

        static Param()
        {
            Args = Environment.GetCommandLineArgs().ToList();
            // Get rid of .exe filename
            ExeName = Args[0];
            Args.RemoveAt(0);
        }

        public static bool Flag(char flagChar)
        {
            var i = Args.IndexOf("-" + flagChar);
            if (i == -1)
                return false;
            Args.RemoveAt(i);
            return true;
        }

        public static T Get<T>(char parameterChar, T defaultValue = default(T))
        {
            var i = Args.IndexOf("-" + parameterChar);
            if (i == -1)
                return defaultValue;
            if (Args.Count <= i + 1) Error($"Missing parameter value after '-{parameterChar}'");

            var converter = TypeDescriptor.GetConverter(typeof(T));
            var ret = (T) converter.ConvertFromString(Args[i + 1]);

            Args.RemoveRange(i, 2);
            return ret;
        }

        public static string FirstOr(string defaultValue)
        {
            var lastWasFlag = false;
            for (var i = 0; i < Args.Count; i++)
            {
                var arg = Args[i];
                if (arg.StartsWith("-"))
                {
                    lastWasFlag = true;
                }
                else if (lastWasFlag)
                {
                    Error("Error: ambigous parameter order");
                }
                else
                {
                    Args.RemoveAt(i);
                    return arg;
                }
            }
            return defaultValue;
        }

        private static void Error(string message)
        {
            Console.Error.WriteLine(message);
            Environment.Exit(1);
        }
    }
}