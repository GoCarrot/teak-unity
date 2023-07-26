#if UNITY_EDITOR 

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
    }

    public override void OnInspectorGUI() {
        EditorGUILayout.LabelField("Teak SDK Version: ", TeakVersion.Version);
        EditorGUILayout.Space();

        GUILayout.Label("Settings", EditorStyles.boldLabel);
        TeakSettings.AppId = EditorGUILayout.TextField("Teak App Id", TeakSettings.AppId);
        TeakSettings.APIKey = EditorGUILayout.TextField("Teak API Key", TeakSettings.APIKey);
        TeakSettings.ShortlinkDomain = EditorGUILayout.TextField("ShortLink Domain", TeakSettings.ShortlinkDomain);
        TeakSettings.TraceLogging = EditorGUILayout.Toggle("Trace Logging", TeakSettings.TraceLogging);

        EditorGUILayout.Space();
        GUILayout.Label("Build Settings", EditorStyles.boldLabel);
        GUIContent justShutUpIKnowWhatImDoingContent = new GUIContent("Build Post-Processing [?]",  "When enabled, Teak will post-proces the Unity build and add dependencies, generate plist, XML, etc.");
        TeakSettings.JustShutUpIKnowWhatImDoing = !EditorGUILayout.Toggle(justShutUpIKnowWhatImDoingContent, !TeakSettings.JustShutUpIKnowWhatImDoing);

        GUIContent sdk5BehaviorsContent = new GUIContent("Enable SDK 5 Behaviors [?]",  "When enabled, the Teak SDK will no longer automatically collect the Facebook Access Token from signed-in users.");
        TeakSettings.EnableSDK5Behaviors = EditorGUILayout.Toggle(sdk5BehaviorsContent, TeakSettings.EnableSDK5Behaviors);
    }
}

#endif // UNITY_EDITOR
