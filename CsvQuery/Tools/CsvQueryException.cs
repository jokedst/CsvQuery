namespace CsvQuery.Tools
{
    using System;

    public class CsvQueryException : Exception
    {
        /// <summary>
        /// 
        /// </summary>
        /// <inheritdoc />
        public CsvQueryException(string message) : base(message) { }
        /// <inheritdoc />
        public CsvQueryException(string message, Exception innerException) : base(message, innerException) { }
    }
}
