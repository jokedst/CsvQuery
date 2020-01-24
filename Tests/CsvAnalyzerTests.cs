namespace Tests
{
    using System.IO;
    using CsvQuery;
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
        public void CanFindColonInQuotedFile()
        {
            var data = File.ReadAllText(@"TestFiles\QuotedWithSeparator.csv");
            var result = CsvAnalyzer.Analyze(data);
            Assert.AreEqual(',', result.Separator);
        }

        [TestMethod]
        public void CanHandleQuotesInText()
        {
            var data = File.ReadAllText(@"TestFiles\TabsWithQuotesInText.csv");
            var result = CsvAnalyzer.Analyze(data);
            Assert.AreEqual('\t', result.Separator);
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

        [TestMethod]
        public void Issue10QuotedCommas()
        {
            var data =
                "\"ControlID\",\"Status\",\"FeatureName\",\"ResourceGroupName\",\"ResourceName\",\"ChildResourceName\",\"ControlSeverity\",\"IsBaselineControl\",\"SupportsAutoFix\",\"Description\",\"Recommendation\",\"DetailedLogFile\"\r\n\"test123\",\"test123\",\"test123\",\"test123\",\"test123\",\"test123\",\"test123\",\"test123\",\"test123\",\"test123\",\"comma goes here, \",\"test123\"";

            var result = CsvAnalyzer.Analyze(data);

            Assert.AreEqual(',', result.Separator);
            Assert.IsTrue(result.UseQuotes);
        }

        [TestMethod]
        public void Issue10UnquotedStringsWithQuotes()
        {
            var data = "artnr, description, cost\n"
                     + "B12332, a 9\" nail, 123.32\n"
                       + "C12322, Screw, 22.1\n";
            
            var result = CsvAnalyzer.Analyze(data);

            Assert.AreEqual(',', result.Separator);
            Assert.IsFalse(result.UseQuotes);
        }

        [TestMethod]
        public void UnquotedStringsWithQuotesOnEveryRow()
        {
            var data = "artnr, description, cost\n"
                       + "B12332, a 9\" nail, 123.32\n"
                       + "C12322, TV 30\", 123.32\n"
                       + "C1222, Samsung 7\", 4000.32\n"
                       + "C3P0, Cyborg 90\", 5.33\n";

            var result = CsvAnalyzer.Analyze(data);

            Assert.AreEqual(',', result.Separator);
            Assert.IsFalse(result.UseQuotes);
        }

        [TestMethod]
        public void Can_detect_xml_file()
        {
            Main.Settings.ParseXmlFiles = true;
            var data = File.ReadAllText("TestFiles\\simple.xml");

            var result = CsvAnalyzer.Analyze(data);

            Assert.IsTrue(result is XmlSettings);
        }
    }
}
