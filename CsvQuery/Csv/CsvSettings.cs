namespace CsvQuery.Csv
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    /// <summary>
    /// Describes how a given CSV file is encoded. Few people follo RFC4180...
    /// </summary>
    public class CsvSettings
    {
        public char Separator { get; set; }
        public char TextQualifier { get; set; }
        public char EscapeCharacter { get; set; }
        public char CommentCharacter { get; set; }

        /// <summary>
        /// Parses a big text blob into rows and columns, using the settings
        /// </summary>
        /// <param name="text">Big blob of text</param>
        /// <returns>Parsed data</returns>
        public List<string[]> Parse(string text)
        {
            // The actual _parsing_ .NET can handle. Well, VisualBasic anyway...
            using(var reader = new StringReader(text))
            using (var parser = new Microsoft.VisualBasic.FileIO.TextFieldParser(reader))
            {
                if(CommentCharacter!=default(char))
                    parser.CommentTokens = new[] { CommentCharacter.ToString() };
                parser.SetDelimiters(Separator.ToString());
                parser.HasFieldsEnclosedInQuotes = TextQualifier != default(char);

                var ret = new List<string[]>();
                while (!parser.EndOfData)
                {
                    ret.Add(parser.ReadFields());
                }
                return ret;
            }
        }

        internal enum ParseState
        {
            WordStart,
            InQuotes,
            UnQuoted,
            OutOfQuotes
        }
    }
}