namespace Tests
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using CsvQuery;
    using CsvQuery.Csv;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Testing class CsvSettings
    /// </summary>
    [TestClass]
    public class CsvSettingsFacts:TestBaseClass
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }
        

        [TestMethod]
        public void CanParse()
        {
            var csvText = "\"header1\",\"header2\"\n"
                + "\"text\",\"more text\"\n"
                + "\"here, might fail\",\":/\"";
            var set = new CsvSettings {Separator = ',', TextQualifier = '"'};

            // Act
            var data = set.Parse(csvText);

            // Assert
            Assert.AreEqual(3, data.Count);
            Assert.AreEqual(2, data[2].Length);
        }


        [TestMethod] public void CanParseCrlf() => this.CanParseDifferent("\r\n",',',true);
        [TestMethod] public void CanParseLf() => this.CanParseDifferent("\n", ',', true);
        [TestMethod] public void CanParseUnquoted() => this.CanParseDifferent("\n", ';', false);

        public void CanParseDifferent(string newline, char separator, bool quoted)
        {
            var indata = new List<string[]>
            {
                new[] {"A number", "another number here", "#¤%i"},
                new[] {"1", "2g\"m", "3"},
                new[] {"3", "12", "1,3\""},
                new[] {"4", "2", "3"}
            };
            var csvText = string.Join(newline, indata.Select(x => string.Join(separator.ToString(), x.Select(l => quoted ? $"\"{l.Replace("\"", "\"\"")}\"" : l))));
            var set = new CsvSettings {Separator = separator, TextQualifier = quoted ? '"' : default(char), HasHeader = false};

            // Act
            var data = set.Parse(csvText);

            // Assert
            this.AssertDataEqual(indata, data);
        }

        [TestMethod]
        public void CanParseW3c()
        {
            var csvData = File.ReadAllText(@"TestFiles\w3c.log");
            var settings = CsvAnalyzer.Analyze(csvData);

            var data = settings.Parse(csvData);

            Assert.AreEqual(6, data.Count, "Number of rows incorrect");
        }

        [TestMethod]
        public void CanParseXml()
        {
            Main.Settings.ParseXmlFiles = true;
            var csvData = File.ReadAllText(@"TestFiles\simple.xml");
            var settings = CsvAnalyzer.Analyze(csvData);

            var data = settings.Parse(csvData);

            Assert.AreEqual(3, data.Count, "Number of rows incorrect");
            CollectionAssert.AreEquivalent(new[] {"name", "age", "foo"}, settings.FieldNames);
            var expectedData = new List<string[]>
            {
                new[] {"first", "45", "bar"},
                new[] {"second", "46", "bar"},
                new[] {"third", "47", "bar"},
            };
            this.AssertDataEqual(expectedData, data);
        }

        [TestMethod]
        public void CanParseComplexXml()
        {
            Main.Settings.ParseXmlFiles = true;
            var csvData = File.ReadAllText(@"TestFiles\headerWithRows.xml");
            var settings = CsvAnalyzer.Analyze(csvData);

            var data = settings.Parse(csvData);

            Assert.AreEqual(3, data.Count, "Number of rows incorrect");
            CollectionAssert.AreEquivalent(new[] { "name", "age", "foo", "extra" }, settings.FieldNames);
            var expectedData = new List<string[]>
            {
                new[] {"first", "45", "bar", null},
                new[] {"second", "46", "bar", null},
                new[] {"third", "47", "bar", "Read all about it!"},
            };
            this.AssertDataEqual(expectedData, data);
        }

        [TestMethod]
        public void CanParseTabSeparatedWithQuotesInText()
        {
            Main.Settings.ParseXmlFiles = true;
            var csvData = File.ReadAllText(@"TestFiles\TabsWithQuotesInText.csv");
            var settings = CsvAnalyzer.Analyze(csvData);
            settings.TextQualifier = default(char);
            var data = settings.Parse(csvData);

            Assert.AreEqual(6, data.Count, "Number of rows incorrect");
            Assert.AreEqual("\"A4\" 210x325mm utskick, v24, h04poo, nära,7st butiker", data[5][3]);
        }
    }

    [TestClass]
    public class CsvSettingsParserFacts : TestBaseClass
    {
        [TestMethod]
        public void CanParseMultilineFields()
        {
            var settings = new CsvSettings {Separator = ',', TextQualifier = '"', UseQuotes = true};

            List<string[]> data;
            using (var sr = new StreamReader(@"TestFiles\multline.csv")) data = settings.Parse(sr).ToList();

            var expectedData = new List<string[]>
            {
                new[] {"Title1", "Title2", "Title 3"},
                new[] {"1", "multi-line text, especially in \r\ncsv-files, is evil", "in my opinion"},
                new[] {"2", "multi-line with \"\r\nshould not be allowed really", "I agree,\r\nbut what do I know"},
                new[] {"3", "\"This\" is evil", "a 4\" brick"},
            };
            this.AssertDataEqual(expectedData, data);
        }

        [TestMethod]
        public void CanParseMultilineFieldsParseStandard()
        {
            var settings = new CsvSettings { Separator = ',', TextQualifier = '"', UseQuotes = true };

            List<string[]> data;
            using (var sr = new StreamReader(@"TestFiles\multline.csv")) data = settings.ParseStandard(sr).ToList();

            this.AssertDataEqual(new List<string[]>
            {
                new[] {"Title1", "Title2", "Title 3"},
                new[] {"1", "multi-line text, especially in \r\ncsv-files, is evil", "in my opinion"},
                new[] {"2", "multi-line with \"\r\nshould not be allowed really", "I agree,\r\nbut what do I know"},
                new[] {"3", "\"This\" is evil", "a 4\" brick"},
            }, data);
        }

        [TestMethod]
        public void CanParseMultilineFieldsRaw()
        {
            var settings = new CsvSettings {Separator = ',', TextQualifier = '"', UseQuotes = true};

            List<string[]> data;
            using (var sr = new StreamReader(@"TestFiles\multline.csv")) data = settings.ParseRaw(sr).ToList();

            var expectedData = new List<string[]>
            {
                new[] {"Title1", "Title2", "Title 3"},
                new[] {"1", "multi-line text, especially in \r\ncsv-files, is evil", "in my opinion"},
                new[] {"2", "multi-line with \"\r\nshould not be allowed really", "I agree,\r\nbut what do I know"},
                new[] {"3", "\"This\" is evil", "a 4\" brick"},
            };
            this.AssertDataEqual(expectedData, data);
        }

        [TestMethod]
        public void CanParseMultilineFieldsParseRawBuffered()
        {
            var settings = new CsvSettings {Separator = ',', TextQualifier = '"', UseQuotes = true};

            List<string[]> data;
            using (var sr = new StreamReader(@"TestFiles\multline.csv")) data = settings.ParseRawBuffered(sr).ToList();

            this.AssertDataEqual(new List<string[]>
            {
                new[] {"Title1", "Title2", "Title 3"},
                new[] {"1", "multi-line text, especially in \r\ncsv-files, is evil", "in my opinion"},
                new[] {"2", "multi-line with \"\r\nshould not be allowed really", "I agree,\r\nbut what do I know"},
                new[] {"3", "\"This\" is evil", "a 4\" brick"},
            }, data);
        }
    }
}
