namespace CsvQuery.Csv
{
    using Tools;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
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
            this.Separator = separator;
        }

        public CsvSettings(char separator, char quoteEscapeChar, char commentChar, bool? hasHeader, List<int> fieldWidths = null)
        {
            this.Separator = separator;
            this.TextQualifier = quoteEscapeChar;
            this.CommentCharacter = commentChar;
            this.FieldWidths = fieldWidths;
            this.HasHeader = hasHeader;
        }

        /// <summary>
        /// Parses a big text blob into rows and columns, using the settings
        /// </summary>
        /// <param name="text">Big blob of text</param>
        /// <returns>Parsed data</returns>
        public List<string[]> Parse(string text)
        {
            using (var reader = new StringReader(text))
            {
                return this.Parse(reader).ToList();
            }
        }

        /// <summary>
        /// Parses a big text blob into rows and columns, using the settings
        /// </summary>
        /// <param name="reader">Big blob of text</param>
        /// <returns>Parsed data</returns>
        public IEnumerable<string[]> ParseVB(TextReader reader)
        {
            // The actual _parsing_ .NET can handle. Well, VisualBasic anyway...
            using (var parser = new TextFieldParser(reader))
            {
                if (this.CommentCharacter != default(char))
                    parser.CommentTokens = new[] {this.CommentCharacter.ToString()};
                parser.SetDelimiters(this.Separator.ToString());
                parser.HasFieldsEnclosedInQuotes = this.TextQualifier != default(char);

                if (this.FieldWidths != null)
                {
                    parser.TextFieldType = FieldType.FixedWidth;
                    parser.SetFieldWidths(this.FieldWidths.ToArray());
                }

                while (!parser.EndOfData)
                {
                    yield return parser.ReadFields();
                }
            }
        }

        public IEnumerable<string[]> Parse(TextReader reader)
        {
            if (this.FieldWidths == null && this.TextQualifier != default(char))
                return this.ParseRaw(reader);
            return this.ParseVB(reader);
        }

        public IEnumerable<string[]> ParseRaw(TextReader reader)
        {
            int ch;
            bool inQuotes = false;
            var rowStart = true;
            var cols = new List<string>();
            var sb = new StringBuilder();

            while ((ch = reader.Read()) != -1)
            {
                var c = (char) ch;
                var nextIsRowStart = false;
                if (rowStart && c == this.CommentCharacter)
                {
                    do { ch = reader.Read(); } while (ch != -1 && ch != '\n');
                    continue;
                }

                if (c == '"')
                {
                    inQuotes = !inQuotes;
                    if (inQuotes && sb.Length > 0) sb.Append('"');
                }
                else if (!inQuotes)
                {
                    if (c == this.Separator)
                    {
                        cols.Add(sb.ToString());
                        sb.Clear();
                    }
                    else if (c == '\r' || c == '\n')
                    {
                        if (c == '\r' && reader.Peek() == '\n') reader.Read();
                        cols.Add(sb.ToString());
                        sb.Clear();
                        yield return cols.ToArray();
                        cols.Clear();
                        nextIsRowStart = true;
                    }
                    else
                    {
                        sb.Append(c);
                    }
                }
                else
                    sb.Append(c);
                rowStart = nextIsRowStart;
            }

            if(rowStart) yield break; // Last row was empty
            cols.Add(sb.ToString());
            yield return cols.ToArray();
        }

        public IEnumerable<string[]> ParseStandard(TextReader reader)
        {
            string line;
            bool inQuotes = false;
            var cols = new List<string>();
            var sb = new StringBuilder();
            while ((line = reader.ReadLine()) != null)
            {
                if (line.Length > 0 && line[0] == this.CommentCharacter) continue;
                foreach (var c in line)
                {
                    if (c == '"')
                    {
                        inQuotes = !inQuotes;
                        if (inQuotes && sb.Length > 0) sb.Append('"');
                    }
                    else if (!inQuotes)
                    {
                        if (c == this.Separator)
                        {
                            cols.Add(sb.ToString());
                            sb.Clear();
                        }
                        else
                        {
                            sb.Append(c);
                        }
                    }
                }
                cols.Add(sb.ToString());
                sb.Clear();
                yield return cols.ToArray();
                cols.Clear();
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
                if (this.HasHeader ?? true)
                {
                    foreach (DataColumn column in dataTable.Columns)
                    {
                        if (first) first = false;
                        else tw.Write(this.Separator);
                        var columnName = headerAlias?.GetValueOrDefault(column.ColumnName) ?? column.ColumnName;
                        this.Escape(tw, columnName);
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
                        else tw.Write(this.Separator);

                        this.Escape(tw, cell.ToString());
                    }
                    tw.WriteLine();
                }
            }
            Trace.TraceInformation("CSV Generation done");
        }

        /// <summary>
        /// Escapes a string if it contains the separator
        /// </summary>
        /// <param name="writer"><see cref="StreamWriter"/> text will be written to</param>
        /// <param name="text"> Text to escape </param>
        protected void Escape(StreamWriter writer, string text)
        {
            if (text.IndexOf(this.Separator) == -1)
                writer.Write(text);
            else
            {
                writer.Write(this.TextQualifier);
                writer.Write(text);
                writer.Write(this.TextQualifier);
            }
        }
    }
}