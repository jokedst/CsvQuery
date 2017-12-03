namespace CsvQuery.Csv
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Tools;

    public class CsvColumnTypes
    {
        public bool HasHeader { get; set; }
        public List<CsvColumnAnalyzer> Columns { get; set; }
        public List<string> ColumnNames { get; }

        public CsvColumnTypes(List<string[]> data, bool? hasHeader)
        {
            if(data==null || data.Count==0)
                throw new ArgumentException("No data to analyze", nameof(data));
            
            var rowLengths = new Dictionary<int, int>();

            var headerTypes = data[0].Select(column => new CsvColumnAnalyzer(column)).ToList();
            IEnumerable<string[]> toAnalyze = data;

            if (hasHeader == false) Columns = headerTypes;
            else
            {
                Columns = new List<CsvColumnAnalyzer>();
                toAnalyze = data.Skip(1);
            }

            foreach (var cols in toAnalyze)
            {
                rowLengths.Increase(cols.Length);

                // Save to Columns
                for (int i = 0; i < cols.Length; i++)
                {
                    var columnText = cols[i];
                    if (Columns.Count <= i) Columns.Add(new CsvColumnAnalyzer(columnText));
                    else Columns[i].Update(columnText);
                }
            }

            // If the header has more columns than the data, create empty columns
            if (headerTypes.Count > Columns.Count)
                Columns.AddRange(Enumerable.Repeat(new CsvColumnAnalyzer(string.Empty),
                    headerTypes.Count - Columns.Count));

            // If the header has fewer columns than the data, add empty columns just for the header analysis
            if (headerTypes.Count < Columns.Count)
                headerTypes.AddRange(Enumerable.Repeat(new CsvColumnAnalyzer(string.Empty),
                    Columns.Count - headerTypes.Count));

            // If any column first row is significantly different from the rest of the rows, it has a header
            HasHeader = hasHeader ?? Columns.Zip(headerTypes, (row, header) => row.IsSignificantlyDifferent(header))
                            .Any(x => x);
            
            if (!hasHeader.HasValue && HasHeader == false)
            {
                // We _detected_ that the file has no headers, so the headerTypes needs to be merged into the other types
                for (int c = 0; c < headerTypes.Count; c++)
                {
                    if (Columns.Count <= c) Columns.Add(headerTypes[c]);
                    else Columns[c].Update(headerTypes[c]);
                }
            }

            // Generate column names
            if (HasHeader)
            {
                ColumnNames=new List<string>();
                var i = 1;
                foreach (var headerType in headerTypes)
                {

                    var columnNameClean = Regex.Replace(headerType.CreationString, @"[^\w_]", "");
                    //if(Sqlite3.yy)
                    if (string.IsNullOrEmpty(columnNameClean)) columnNameClean = "Col" + i;
                    if (ColumnNames.Contains(columnNameClean))
                    {
                        var c = 2;
                        var fixedName = columnNameClean + c;
                        while (ColumnNames.Contains(fixedName))
                            fixedName = columnNameClean + ++c;
                        columnNameClean = fixedName;
                    }
                    ColumnNames.Add(columnNameClean);
                    i++;
                }
            }
            else
            {
                // Just create Col1, Col2, Col3 etc
                ColumnNames = Enumerable.Range(1, Columns.Count).Select(i => "Col" + i).ToList();
            }

            if (rowLengths.Count > 1)
            {
                Trace.TraceWarning("Column count mismatch:" + string.Join(",", rowLengths.Select(p => $"{p.Value} rows had {p.Key} columns")));
            }
        }
        

        public override string ToString()
        {
            return $"{(HasHeader?"header":"no header")}, {Columns.Count} columns: [{string.Join(",", Columns)}]";
        }
    }
}