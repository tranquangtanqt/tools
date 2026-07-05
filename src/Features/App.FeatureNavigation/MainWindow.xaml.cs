// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.UI.Xaml;
using System;

namespace WinUIGallery;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        this.InitializeComponent();
    }

    public void Navigate(Type pageType, object? parameter = null)
    {
        RootFrame.Navigate(pageType, parameter);
    }
}
