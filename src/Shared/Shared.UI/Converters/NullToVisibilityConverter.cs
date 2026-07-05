// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace WinUIGallery.Converters;

/// <summary>
/// Local, dependency-free replacement for CommunityToolkit.WinUI.Converters.EmptyObjectToObjectConverter
/// (used app-wide as "nullToVisibilityConverter"), which pulls a WinRT.Runtime version that conflicts
/// with WindowsAppSDK.Foundation's own Projection assemblies when referenced from a class library.
/// </summary>
public sealed partial class NullToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value is null ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
