using System.Linq;

namespace Tests
{
    using System;
    using System.IO;
    using System.Text;
    using CsvQuery.Tools;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Testing ReadTwiceStream
    /// </summary>
    [TestClass]
    public class ReadTwiceStreamFacts
    {

        [TestMethod]
        public void AllPhasesWork()
        {
            // Arrange
            var sourceStream = new MemoryStream(Encoding.UTF8.GetBytes("First line\nSecond line\nThird line"));
            var twiceStream = new ReadTwiceStream(sourceStream);
            var buffer = new byte[1000];

            // Phase 1
            var read = twiceStream.Read(buffer, 0, 10);
            Assert.AreEqual(10, read);
            var firstLine = Encoding.UTF8.GetString(buffer, 0, 10);
            Assert.AreEqual("First line", firstLine);


            // Phase 2
            twiceStream.Seek(0, SeekOrigin.Begin);
            read = twiceStream.Read(buffer, 0, 10);
            Assert.AreEqual(10, read);
            firstLine = Encoding.UTF8.GetString(buffer, 0, 10);
            Assert.AreEqual("First line", firstLine);
            
            // Phase 3
            string theRest;
            using (var memoryStream = new MemoryStream())
            {
                twiceStream.CopyTo(memoryStream);
                theRest = Encoding.UTF8.GetString(memoryStream.GetBuffer(),0,(int) memoryStream.Position);
            }
            Assert.AreEqual("\nSecond line\nThird line", theRest);
        }

        public void Binary_works()
        {
            var r = new Random();
            var buffer = new byte[1024];
            r.NextBytes(buffer);

            var sourceStream = new MemoryStream(buffer);
            var twiceStream = new ReadTwiceStream(sourceStream);

            var readBuffer = new byte[256];
            twiceStream.Read(readBuffer, 0, 256);

            Assert.IsTrue(readBuffer.EqualData(buffer, 0, 256));
        }
    }
}
