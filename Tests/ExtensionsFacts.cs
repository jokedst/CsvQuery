namespace Tests
{
    using System.IO;
    using System.Linq;
    using CsvQuery.Csv;
    using CsvQuery.Tools;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ExtensionsFacts
    {
        [TestMethod]
        public void CommonPrefix()
        {
            Assert.AreEqual("abc", Extensions.CommonPrefix("abcdefgh", "abc"));
            Assert.AreEqual("", Extensions.CommonPrefix("abcdefgh", "qwe"));
            Assert.AreEqual("abc", Extensions.CommonPrefix("abc", "abcdefgh"));
            Assert.AreEqual("", Extensions.CommonPrefix("abcdefgh", null));
            Assert.AreEqual("", Extensions.CommonPrefix("1abc", "2abc"));
            Assert.AreEqual("abc", Extensions.CommonPrefix("abc", "abc"));
            Assert.AreEqual("", Extensions.CommonPrefix("", ""));
        }

        [TestMethod]
        public void CommonSuffix()
        {
            Assert.AreEqual("fgh", Extensions.CommonSuffix("abcdefgh", "fgh"));
            Assert.AreEqual("fgh", Extensions.CommonSuffix("fgh", "abcdefgh"));
            Assert.AreEqual("", Extensions.CommonSuffix("abcdefgh", "qwe"));
            Assert.AreEqual("fgh", Extensions.CommonSuffix("abcfgh", "abcdefgh"));
            Assert.AreEqual("", Extensions.CommonSuffix("abcdefgh", null));
            Assert.AreEqual("abc", Extensions.CommonSuffix("1abc", "2abc"));
            Assert.AreEqual("abc", Extensions.CommonSuffix("abc", "abc"));
            Assert.AreEqual("", Extensions.CommonSuffix("", ""));
        }
    }
}
