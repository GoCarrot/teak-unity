#region References
using System;
using System.IO;

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif
#endregion

#if UNITY_EDITOR
[InitializeOnLoad]
#endif
public class TeakSettings : ScriptableObject {
    const string teakSettingsAssetName = "TeakSettings";
    const string teakSettingsPath = "Resources";
    const string teakSettingsAssetExtension = ".asset";

    static TeakSettings Instance {
        get {
            if (mInstance == null) {
                mInstance = Resources.Load<TeakSettings>(teakSettingsAssetName);
                if (mInstance == null) {
                    // If not found, autocreate the asset object.
                    mInstance = CreateInstance<TeakSettings>();
#if UNITY_EDITOR
                    System.IO.Directory.CreateDirectory(Path.Combine(Application.dataPath, teakSettingsPath));

                    AssetDatabase.CreateAsset(mInstance, Path.Combine(
                                                  Path.Combine("Assets", teakSettingsPath),
                                                  teakSettingsAssetName + teakSettingsAssetExtension));

                    AssetDatabase.Refresh();
#endif
                }
            }
            return mInstance;
        }
    }

    public static string AppId {
        get { return Instance.mAppId; }
#if UNITY_EDITOR
        set {
            string valueTrim = value.Trim();
            if (valueTrim != Instance.mAppId) {
                Instance.mAppId = valueTrim;
                DirtyEditor();
            }
        }
#endif
    }

    public static string APIKey {
        get { return Instance.mAPIKey; }
#if UNITY_EDITOR
        set {
            string valueTrim = value.Trim();
            if (valueTrim != Instance.mAPIKey) {
                Instance.mAPIKey = valueTrim;
                DirtyEditor();
            }
        }
#endif
    }

    public static bool TraceLogging {
        get { return Instance.mTraceLogging; }
#if UNITY_EDITOR
        set {
            if (value != Instance.mTraceLogging) {
                Instance.mTraceLogging = value;
                DirtyEditor();
            }
        }
#endif
    }

    public static string ShortlinkDomain {
        get { return Instance.mShortlinkDomain; }
#if UNITY_EDITOR
        set {
            string valueTrim = value.Trim();
            if (valueTrim != Instance.mShortlinkDomain) {
                Instance.mShortlinkDomain = valueTrim;
                DirtyEditor();
            }
        }
#endif
    }

    public static bool JustShutUpIKnowWhatImDoing {
        get { return Instance.mJustShutUpIKnowWhatImDoing; }
#if UNITY_EDITOR
        set {
            if (value != Instance.mJustShutUpIKnowWhatImDoing) {
                Instance.mJustShutUpIKnowWhatImDoing = value;
                DirtyEditor();
            }
        }
#endif
    }

    public static bool EnableSDK5Behaviors {
        get { return Instance.mEnableSDK5Behaviors; }
#if UNITY_EDITOR
        set {
            if (value != Instance.mEnableSDK5Behaviors) {
                Instance.mEnableSDK5Behaviors = value;
                DirtyEditor();
            }
        }
#endif
    }

#if UNITY_EDITOR
    [MenuItem("Edit/Teak Settings...")]
    public static void Edit() {
        Selection.activeObject = Instance;
    }

    private static void DirtyEditor() {
        EditorUtility.SetDirty(Instance);
    }
#endif

    [SerializeField]
    private string mAppId = "";
    [SerializeField]
    private string mAPIKey = "";
    [SerializeField]
    private string mShortlinkDomain = "";
    [SerializeField]
    private bool mJustShutUpIKnowWhatImDoing = false;
    [SerializeField]
    private bool mEnableSDK5Behaviors = true;
    [SerializeField]
    private bool mTraceLogging = false;

    private static TeakSettings mInstance;
}
