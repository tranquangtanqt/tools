// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Linq;
using WinUIGallery.Helpers;
using WinUIGallery.Models;

namespace WinUIGallery.Pages;

/// <summary>
/// A page that displays details for a single item within this exe's own category.
/// </summary>
public sealed partial class ItemPage : Page
{
    private static string WinUIBaseUrl = "https://github.com/microsoft/microsoft-ui-xaml/tree/main/controls/dev";
    private static string GalleryBaseUrl = "https://github.com/microsoft/WinUI-Gallery/tree/main/WinUIGallery/Samples/";

    public ControlInfoDataItem? Item { get; set; }
    private ElementTheme? _currentElementTheme;

    public ItemPage()
    {
        this.InitializeComponent();
        Loaded += (s, e) =>
        {
            pageHeader.ToggleThemeAction = OnToggleTheme;
            this.Focus(FocusState.Programmatic);
        };
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        if (e.Parameter is string uniqueId)
        {
            var item = await ControlInfoDataSource.Instance.GetItemAsync(uniqueId);

            if (item != null)
            {
                Item = item;

                NavigationPageMappings.PageDictionary.TryGetValue("WinUIGallery.ControlPages." + item.UniqueId + "Page", out Type? pageType);

                if (pageType != null)
                {
                    var pageName = $"{item.UniqueId}/{pageType.Name}";
                    pageHeader.SetControlSourceLink(WinUIBaseUrl, item.SourcePath);
                    pageHeader.SetSamplePageSourceLinks(GalleryBaseUrl, pageName);
                    contentFrame.Navigate(pageType);
                }
            }
        }
        base.OnNavigatedTo(e);
    }

    protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
    {
        SetControlExamplesTheme(ThemeHelper.ActualTheme);
        base.OnNavigatingFrom(e);
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        pageHeader.ToggleThemeAction = null;
        pageHeader.CopyLinkAction = null;
        base.OnNavigatedFrom(e);
    }

    private void OnToggleTheme()
    {
        var currentElementTheme = ((_currentElementTheme ?? ElementTheme.Default) == ElementTheme.Default) ? ThemeHelper.ActualTheme : _currentElementTheme;
        var newTheme = currentElementTheme == ElementTheme.Dark ? ElementTheme.Light : ElementTheme.Dark;
        SetControlExamplesTheme(newTheme);
    }

    private void SetControlExamplesTheme(ElementTheme theme)
    {
        var controlExamples = (this.contentFrame.Content as UIElement)?.GetDescendantsOfType<Controls.SampleThemeListener>();

        if (controlExamples != null)
        {
            _currentElementTheme = theme;
            foreach (var controlExample in controlExamples)
            {
                controlExample.RequestedTheme = theme;
            }
            if (!controlExamples.Any())
            {
                this.RequestedTheme = theme;
            }
        }
    }
}
