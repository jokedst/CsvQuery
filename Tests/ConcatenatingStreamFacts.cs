using System;
using System.IO;
using CsvQuery.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass]
    public class ConcatenatingStreamFacts
    {
        [TestMethod]
        public void Meh()
        {
            var r = new Random();

            var buffer1 = new byte[1024];
            r.NextBytes(buffer1);
            var stream1 = new MemoryStream(buffer1);

            var buffer2 = new byte[1024];
            r.NextBytes(buffer2);
            var stream2 = new MemoryStream(buffer2);

            var c = new ConcatenatingStream(new Stream[] {stream1, stream2}, true);

            var resultBuffer = new byte[2048];
            var data = c.Read(resultBuffer, 0, 2048);

            Assert.AreEqual(2048, data);
            resultBuffer.AssertEqualData(0, buffer1, 0, 1024);
            resultBuffer.AssertEqualData(1024, buffer2, 0, 1024);

        }

        [TestMethod]
        public void Meh2()
        {
            var buffer1 = new byte[128];
            var buffer2 = new byte[128];
            for (byte i = 0; i < 128; i++)
            {
                buffer1[i] = i;
                buffer2[i] = (byte) (i+128);
            }
            var stream1 = new MemoryStream(buffer1);
            var stream2 = new MemoryStream(buffer2);

            var c = new ConcatenatingStream(new Stream[] { stream1, stream2 }, true);
            
            var resultBuffer = new byte[256];
            var data = c.Read(resultBuffer, 0, 256);

            Assert.AreEqual(256, data);
            resultBuffer.AssertEqualData(0, buffer1, 0, 128);
            resultBuffer.AssertEqualData(128, buffer2, 0, 128);

        }
    }
}
