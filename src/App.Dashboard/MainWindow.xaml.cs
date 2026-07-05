// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WinUIGallery.Helpers;
using WinUIGallery.Models;
using WinUIGallery.Services;

namespace WinUIGallery;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        this.InitializeComponent();
        RootGrid.Loaded += MainWindow_Loaded;
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        var groups = await ControlInfoDataSource.Instance.GetGroupsAsync();
        CategoryListView.ItemsSource = groups;
    }

    private void CategoryListView_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is ControlInfoDataGroup group)
        {
            if (!FeatureAppLauncher.TryLaunch(group.UniqueId))
            {
                System.Diagnostics.Debug.WriteLine($"[Dashboard] '{group.Title}' isn't split out into its own app yet.");
            }
        }
    }
}
