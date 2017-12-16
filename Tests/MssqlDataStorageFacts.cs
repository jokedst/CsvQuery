namespace Tests
{
    using System.Data.SqlClient;
    using System.Diagnostics;
    using CsvQuery;
    using CsvQuery.Database;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

   // [TestClass]
    public class MssqlDataStorageFacts : DataStorageFacts
    {
        public override IDataStorage DataStorage { get; } = new MssqlDataStorage("CsvQueryTest");

        [TestMethod]
        [ExpectedException(typeof(SqlException))]
        public void CanHandleCpQuery()
        {
            // This should throw an SQliteException
            DataStorage.ExecuteQuery("cp", true);
        }
    }

   // [TestClass]
    public class MssqlWithTypesDataStorageFacts : DataStorageFacts
    {
        public override IDataStorage DataStorage { get; } = new MssqlDataStorage("CsvQueryTest");

        [TestInitialize]
        public void ActivateTypes()
        {
            Main.Settings.DetectDbColumnTypes = true;
            Trace.TraceInformation("Activated type system");
        }
    }
}