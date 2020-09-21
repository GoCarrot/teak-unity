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

        mAndroidFoldout = !String.IsNullOrEmpty(TeakSettings.GCMSenderId) ||
                          !String.IsNullOrEmpty(TeakSettings.FirebaseAppId) ||
                          !String.IsNullOrEmpty(TeakSettings.FirebaseApiKey) ||
                          !String.IsNullOrEmpty(TeakSettings.FirebaseProjectId);
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

            GUIContent firebaseApiKeyContent = new GUIContent("Firebase API Key [?]",  "Your Firebase API Key, found on your Firebase Dashboard.");
            TeakSettings.FirebaseApiKey = EditorGUILayout.TextField(firebaseApiKeyContent, TeakSettings.FirebaseApiKey);

            GUIContent firebaseProjectIdContent = new GUIContent("Firebase Project Id [?]",  "Your Firebase Project Id, found on your Firebase Dashboard.");
            TeakSettings.FirebaseProjectId = EditorGUILayout.TextField(firebaseProjectIdContent, TeakSettings.FirebaseProjectId);
        }

        EditorGUILayout.Space();
        GUILayout.Label("Build Settings", EditorStyles.boldLabel);
        GUIContent justShutUpIKnowWhatImDoingContent = new GUIContent("Build Post-Processing [?]",  "When enabled, Teak will post-proces the Unity build and add dependencies, generate plist, XML, etc.");
        TeakSettings.JustShutUpIKnowWhatImDoing = !EditorGUILayout.Toggle(justShutUpIKnowWhatImDoingContent, !TeakSettings.JustShutUpIKnowWhatImDoing);
    }
}
