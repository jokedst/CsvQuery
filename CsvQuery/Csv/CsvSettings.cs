using CsvQuery.Tools;

namespace CsvQuery.Csv
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics;
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

        public CsvSettings() {}

        public CsvSettings(char separator)
        {
            this.Separator = separator;
        }
        public CsvSettings(char separator, char quoteEscapeChar, char commentChar, bool? hasHeader, List<int> fieldWidths=null)
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
            // The actual _parsing_ .NET can handle. Well, VisualBasic anyway...
            using(var reader = new StringReader(text))
            using (var parser = new Microsoft.VisualBasic.FileIO.TextFieldParser(reader))
            {
                var errors = new StringBuilder();
                if(this.CommentCharacter !=default(char))
                    parser.CommentTokens = new[] { this.CommentCharacter.ToString() };
                parser.SetDelimiters(this.Separator.ToString());
                parser.HasFieldsEnclosedInQuotes = this.TextQualifier != default(char);

                if (this.FieldWidths != null)
                {
                    parser.TextFieldType = FieldType.FixedWidth;
                    try
                    {
                        parser.SetFieldWidths(this.FieldWidths.ToArray());
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

        /// <summary>
        /// Generates a CSV file from a <see cref="DataTable"/>, using the settings
        /// </summary>
        /// <param name="dataTable"> Table containing data to create CSV from </param>
        /// <param name="output"> Stream to write (UTF8) to </param>
        /// <param name="headerAlias"> Column header translation table </param>
        public void GenerateToStream(DataTable dataTable, Stream output, IReadOnlyDictionary<string,string> headerAlias = null)
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
                        else tw.Write(this.Separator);
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
                        else tw.Write(this.Separator);
                        
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
            if (!always && text.IndexOf(this.Separator) == -1)
                return text;
            return this.TextQualifier
                   + text.Replace(this.TextQualifier.ToString(), this.TextQualifier.ToString() + this.TextQualifier)
                   + this.TextQualifier;
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