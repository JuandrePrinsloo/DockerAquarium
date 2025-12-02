namespace DockerAquarium.Presentation.Converters;

using System.Globalization;
using System.Windows.Data;

/// <summary>
/// Converts boolean value to its inverse.
/// </summary>
public class NotBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo? culture)
    {
        if (value is bool boolValue)
        {
            return !boolValue;
        }
        return false;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo? culture)
    {
        if (value is bool boolValue)
        {
            return !boolValue;
        }
        return false;
    }
}

