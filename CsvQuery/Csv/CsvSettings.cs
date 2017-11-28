namespace CsvQuery.Csv
{
    using System;
    using System.Collections.Generic;
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

        /// <summary>
        /// Generates a CSV file from a <see cref="DataGridView"/>, using the settings
        /// </summary>
        /// <param name="dataGrid"> Grid containing data to create CSV from </param>
        /// <returns> string in CSV format </returns>
        public string Generate(DataGridView dataGrid)
        {
            var results = new StringBuilder();

            var first = true;
            foreach (DataGridViewColumn column in dataGrid.Columns)
            {
                if (first) first = false; else results.Append(Separator);
                results.Append(Escape(column.HeaderText));
            }
            results.AppendLine();

            foreach (DataGridViewRow row in dataGrid.Rows)
            {
                first = true;
                foreach (DataGridViewCell cell in row.Cells)
                {
                    if (first) first = false; else results.Append(Separator);
                    results.Append(Escape(cell.Value.ToString()));
                }
                results.AppendLine();
            }

            return results.ToString();
        }

        /// <summary>
        /// Generates a CSV file from a <see cref="DataGridView"/>, using the settings
        /// </summary>
        /// <param name="dataGrid"> Grid containing data to create CSV from </param>
        /// <param name="output"> Stream to write (UTF8) to </param>
        /// <returns> string in CSV format </returns>
        public void GenerateToStream(DataGridView dataGrid, Stream output)
        {
            if(!output.CanWrite)
                throw new ArgumentException("Stream is not writeable", nameof(output));

            using (var tw = new StreamWriter(output, Encoding.UTF8, 8096, true))
            {
                var first = true;
                foreach (DataGridViewColumn column in dataGrid.Columns)
                {
                    if (first) first = false;
                    else tw.Write(Separator);
                    tw.Write(Escape(column.HeaderText));
                }
                tw.WriteLine();
                Trace.TraceInformation("CSV Generated header");

                foreach (DataGridViewRow row in dataGrid.Rows)
                {
                    first = true;
                    foreach (DataGridViewCell cell in row.Cells)
                    {
                        if (first) first = false;
                        else tw.Write(Separator);
                        tw.Write(Escape(cell.Value.ToString()));
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
    }
}