using System.Windows.Media;
using System;

namespace LibrarySystem
{
    public static class UiHelpers
    {
        public static SolidColorBrush BrushFromHex(string hex)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(hex)) return new SolidColorBrush(Colors.Transparent);
                var color = (Color)ColorConverter.ConvertFromString(hex);
                return new SolidColorBrush(color);
            }
            catch
            {
                return new SolidColorBrush(Colors.Transparent);
            }
        }
    }
}