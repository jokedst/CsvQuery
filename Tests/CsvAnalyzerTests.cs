namespace Tests
{
    using CsvQuery.Csv;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class CsvAnalyzerTests
    {
        [TestMethod]
        public void CanFindCommaAsSeparator()
        {
            var csvData = "header1,header2,header2\n" 
                + "data1,123,12.34\n" 
                + "data1,123,12.34\n" 
                + "data1,123,12.34\n"
                + "data1,123,12.34\n" 
                + "data1,123,12.34\n";
            var result = CsvAnalyzer.Analyze(csvData);
            Assert.AreEqual(',',result.Separator);
        }

        [TestMethod]
        public void CanFindSemicolonInBigFile()
        {
            var csvData = Helpers.GetResource("Avtalsprislista992220_20140623-112840802.csv");
            var result = CsvAnalyzer.Analyze(csvData);
            Assert.AreEqual(';', result.Separator);
        }
    }
}
