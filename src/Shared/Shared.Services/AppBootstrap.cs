// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace WinUIGallery.Helpers;

/// <summary>
/// Shared startup sequence for every Feature/Dashboard exe's App.OnLaunched. Kept as a plain
/// static helper (not an Application base class) because WinUI3's generated InitializeComponent()
/// requires each exe to declare its own App.xaml.cs partial class.
/// </summary>
public static class AppBootstrap
{
    public static void InitializeCommon()
    {
        ThemeHelper.Initialize();
    }

    public static void RunFirstRunMigrationIfNeeded()
    {
        if (SettingsHelper.Current.IsFirstRun)
        {
            SettingsMigration.MigrateRecentlyVisited();
            SettingsMigration.MigrateFavorites();
            SettingsHelper.Current.IsFirstRun = false;
        }
    }
}
