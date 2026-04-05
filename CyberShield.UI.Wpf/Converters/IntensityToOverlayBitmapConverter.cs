using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CyberShield.UI.Wpf.Converters;

public sealed class IntensityToOverlayBitmapConverter : IValueConverter
{
    public int Width { get; set; } = 640;
    public int Height { get; set; } = 360;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        double intensity = 0.0;
        if (value is double d) intensity = d;

        intensity = Math.Clamp(intensity, 0, 1);

        var wb = new WriteableBitmap(Width, Height, 96, 96, PixelFormats.Bgra32, null);
        var pixels = new byte[Width * Height * 4];

        byte r = 255;
        byte g = (byte)(255 * (1.0 - intensity));
        byte b = 0;
        byte a = (byte)(120 * intensity);

        for (int i = 0; i < pixels.Length; i += 4)
        {
            pixels[i + 0] = b;
            pixels[i + 1] = g;
            pixels[i + 2] = r;
            pixels[i + 3] = a;
        }

        wb.WritePixels(new Int32Rect(0, 0, Width, Height), pixels, Width * 4, 0);
        wb.Freeze();
        return wb;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}