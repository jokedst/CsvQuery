using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tests
{
    using System.IO;
    using System.Reflection;

    internal class Helpers
    {
        public static string GetResource(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            // var resourceName = "MyCompany.MyProduct.MyFile.txt";

            using (Stream stream = assembly.GetManifestResourceStream("Tests.TestFiles." + resourceName))
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
