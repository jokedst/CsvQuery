namespace CsvQuery.Csv
{
    /// <summary>
    /// Type of data in a column. Higher values can always include lower (i.e. a decimal column can have an integer, but not the other way)
    /// </summary>
    public enum ColumnType
    {
        Empty=0,
        Integer=1,
        Decimal=2,
        String=4
    }
}