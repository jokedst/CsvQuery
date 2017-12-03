//#define HISTOGRAM
#define COMMONONLY

namespace CsvQuery.Csv
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Tools;

    [Flags]
    public enum DecimalTypes : byte
    {
        None = 0,
        Invariant = 1,
        Comma = 2,
        Local = 4,
        Any = Invariant | Comma | Local
    }

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
#if COMMONONLY
        public string CreationString
        {
            get => _commonStrings[0];
            set => _commonStrings[0] = value;
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
        private readonly string[] _commonStrings = new string[10];
        private bool _tooManyStrings;
#else
        public bool IsSingleValue = true;
#endif

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
            CreationString = Prefix = Suffix = csvText;

            if (string.IsNullOrWhiteSpace(csvText))
            {
                DataType = ColumnType.Empty;
                Nullable = true;
                return;
            }
            MinSize = MaxSize = csvText.Length;

            // TryParse is the bottleneck in this code, so only use when necessary
            if (updatedType <= ColumnType.Integer && long.TryParse(csvText, out var iout))
                if (!Main.Settings.ConvertInitialZerosToNumber && csvText.StartsWith("0") && csvText.Length > 1
                    || Main.Settings.MaxIntegerStringLength < csvText.Length)
                    DataType = ColumnType.String;
                else
                {
                    DataType = ColumnType.Integer;
                    MinInteger = MaxInteger = iout;
                }
            else if (updatedType<=ColumnType.Decimal && IsDecimal(csvText, updatedDecimalType))
                DataType = ColumnType.Decimal;
            else
                DataType = ColumnType.String;
        }

#if HISTOGRAM
        public bool IsSingleValue => DistinctCounts == null || DistinctCounts.Count == 1;

        public bool IsOnlyCommonValues => ValuesAnalyzed > 30 && DistinctCounts.Count<10 && DistinctCounts.All(x => x.Value > ValuesAnalyzed / 8);
#elif COMMONONLY
        public bool IsSingleValue => _commonStrings[1] == null;
