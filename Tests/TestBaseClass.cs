namespace Tests
{
    using System;
    using System.Collections.Generic;
    using CsvQuery;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    public abstract class TestBaseClass
    {
        protected static int TestCount;

        [TestInitialize]
        public void InitializeTestBase()
        {
            TestCount++;
        }

        public virtual void AssertDataEqual(IList<string[]> expected, IList<string[]> actual)
        {
            var rowsToTest = Math.Min(expected.Count, actual.Count);
            for (var row = 0; row < rowsToTest; row++)
            {
                Assert.AreEqual(expected[row].Length, actual[row].Length,
                    "Row " + row + " does not have same number of columns");
                for (var column = 0; column < expected[row].Length; column++)
                    Assert.AreEqual(expected[row][column], actual[row][column],
                        "Value in row " + row + ", column " + column + " are not equal");
            }
            Assert.AreEqual(expected.Count, actual.Count, "Not same number of rows");
        }

        public virtual void AssertDataEqualAfterTypeConversion(IList<string[]> expected, IList<string[]> actual)
        {
            if (!Main.Settings.DetectDbColumnTypes)
            {
                AssertDataEqual(expected,actual);
                return;
            }
            Assert.AreEqual(expected.Count, actual.Count, "Not same number of rows");
            for (var row = 0; row < expected.Count; row++)
            {
                Assert.AreEqual(expected[row].Length, actual[row].Length,
                    "Row " + row + " does not have same number of columns");
                for (var column = 0; column < expected[row].Length; column++)
                {
                    // Right now only decimal numbers and ints with leading zeros that are affected
                    if(expected[row][column]== actual[row][column]) continue;
                    if (expected[row][column].Trim('0').Replace(',','.').TrimEnd('.') 
                        == actual[row][column].Trim('0').Replace(',', '.').TrimEnd('.')) continue;
                    
                    Assert.AreEqual(expected[row][column], actual[row][column],
                        "Value in row " + row + ", column " + column + " are not equal");
                }
            }
        }
    }
}