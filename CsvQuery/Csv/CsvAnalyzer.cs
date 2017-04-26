namespace CsvQuery.Csv
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    public class CsvAnalyzer
    {
        private class Stat
        {
            public int Occurances;
            public float Variance;
        }

        /// <summary>
        /// Analyzes a CSV text and tries to figure out separators, quote chars etc
        /// </summary>
        /// <param name="csvString"></param>
        /// <returns></returns>
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
            var preferredSeparators = Main.Settings.Separators.Replace("\\t", "\t");

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
            var sortedVariances = variances.OrderBy(x => x.Value).ToList();
            var best = sortedVariances.FirstOrDefault(x => occurrences[x.Key] >= lineCount);
            if (!best.Equals(defaultKV) && preferredSeparators.IndexOf(best.Key) != -1) 
                return best.Key;

            // No? Second best?
            best = sortedVariances.Where(x => occurrences[x.Key] >= lineCount).Skip(1).FirstOrDefault();
            if (!best.Equals(defaultKV) && preferredSeparators.IndexOf(best.Key) != -1)
                return best.Key;

            // Ok, screw the preferred separators, is any other char a perfect separator? (and common, i.e. at least 3 per line)
            separator = variances
                .Where(x => x.Value == 0f && occurrences[x.Key] >= lineCount*2)
                .OrderByDescending(x => occurrences[x.Key])
                .Select(x => (char?)x.Key)
                .FirstOrDefault();
            if (separator != null)
                return separator.Value;
            
            // Ok, I have no idea
            return '\0';
        }
    }
}
