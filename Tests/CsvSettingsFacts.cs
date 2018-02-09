namespace Tests
{
    using System.Linq;
    using CsvQuery.Csv;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Testing class CsvSettings
    /// </summary>
    [TestClass]
    public class CsvSettingsFacts
    {
        public CsvSettingsFacts()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void CanParse()
        {
            var csvText = "\"header1\",\"header2\"\n"
                + "\"text\",\"more text\"\n"
                + "\"here, might fail\",\":/\"";
            var set = new CsvSettings {Separator = ',', TextQualifier = '"'};

            // Act
            var data = set.Parse(csvText).ToList();

            // Assert
            Assert.AreEqual(3, data.Count);
            Assert.AreEqual(2, data[2].Length);
        }
    }
}
