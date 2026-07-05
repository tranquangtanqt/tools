// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace WinUIGallery.Services;

/// <summary>
/// Launches a Feature exe as a separate process. Category -> exe-folder-name mapping is
/// hardcoded for the pilot's 3 categories; once all 19 categories are split this should
/// become a small shipped manifest (AvailableFeatures.json) instead of a code change per category.
/// </summary>
public static class FeatureAppLauncher
{
    // NOTE: "BasicInput" maps to the pilot's single-item Button app, not the full 14-item
    // Basic Input category — Button was promoted out of BasicInput for the pilot (see plan doc).
    private static readonly Dictionary<string, string> CategoryToExeFolder = new()
    {
        ["BasicInput"] = "App.FeatureButton",
        ["Navigation"] = "App.FeatureNavigation",
        ["FundamentalsItem"] = "App.FeatureFundamentals",
    };

    public static bool IsAvailable(string categoryUniqueId)
        => CategoryToExeFolder.TryGetValue(categoryUniqueId, out var exeFolder) && TryResolveExePath(exeFolder, out _);

    public static bool TryLaunch(string categoryUniqueId)
    {
        if (!CategoryToExeFolder.TryGetValue(categoryUniqueId, out var exeFolder))
            return false;

        if (!TryResolveExePath(exeFolder, out var exePath))
            return false;

        Process.Start(new ProcessStartInfo(exePath) { UseShellExecute = false });
        return true;
    }

    // BaseOutputPath for every project in src/** is "artifacts/bin/<ProjectName>/<Platform>/<Config>/<TFM>/"
    // (see src/Directory.Build.props). Sibling exes share the same Platform/Config/TFM segments, so the
    // sibling's output dir is found by swapping just the "<ProjectName>" path segment.
    private const string OwnAssemblyName = "App.Dashboard";

    private static bool TryResolveExePath(string exeFolder, out string exePath)
    {
        string baseDir = AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        string marker = Path.DirectorySeparatorChar + OwnAssemblyName + Path.DirectorySeparatorChar;
        string replacement = Path.DirectorySeparatorChar + exeFolder + Path.DirectorySeparatorChar;
        string siblingDir = baseDir.Replace(marker, replacement);

        exePath = Path.Combine(siblingDir, exeFolder + ".exe");
        return File.Exists(exePath);
    }
}
