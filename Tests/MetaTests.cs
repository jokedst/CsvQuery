using System;

namespace Tests
{
    using System.Globalization;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class MetaTests
    {
        //// Ensure GitVersion sets the DLL version
        //[TestMethod]
        //public void GitVersion()
        //{
        //    var assembly = Assembly.GetAssembly(typeof(CsvQuery.Csv.CsvAnalyzer));
        //    var assemblyName = assembly.GetName().Name;
        //    var gitVersionInformationType = assembly.GetType(assemblyName + ".GitVersionInformation");
        //    var fields = gitVersionInformationType.GetFields();

        //    foreach (var field in fields)
        //    {
        //        Trace.WriteLine(string.Format("{0}: {1}", field.Name, field.GetValue(null)));
        //    }
        //    Console.WriteLine();

        //    var dllVersion = assembly.GetName().Version;
        //    var gitVersion = gitVersionInformationType.GetField("SemVer").GetValue(null);
        //    Assert.AreEqual(gitVersion, $"{dllVersion.Major}.{dllVersion.Minor}.{dllVersion.Revision}");
        //}

        [TestMethod]
        public void IUnderstandDecimalParsing()
        {
            var ok = decimal.TryParse("12,34", NumberStyles.Any, NumberFormatInfo.InvariantInfo, out var decimalResult);
            Console.WriteLine($"ok={ok}, res={decimalResult}");
            ok = decimal.TryParse("12,34", NumberStyles.AllowDecimalPoint, NumberFormatInfo.InvariantInfo, out decimalResult);
            Console.WriteLine($"ok={ok}, res={decimalResult}");
            ok = decimal.TryParse("12,34", NumberStyles.Number, NumberFormatInfo.InvariantInfo, out decimalResult);
            Console.WriteLine($"ok={ok}, res={decimalResult}");
            ok = decimal.TryParse("12,34", NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out decimalResult);
            Console.WriteLine($"ok={ok}, res={decimalResult}");
            ok = decimal.TryParse("12,34", NumberStyles.Integer|NumberStyles.AllowDecimalPoint | NumberStyles.AllowTrailingSign, NumberFormatInfo.InvariantInfo, out decimalResult);
            Console.WriteLine($"ok={ok}, res={decimalResult}");
            ok = decimal.TryParse("12,34", out decimalResult);
            Console.WriteLine($"ok={ok}, res={decimalResult}");
            
        }
    }
}
