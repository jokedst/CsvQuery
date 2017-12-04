namespace Tests
{
    using CsvQuery;
    using CsvQuery.Database;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class MsSqlProviderFacts
    {
        [TestMethod]
        public void TestConnectionWorks()
        {
            var db = new MssqlDataStorage("CsvQueryTest");

            db.TestConnection();
        }
    }
}