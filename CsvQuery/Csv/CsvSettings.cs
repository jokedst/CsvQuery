namespace CsvQuery.Csv
{
    using System.Collections.Generic;
    using System.IO;

    /// <summary>
    /// Describes how a given CSV file is encoded. Few people follo RFC4180...
    /// </summary>
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
}