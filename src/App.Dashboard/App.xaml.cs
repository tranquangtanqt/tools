// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.UI.Xaml;
using WinUIGallery.Helpers;

namespace WinUIGallery;

sealed partial class App : Application
{
    internal static MainWindow MainWindow { get; private set; } = null!;

    public App()
    {
        InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        AppBootstrap.InitializeCommon();
        AppBootstrap.RunFirstRunMigrationIfNeeded();

        MainWindow = new MainWindow();
        WindowHelper.TrackWindow(MainWindow);
        MainWindow.Activate();
    }
}
