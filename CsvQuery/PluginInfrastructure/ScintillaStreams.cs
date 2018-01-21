using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CsvQuery.Tools;

namespace CsvQuery.PluginInfrastructure
{
    public class ScintillaStreams
    {
        /// <summary>
        /// Reads the whole document as a (utf8) stream
        /// </summary>
        public static Stream StreamAllText()
        {
            var doc = PluginBase.CurrentScintillaGateway;
            var length = doc.GetLength();
            int gap = doc.GetGapPosition();

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
