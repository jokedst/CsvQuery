namespace Tests
{
    using System.Text;
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

        [TestMethod]
        public void TrimToStringMethod()
        {
            Assert.AreEqual("", new StringBuilder("  \t ").TrimToString());
            Assert.AreEqual("", new StringBuilder(" ").TrimToString());
            Assert.AreEqual("", new StringBuilder("").TrimToString());
            Assert.AreEqual("", new StringBuilder().TrimToString());

            Assert.AreEqual("h", new StringBuilder("h").TrimToString());
            Assert.AreEqual("h", new StringBuilder(" h").TrimToString());
            Assert.AreEqual("h", new StringBuilder("h ").TrimToString());
            Assert.AreEqual("h", new StringBuilder(" h ").TrimToString());

            Assert.AreEqual("hello", new StringBuilder(" \t\r\n hello\n\t \r").TrimToString());
            Assert.AreEqual("hello", new StringBuilder("hello").TrimToString());
            Assert.AreEqual("foo bar", new StringBuilder(" foo bar").TrimToString());
            Assert.AreEqual("foo bar", new StringBuilder("foo bar ").TrimToString());
            Assert.AreEqual("foo bar", new StringBuilder("foo bar").TrimToString());
        }

        [TestMethod]
        public void TrimSingleCharToStringMethod()
        {
            Assert.AreEqual("h", new StringBuilder("h").TrimToString());
            Assert.AreEqual("h", new StringBuilder("h ").TrimToString());
            Assert.AreEqual("h", new StringBuilder("h  ").TrimToString());
            Assert.AreEqual("h", new StringBuilder(" h").TrimToString());
            Assert.AreEqual("h", new StringBuilder(" h ").TrimToString());
            Assert.AreEqual("h", new StringBuilder(" h  ").TrimToString());
            Assert.AreEqual("h", new StringBuilder("  h").TrimToString());
            Assert.AreEqual("h", new StringBuilder("  h ").TrimToString());
            Assert.AreEqual("h", new StringBuilder("  h  ").TrimToString());
        }

        [TestMethod]
        public void TrimSentanceToStringMethod()
        {
            Assert.AreEqual("foo bar", new StringBuilder("foo bar").TrimToString());
            Assert.AreEqual("foo bar", new StringBuilder("foo bar ").TrimToString());
            Assert.AreEqual("foo bar", new StringBuilder("foo bar  ").TrimToString());
            Assert.AreEqual("foo bar", new StringBuilder(" foo bar").TrimToString());
            Assert.AreEqual("foo bar", new StringBuilder(" foo bar ").TrimToString());
            Assert.AreEqual("foo bar", new StringBuilder(" foo bar  ").TrimToString());
            Assert.AreEqual("foo bar", new StringBuilder("  foo bar").TrimToString());
            Assert.AreEqual("foo bar", new StringBuilder("  foo bar ").TrimToString());
            Assert.AreEqual("foo bar", new StringBuilder("  foo bar  ").TrimToString());
        }
    }
}
