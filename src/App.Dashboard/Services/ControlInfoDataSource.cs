// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace WinUIGallery.Helpers;

/// <summary>
/// Dashboard's catalog: loads the FULL, unfiltered catalog (every group/item) purely for display
/// (category list). Dashboard hosts no ControlPages itself, so there is no per-project
/// NavigationPageMappings to check here — IncludedInBuild is irrelevant to the category list.
/// </summary>
public sealed class ControlInfoDataSource : ControlInfoDataSourceBase
{
    public static ControlInfoDataSource Instance { get; } = new();

    private ControlInfoDataSource() : base("SampleSupport/Data/ControlInfoData.json")
    {
    }
}
