// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace WinUIGallery.Helpers;

/// <summary>
/// This exe's own catalog: loads only its own filtered ControlInfoData json, and reports
/// IncludedInBuild by checking this exe's own generated NavigationPageMappings.PageDictionary
/// (a type the shared ControlInfoDataSourceBase can't see, since it's generated per-project).
/// </summary>
public sealed class ControlInfoDataSource : ControlInfoDataSourceBase
{
    public static ControlInfoDataSource Instance { get; } = new();

    private ControlInfoDataSource() : base("SampleSupport/Data/Navigation.ControlInfoData.json")
    {
    }

    protected override bool IsIncludedInBuild(string uniqueId)
        => NavigationPageMappings.PageDictionary.ContainsKey("WinUIGallery.ControlPages." + uniqueId + "Page");
}
