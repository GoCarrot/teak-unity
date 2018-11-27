#region License
/* Teak -- Copyright (C) 2016 GoCarrot Inc.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
#endregion

#region References
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using UnityEditor.iOS.Xcode.Extensions;

using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
#endregion

public class TeakPostProcessBuild
{
    [PostProcessBuild(100)]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
    {
        if (TeakSettings.JustShutUpIKnowWhatImDoing) return;
        if(target != BuildTarget.iOS) return;

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
        project = AddFrameworksToProjectTarget(teakRequiredFrameworks, project, unityTarget);

        /////
        // Modify plist
        string plistPath = pathToBuiltProject + "/Info.plist";
        File.WriteAllText(plistPath, AddTeakEntriesToPlist(File.ReadAllText(plistPath)));

        /////
        // Add Teak app extensions
        AddTeakExtensionToProjectTarget("TeakNotificationService",
            new string[] {"MobileCoreServices", "UserNotifications", "UIKit", "SystemConfiguration"},
            project, unityTarget);

        AddTeakExtensionToProjectTarget("TeakNotificationContent",
            new string[] {"UserNotifications", "UserNotificationsUI", "AVFoundation", "UIKit", "ImageIO", "CoreGraphics"},
            project, unityTarget);

        /////
        // Write out modified project
        project.WriteToFile(projectPath);
    }

    private static PBXProject AddFrameworksToProjectTarget(string[] frameworks, PBXProject project, string target) {
        foreach (string f in frameworks) {
            string framework = f;
            if (!framework.EndsWith(".framework")) framework = framework + ".framework";
#if UNITY_2017_1_OR_NEWER
            if (!project.ContainsFramework(target, framework))
#else
            if (!project.HasFramework(framework))
#endif
            {
                project.AddFrameworkToProject(target, framework, false);
            }
        }

        return project;
    }

    private static string AddTeakEntriesToPlist(string inputPlist) {
        PlistDocument plist = new PlistDocument();
        plist.ReadFromString(inputPlist);

        // Teak credentials
        plist.root.SetString("TeakAppId", TeakSettings.AppId);
        plist.root.SetString("TeakApiKey", TeakSettings.APIKey);

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

        // Add Teak URL scheme
        urlSchemesArray.AddString("teak" + TeakSettings.AppId);

        return plist.WriteToString();
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
        AddFrameworksToProjectTarget(frameworks, project, extensionTarget);

        /////
        // Add libTeak.a
        project.AddFileToBuild(extensionTarget, project.AddFile("libTeak.a", name + "/libTeak.a"));
        project.AddBuildProperty(extensionTarget, "LIBRARY_SEARCH_PATHS", "$(SRCROOT)/Libraries/Teak/Plugins/iOS");

        /////
        // Build properties
        project.SetBuildProperty(extensionTarget, "IPHONEOS_DEPLOYMENT_TARGET", "10.0");
        project.AddBuildProperty(extensionTarget, "TARGETED_DEVICE_FAMILY", "1,2");

        // armv7 and armv7s do not support Notification Content Extensions
        project.AddBuildProperty(extensionTarget, "ARCHS", "arm64");

        return extensionTarget;
    }
}
