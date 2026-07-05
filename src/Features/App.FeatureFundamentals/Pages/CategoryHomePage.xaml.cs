// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System.Linq;
using WinUIGallery.Helpers;
using WinUIGallery.Models;

namespace WinUIGallery.Pages;

/// <summary>
/// Shows the list of items in this exe's own (single) category; forwards straight to
/// <see cref="ItemPage"/> when the category has exactly one item.
/// </summary>
public sealed partial class CategoryHomePage : Page
{
    public CategoryHomePage()
    {
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        var groups = await ControlInfoDataSource.Instance.GetGroupsAsync();
        var group = groups.FirstOrDefault();
        if (group is null)
        {
            return;
        }

        if (group.Items.Count == 1)
        {
            Frame.Navigate(typeof(ItemPage), group.Items[0].UniqueId);
            return;
        }

        CategoryTitleText.Text = group.Title;
        ItemsListView.ItemsSource = group.Items;
    }

    private void ItemsListView_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is ControlInfoDataItem item)
        {
            Frame.Navigate(typeof(ItemPage), item.UniqueId);
        }
    }
}
