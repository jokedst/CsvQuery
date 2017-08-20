namespace CsvQuery.Tools
{
    using System;
    using System.Drawing;

    internal class StyleHelper
    {
        public static Color HSVToRGB(double hue, double saturation, double value)
        {
            double R, G, B;
            if (hue == 1.0)
            {
                hue = 0.0;
            }

            var vh = hue / (1.0 / 6.0);

            var i = (int)Math.Floor(vh);

            var f = vh - i;
            var p = value * (1.0 - saturation);
            var q = value * (1.0 - (saturation * f));
            var t = value * (1.0 - (saturation * (1.0 - f)));

            switch (i)
            {
                case 0:
                {
                    R = value;
                    G = t;
                    B = p;
                    break;
                }
                case 1:
                {
                    R = q;
                    G = value;
                    B = p;
                    break;
                }
                case 2:
                {
                    R = p;
                    G = value;
                    B = t;
                    break;
                }
                case 3:
                {
                    R = p;
                    G = q;
                    B = value;
                    break;
                }
                case 4:
                {
                    R = t;
                    G = p;
                    B = value;
                    break;
                }
                case 5:
                {
                    R = value;
                    G = p;
                    B = q;
                    break;
                }
                default:
                {
                    // not possible - if we get here it is an internal error
                    throw new ArgumentException();
                }
            }

            return Color.FromArgb((int) (R * 255), (int) (G * 255), (int) (B * 255));
        }
    }
}
