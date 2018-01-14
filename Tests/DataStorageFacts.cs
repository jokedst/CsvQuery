using System.IO;
using System.Text;

namespace Tests
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Threading;
    using CsvQuery;
    using CsvQuery.Csv;
    using CsvQuery.Database;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    public abstract class DataStorageFacts : TestBaseClass
    {
        [TestInitialize]
        public void InitializeDataStorageFacts()
        {
            ((DataStorageBase) DataStorage).SetLastCreatedTableName(TestCount);
        }

        public abstract IDataStorage DataStorage { get; }

        protected List<string[]> FourRowsThreeColumnsWithHeader = new List<string[]>
        {
            new[] {"hej", "du", "där"},
            new[] {"1", "2", "3"},
            new[] {"2", "12", "14"},
            new[] {"3", "12", "13"},
            new[] {"4", "2", "3"}
        };
        protected List<string[]> FiveRowsFourColumnsWithNoHeader = new List<string[]>
        {
            new[] {"first","1", "2", "3"},
            new[] {"second","2", "12", "14"},
            new[] {"third","3", "12", "13"},
            new[] {"fourth","4", "3.14", "3"},
            new[] {"fifth","5", "2", "3"}
        };

        [TestMethod]
        public void SaveToDbAndReadItBack()
        {
            var data = new List<string[]>
            {
                new[] {"hej", "du", "där"},
                new[] {"1", "2", "3"},
                new[] {"2", "12", "14"},
                new[] {"3", "12", "13"},
                new[] {"4", "2", "3"}
            };
            var ctypes = new CsvColumnTypes(data, new CsvSettings{HasHeader = false});

            var tableName = DataStorage.SaveData(new IntPtr(10), data, ctypes);

            var result = DataStorage.ExecuteQuery("SELECT * FROM " + tableName, false);

            AssertDataEqual(data, result);
        }

        [TestMethod]
        public void SaveDecimalsToDbAndReadItBack()
        {
            Thread.CurrentThread.CurrentCulture  = CultureInfo.CreateSpecificCulture("en-US");
            var data = new List<string[]>
            {
                new[] {"hej", "du", "där"},
                new[] {"1", "2,44", "3"},
                new[] {"2", "12,300", "14"},
                new[] {"3", "12,000", "13"},
                new[] {"4", "2,010", "3"}
            };
            var ctypes = new CsvColumnTypes(data, new CsvSettings { HasHeader = true });

            var tableName = DataStorage.SaveData(new IntPtr(10), data, ctypes);

            var result = DataStorage.ExecuteQuery("SELECT * FROM " + tableName, true);

            AssertDataEqualAfterTypeConversion(data, result);
        }

        [TestMethod]
        public void CanSelectFromThis()
        {
            var data = new List<string[]>
            {
                new[] {"hej", "du", "där"},
                new[] {"1", "2", "3"},
                new[] {"2", "12", "14"},
                new[] {"3", "12", "13"},
                new[] {"4", "2", "3"}
            };

            var tableName = DataStorage.SaveData(new IntPtr(11), data, new CsvColumnTypes(data, new CsvSettings { HasHeader = false }));
            DataStorage.SetActiveTab(new IntPtr(11));

            var result = DataStorage.ExecuteQuery("SELECT * FROM this", false);

            AssertDataEqual(data, result);
        }

        [TestMethod]
        public void CanSelectFromThisWithHeaders()
        {
            var data = new List<string[]>
            {
                new[] {"hej", "du", "all"},
                new[] {"1", "2", "3"},
                new[] {"2", "12", "14"},
                new[] {"3", "12", "13"},
                new[] {"4", "2", "3"}
            };

            DataStorage.SaveData(new IntPtr(11), data, new CsvColumnTypes(data, new CsvSettings { HasHeader = true }));
            DataStorage.SetActiveTab(new IntPtr(11));

            var result = DataStorage.ExecuteQuery("SELECT * FROM this", true);

            AssertDataEqual(data, result);
        }

        [TestMethod]
        public void CanDetectHeaders()
        {
            var data = new List<string[]>
            {
                new[] {"hej", "du", "där"},
                new[] {"1", "2", "3"},
                new[] {"2", "12", "14"},
                new[] {"3", "12", "13"},
                new[] {"4", "2", "3"}
            };

            DataStorage.SaveData(new IntPtr(11), data, new CsvColumnTypes(data, null));
            DataStorage.SetActiveTab(new IntPtr(11));

            var result = DataStorage.ExecuteQuery("SELECT * FROM this", true);

            AssertDataEqual(data, result);
        }

        [TestMethod]
        public void CanHandleOneRowWithMoreData()
        {
            var data = new List<string[]>
            {
                new[] {"hej", "du", "där"},
                new[] {"1", "2", "3"},
                new[] {"2", "12", "14", "extra"},
                new[] {"3", "12", "13"},
                new[] {"4", "2", "3"}
            };

            var columnTypes = new CsvColumnTypes(data, null);
            DataStorage.SaveData(new IntPtr(11), data, columnTypes);
            DataStorage.SetActiveTab(new IntPtr(11));

            var result = DataStorage.ExecuteQuery("SELECT * FROM this", true);

            Assert.AreEqual(5, result.Count);
            Assert.IsTrue(result.TrueForAll(x => x.Length == 4));
        }

        [TestMethod]
        public void CanHandleNoHeaderDetectionWithDifferentRowLengths()
        {
            var data = new List<string[]>
            {
                new[] {"5", "7", "34"},
                new[] {"1", "2", "3", "12.2"},
                new[] {"2", "12", "14"},
                new[] {"3", "12", "13"},
                new[] {"4", "2", "3"}
            };

            var columnTypes = new CsvColumnTypes(data, null);
            DataStorage.SaveData(new IntPtr(11), data, columnTypes);
            DataStorage.SetActiveTab(new IntPtr(11));

            var result = DataStorage.ExecuteQuery("SELECT * FROM this", true);

            Assert.AreEqual(6, result.Count);
            Assert.AreEqual(result[0][3], "Col4");
            Assert.IsNull(result[1][3]);
            Assert.IsNotNull(result[2][3]);
            Assert.IsNull(result[3][3]);
        }

        [TestMethod]
        public void CanDetectNoHeaders()
        {
            var data = new List<string[]>
            {
                new[] {"5", "7", "34"},
                new[] {"1", "2", "3"},
                new[] {"2", "12", "14"},
                new[] {"3", "12", "13"},
                new[] {"4", "2", "3"}
            };

            DataStorage.SaveData(new IntPtr(11), data, new CsvColumnTypes(data, null));
            DataStorage.SetActiveTab(new IntPtr(11));

            var result = DataStorage.ExecuteQuery("SELECT * FROM this", false);

            AssertDataEqual(data, result);
        }

        [TestMethod]
        public void CanDetectHeadersInBigFile()
        {
            var data = new List<string[]>
            {
                new[]
                {
                    "Art.Grp", "Artikelgruppbeskrivning", "Artnr", "Beställningsvara", "Avtalsartikel", "Varubenämning",
                    "Levartnr", "Enhet", "Minikvant", "Antalenh", "Bruttovikt", "Jmf enh vikt", "Nettovikt", "Pris",
                    "Enhet", "Jmf pris", "Jmf enh vikt", "Pris frp", "Pris delat", "DP", "Offertgrupp",
                    "Offertgruppsbeskrivning", "Pos", "", ""
                },
                new[]
                {
                    "103", "Potatis mat", "629477", "", "J", "POTATIS LUNCH TV 10 KG    FAR", "FAR32064", "SÄCK",
                    "1,00", "1", "1,00", "KG", "", "65,02", "SÄCK", "6,50", "KG", "", "", "", "031", "POTATIS", "03103"
                },
                new[]
                {
                    "103", "Potatis mat", "314781", "B", "J", "POTATIS 52-60     10KG     FAR", "FAR31060", "SÄCK",
                    "1,00", "1", "1,00", "KG", "", "43,12", "SÄCK", "4,31", "KG", "", "", "", "031", "POTATIS", "03101"
                },
                new[]
                {
                    "104", "Potatis bak", "330027", "", "J", "BAKPOTATIS 250-300G 10KG   FAR", "FAR34530", "SÄCK",
                    "1,00", "1", "1,00", "KG", "", "67,66", "SÄCK", "6,77", "KG", "", "", "", "031", "POTATIS", "03104"
                },
                new[]
                {
                    "201", "Kött nöt helt obeh import   fv", "387142", "", "J", "HÖGREV DE KY      4/13KG   MMK",
                    "MMK1687", "KG", "1,00", "1", "1,000", "KG", "", "49,47", "KG", "49,47", "KG", "", "", "", "029",
                    "FÄRSK KÖTT-FÄRSER", "02921"
                },
                new[]
                {
                    "201", "Kött nöt helt obeh import   fv", "388165", "", "J", "INNANLÅR DE       3,5/11KG MMK",
                    "MMK1755", "KG", "1,00", "1", "1,000", "KG", "", "60,03", "KG", "60,03", "KG", "", "", "", "029",
                    "FÄRSK KÖTT-FÄRSER", "02922"
                },
                new[]
                {
                    "201", "Kött nöt helt obeh import   fv", "388157", "B", "J", "YTTERLÅR DE       4/20KG   MMK",
                    "MMK1752", "KG", "1,00", "1", "1,000", "KG", "", "51,90", "KG", "51,90", "KG", "", "", "", "029",
                    "FÄRSK KÖTT-FÄRSER", "02923"
                },
                new[]
                {
                    "202", "Kött fläsk helt obeh svensktfv", "589705", "", "J", "SKINKSTEK BFR NÄT 3KG      GUD",
                    "GUD1927", "KG", "1,00", "1", "1,000", "KG", "", "46,17", "KG", "46,17", "KG", "", "", "", "029",
                    "FÄRSK KÖTT-FÄRSER", "02913"
                },
                new[]
                {
                    "203", "Kött fläsk helt obeh import fv", "124560", "", "J", "FLÄSKFILE U HUV DE KY 1/15KIMP",
                    "SER42273", "KG", "1,00", "1", "1,000", "KG", "", "77,06", "KG", "77,06", "KG", "", "", "", "029",
                    "FÄRSK KÖTT-FÄRSER", "02916"
                },
                new[]
                {
                    "203", "Kött fläsk helt obeh import fv", "124578", "", "J", "FLÄSKKARRE BF DE KY 2,5/15KIMP",
                    "SER42252", "KG", "1,00", "1", "1,000", "KG", "", "42,77", "KG", "42,77", "KG", "", "", "", "029",
                    "FÄRSK KÖTT-FÄRSER", "02907"
                },
                new[]
                {
                    "203", "Kött fläsk helt obeh import fv", "124586", "", "J", "FLÄSKKOTL BF 1606 3,5/17,5KIMP",
                    "SER42211", "KG", "1,00", "1", "1,000", "KG", "", "45,26", "KG", "45,26", "KG", "", "", "", "029",
                    "FÄRSK KÖTT-FÄRSER", "02909"
                },
                new[]
                {
                    "204", "Kött övr lamm vilt          fv", "494096", "B", "J", "LAMMSTEK M BEN  2,5KG      SBU",
                    "SBU4134", "KG", "1,00", "", "1,000", "KG", "", "99,49", "KG", "99,49", "KG", "", "", "", "029",
                    "FÄRSK KÖTT-FÄRSER", "02924"
                },
                new[]
                {
                    "204", "Kött övr lamm vilt          fv", "571760", "B", "J", "LAMMYTTERFILE M KAPPA 2,5KGSBU",
                    "SBU4196", "KG", "1,00", "1", "1,000", "KG", "", "273,77", "KG", "273,77", "KG", "", "", "", "029",
                    "FÄRSK KÖTT-FÄRSER", "02925"
                },
                new[]
                {
                    "205", "Kött kalv                   fv", "216358", "B", "J", "KALVBOG/RYGG BF SE4-5KG    SBU",
                    "SBU3141/3126", "KG", "1,00", "1", "1,000", "KG", "", "64,28", "KG", "64,28", "KG", "", "", "",
                    "029", "FÄRSK KÖTT-FÄRSER", "02912"
                },
                new[]
                {
                    "208", "Fläsk tärn skiv strim       fv", "604900", "B", "J", "FLÄSKBOG MAR TÄ DE KY 2,5KGSIG",
                    "SIG2024", "KG", "1,00", "10", "1,000", "KG", "", "35,96", "KG", "35,96", "KG", "", "", "", "029",
                    "FÄRSK KÖTT-FÄRSER", "02911"
                },
                new[]
                {
                    "209", "Färs av nöt fläsk och övr   fv", "619734", "", "J", "BLANDFÄRS 50/50 NL KY 2,5/5IMP",
                    "FÅD570051", "KG", "1,00", "1", "1,000", "KG", "", "46,48", "KG", "46,48", "KG", "", "", "", "029",
                    "FÄRSK KÖTT-FÄRSER", "02901"
                },
                new[]
                {
                    "209", "Färs av nöt fläsk och övr   fv", "434449", "", "J", "BLANDFÄRS 50/50 15% 2,5KG  SCA",
                    "SCA426193", "ST", "1,00", "4", "2,500", "KG", "", "136,74", "ST", "54,70", "KG", "", "", "", "029",
                    "FÄRSK KÖTT-FÄRSER", "02901"
                },
                new[]
                {
                    "209", "Färs av nöt fläsk och övr   fv", "187096", "B", "J", "LAMMFÄRS 13-15% SE KY 2,5KGSBU",
                    "SBU4158", "KG", "1,00", "1", "1,000", "KG", "", "60,30", "KG", "60,30", "KG", "", "", "", "029",
                    "FÄRSK KÖTT-FÄRSER", "02906"
                },
                new[]
                {
                    "209", "Färs av nöt fläsk och övr   fv", "616250", "", "J", "NÖTFÄRS KY 10% NL 2,5/5KG  IMP",
                    "FÅD560050", "KG", "1,00", "1", "1,000", "KG", "", "53,44", "KG", "53,44", "KG", "", "", "", "029",
                    "FÄRSK KÖTT-FÄRSER", "02903"
                },
                new[]
                {
                    "209", "Färs av nöt fläsk och övr   fv", "616268", "B", "J", "NÖTFÄRS 10% KRAV2,5/5KG   SBU",
                    "SBU1358", "KG", "1,00", "1", "1,000", "KG", "", "70,95", "KG", "70,95", "KG", "", "", "", "029",
                    "FÄRSK KÖTT-FÄRSER", "02902"
                },
                new[]
                {
                    "211", "Skinka kokt rökt hel press rim", "062927", "", "J", "FATBURSKINKA ILÅR 3/10KG   SBU",
                    "SBU5286", "KG", "1,00", "1", "1,000", "KG", "", "53,94", "KG", "53,94", "KG", "", "", "", "033",
                    "Konverterad: beskriving saknas", "03314"
                },
                new[]
                {
                    "211", "Skinka kokt rökt hel press rim", "148999", "B", "J", "SKINKA JUL RI 3D MSVGOTL3KGSBU",
                    "SBU5255", "KG", "1,00", "1", "1,000", "KG", "", "44,87", "KG", "44,87", "KG", "", "", "", "029",
                    "FÄRSK KÖTT-FÄRSER", "02928"
                }
            };

            DataStorage.SaveData(new IntPtr(11), data, new CsvColumnTypes(data, null));
            DataStorage.SetActiveTab(new IntPtr(11));

            var result = DataStorage.ExecuteQuery("SELECT * FROM this", true);

            //AssertDataEqual(data.Skip(1).ToList(), result.Skip(1).ToList());
            Assert.AreEqual("ArtGrp", result[0][0], "whitelisting not working");
        }

        [TestMethod]
        public void CanOverwriteExistingTable()
        {
            DataStorage.SaveData(new IntPtr(77), FourRowsThreeColumnsWithHeader, new CsvColumnTypes(FourRowsThreeColumnsWithHeader, new CsvSettings{HasHeader = true}));
            DataStorage.SetActiveTab(new IntPtr(77));

            var result = DataStorage.ExecuteQuery("SELECT * FROM this", false);
            Assert.AreEqual(4, result.Count);
            Assert.IsTrue(result.TrueForAll(x => x.Length == 3));

            DataStorage.SaveData(new IntPtr(77), FiveRowsFourColumnsWithNoHeader, new CsvColumnTypes(FiveRowsFourColumnsWithNoHeader, new CsvSettings{HasHeader = false}));
            DataStorage.SetActiveTab(new IntPtr(77));

            result = DataStorage.ExecuteQuery("SELECT * FROM this", false);
            Assert.AreEqual(5, result.Count);
            Assert.IsTrue(result.TrueForAll(x => x.Length == 4));
        }

        [TestMethod]
        public void Can_generate_csv()
        {
            var data = new List<string[]>
            {
                new[] {"A number", "another number here", "#¤%i"},
                new[] {"1", "2", "3"},
                new[] {"2", "12", "14"},
                new[] {"3", "12", "13"},
                new[] {"4", "2", "3"}
            };
            var csvSettings = new CsvSettings(',') { HasHeader = true };
            var ctypes = new CsvColumnTypes(data, csvSettings);

            var tableName = DataStorage.SaveData(new IntPtr(10), data, ctypes);

            var result = DataStorage.ExecuteQueryToDataTable("SELECT * FROM " + tableName, new IntPtr(10));
            Assert.AreEqual("Anumber", result.Columns[0].ColumnName);

            string csv;
            using (var memStream = new MemoryStream())
            {
                var lookup = DataStorage.GetUnsafeColumnMaps(new IntPtr(10));
                csvSettings.GenerateToStream(result, memStream, lookup);
                csv= Encoding.UTF8.GetString(memStream.GetBuffer());
            }
            Assert.IsTrue(csv.StartsWith("A number,another number here,#¤%i"));
        }
    }
}