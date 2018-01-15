using CsvQuery.PluginInfrastructure;

namespace Tests.PluginInfrastructure
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ColourFacts
    {
        [TestMethod]
        public void Int_Constructor_returns_same_value()
        {
            var val = 0x123456;
            var c = new Colour(val);
            Assert.AreEqual(val, c.Value);
        }
    }
}
