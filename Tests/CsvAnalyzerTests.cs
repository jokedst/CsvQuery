namespace Tests
{
    using System.IO;
    using CsvQuery.Csv;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class CsvAnalyzerFacts
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
            var csvData = Helpers.GetResource("semicolon.csv");
            var result = CsvAnalyzer.Analyze(csvData);
            Assert.AreEqual(';', result.Separator);
        }

        [TestMethod]
        public void CanReadFiles()
        {
            var csvData = File.ReadAllText(@"TestFiles\sentences.csv");
            var result = CsvAnalyzer.Analyze(csvData);
            Assert.AreEqual(',', result.Separator);
        }

        [TestMethod]
        public void CanDetectFixedWidth()
        {
            var csvData = "header1  header2  header2  and4\n"
                        + "data1    123      12.34    qwfw\n"
                        + "more 00  23       2.34     frfrsd\n"
                        + "data3    111      11.34    vrvvv fvf\n";
            var result = CsvAnalyzer.Analyze(csvData);
            Assert.IsNotNull(result.FieldWidths);
            Assert.AreEqual(4, result.FieldWidths.Count);
        }

        [TestMethod]
        public void CanDetectW3C()
        {
            var csvData = File.ReadAllText(@"TestFiles\w3c.log");
            var result = CsvAnalyzer.Analyze(csvData);
            Assert.AreEqual(' ', result.Separator);
            Assert.AreEqual('#', result.CommentCharacter);
        }

        [TestMethod]
        public void CanDetectTabW3C()
        {
            var csvData = File.ReadAllText(@"TestFiles\w3c.log").Replace(' ','\t');
            var result = CsvAnalyzer.Analyze(csvData);
            Assert.AreEqual('\t', result.Separator);
            Assert.AreEqual('#', result.CommentCharacter);
        }
    }
}
