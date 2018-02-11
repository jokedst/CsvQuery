//#define HISTOGRAM
#define COMMONONLY

namespace CsvQuery.Csv
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlTypes;
    using System.Globalization;
    using System.Linq;
    using Tools;

    /// <summary>
    /// Decimal numbers can use dot ("invariant"), comma or other local settings
    /// </summary>
    [Flags]
    public enum DecimalTypes : byte
    {
        None = 0,
        Invariant = 1,
        Comma = 2,
        Local = 4,
        Any = Invariant | Comma | Local
    }

    public enum IntegerTypes
    {
        None=0,
        Bit,
        UInt8,
        Int16,
        Int32,
        Int64
    }

    /// <summary>
    /// Analyses one column and figures out the type
    /// </summary>
    public class CsvColumnAnalyzer
    {
        private static readonly NumberFormatInfo CommaDecimal = new NumberFormatInfo
        {
            NumberDecimalSeparator = ",",
            NumberGroupSeparator = " ",
            PercentDecimalSeparator = ",",
            PercentGroupSeparator = " ",
            CurrencyDecimalSeparator = ",",
            CurrencyGroupSeparator = " "
        };
        private static readonly NumberFormatInfo DotDecimal = new NumberFormatInfo
        {
            NumberGroupSeparator = " ",
            PercentGroupSeparator = " ",
            CurrencyGroupSeparator = " "
        };
#if COMMONONLY
        public string CreationString
        {
            get => this._commonStrings[0];
            set => this._commonStrings[0] = value;
        }
#else
        public string CreationString;
#endif
        public ColumnType DataType;
        public DecimalTypes DecimalType = DecimalTypes.Any;
        public int MinSize;
        public int MaxSize;
        public bool Nullable;
        public string Prefix;
        public string Suffix;
        public int ValuesAnalyzed = 1;
        public long MinInteger;
        public long MaxInteger;
        public string Name { get; set; }
#if HISTOGRAM
        public Dictionary<string, int> DistinctCounts;
#elif COMMONONLY
        private string[] _commonStrings = new string[10];
        private bool _tooManyStrings;
#else
        public bool IsSingleValue = true;
#endif
        private static NumberStyles _numberStyles = NumberStyles.Number;

        /// <summary>
        ///     Detect data type from string
        /// </summary>
        /// <param name="csvText"></param>
        public CsvColumnAnalyzer(string csvText)
            : this(csvText, ColumnType.Empty, DecimalTypes.Any)
        {
        }

        public CsvColumnAnalyzer(string csvText, ColumnType updatedType, DecimalTypes updatedDecimalType)
        {
            this.CreationString = this.Prefix = this.Suffix = csvText;

            if (string.IsNullOrWhiteSpace(csvText))
            {
                this.DataType = ColumnType.Empty;
                this.Nullable = true;
                return;
            }

            this.MinSize = this.MaxSize = csvText.Length;

            // TryParse is the bottleneck in this code, so only use when necessary
            if (updatedType <= ColumnType.Integer && long.TryParse(csvText, out var iout))
                if (!Main.Settings.ConvertInitialZerosToNumber && csvText.StartsWith("0") && csvText.Length > 1
                    || Main.Settings.MaxIntegerStringLength < csvText.Length)
                {
                    this.DataType = ColumnType.String;
                }
                else
                {
                    this.DataType = ColumnType.Integer;
                    this.MinInteger = this.MaxInteger = iout;
                }
            else if (updatedType <= ColumnType.Decimal && this.IsDecimal(csvText, updatedDecimalType))
                this.DataType = ColumnType.Decimal;
            else
                this.DataType = ColumnType.String;
        }

#if HISTOGRAM
        public bool IsSingleValue => DistinctCounts == null || DistinctCounts.Count == 1;

        public bool IsOnlyCommonValues => ValuesAnalyzed > 30 && DistinctCounts.Count<10 && DistinctCounts.All(x => x.Value > ValuesAnalyzed / 8);
#elif COMMONONLY
        public bool IsSingleValue => this._commonStrings[1] == null;
#endif
        public bool IsNumeric => this.DataType == ColumnType.Integer || this.DataType == ColumnType.Decimal;

        private bool IsDecimal(string text, DecimalTypes updatedDecimalType)
        {
            // Try invariant, comma and user local settings
            if (updatedDecimalType.HasFlag( DecimalTypes.Invariant) && decimal.TryParse(text, _numberStyles, DotDecimal, out _))
            {
                this.DecimalType = DecimalTypes.Invariant;
                return true;
            }
            if (updatedDecimalType.HasFlag(DecimalTypes.Comma) && decimal.TryParse(text, _numberStyles, CommaDecimal, out _))
            {
                this.DecimalType = DecimalTypes.Comma;
                return true;
            }
            if (updatedDecimalType.HasFlag(DecimalTypes.Local) && decimal.TryParse(text, _numberStyles | NumberStyles.AllowCurrencySymbol, NumberFormatInfo.CurrentInfo,
                out _))
            {
                this.DecimalType = DecimalTypes.Local;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Converts a string to a native type representing this column type.
        /// ONLY if DetectDbColumnTypes is activated, otherwise returns the input.
        /// </summary>
        /// <param name="input"> String containing data of this column's type </param>
        /// <returns></returns>
        public object Parse(string input)
        {
            if (string.IsNullOrEmpty(input))
                return null;
            if (this.DataType == ColumnType.Decimal)
            {
                if (this.DecimalType.HasFlag(DecimalTypes.Invariant))
                    return decimal.Parse(input, _numberStyles, DotDecimal);
                if (this.DecimalType.HasFlag(DecimalTypes.Comma))
                    return decimal.Parse(input, _numberStyles, CommaDecimal);
                if (this.DecimalType.HasFlag(DecimalTypes.Local))
                    return decimal.Parse(input, _numberStyles | NumberStyles.AllowCurrencySymbol, NumberFormatInfo.CurrentInfo);
            }
            return input;
        }

        /// <summary>
        ///     Updates a type with a new value - it becomes the most generic of the two
        /// </summary>
        /// <param name="csvText"> text from CSV file </param>
        public void Update(string csvText)
        {
            //Update(new CsvColumnAnalyzer(csvType, DataType, DecimalType));
            var csvTypeNullable = string.IsNullOrEmpty(csvText);
            if (!csvTypeNullable)
                if (this.DataType <= ColumnType.Integer && long.TryParse(csvText, out var iout))
                    if (!Main.Settings.ConvertInitialZerosToNumber && csvText.StartsWith("0") && csvText.Length > 1
                        || Main.Settings.MaxIntegerStringLength < csvText.Length)
                    {
                        this.DataType = ColumnType.String;
                    }
                    else if (this.DataType == ColumnType.Integer)
                    {
                        this.MinInteger = Math.Min(this.MinInteger, iout);
                        this.MaxInteger = Math.Max(this.MaxInteger, iout);
                    }
                    else
                    {
                        this.MinInteger = this.MaxInteger = iout;
                        this.DataType = ColumnType.Integer;
                    }
                else if (this.DataType <= ColumnType.Decimal && this.IsDecimal(csvText, this.DecimalType))
                    this.DataType = ColumnType.Decimal;
                else
                    this.DataType = ColumnType.String;

            this.MaxSize = Math.Max(this.MaxSize, csvText?.Length ?? 0);
            this.MinSize = Math.Min(this.MinSize, csvText?.Length ?? 0);
            this.Nullable = this.Nullable || csvTypeNullable;

            this.Prefix = Extensions.CommonPrefix(this.Prefix, csvText);
            this.Suffix = Extensions.CommonSuffix(this.Suffix, csvText);


#if HISTOGRAM
            if (DistinctCounts == null) 
                DistinctCounts =  new Dictionary<string, int>(10) { { CreationString, 1 } };

            if (DistinctCounts.Count < 10)
                DistinctCounts.Increase(csvText);
#elif COMMONONLY
            if (!this._tooManyStrings)
            {
                int i;
                for (i = 0; i < 10 && csvText!= this._commonStrings[i]; i++)
                    if (this._commonStrings[i] == null)
                    {
                        this._commonStrings[i] = csvText;
                        break;
                    }

                if (i == 10) this._tooManyStrings = true;
            }
#else
            if (IsSingleValue && csvText != CreationString)
                IsSingleValue = false;
#endif
            this.ValuesAnalyzed++;
        }

        public void Update(CsvColumnAnalyzer csvType)
        {
            this.DataType = csvType.DataType > this.DataType ? csvType.DataType : this.DataType;
            this.MaxSize = Math.Max(this.MaxSize, csvType.MaxSize);
            this.MinSize = Math.Min(this.MinSize, csvType.MinSize);
            this.Nullable = this.Nullable || csvType.Nullable;

            this.Prefix = Extensions.CommonPrefix(this.Prefix, csvType.Prefix);
            this.Suffix = Extensions.CommonSuffix(this.Suffix, csvType.Suffix);

            this.DecimalType &= csvType.DecimalType;
            if (this.DataType == ColumnType.Decimal && this.DecimalType == DecimalTypes.None) this.DataType = ColumnType.String;
            if (this.DataType == ColumnType.Integer && csvType.DataType == ColumnType.Integer)
            {
                if (csvType.MaxInteger > this.MaxInteger) this.MaxInteger = csvType.MaxInteger;
                if (csvType.MinInteger < this.MinInteger) this.MinInteger = csvType.MinInteger;
            }
#if HISTOGRAM
            DistinctCounts = DistinctCounts ?? new Dictionary<string, int>(10) {{CreationString, 1}};
            if (csvType.DistinctCounts != null)
            {
                // not gonna happen
                throw new NotImplementedException();
                ValuesAnalyzed += csvType.ValuesAnalyzed;
            }

            if (DistinctCounts.Count < 10)
                DistinctCounts.Increase(csvType.CreationString);
#elif COMMONONLY
            if (!this._tooManyStrings)
            {
                int i;
                for (i = 0; i < 10 && csvType._commonStrings[0] != this._commonStrings[i]; i++)
                    if (this._commonStrings[i] == null)
                    {
                        this._commonStrings[i] = csvType._commonStrings[0];
                        break;
                    }

                if (i == 10) this._tooManyStrings = true;
            }
#else
            if (IsSingleValue && (!csvType.IsSingleValue || csvType.CreationString != CreationString))
                IsSingleValue = false;
#endif
            this.ValuesAnalyzed++;
        }

        public IntegerTypes SmallestPossibleIntegerType()
        {
            if (this.MinInteger >= 0)
            {
                if (this.MaxInteger <= 1)
                    return IntegerTypes.Bit;
                if (this.MaxInteger < 256)
                    return IntegerTypes.UInt8;
            }
            if (this.MinInteger >= -32768 && this.MaxInteger <= 32767)
                return IntegerTypes.Int16;
            if (this.MinInteger >= -2147483648 && this.MaxInteger <= 2147483647)
                return IntegerTypes.Int32;
            return IntegerTypes.Int64;
        }

        public bool FitsIn(CsvColumnAnalyzer other)
        {
            if (this.DataType > other.DataType) return false;
            if (this.Nullable && other.Nullable == false) return false;
            if (this.DataType == ColumnType.String
                && other.DataType == ColumnType.String
                && Math.Min(this.MaxSize, 4000) > other.MaxSize) return false; // The 4000 is MSSQL-specific, but meh
            if (this.DataType == ColumnType.Integer
                && other.DataType == ColumnType.Integer
                && this.SmallestPossibleIntegerType() > other.SmallestPossibleIntegerType()) return false;

            return true;
        }

        public override string ToString()
        {
            var ret = $"{this.DataType}({this.MaxSize})";
            if (this.IsSingleValue)
                return $"{ret} '{this.CreationString}'";

            if (this.Prefix.Length > 0) ret += $" prefix:\"{this.Prefix}\"";
            if (this.Suffix.Length > 0) ret += $" suffix:\"{this.Suffix}\"";

#if HISTOGRAM
            var twentypercent = ValuesAnalyzed / 5 + 1;
            var commonValues = DistinctCounts.Where(x => x.Value > twentypercent).ToList();
            if (commonValues.Any()) ret += commonValues.Select(x => $"{x.Key}#{x.Value}").JoinStrings(", ", " [", "]");
#endif
            return ret;
        }

        public CsvColumnAnalyzer Clone()
        {
            var clone =(CsvColumnAnalyzer) this.MemberwiseClone();
#if HISTOGRAM
            clone.DistinctCounts = new Dictionary<string, int>(this.DistinctCounts);
#elif COMMONONLY
            clone._commonStrings = new string[10];
            Array.Copy(this._commonStrings, clone._commonStrings, 10);
#endif
            return clone;
        }

        public bool IsSignificantlyDifferent(CsvColumnAnalyzer head)
        {
            if (head.DataType == ColumnType.String && this.DataType != ColumnType.String)
                return true;

            if (head.DataType == ColumnType.Empty && this.Nullable)
                return false;

            if (head.IsNumeric && this.IsNumeric)
                return false;

            if (this.IsSingleValue)
                return head.CreationString != this.CreationString;

            if (this.Prefix.Length > 0)
                return !head.CreationString.StartsWith(this.Prefix);

            if (this.Suffix.Length > 0)
                return !head.CreationString.EndsWith(this.Suffix);

#if HISTOGRAM
            if (IsOnlyCommonValues)
                return DistinctCounts.ContainsKey(head.CreationString);
#elif COMMONONLY
            if (!this._tooManyStrings && this.ValuesAnalyzed > 30 && !this._commonStrings.Contains(head._commonStrings[0]))
                return true;
#endif
            if (this.MaxSize < 10 && head.MaxSize > 20)
                return true;

            if (this.MinSize > 20 && head.MaxSize < 10)
                return true;

            return false;
        }
    }
}