#endif
        public bool IsNumeric => this.DataType == ColumnType.Integer || this.DataType == ColumnType.Decimal;

        private bool IsDecimal(string text, DecimalTypes updatedDecimalType)
        {
            // Try invariant, comma and user local settings
            if (updatedDecimalType.HasFlag( DecimalTypes.Invariant) && decimal.TryParse(text, NumberStyles.Any, NumberFormatInfo.InvariantInfo, out _))
            {
                DecimalType = DecimalTypes.Invariant;
                return true;
            }
            if (updatedDecimalType.HasFlag(DecimalTypes.Comma) && decimal.TryParse(text, NumberStyles.Any, CommaDecimal, out _))
            {
                DecimalType = DecimalTypes.Comma;
                return true;
            }
            if (updatedDecimalType.HasFlag(DecimalTypes.Local) && decimal.TryParse(text, NumberStyles.Any, NumberFormatInfo.CurrentInfo,
                out _))
            {
                DecimalType = DecimalTypes.Local;
                return true;
            }
            return false;
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
            {
                if (DataType <= ColumnType.Integer && long.TryParse(csvText, out var iout))
                    if (!Main.Settings.ConvertInitialZerosToNumber && csvText.StartsWith("0") && csvText.Length > 1
                        || Main.Settings.MaxIntegerStringLength < csvText.Length)
                        DataType = ColumnType.String;
                    else if (DataType == ColumnType.Integer)
                    {
                        MinInteger = Math.Min(MinInteger, iout);
                        MaxInteger = Math.Max(MaxInteger, iout);
                    }
                    else
                    {
                        MinInteger = MaxInteger = iout;
                        DataType = ColumnType.Integer;
                    }
                else if (DataType <= ColumnType.Decimal && IsDecimal(csvText, DecimalType))
                    DataType = ColumnType.Decimal;
                else
                    DataType = ColumnType.String;
            }
            
            MaxSize = Math.Max(MaxSize, csvText.Length);
            MinSize = Math.Min(MinSize, csvText.Length);
            Nullable = Nullable || csvTypeNullable;

            Prefix = Extensions.CommonPrefix(Prefix, csvText);
            Suffix = Extensions.CommonSuffix(Suffix, csvText);


#if HISTOGRAM
            if (DistinctCounts == null) 
                DistinctCounts =  new Dictionary<string, int>(10) { { CreationString, 1 } };

            if (DistinctCounts.Count < 10)
                DistinctCounts.Increase(csvText);
#elif COMMONONLY
            if (!_tooManyStrings)
            {
                int i;
                for (i = 0; i < 10 && csvText!=_commonStrings[i]; i++)
                {
                    if (_commonStrings[i] == null)
                    {
                        _commonStrings[i] = csvText;
                        break;
                    }
                }
                if (i == 10) _tooManyStrings = true;
            }
#else
            if (IsSingleValue && csvText != CreationString)
                IsSingleValue = false;
#endif
            ValuesAnalyzed++;
        }

        public void Update(CsvColumnAnalyzer csvType)
        {
            DataType = csvType.DataType > DataType ? csvType.DataType : DataType;
            MaxSize = Math.Max(MaxSize, csvType.MaxSize);
            MinSize = Math.Min(MinSize, csvType.MinSize);
            Nullable = Nullable || csvType.Nullable;

            Prefix = Extensions.CommonPrefix(Prefix, csvType.Prefix);
            Suffix = Extensions.CommonSuffix(Suffix, csvType.Suffix);

            DecimalType &= csvType.DecimalType;
            if (DataType == ColumnType.Decimal && DecimalType == DecimalTypes.None)
                DataType = ColumnType.String;
            if (DataType == ColumnType.Integer && csvType.DataType == ColumnType.Integer)
            {
                if (csvType.MaxInteger > MaxInteger) MaxInteger = csvType.MaxInteger;
                if (csvType.MinInteger < MinInteger) MinInteger = csvType.MinInteger;
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
            if (!_tooManyStrings)
            {
                int i;
                for (i = 0; i < 10 && csvType._commonStrings[0] != _commonStrings[i]; i++)
                {
                    if (_commonStrings[i] == null)
                    {
                        _commonStrings[i] = csvType._commonStrings[0];
                        break;
                    }
                }
                if (i == 10) _tooManyStrings = true;
            }
#else
            if (IsSingleValue && (!csvType.IsSingleValue || csvType.CreationString != CreationString))
                IsSingleValue = false;
#endif
            ValuesAnalyzed++;
        }

        public override string ToString()
        {
            var ret = $"{DataType}({MaxSize})";
            if (IsSingleValue)
                return $"{ret} '{CreationString}'";

            if (Prefix.Length > 0) ret += $" prefix:\"{Prefix}\"";
            if (Suffix.Length > 0) ret += $" suffix:\"{Suffix}\"";

#if HISTOGRAM
            var twentypercent = ValuesAnalyzed / 5 + 1;
            var commonValues = DistinctCounts.Where(x => x.Value > twentypercent).ToList();
            if (commonValues.Any()) ret += commonValues.Select(x => $"{x.Key}#{x.Value}").JoinStrings(", ", " [", "]");
#endif
            return ret;
        }
        
        public bool IsSignificantlyDifferent(CsvColumnAnalyzer head)
        {
            if (head.DataType == ColumnType.String && DataType != ColumnType.String)
                return true;

            if (head.DataType == ColumnType.Empty && this.Nullable)
                return false;

            if (head.IsNumeric && this.IsNumeric)
                return false;

            if (IsSingleValue)
                return head.CreationString != CreationString;

            if (Prefix.Length > 0)
                return !head.CreationString.StartsWith(Prefix);

            if (Suffix.Length > 0)
                return !head.CreationString.EndsWith(Suffix);

#if HISTOGRAM
            if (IsOnlyCommonValues)
                return DistinctCounts.ContainsKey(head.CreationString);
#elif COMMONONLY
            if (!_tooManyStrings && ValuesAnalyzed > 30 && !_commonStrings.Contains(head._commonStrings[0]))
                return true;
#endif
            if (MaxSize < 10 && head.MaxSize > 20)
                return true;

            if (MinSize > 20 && head.MaxSize < 10)
                return true;

            return false;
        }
    }
}