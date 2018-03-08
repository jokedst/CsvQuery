namespace CsvQuery.PluginInfrastructure
{
    using System.Diagnostics;
    using System.Text;
    using System.IO;
    using CsvQuery.Tools;

    public class ScintillaStreams
    {
        /// <summary>
        /// Reads the whole document as a text stream, trying to use the right encoding
        /// </summary>
        public static StreamReader StreamAllText()
        {
            var doc = PluginBase.CurrentScintillaGateway;
            var codepage = doc.GetCodePage();
            var encoding = codepage == (int) SciMsg.SC_CP_UTF8 ? Encoding.UTF8 : Encoding.Default;
            //if (codepage == 0)
            //{
            //   var style = doc.StyleGetCharacterSet((int) SciMsg.STYLE_DEFAULT);
            //}
            return new StreamReader(StreamAllRawText(), encoding);
        }

        /// <summary>
        /// Reads the whole document as a byte stream
        /// </summary>
        public static Stream StreamAllRawText()
        {
            var doc = PluginBase.CurrentScintillaGateway;
            var length = doc.GetLength();
            int gap = doc.GetGapPosition();
            Debug.WriteLine($"ScintillaStreams StreamAllRawText: gap at {gap}/{length}");

            if (length == gap)
            {
                var characterPointer = doc.GetCharacterPointer();
                unsafe
                {
                    return new UnmanagedMemoryStream((byte*)characterPointer.ToPointer(), length, length, FileAccess.Read);
                }
            }

            return ConcatenatingStream.FromFactoryFunctions(false, () =>
            {
                var rangePtr = doc.GetRangePointer(0, gap);
                unsafe
                {
                    return new UnmanagedMemoryStream((byte*) rangePtr.ToPointer(), gap, gap, FileAccess.Read);
                }
            }, () =>
            {
                var rangePtr = doc.GetRangePointer(gap+1, length);
                unsafe
                {
                    return new UnmanagedMemoryStream((byte*)rangePtr.ToPointer(), length-gap, length-gap, FileAccess.Read);
                }
            });
        }
    }
}
