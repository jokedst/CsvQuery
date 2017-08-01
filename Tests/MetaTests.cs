using System;
using System.Diagnostics;
using System.Reflection;

namespace Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class MetaTests
    {
        // Ensure GitVersion sets the DLL version
        [TestMethod]
        public void GitVersion()
        {
            var assembly = Assembly.GetAssembly(typeof(CsvQuery.Csv.CsvAnalyzer));
            var assemblyName = assembly.GetName().Name;
            var gitVersionInformationType = assembly.GetType(assemblyName + ".GitVersionInformation");
            var fields = gitVersionInformationType.GetFields();

            foreach (var field in fields)
            {
                Trace.WriteLine(string.Format("{0}: {1}", field.Name, field.GetValue(null)));
            }
            Console.WriteLine();

            var dllVersion = assembly.GetName().Version;
            var gitVersion = gitVersionInformationType.GetField("SemVer").GetValue(null);
            Assert.AreEqual(gitVersion, $"{dllVersion.Major}.{dllVersion.Minor}.{dllVersion.Revision}");
        }
    }
}
