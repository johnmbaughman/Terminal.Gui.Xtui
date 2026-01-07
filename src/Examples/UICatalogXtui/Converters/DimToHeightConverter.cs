using System;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Xtui;

namespace UICatalogXtui.Converters;

public class DimToHeightConverter : IValueConverter
{
    public object? Convert (object? value, Type targetType, object? parameter, string? language)
    {
        if (value is StatusBar statusBar)
        {
            return new DimAuto (
                Dim.Func (_ => statusBar.Visible ? 1 : 0),
                Dim.Func (_ => statusBar.Visible ? 1 : 0),
                DimAutoStyle.Auto);
        }

        return null;
    }

    public object? ConvertBack (object? value, Type targetType, object? parameter, string? language)
    {
        throw new NotImplementedException ();
    }
}