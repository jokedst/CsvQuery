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

            // If some columns only got values sometimes, add empty to them to represent the other lines
            var valuesAnalyzed = Columns[0].ValuesAnalyzed;
            foreach (var column in Columns.Skip(1))
            {
                if(column.ValuesAnalyzed < valuesAnalyzed)
                    column.Update(string.Empty);
            } 

            // If the header has more columns than the data, create empty columns
            while (headerTypes.Count > Columns.Count)
                Columns.Add(new CsvColumnAnalyzer(string.Empty));

            // If the header has fewer columns than the data, add empty columns just for the header analysis
            while (headerTypes.Count < Columns.Count)
                headerTypes.Add(new CsvColumnAnalyzer(string.Empty));

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
                var usedNames = new List<string>();
                for (var columnIndex = 0; columnIndex < headerTypes.Count; columnIndex++)
                {
                    var headerType = headerTypes[columnIndex];
                    var columnNameClean = Regex.Replace(headerType.CreationString, @"[^\w_]", "");

                    if (string.IsNullOrEmpty(columnNameClean)) columnNameClean = $"Col{columnIndex + 1}";
                    if (usedNames.Contains(columnNameClean))
                    {
                        var c = 2;
                        var fixedName = columnNameClean + c;
                        while (usedNames.Contains(fixedName))
                            fixedName = columnNameClean + ++c;
                        columnNameClean = fixedName;
                    }
                    usedNames.Add(columnNameClean);
                    Columns[columnIndex].Name = columnNameClean;
                }
            }
            else
            {
                // Just create Col1, Col2, Col3 etc
                var i = 1;
                foreach (var column in Columns)
                {
                    column.Name = "Col" + i++;
                }
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