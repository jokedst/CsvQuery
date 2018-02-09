namespace CsvQuery.Csv
{
    using CsvQuery.Tools;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics;
    using System.IO;
    using System.Text;
    using Microsoft.VisualBasic.FileIO;

    /// <summary>
    /// Describes how a given CSV file is encoded. Few people follo RFC4180...
    /// </summary>
    public class CsvSettings
    {
        /// <summary> Shorthand reference to comma-separated CSV </summary>
        public static readonly CsvSettings Comma = new CsvSettings(',');

        /// <summary> Shorthand reference to semicolon-separated CSV </summary>
        public static readonly CsvSettings Semicolon = new CsvSettings(';');

        public char Separator { get; set; }
        public char TextQualifier { get; set; } = '"';
        public char CommentCharacter { get; set; }
        public List<int> FieldWidths { get; set; }
        public bool? HasHeader { get; set; }
        public string[] FieldNames { get; set; }

        public CsvSettings()
        {
        }

        public CsvSettings(char separator)
        {
            Separator = separator;
        }

        public CsvSettings(char separator, char quoteEscapeChar, char commentChar, bool? hasHeader, List<int> fieldWidths = null)
        {
            Separator = separator;
            TextQualifier = quoteEscapeChar;
            CommentCharacter = commentChar;
            FieldWidths = fieldWidths;
            HasHeader = hasHeader;
        }


        /// <summary>
        /// Parses a big text blob into rows and columns, using the settings
        /// </summary>
        /// <param name="text">Big blob of text</param>
        /// <returns>Parsed data</returns>
        public IEnumerable<string[]> Parse(string text)
        {
            using (var reader = new StringReader(text))
            {
                return Parse(reader);
            }
        }

        /// <summary>
        /// Parses a big text blob into rows and columns, using the settings
        /// </summary>
        /// <param name="reader">Big blob of text</param>
        /// <returns>Parsed data</returns>
        public IEnumerable<string[]> Parse(TextReader reader)
        {
            // The actual _parsing_ .NET can handle. Well, VisualBasic anyway...
            using (var parser = new TextFieldParser(reader))
            {
                if (CommentCharacter != default(char))
                    parser.CommentTokens = new[] {CommentCharacter.ToString()};
                parser.SetDelimiters(Separator.ToString());
                parser.HasFieldsEnclosedInQuotes = TextQualifier != default(char);

                if (FieldWidths != null)
                {
                    parser.TextFieldType = FieldType.FixedWidth;
                    parser.SetFieldWidths(FieldWidths.ToArray());
                }

                while (!parser.EndOfData)
                {
                    yield return parser.ReadFields();
                }
            }
        }

        /// <summary>
        /// Generates a CSV file from a <see cref="DataTable"/>, using the settings
        /// </summary>
        /// <param name="dataTable"> Table containing data to create CSV from </param>
        /// <param name="output"> Stream to write (UTF8) to </param>
        /// <param name="headerAlias"> Column header translation table </param>
        public void GenerateToStream(DataTable dataTable, Stream output, IReadOnlyDictionary<string, string> headerAlias = null)
        {
            if (!output.CanWrite)
                throw new ArgumentException("Stream is not writeable", nameof(output));

            using (var tw = new StreamWriter(output, Encoding.UTF8, 1024 * 8, true))
            {
                var first = true;
                if (HasHeader ?? true)
                {
                    foreach (DataColumn column in dataTable.Columns)
                    {
                        if (first) first = false;
                        else tw.Write(Separator);
                        var columnName = headerAlias?.GetValueOrDefault(column.ColumnName) ?? column.ColumnName;
                        Escape(tw, columnName);
                    }
                    tw.WriteLine();
                    Trace.TraceInformation("CSV Generated header");
                }

                foreach (DataRow row in dataTable.Rows)
                {
                    first = true;
                    foreach (object cell in row.ItemArray)
                    {
                        if (first) first = false;
                        else tw.Write(Separator);

                        Escape(tw, cell.ToString());
                    }
                    tw.WriteLine();
                }
            }
            Trace.TraceInformation("CSV Generation done");
        }

        /// <summary>
        /// Escapes a string if it contains the separator
        /// </summary>
        /// <param name="text"> Text to escape </param>
        /// <param name="always"> If true the text will always be escaped </param>
        /// <returns> Escaped text </returns>
        protected string Escape(string text, bool always = false)
        {
            if (!always && text.IndexOf(Separator) == -1)
                return text;
            return TextQualifier
                   + text.Replace(TextQualifier.ToString(), TextQualifier.ToString() + TextQualifier)
                   + TextQualifier;
        }

        /// <summary>
        /// Escapes a string if it contains the separator
        /// </summary>
        /// <param name="writer"><see cref="StreamWriter"/> text will be written to</param>
        /// <param name="text"> Text to escape </param>
        protected void Escape(StreamWriter writer, string text)
        {
            if (text.IndexOf(Separator) == -1)
                writer.Write(text);
            else
            {
                writer.Write(TextQualifier);
                writer.Write(text);
                writer.Write(TextQualifier);
            }
        }
    }
}