// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;
using System.Collections;

namespace WinUIGallery.Converters;

/// <summary>
/// Local, dependency-free replacement for CommunityToolkit.WinUI.Converters.CollectionVisibilityConverter,
/// which pulls a WinRT.Runtime version that conflicts with WindowsAppSDK.Foundation's own Projection
/// assemblies when referenced from a class library (see Shared.UI build notes).
/// </summary>
public sealed partial class CollectionVisibilityConverter : IValueConverter
{
    public Visibility EmptyValue { get; set; } = Visibility.Collapsed;
    public Visibility NotEmptyValue { get; set; } = Visibility.Visible;

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        bool isEmpty = value is null || (value is ICollection collection && collection.Count == 0);
        return isEmpty ? EmptyValue : NotEmptyValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
