namespace CsvQuery.Database
{
    using System;

    public class DataStorageException : Exception
    {
        public DataStorageException(string message): base(message) { }
        public DataStorageException(string message, Exception innerException) : base(message, innerException) { }
    }
}
