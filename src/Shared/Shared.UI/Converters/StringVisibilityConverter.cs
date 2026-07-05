// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace WinUIGallery.Converters;

/// <summary>
/// Local, dependency-free replacement for CommunityToolkit.WinUI.Converters.StringVisibilityConverter,
/// which pulls a WinRT.Runtime version that conflicts with WindowsAppSDK.Foundation's own Projection
/// assemblies when referenced from a class library (see Shared.UI build notes).
/// </summary>
public sealed partial class StringVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return string.IsNullOrEmpty(value as string) ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
