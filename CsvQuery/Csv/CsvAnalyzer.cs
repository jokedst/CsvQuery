namespace CsvQuery.Csv
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Tools;

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
            var result = DetectW3C(csvString);
            if (result != null)
                return result;

            // First do a letter frequency analysis on each row
            var s = new StringReader(csvString);
            string line;
            int lineCount = 0, linesQuoted=0;
            var frequencies = new List<Dictionary<char, int>>();
            var occurrences = new Dictionary<char, int>();
            var frequenciesQuoted = new List<Dictionary<char, int>>();
            var occurrencesQuoted = new Dictionary<char, int>();
            var wordStarts = new Dictionary<int, int>();
            var bigSpaces = new Dictionary<int, int>();
            var inQuotes = false;
            var letterFrequencyQuoted = new Dictionary<char, int>();

            while ((line = s.ReadLine()) != null)
            {
                var letterFrequency = new Dictionary<char, int>();
                int spaces = 0, i = 0;
                foreach (var c in line)
                {
                    letterFrequency.Increase(c);
                    occurrences.Increase(c);

                    if (c == '"') inQuotes = !inQuotes;
                    else if (!inQuotes)
                    {

                        letterFrequencyQuoted.Increase(c);
                        occurrencesQuoted.Increase(c);
                    }

                    if (c == ' ')
                    {
                        if (++spaces >= 2) bigSpaces.Increase(i);
                    }
                    else
                    {
                        if (spaces >= 2) wordStarts.Increase(i);
                        spaces = 0;
                    }
                    i++;
                }

                frequencies.Add(letterFrequency);
                if(!inQuotes)
                {
                    frequenciesQuoted.Add(letterFrequencyQuoted);
                    letterFrequencyQuoted=new Dictionary<char, int>();
                    linesQuoted++;
                }

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
                variances.Add(c, variance);
            }

            var variancesQuoted = new Dictionary<char, float>();
            foreach (var c in occurrencesQuoted.Keys)
            {
                var mean = (float)occurrencesQuoted[c] / linesQuoted;
                float variance = 0;
                foreach (var frequency in frequenciesQuoted)
                {
                    var f = 0;
                    if (frequency.ContainsKey(c)) f = frequency[c];
                    variance += (f - mean) * (f - mean);
                }
                variance /= lineCount;
                variancesQuoted.Add(c, variance);
            }

            // The char with lowest variance is most likely the separator
            result = new CsvSettings { Separator = GetSeparatorFromVariance(variances, occurrences, lineCount, out var uncertancy) };
            var separatorQuoted = GetSeparatorFromVariance(variancesQuoted, occurrencesQuoted, linesQuoted, out var uncertancyQuoted);
            if (uncertancyQuoted < uncertancy)
                result.Separator = separatorQuoted;
            else if (uncertancy < uncertancyQuoted) // It was better ignoring quotes!
                result.TextQualifier = '\0';

            if (result.Separator != default(char)) return result;

            // Failed to detect separator. Could it be a fixed-width file?
            var commonSpace = bigSpaces.Where(x => x.Value == lineCount).Select(x => x.Key).OrderBy(x => x);
            var lastvalue = 0;
            int lastStart = 0;
            var foundfieldWidths = new List<int>();
            foreach (var space in commonSpace)
            {
                if (space != lastvalue + 1)
                {
                    foundfieldWidths.Add(space - lastStart);
                    lastStart = space;
                }

                lastvalue = space;
            }
            if (foundfieldWidths.Count < 3) return result; // unlikely fixed width
            foundfieldWidths.Add(-1); // Last column gets "the rest"
            result.FieldWidths = foundfieldWidths;
            return result;
        }

        private static CsvSettings DetectW3C(string csvString)
        {
            if (!csvString.StartsWith("#")) return null;
            var header = new Regex(@"^#([^:]*):\s*(.*)$");
            using (var s = new StringReader(csvString))
            {
                var headers = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
                string line;
                while ((line = s.ReadLine()) != null)
                {
                    var headerMatch = header.Match(line);
                    if (!headerMatch.Success) break;
                    headers.Add(headerMatch.Groups[1].Value, headerMatch.Groups[2].Value);
                }
                if (!headers.ContainsKey("Version") || !headers.ContainsKey("Fields") || line == null)
                    return null;

                // Ok, fairly sure this is a w3c log... Check separator
                var result = new W3CSettings { FieldNames = Regex.Split(headers["Fields"], @"\s") };
                int spaces = 0, tabs = 0, runs = 0;
                var lastCharWhite = true;
                foreach (var t in line)
                {
                    var white = t == ' ' || t == '\t';
                    if (white && !lastCharWhite) runs++;
                    lastCharWhite = white;
                    if (t == ' ') spaces++;
                    if (t == '\t') tabs++;
                }
                if (tabs == result.FieldNames.Length - 1) result.Separator = '\t';
                else if (spaces == result.FieldNames.Length - 1) result.Separator = ' ';
                else if (tabs > spaces && tabs < result.FieldNames.Length) result.Separator = '\t';
                else if (spaces < result.FieldNames.Length && spaces > 1) result.Separator = ' ';
                else return null;

                return result;
            }
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
                        statistics.Add(c, new Stat { Occurances = 1 });
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

        private static char GetSeparatorFromVariance(Dictionary<char, float> variances, Dictionary<char, int> occurrences, int lineCount, out int uncertancy)
        {
            var preferredSeparators = Main.Settings.Separators.Replace("\\t", "\t");
            uncertancy = 0;

            // The char with lowest variance is most likely the separator
            // Optimistic: check prefered with 0 variance 
            var separator = variances
                .Where(x => x.Value == 0f && preferredSeparators.IndexOf(x.Key) != -1)
                .OrderByDescending(x => occurrences[x.Key])
                .Select(x => (char?)x.Key)
                .FirstOrDefault();

            if (separator != null)
                return separator.Value;

            uncertancy++;
            var defaultKV = default(KeyValuePair<char, float>);

            // Ok, no perfect separator. Check if the best char that exists on all lines is a prefered separator
            var sortedVariances = variances.OrderBy(x => x.Value).ToList();
            var best = sortedVariances.FirstOrDefault(x => occurrences[x.Key] >= lineCount);
            if (!best.Equals(defaultKV) && preferredSeparators.IndexOf(best.Key) != -1)
                return best.Key;
            uncertancy++;

            // No? Second best?
            best = sortedVariances.Where(x => occurrences[x.Key] >= lineCount).Skip(1).FirstOrDefault();
            if (!best.Equals(defaultKV) && preferredSeparators.IndexOf(best.Key) != -1)
                return best.Key;
            uncertancy++;

            // Ok, screw the preferred separators, is any other char a perfect separator? (and common, i.e. at least 3 per line)
            separator = variances
                .Where(x => x.Value == 0f && occurrences[x.Key] >= lineCount * 2)
                .OrderByDescending(x => occurrences[x.Key])
                .Select(x => (char?)x.Key)
                .FirstOrDefault();
            if (separator != null)
                return separator.Value;

            uncertancy++;
            // Ok, I have no idea
            return '\0';
        }
    }
}
