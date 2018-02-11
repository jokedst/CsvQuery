namespace Tests
{
    using System.IO;
    using System.Reflection;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    internal static class Helpers
    {
        public static string GetResource(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();

            using (Stream stream = assembly.GetManifestResourceStream("Tests.TestFiles." + resourceName))
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }


        public static bool AssertEqualData<T>(this T[] buffer, int offset, T[] other, int otherOffset, int length)
        {
            if (buffer.Length < offset + length)
                throw new AssertFailedException($"First buffer too small ({buffer.Length} - {offset}) to compare {length} items");
            if (other.Length < otherOffset + length)
                throw new AssertFailedException($"Second buffer too small ({other.Length} - {otherOffset}) to compare {length} items");
            for (int i = 0; i < length; i++)
            {
                if (!buffer[offset + i].Equals(other[otherOffset + i])) 
                    throw new AssertFailedException($"Buffers differ on index {offset+i} vs {otherOffset+i}: {buffer[offset + i]} != {other[otherOffset + i]}");
            }

            return true;
        }
    }
}
