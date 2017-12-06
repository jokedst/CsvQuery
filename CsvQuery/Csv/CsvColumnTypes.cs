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

        public CsvColumnTypes(List<string[]> data, CsvSettings csvSettings)
        {
            if(data==null || data.Count==0)
                throw new ArgumentException("No data to analyze", nameof(data));

            if(csvSettings==null)
                csvSettings=new CsvSettings();

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
            for (var z = 0; z < headerTypes.Count; z++)
            {
                var unsafeName = namedColumns > z
                    ? csvSettings.FieldNames[z]
                    : HasHeader
                        ? headerTypes[z].CreationString
                        : null;
                if(!string.IsNullOrWhiteSpace(unsafeName))
                    unsafeName = Regex.Replace(unsafeName, @"[^\w_]", "");
                if (string.IsNullOrWhiteSpace(unsafeName))
                    unsafeName= $"Col{z + 1}";
                if (usedNames.Contains(unsafeName))
                {
                    var c = 2;
                    var fixedName = unsafeName + c;
                    while (usedNames.Contains(fixedName))
                        fixedName = unsafeName + ++c;
                    unsafeName = fixedName;
                }
                usedNames.Add(unsafeName);
                this.Columns[z].Name = unsafeName;
            }

            //if (this.HasHeader)
            //{
            ////    var usedNames = new List<string>();
            //    for (var columnIndex = 0; columnIndex < headerTypes.Count; columnIndex++)
            //    {
            //        var headerType = headerTypes[columnIndex];
            //        var columnNameClean = Regex.Replace(headerType.CreationString, @"[^\w_]", "");

            //        if (string.IsNullOrEmpty(columnNameClean)) columnNameClean = $"Col{columnIndex + 1}";
            //        if (usedNames.Contains(columnNameClean))
            //        {
            //            var c = 2;
            //            var fixedName = columnNameClean + c;
            //            while (usedNames.Contains(fixedName))
            //                fixedName = columnNameClean + ++c;
            //            columnNameClean = fixedName;
            //        }
            //        usedNames.Add(columnNameClean);
            //        this.Columns[columnIndex].Name = columnNameClean;
            //    }
            //}
            //else
            //{
            //    // Just create Col1, Col2, Col3 etc unless the settings had something
            //   // var namedColumns = csvSettings.FieldNames?.Length ?? 0;
            //    for (var index = 0; index < this.Columns.Count; index++)
            //    {
            //        if (namedColumns > index)
            //            this.Columns[index].Name = csvSettings.FieldNames[index];
            //        else
            //            this.Columns[index].Name = "Col" + (index + 1);
            //    }
            //}
        }

        public override string ToString()
        {
            return $"{(this.HasHeader ? "header" : "no header")}, {this.Columns.Count} columns: [{string.Join(",", this.Columns)}]";
        }
    }
}