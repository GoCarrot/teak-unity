#region References
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

using TeakEditor.iOS.Xcode;
using TeakEditor.iOS.Xcode.Extensions;

using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
#endregion

public class TeakPostProcessBuild {
    [PostProcessBuild(100)]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject) {
        if (TeakSettings.JustShutUpIKnowWhatImDoing) return;
        if (target != BuildTarget.iOS) return;

        string projectPath = PBXProject.GetPBXProjectPath(pathToBuiltProject);
        PBXProject project = new PBXProject();
        project.ReadFromFile(projectPath);

        string unityTarget = project.TargetGuidByName(PBXProject.GetUnityTargetName());

        /////
        // Add Frameworks to Unity target
        string[] teakRequiredFrameworks = new string[] {
            "AdSupport",
            "AVFoundation",
            "MobileCoreServices",
            "StoreKit",
            "UserNotifications",
            "ImageIO"
        };
        project.AddFrameworksToTarget(unityTarget, teakRequiredFrameworks);

        /////
        // Modify plist
        string plistPath = pathToBuiltProject + "/Info.plist";
        File.WriteAllText(plistPath, AddTeakEntriesToPlist(File.ReadAllText(plistPath)));

        /////
        // Add Teak app extensions
        string[] teakExtensionCommonFrameworks = new string[] {"AdSupport", "AVFoundation", "CoreGraphics", "ImageIO", "MobileCoreServices", "StoreKit", "SystemConfiguration", "UIKit", "UserNotifications"};

        AddTeakExtensionToProjectTarget("TeakNotificationService",
                                        teakExtensionCommonFrameworks,
                                        project, unityTarget);

        AddTeakExtensionToProjectTarget("TeakNotificationContent",
                                        new string[] {"UserNotificationsUI"}.Concat(teakExtensionCommonFrameworks).ToArray(),
                                        project, unityTarget);

        /////
        // Write out modified project
        project.WriteToFile(projectPath);

        /////
        // Add/modify entitlements
        string entitlementsFileName = PBXProject.GetUnityTargetName() + ".entitlements";
        ProjectCapabilityManager capabilityManager = new ProjectCapabilityManager(projectPath, entitlementsFileName, PBXProject.GetUnityTargetName());
        capabilityManager.AddPushNotifications(UnityEngine.Debug.isDebugBuild);
        capabilityManager.AddAssociatedDomains(new string[] {"applinks:" + TeakSettings.ShortlinkDomain});
        capabilityManager.WriteToFile();
    }

    private static string AddTeakEntriesToPlist(string inputPlist) {
        PlistDocument plist = new PlistDocument();
        plist.ReadFromString(inputPlist);

        // Teak credentials
        plist.root.SetString("TeakAppId", TeakSettings.AppId);
        plist.root.SetString("TeakApiKey", TeakSettings.APIKey);

        // Teak URL Scheme
        AddURLSchemeToPlist(plist, "teak" + TeakSettings.AppId);

        // Add remote notifications background mode
        AddElementToArrayIfMissing(plist, "UIBackgroundModes", "remote-notification");

        return plist.WriteToString();
    }

    public static void AddURLSchemeToPlist(PlistDocument plist, string urlSchemeToAdd) {
        // Get/create array of URL types
        PlistElementArray urlTypesArray = null;
        if (!plist.root.values.ContainsKey("CFBundleURLTypes")) {
            urlTypesArray = plist.root.CreateArray("CFBundleURLTypes");
        } else {
            urlTypesArray = plist.root.values["CFBundleURLTypes"].AsArray();
        }
        if (urlTypesArray == null) {
            urlTypesArray = plist.root.CreateArray("CFBundleURLTypes");
        }

        // Get/create an entry in the array
        PlistElementDict urlTypesItems = null;
        if (urlTypesArray.values.Count == 0) {
            urlTypesItems = urlTypesArray.AddDict();
        } else {
            urlTypesItems = urlTypesArray.values[0].AsDict();

            if (urlTypesItems == null) {
                urlTypesItems = urlTypesArray.AddDict();
            }
        }

        // Get/create array of URL schemes
        PlistElementArray urlSchemesArray = null;
        if (!urlTypesItems.values.ContainsKey("CFBundleURLSchemes")) {
            urlSchemesArray = urlTypesItems.CreateArray("CFBundleURLSchemes");
        } else {
            urlSchemesArray = urlTypesItems.values["CFBundleURLSchemes"].AsArray();

            if (urlSchemesArray == null) {
                urlSchemesArray = urlTypesItems.CreateArray("CFBundleURLSchemes");
            }
        }

        // Add URL scheme
        if (!urlSchemesArray.ContainsElement(urlSchemeToAdd)) {
            urlSchemesArray.Add(urlSchemeToAdd);
        }
    }

    private static PlistElementArray AddElementToArrayIfMissing(PlistDocument plist, string key, object element) {
        PlistElementArray plistArray = null;
        if (!plist.root.values.ContainsKey(key)) {
            plistArray = plist.root.CreateArray(key);
        } else {
            plistArray = plist.root.values[key].AsArray();
        }

        if (!plistArray.ContainsElement(element)) {
            plistArray.Add(element);
        }

        return plistArray;
    }

    private static string AddTeakExtensionToProjectTarget(string name, string[] frameworks, PBXProject project, string target) {
        string __FILE__ = new StackTrace(new StackFrame(true)).GetFrame(0).GetFileName();
        string teakEditorIosPath = Path.GetDirectoryName(__FILE__) + "/iOS";
        string extensionSrcPath = teakEditorIosPath + "/" + name;

        /////
        // Create app extension target
        string extensionTarget = project.AddAppExtension(target, name,
                                 PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.iOS) + "." + name,
                                 extensionSrcPath + "/Info.plist");

        /////
        // Set TeamId
        project.SetTeamId(extensionTarget, PlayerSettings.iOS.appleDeveloperTeamID);

        /////
        // Add source files
        string[] extensionsIncluded = new string[] {".h", ".m", ".mm"};
        string[] fileEntries = Directory.GetFiles(extensionSrcPath);
        foreach (string fileName in fileEntries) {
            if (!extensionsIncluded.Contains(Path.GetExtension(fileName))) continue;

            project.AddFileToBuild(extensionTarget,
                                   project.AddFile(fileName, name + "/" + Path.GetFileName(fileName)));
        }

        /////
        // Add Frameworks
        project.AddFrameworksToTarget(extensionTarget, frameworks);

        /////
        // Add libTeak.a

        // If the 'Runtime' directory exists, this is coming from a UPM package
        string relativeTeakPath = GetRelativeAssetPath(Path.GetDirectoryName(Path.GetDirectoryName(__FILE__)));
        if (Directory.Exists(relativeTeakPath + "/Runtime")) {
            relativeTeakPath = "io.teak.unity.sdk/Runtime";
        }
        project.AddFileToBuild(extensionTarget, project.AddFile("libTeak.a", name + "/libTeak.a"));
        project.AddBuildProperty(extensionTarget, "LIBRARY_SEARCH_PATHS", "$(SRCROOT)/Libraries/" + relativeTeakPath + "/Plugins/iOS");

        /////
        // Build properties
        project.SetBuildProperty(extensionTarget, "IPHONEOS_DEPLOYMENT_TARGET", "10.0");
        project.AddBuildProperty(extensionTarget, "TARGETED_DEVICE_FAMILY", "1,2");

        // armv7 and armv7s do not support Notification Content Extensions
        project.AddBuildProperty(extensionTarget, "ARCHS", "arm64");

        return extensionTarget;
    }

    private static string GetRelativeAssetPath(string path) {
        Uri pathUri = new Uri(path);
        Uri projectUri = new Uri(Application.dataPath);
        string relativePath = projectUri.MakeRelativeUri(pathUri).ToString();
        string assets = "Assets";
        int index = relativePath.IndexOf(assets, StringComparison.Ordinal);
        return (index < 0) ? relativePath : relativePath.Remove(index, assets.Length + 1);
    }
}
