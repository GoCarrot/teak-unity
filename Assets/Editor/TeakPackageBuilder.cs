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
public class TeakPackageBuilder : Editor
{
    public static void BuildUnityPackage()
    {
        string[] assetPaths = new string[] {
            "Assets/Teak",
            "Assets/Plugins/WebGL/Teak.jslib",
            "Assets/Plugins/Android/teak.aar"
        };
        AssetDatabase.ExportPackage(assetPaths, "Teak.unitypackage", ExportPackageOptions.Recurse | ExportPackageOptions.IncludeDependencies);
    }
}
