namespace Tests
{
    using Community.CsharpSqlite;
    using CsvQuery;
    using CsvQuery.Database;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class SqliteProviderFacts : DataStorageFacts
    {
        public override IDataStorage DataStorage { get; } = new SQLiteDataStorage();

        [TestMethod]
        public void AffinityTest()
        {
            const string create = "CREATE TABLE affinity ( " +
                                  " t  TEXT,      " + //-- text affinity by rule 2
                                  " nu NUMERIC,   " + //-- numeric affinity by rule 5
                                  " i  INTEGER,   " + //-- integer affinity by rule 1
                                  " r  REAL,      " + //-- real affinity by rule 4
                                  " no BLOB       " + //-- no affinity by rule 3
                                  ");";

            DataStorage.ExecuteNonQuery(create);

            DataStorage.ExecuteNonQuery(
                "INSERT INTO affinity VALUES('500.01', '500.01', '500.01', '500.01', '500.01')");
            var res = DataStorage.ExecuteQuery(
                "SELECT typeof(t), typeof(nu), typeof(i), typeof(r), typeof(no) FROM affinity", false);

            var data = DataStorage.ExecuteQuery("SELECT * FROM Affinity", false);

            // This doesn't actually test anything, it's just to check how sqlite affinity works...
        }

        [TestMethod]
        [ExpectedException(typeof(SQliteException))]
        public void CanHandleCpQuery()
        {
            // This should throw an SQliteException
            DataStorage.ExecuteQuery("cp", true);
        }
    }
}