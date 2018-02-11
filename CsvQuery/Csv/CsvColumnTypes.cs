namespace CsvQuery.Csv
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    public class CsvColumnTypes
    {
        public bool HasHeader { get; set; }
        public List<CsvColumnAnalyzer> Columns { get; set; }
        public Dictionary<string,string> ColumnUnsafeNames { get; }

        public CsvColumnTypes(List<string[]> data, CsvSettings csvSettings)
        {
            if(data==null || data.Count==0)
                throw new ArgumentException("No data to analyze", nameof(data));

            if (csvSettings == null)
                csvSettings = new CsvSettings();

            var headerTypes = data[0].Select(column => new CsvColumnAnalyzer(column)).ToList();
            IEnumerable<string[]> toAnalyze = data;

            if (csvSettings.HasHeader == false)
                this.Columns = headerTypes;
            else
            {
                this.Columns = new List<CsvColumnAnalyzer>();
                toAnalyze = data.Skip(1);
            }

            foreach (var cols in toAnalyze)
            {
                // Save to Columns
                for (int i = 0; i < cols.Length; i++)
                {
                    var columnText = cols[i];
                    if (this.Columns.Count <= i)
                        this.Columns.Add(new CsvColumnAnalyzer(columnText));
                    else this.Columns[i].Update(columnText);
                }
            }

            // If some columns only got values sometimes, add empty to them to represent the other lines
            var valuesAnalyzed = this.Columns[0].ValuesAnalyzed;
            foreach (var column in this.Columns.Skip(1))
            {
                if(column.ValuesAnalyzed < valuesAnalyzed)
                    column.Update(string.Empty);
            } 

            // If the header has more columns than the data, create empty columns
            while (headerTypes.Count > this.Columns.Count)
                this.Columns.Add(new CsvColumnAnalyzer(string.Empty));

            // If the header has fewer columns than the data, add empty columns just for the header analysis
            while (headerTypes.Count < this.Columns.Count)
                headerTypes.Add(new CsvColumnAnalyzer(string.Empty));

            // If any column first row is significantly different from the rest of the rows, it has a header
            this.HasHeader = csvSettings.HasHeader ?? this.Columns.Zip(headerTypes, (row, header) => row.IsSignificantlyDifferent(header))
                            .Any(x => x);
            
            if (!csvSettings.HasHeader.HasValue && this.HasHeader == false)
            {
                // We _detected_ that the file has no headers, so the headerTypes needs to be merged into the other types
                for (int c = 0; c < headerTypes.Count; c++)
                {
                    if (this.Columns.Count <= c) this.Columns.Add(headerTypes[c]);
                    else this.Columns[c].Update(headerTypes[c]);
                }
            }

            // Generate column names
            var usedNames = new List<string>();
            var namedColumns = csvSettings.FieldNames?.Length ?? 0;
            this.ColumnUnsafeNames = new Dictionary<string, string>();
            for (var z = 0; z < headerTypes.Count; z++)
            {
                var unsafeName = namedColumns > z
                    ? csvSettings.FieldNames[z]
                    : this.HasHeader
                        ? headerTypes[z].CreationString
                        : null;
                var safeName = unsafeName;
                if (!string.IsNullOrWhiteSpace(safeName))
                    safeName = Regex.Replace(safeName, @"[^\w_]", "");
                if (string.IsNullOrWhiteSpace(safeName))
                    safeName = $"Col{z + 1}";
                if (usedNames.Contains(safeName))
                {
                    var c = 2;
                    var fixedName = safeName + c;
                    while (usedNames.Contains(fixedName))
                        fixedName = safeName + ++c;
                    safeName = fixedName;
                }
                usedNames.Add(safeName);
                this.Columns[z].Name = safeName;

                if(!string.IsNullOrWhiteSpace(unsafeName) && unsafeName != safeName)
                    this.ColumnUnsafeNames.Add(safeName, unsafeName);
            }
        }

        public override string ToString()
        {
            return $"{(this.HasHeader ? "header" : "no header")}, {this.Columns.Count} columns: [{string.Join(",", this.Columns)}]";
        }
    }
}