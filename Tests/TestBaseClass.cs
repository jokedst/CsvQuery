namespace Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    public abstract class TestBaseClass
    {
        public void AssertDataEqual(IList<string[]> expected, IList<string[]> actual)
        {
            Assert.AreEqual(expected.Count, actual.Count, "Not same number of rows");
            for (var row = 0; row < expected.Count; row++)
            {
                Assert.AreEqual(expected[row].Count(), actual[row].Count(),
                    "Row " + row + " does not have same number of columns");
                for (var column = 0; column < expected[row].Count(); column++)
                    Assert.AreEqual(expected[row][column], actual[row][column],
                        "Value in row " + row + ", column " + column + " are not equal");
            }
        }
    }
}