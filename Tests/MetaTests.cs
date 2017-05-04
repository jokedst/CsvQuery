using System;
using System.Diagnostics;
using System.Reflection;

namespace Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class MetaTests
    {
        [TestMethod]
        public void GitVersion()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var assemblyName = assembly.GetName().Name;
            var gitVersionInformationType = assembly.GetType(assemblyName + ".GitVersionInformation");
            var fields = gitVersionInformationType.GetFields();

            foreach (var field in fields)
            {
                Trace.WriteLine(string.Format("{0}: {1}", field.Name, field.GetValue(null)));
            }
            Console.WriteLine();

            Assert.AreEqual("1.0.0.0", fields[1]);
        }
    }
}
