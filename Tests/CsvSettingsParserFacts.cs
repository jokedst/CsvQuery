namespace Tests
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using CsvQuery.Csv;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class CsvSettingsParserFacts : TestBaseClass
    {
        private static List<string[]> _expectedData = new List<string[]>
        {
            new[] {"Title1", "Title2", "Title 3"},
            new[] {"1", "multi-line text, especially in \r\ncsv-files, is evil", "in my opinion"},
            new[] {"2", "multi-line with \"\r\nshould not be allowed really", "I agree,\r\nbut what do I know"},
            new[] {"3", "\"This\" is evil", "a 4\" brick"},
            new[] {" ", " spaces ", "\ttabs\t\t"},
        };

        [TestMethod]
        public void CanParseMultilineFields()
        {
            var settings = new CsvSettings {Separator = ',', UseQuotes = true};

            List<string[]> data;
            using (var sr = new StreamReader(@"TestFiles\multline.csv")) data = settings.Parse(sr).ToList();

            this.AssertDataEqual(_expectedData, data);
        }

        [TestMethod]
        public void CanParseMultilineFieldsParseVB()
        {
            var settings = new CsvSettings { Separator = ',', UseQuotes = true };

            List<string[]> data;
            using (var sr = new StreamReader(@"TestFiles\multline.csv")) data = settings.ParseVB(sr).ToList();

            this.AssertDataEqual(_expectedData, data);
        }

        [TestMethod]
        public void CanParseMultilineFieldsParseStandard()
        {
            var settings = new CsvSettings { Separator = ',', UseQuotes = true };

            List<string[]> data;
            using (var sr = new StreamReader(@"TestFiles\multline.csv")) data = settings.ParseStandard(sr).ToList();

            this.AssertDataEqual(_expectedData, data);
        }

        [TestMethod]
        public void CanParseMultilineFieldsRaw()
        {
            var settings = new CsvSettings {Separator = ',', UseQuotes = true};

            List<string[]> data;
            using (var sr = new StreamReader(@"TestFiles\multline.csv")) data = settings.ParseRaw(sr).ToList();

            this.AssertDataEqual(_expectedData, data);
        }

        [TestMethod]
        public void CanParseMultilineFieldsParseRawBuffered()
        {
            var settings = new CsvSettings {Separator = ',', UseQuotes = true};

            List<string[]> data;
            using (var sr = new StreamReader(@"TestFiles\multline.csv")) data = settings.ParseRawBuffered(sr).ToList();
            
            this.AssertDataEqual(_expectedData, data);
        }

        [TestMethod]
        public void CanParseMultilineFieldsParseCustom()
        {
            var settings = new CsvSettings { Separator = ',', UseQuotes = true };

            List<string[]> data;
            using (var sr = new StreamReader(@"TestFiles\multline.csv")) data = settings.ParseCustom(sr).ToList();

            this.AssertDataEqual(_expectedData, data);
        }
    }
}