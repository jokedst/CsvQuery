namespace CsvQuery.Csv
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Windows.Forms;
    using Microsoft.VisualBasic.FileIO;

    /// <summary>
    /// Describes how a given CSV file is encoded. Few people follo RFC4180...
    /// </summary>
    public class CsvSettings
    {
        public char Separator { get; set; }
        public char TextQualifier { get; set; } = '"';
        public char CommentCharacter { get; set; }
        public List<int> FieldWidths { get; set; }

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
                var errors = new StringBuilder();
                if(CommentCharacter!=default(char))
                    parser.CommentTokens = new[] { CommentCharacter.ToString() };
                parser.SetDelimiters(Separator.ToString());
                parser.HasFieldsEnclosedInQuotes = TextQualifier != default(char);

                if (FieldWidths != null)
                {
                    parser.TextFieldType = FieldType.FixedWidth;
                    try
                    {
                        parser.SetFieldWidths(FieldWidths.ToArray());
                    }
                    catch (Exception e)
                    {
                        errors.AppendLine(e.Message);
                    }
                }

                var ret = new List<string[]>();
                while (!parser.EndOfData)
                {
                    try
                    {
                        ret.Add(parser.ReadFields());
                    }
                    catch (MalformedLineException e)
                    {
                        errors.AppendFormat("Error on line {0}: {1}\n", e.LineNumber, e.Message);
                    }
                }
                if (errors.Length > 0) MessageBox.Show(errors.ToString(), "Errors");
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