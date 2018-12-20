#region References
using System;
using System.IO;
using System.Threading;
using System.Reflection;
using System.Diagnostics;
using System.Reflection.Emit;

using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
#endregion

[InitializeOnLoad]
public class TeakPackageBuilder : Editor {
    public static void BuildUnityPackage() {
        string[] assetPaths = new string[] {
            "Assets/Teak"
        };
        AssetDatabase.ExportPackage(assetPaths, "Teak.unitypackage", ExportPackageOptions.Recurse | ExportPackageOptions.IncludeDependencies);
    }
}
