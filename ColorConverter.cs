namespace HueAutomation
{
    public class ColorConverter
    {
        public static (double x, double y, double z) TemperatureToXyz(double temp)
        {
            var rgb = TemperatureToRgb(temp);
            return RgbToXyz(rgb);
        }
        public static (double x, double y, double z) RgbToXyz((double R, double G, double B) rgb)
        {
            double x = 0.4124 * rgb.R + 0.3576 * rgb.G + 0.1805 * rgb.B;
            double y = 0.2126 * rgb.R  + 0.7152 * rgb.G + 0.0722 * rgb.B;
            double z = 0.0193 * rgb.R + 0.1192 * rgb.G + 0.9505 * rgb.B;;
            double total = x + y + z;

            x = x / total;
            y = y / total;
            z = z / total;

            return (x, y, z);
        }

        public static (double r, double g, double b) TemperatureToRgb(double value)
        {
            double min = 0;
            double max = 0;
            double segmentSize = (max - min) / 4;

            var ratio = ((value - min) % segmentSize) / segmentSize;
            var segment = (int)Math.Floor(value / segmentSize);


            double r = segment switch
            {
                0 => 0,
                1 => 0,
                2 => 255 * ratio,
                _ => 255
            };
            double g = segment switch
            {
                0 => 255 * ratio,
                1 => 255,
                2 => 255,
                3 => 255 * (1-ratio),
                _ => 0
            };
            double b = segment switch
            {
                0 => 255,
                1 => 255 * (1 - ratio),
                _ => 0
            };

            return (r, g, b);
        }
    }
}