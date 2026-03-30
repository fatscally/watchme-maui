using System.Globalization;

namespace watchme;

public class BoolToIconConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isOn)
        {
            return isOn
                ? ImageSource.FromFile("power_circle_on.png")
                : ImageSource.FromFile("power_circle_off.png");
        }

        return ImageSource.FromFile("power_circle_off.png");
    }


    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
}

public class BoolToGreenGrayConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isOn)
        {
            return isOn
                ? Colors.Green
                : Colors.Gray;
        }

        return Colors.Gray;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

}


public class BoolToGrayRedConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isOn)
        {
            return isOn
                ? Colors.Gray
                : Colors.Red;
        }

        return Colors.Gray;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}