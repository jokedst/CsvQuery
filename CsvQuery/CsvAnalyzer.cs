namespace CsvQuery
{
    using System.IO;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class CsvSettings
    {
        public char Separator { get; set; }
        public char TextQualifier { get; set; }
        public char EscapeCharacter { get; set; }

        /// <summary>
        /// Parses a big text blob into rows and columns, using the settings
        /// </summary>
        /// <param name="text">Big blob of text</param>
        /// <returns>Parsed data</returns>
        public List<string[]> Parse(string text)
        {
            var data = new List<string[]>();

            var textreader = new StringReader(text);
            string line;
            int columnsCount = 0;
            while ((line = textreader.ReadLine()) != null)
            {
                var cols = line.Split(this.Separator);
                data.Add(cols);

                if (cols.Length > columnsCount)
                    columnsCount = cols.Length;
            }

            return data;
        }
    }

    public class CsvAnalyzer
    {
        private static string preferredSeparators = ",;|:\t ";
        private class Stat
        {
            public int Occurances;

            public float Variance;
        }

        public static CsvSettings Analyze(string csvString)
        {
            // TODO: strings with quoted values (e.g. 'hej,san')
            // Not sure how to detect this, but we could just run the variance analysis
            // 3 times, one for none, one for ' and one for " and see which has best variances
            // That wouldn't detect escape chars though, or odd variants like [this]

            // First do a letter frequency analysis on each row
            var s = new StringReader(csvString);
            string line;
            int lineCount = 0;
            var frequencies = new List<Dictionary<char, int>>();
            var occurrences = new Dictionary<char, int>();
            while ((line = s.ReadLine()) != null)
            {
                var letterFrequency = new Dictionary<char, int>();
                foreach (var c in line)
                {
                    if (!letterFrequency.ContainsKey(c)) 
                        letterFrequency.Add(c, 1);
                    else
                        letterFrequency[c]++;

                    if (!occurrences.ContainsKey(c))
                        occurrences.Add(c, 1);
                    else
                        occurrences[c]++;
                }

                frequencies.Add(letterFrequency);
                if (lineCount++ > 20) break;
            }

            // Then check the variance on the frequency of each char
            var variances = new Dictionary<char, float>();
            foreach (var c in occurrences.Keys)
            {
                var mean = (float)occurrences[c] / lineCount;
                float variance = 0;
                foreach (var frequency in frequencies)
                {
                    var f = 0;
                    if (frequency.ContainsKey(c)) f = frequency[c];
                    variance += (f - mean) * (f - mean);
                }
                variance /= lineCount;
                variances.Add(c,variance);
            }
            
            // The char with lowest variance is most likely the separator
            var result = new CsvSettings();
            result.Separator = GetSeparatorFromVariance(variances, occurrences, lineCount);
            
            return result;
        }

        private static Dictionary<char, Stat> CalcVariances(string csvString, char textQualifyer, char escapeChar)
        {
            var s = new StringReader(csvString);
            string line;
            int lineCount = 0;
            var statistics = new Dictionary<char, Stat>();
            var frequencies = new List<Dictionary<char, int>>();
            while ((line = s.ReadLine()) != null)
            {
                var letterFrequency = new Dictionary<char, int>();
                foreach (var c in line)
                {
                    if (!statistics.ContainsKey(c))
                        statistics.Add(c, new Stat{Occurances = 1});
                    else
                        statistics[c].Occurances++;

                    if (!letterFrequency.ContainsKey(c))
                        letterFrequency.Add(c, 1);
                    else
                        letterFrequency[c]++;
                }

                frequencies.Add(letterFrequency);
                if (lineCount++ > 20) break;
            }

            // Then check the variance on the frequency of each char
            foreach (var c in statistics.Keys)
            {
                var mean = (float)statistics[c].Occurances / lineCount;
                float variance = 0;
                foreach (var frequency in frequencies)
                {
                    var f = 0;
                    if (frequency.ContainsKey(c)) f = frequency[c];
                    variance += (f - mean) * (f - mean);
                }
                variance /= lineCount;
                statistics[c].Variance = variance;
            }

            return statistics;
        }

        private static char GetSeparatorFromVariance(Dictionary<char, float> variances, Dictionary<char, int> occurrences, int lineCount)
        {
            // The char with lowest variance is most likely the separator
            // Optimistic: check prefered with 0 variance 
            var separator = variances
                .Where(x => x.Value == 0f && preferredSeparators.IndexOf(x.Key) != -1)
                .OrderByDescending(x => occurrences[x.Key])
                .Select(x => (char?)x.Key)
                .FirstOrDefault();

            if (separator != null) 
                return separator.Value;

            var defaultKV = default(KeyValuePair<char, float>);

            // Ok, no perfect separator. Check if the best char that exists on all lines is a prefered separator
            var best = variances.OrderBy(x => x.Value).FirstOrDefault(x => occurrences[x.Key] >= lineCount);
            if (!best.Equals(defaultKV) && preferredSeparators.IndexOf(best.Key) != -1) 
                return best.Key;

            // Now we need to somehow decide how much of a 'bonus' a prefered separator should have
            var bestPreffered = variances.OrderBy(x => x.Value).FirstOrDefault(x => occurrences[x.Key] >= lineCount*2 && preferredSeparators.IndexOf(x.Key) != -1);


            // Ok, I have no idea
            return '\0';
        }
    }
}
