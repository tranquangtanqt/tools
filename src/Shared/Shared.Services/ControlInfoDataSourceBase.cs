// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using WinUIGallery.Models;

namespace WinUIGallery.Helpers;

/// <summary>
/// Creates a collection of groups and items with content read from a static json file.
///
/// Each exe project has its own thin derived singleton (see App.*/Services/ControlInfoDataSource.cs)
/// that supplies its own json path and its own NavigationPageMappings-backed IsIncludedInBuild
/// check, since NavigationPageMappings is generated per-compiling-project and isn't visible here.
/// </summary>
public abstract class ControlInfoDataSourceBase
{
    private readonly object _lock = new();
    private readonly string _jsonRelativePath;

    /// <summary>
    /// The most recently constructed instance in this process. Set so that process-wide
    /// consumers that can't take a direct reference (e.g. <see cref="JumpListHelper"/>, which
    /// lives in this same shared assembly) can still reach the current app's catalog.
    /// </summary>
    public static ControlInfoDataSourceBase? Current { get; private set; }

    protected ControlInfoDataSourceBase(string jsonRelativePath)
    {
        _jsonRelativePath = jsonRelativePath;
        Current = this;
    }

    private readonly IList<ControlInfoDataGroup> _groups = new List<ControlInfoDataGroup>();
    public IList<ControlInfoDataGroup> Groups
    {
        get { return this._groups; }
    }

    public async Task<IEnumerable<ControlInfoDataGroup>> GetGroupsAsync()
    {
        await GetControlInfoDataAsync();
        return Groups;
    }

    public async Task<ControlInfoDataGroup?> GetGroupAsync(string uniqueId)
    {
        await GetControlInfoDataAsync();
        // Simple linear search is acceptable for small data sets
        var matches = Groups.Where((group) => group.UniqueId.Equals(uniqueId));
        if (matches.Count() == 1) return matches.First();
        return null;
    }

    public async Task<ControlInfoDataItem?> GetItemAsync(string uniqueId)
    {
        await GetControlInfoDataAsync();
        // Simple linear search is acceptable for small data sets
        var matches = Groups.SelectMany(group => group.Items).Where((item) => item.UniqueId.Equals(uniqueId));
        if (matches.Count() > 0) return matches.First();
        return null;
    }

    public async Task<ControlInfoDataGroup?> GetGroupFromItemAsync(string uniqueId)
    {
        await GetControlInfoDataAsync();
        var matches = Groups.Where((group) => group.Items.FirstOrDefault(item => item.UniqueId.Equals(uniqueId)) != null);
        if (matches.Count() == 1) return matches.First();
        return null;
    }

    /// <summary>
    /// Whether the given item's sample page is compiled into this process's own exe. Overridden
    /// per-exe to check that exe's own generated NavigationPageMappings.PageDictionary.
    /// </summary>
    protected virtual bool IsIncludedInBuild(string uniqueId) => false;

    private async Task GetControlInfoDataAsync()
    {
        lock (_lock)
        {
            if (Groups.Count() != 0)
            {
                return;
            }
        }

        var jsonText = await FileLoader.LoadText(_jsonRelativePath);
        var controlInfoDataGroup = JsonSerializer.Deserialize(jsonText, typeof(Root), RootContext.Default) as Root;

        if (controlInfoDataGroup is null)
        {
            return;
        }

        lock (_lock)
        {
            controlInfoDataGroup.Groups.SelectMany(g => g.Items).ToList().ForEach(item =>
            {
                string badgeString = item switch
                {
                    { IsNew: true } => "New",
                    { IsUpdated: true } => "Updated",
                    { IsPreview: true } => "Preview",
                    _ => string.Empty,
                };

                item.BadgeString = badgeString;
                item.IncludedInBuild = IsIncludedInBuild(item.UniqueId);
                item.ImagePath ??= "ms-appx:///Assets/ControlImages/Placeholder.png";
            });

            foreach (var group in controlInfoDataGroup.Groups)
            {
                if (!Groups.Any(g => g.Title == group.Title))
                {
                    Groups.Add(group);
                }
            }
        }
    }
}
