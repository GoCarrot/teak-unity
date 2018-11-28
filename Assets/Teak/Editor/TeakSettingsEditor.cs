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
using System.Net;
using System.Text;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
#endregion

[InitializeOnLoad]
[CustomEditor(typeof(TeakSettings))]
public class TeakSettingsEditor : Editor {
    static TeakSettingsEditor() {
        EditorApplication.update += EditorRunOnceOnLoad;
    }
    static void EditorRunOnceOnLoad() {
        EditorApplication.update -= EditorRunOnceOnLoad;

        mAndroidFoldout = !String.IsNullOrEmpty(TeakSettings.GCMSenderId) || !String.IsNullOrEmpty(TeakSettings.FirebaseAppId);
    }

    static bool mAndroidFoldout;

    public override void OnInspectorGUI() {

        GUILayout.Label("Settings", EditorStyles.boldLabel);
        TeakSettings.AppId = EditorGUILayout.TextField("Teak App Id", TeakSettings.AppId);
        TeakSettings.APIKey = EditorGUILayout.TextField("Teak API Key", TeakSettings.APIKey);
        TeakSettings.ShortlinkDomain = EditorGUILayout.TextField("Short Link Domain", TeakSettings.ShortlinkDomain);

        EditorGUILayout.Space();
        GUILayout.Label("Additional Settings", EditorStyles.boldLabel);
        mAndroidFoldout = EditorGUILayout.Foldout(mAndroidFoldout, "Android");
        if (mAndroidFoldout) {
            GUIContent gcmSenderIdContent = new GUIContent("GCM Sender Id [?]",  "Your GCM Sender Id, found on your Firebase Dashboard");
            TeakSettings.GCMSenderId = EditorGUILayout.TextField(gcmSenderIdContent, TeakSettings.GCMSenderId);

            GUIContent firebaseAppIdContent = new GUIContent("Firebase App Id [?]",  "Your Firebase App Id, found on your Firebase Dashboard.");
            TeakSettings.FirebaseAppId = EditorGUILayout.TextField(firebaseAppIdContent, TeakSettings.FirebaseAppId);
        }

        EditorGUILayout.Space();
        GUILayout.Label("Build Settings", EditorStyles.boldLabel);
        GUIContent justShutUpIKnowWhatImDoingContent = new GUIContent("Build Post-Processing [?]",  "When enabled, Teak will post-proces the Unity build and add dependencies, generate plist, XML, etc.");
        TeakSettings.JustShutUpIKnowWhatImDoing = !EditorGUILayout.Toggle(justShutUpIKnowWhatImDoingContent, !TeakSettings.JustShutUpIKnowWhatImDoing);
    }
}
