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
        public char CommentCharacter { get; set; }
        public List<int> FieldWidths { get; set; }
        public bool? HasHeader { get; set; }
        public string[] FieldNames { get; set; }
        public bool UseQuotes { get; set; } = true;

        public CsvSettings()
        {
        }

        public CsvSettings(char separator)
        {
            this.Separator = separator;
        }

        public CsvSettings(char separator, bool useQuotes, char commentChar, bool? hasHeader, List<int> fieldWidths = null)
        {
            this.Separator = separator;
            this.CommentCharacter = commentChar;
            this.FieldWidths = fieldWidths;
            this.HasHeader = hasHeader;
            this.UseQuotes = useQuotes;
        }

        /// <summary>
        /// Parses a big text blob into rows and columns, using the settings
        /// </summary>
        /// <remarks>
        /// At this point there are FOUR parsers implemented:
        ///   ParseVB - The original, included in .NET framework. Currently used. Slow though (^10 times slower).
        ///   ParseStandard - My first replacement attempt. Reads line by line. Fastest for some reason.
        ///   ParseRaw - Second attempt. Reads character by character, allowing multi-line quoted strings. Somewhat slower.
        ///   ParseRawBuffered - Attempt to make ParseRaw faster by buffering directly instead of using StringBuilder. It's actually slower though. :(
        /// However, since the parsing is NOT the current bottleneck, it doesn't really matter which one is used
        /// </remarks>
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
                parser.HasFieldsEnclosedInQuotes = this.UseQuotes;

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

        public virtual IEnumerable<string[]> Parse(TextReader reader)
        {
            if (this.FieldWidths == null)
                return this.ParseCustom(reader);
            return this.ParseVB(reader);
        }

        /// <summary>
        /// Parse character by character. Still has bugs (see failing tests)
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public IEnumerable<string[]> ParseRaw(TextReader reader)
        {
            int ch;
            bool inQuotes = false;
            var rowStart = true;
            var cols = new List<string>();
            var sb = new StringBuilder(100, 100000);

            while ((ch = reader.Read()) != -1)
            {
                var c = (char) ch;
                var nextIsRowStart = false;
                if (rowStart && (c == this.CommentCharacter || c == '\r' || c == '\n'))
                {
                    do { ch = reader.Read(); } while (ch != -1 && ch != '\n');
                    continue;
                }

                if (c == '"' && this.UseQuotes)
                {
                    inQuotes = !inQuotes;
                    if (inQuotes && sb.Length > 0) sb.Append('"');
                }
                else if (!inQuotes)
                {
                    if (c == this.Separator)
                    {
                        cols.Add(sb.TrimToString());
                        sb.Clear();
                    }
                    else if (c == '\r' || c == '\n')
                    {
                        if (c == '\r' && reader.Peek() == '\n') reader.Read();
                        cols.Add(sb.TrimToString());
                        sb.Clear();
                        //sb.Capacity = 100;
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
            cols.Add(sb.TrimToString());
            yield return cols.ToArray();
        }

        /// <summary>
        /// Does not use string builder - written to fix a problem, but kept until I can benchmark it (It's probably faster, but the bottleneck isn't really here).
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public IEnumerable<string[]> ParseRawBuffered(TextReader reader)
        {
            int ch;
            bool inQuotes = false;
            var rowStart = true;
            var cols = new List<string>();
            var buffer = new char[1000];
            var bpos = 0;

            void Append(char c)
            {
                if (bpos >= buffer.Length)
                {
                    // Realloc
                    var chArray = new char[buffer.Length + 1000];
                    Array.Copy(buffer, chArray, buffer.Length);
                    buffer = chArray;
                }
                buffer[bpos++] = c;
            }

            while ((ch = reader.Read()) != -1)
            {
                var c = (char)ch;
                var nextIsRowStart = false;
                if (rowStart && c == this.CommentCharacter)
                {
                    do { ch = reader.Read(); } while (ch != -1 && ch != '\n');
                    continue;
                }

                if (c == '"' && this.UseQuotes)
                {
                    inQuotes = !inQuotes;
                    if (inQuotes && bpos > 0) Append('"');
                }
                else if (!inQuotes)
                {
                    if (c == this.Separator)
                    {
                        cols.Add(new string(buffer, 0, bpos));
                        bpos = 0;
                    }
                    else if (c == '\r' || c == '\n')
                    {
                        if (c == '\r' && reader.Peek() == '\n') reader.Read();
                        cols.Add(new string(buffer, 0, bpos));
                        bpos = 0;
                        yield return cols.ToArray();
                        cols.Clear();
                        nextIsRowStart = true;
                    }
                    else
                    {
                        Append(c);
                    }
                }
                else
                    Append(c);
                rowStart = nextIsRowStart;
            }

            if (rowStart) yield break; // Last row was empty
            cols.Add(new string(buffer, 0, bpos));
            yield return cols.ToArray();
        }

        /// <summary>
        /// Read line-by-line. Can't handle multi-line text fields.
        /// </summary>
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
                    if (c == '"' && this.UseQuotes)
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
        /// Reads char-by-char, but works (mostly) 
        /// </summary>
        /// <remarks>
        /// Based on code from https://www.codeproject.com/Tips/823670/Csharp-Light-and-Fast-CSV-Parser
        /// </remarks>
        public IEnumerable<string[]> ParseCustom(TextReader reader)
        {
            int ch;
            var inQuotes = false;
            var cols = new List<string>();
            var sb = new StringBuilder();
            char qualifier = this.UseQuotes ? '"' : '\0';

            while ((ch = reader.Read()) != -1)
            {
                var c = (char)ch;

                if (c == '\n' || (c == '\r' && (char)reader.Peek() == '\n'))
                {
                    // If it's a \r\n combo consume the \n part and throw it away.
                    if (c == '\r')
                        reader.Read();

                    if (inQuotes)
                    {
                        if (c == '\r')
                            sb.Append('\r');
                        sb.Append('\n');
                    }
                    else
                    {
                        if (cols.Count > 0 || sb.Length > 0)
                        {
                            cols.Add(sb.TrimEnd().ToString());
                            sb.Clear();
                        }

                        if (cols.Count > 0)
                            yield return cols.ToArray();
                        
                        cols.Clear();
                    }
                }
                else if (sb.Length == 0 && !inQuotes)
                {
                    if (c == qualifier)
                        inQuotes = true;
                    else if (c == this.Separator)
                    {
                        cols.Add(sb.TrimEnd().ToString());
                        sb.Clear();
                    }
                    else if (char.IsWhiteSpace(c))
                    {
                        // Ignore leading whitespace
                    }
                    else if (c == this.CommentCharacter)
                    {
                        // Read to end of line
                        do { ch = reader.Read(); } while (ch != -1 && ch != '\n');
                    }
                    else
                        sb.Append(c);
                }
                else if (c == this.Separator)
                {
                    if (inQuotes)
                        sb.Append(this.Separator);
                    else
                    {
                        cols.Add(sb.TrimEnd().ToString());
                        sb.Clear();
                    }
                }
                else if (c == qualifier)
                {
                    if (inQuotes)
                    {
                        if ((char)reader.Peek() == qualifier)
                        {
                            reader.Read();
                            sb.Append(qualifier);
                        }
                        else
                            inQuotes = false;
                    }
                    else
                        sb.Append(c);
                }
                else
                    sb.Append(c);
            }

            if (cols.Count > 0 || sb.Length > 0)
                cols.Add(sb.TrimEnd().ToString());

            if (cols.Count > 0)
                yield return cols.ToArray();
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
        /// Escapes a string if it contains the separator, newline or quotes, or has leading or trailing spaces
        /// </summary>
        /// <param name="writer"><see cref="StreamWriter"/> text will be written to</param>
        /// <param name="text"> Text to escape </param>
        protected void Escape(StreamWriter writer, string text)
        {
            if (text.Length == 0) return;
            if (text.IndexOf(this.Separator) == -1 && text.IndexOf('\r') == -1 && text.IndexOf('\n') == -1 && text.IndexOf('"') == -1
                && !char.IsWhiteSpace(text[0]) && !char.IsWhiteSpace(text[text.Length-1]))
            {
                writer.Write(text);
            }
            else
            {
                writer.Write('"');
                writer.Write(text.Replace("\"","\"\""));
                writer.Write('"');
            }
        }
    }
}