namespace CsvQuery.PluginInfrastructure
{
    using System.Drawing;
    using System.IO;

    public class IcoCreator
    {
        public static bool Convert(Stream inputStream, Stream outputStream, int size, bool keepAspectRatio = false)
        {
            var originalBitmap = (Bitmap)Image.FromStream(inputStream);
            if (originalBitmap == null) return false;
            return Convert(originalBitmap, outputStream, size, keepAspectRatio);
        }

        public static bool Convert(Bitmap originalBitmap, Stream outputStream, int size, bool keepAspectRatio = false)
        {
            int width = size;
            var height = keepAspectRatio ? originalBitmap.Height / originalBitmap.Width * size : size;

            var newBit = new Bitmap(originalBitmap, new Size(width, height));
            return Convert(newBit, outputStream);
        }

        public static bool Convert(Bitmap originalBitmap, Stream outputStream)
        {
            // save the png into a memory stream for future use
            var memData = new MemoryStream();
            originalBitmap.Save(memData, System.Drawing.Imaging.ImageFormat.Png);

            var iconWriter = new BinaryWriter(outputStream);
            if (outputStream == null || iconWriter == null) return false;
            // 0-1 reserved, 0
            iconWriter.Write((byte) 0);
            iconWriter.Write((byte) 0);

            // 2-3 image type, 1 = icon, 2 = cursor
            iconWriter.Write((short) 1);

            // 4-5 number of images
            iconWriter.Write((short) 1);

            // image entry 1
            // 0 image width
            iconWriter.Write((byte) originalBitmap.Width);
            // 1 image height
            iconWriter.Write((byte) originalBitmap.Height);

            // 2 number of colors
            iconWriter.Write((byte) 0);

            // 3 reserved
            iconWriter.Write((byte) 0);

            // 4-5 color planes
            iconWriter.Write((short) 0);

            // 6-7 bits per pixel
            iconWriter.Write((short) 32);

            // 8-11 size of image data
            iconWriter.Write((int) memData.Length);

            // 12-15 offset of image data
            iconWriter.Write((int) (6 + 16));

            // write image data
            // png data must contain the whole png data file
            iconWriter.Write(memData.ToArray());

            iconWriter.Flush();

            return true;
        }

        public static bool Convert(string inputImage, string outputIcon, int size, bool keepAspectRatio = false)
        {
            var inputStream = new FileStream(inputImage, FileMode.Open);
            var outputStream = new FileStream(outputIcon, FileMode.OpenOrCreate);

            var result = Convert(inputStream, outputStream, size, keepAspectRatio);

            inputStream.Close();
            outputStream.Close();

            return result;
        }

        public static byte[] BitmapToIco(Bitmap originalBitmap)
        {
            var output = new MemoryStream();
            Convert(originalBitmap, output);
            return output.GetBuffer();
        }
    }
}
