#region References
using System;
using System.IO;
using System.Xml;
using System.Xml.Linq;

using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
#endregion

public class TeakPostProcessScene {
    [PostProcessScene]
    public static void OnPostprocessScene() {
        if (!mRanThisBuild) {
            mRanThisBuild = true;

            if (!TeakSettings.JustShutUpIKnowWhatImDoing) {
                if (string.IsNullOrEmpty(TeakSettings.AppId)) {
                    Debug.LogError("Teak App Id needs to be assigned in the Edit/Teak menu.");
                }

                if (string.IsNullOrEmpty(TeakSettings.APIKey)) {
                    Debug.LogError("Teak API Key needs to be assigned in the Edit/Teak menu.");
                }

                Directory.CreateDirectory(Path.Combine(Application.dataPath, "Plugins/Android/res/values"));
                XDocument doc = new XDocument(
                    new XElement("resources",
                                 new XElement("string", TeakSettings.AppId, new XAttribute("name", "io_teak_app_id")),
                                 new XElement("string", TeakSettings.APIKey, new XAttribute("name", "io_teak_api_key")),
                                 String.IsNullOrEmpty(TeakSettings.GCMSenderId) ? null : new XElement("string", TeakSettings.GCMSenderId, new XAttribute("name", "io_teak_gcm_sender_id")),
                                 String.IsNullOrEmpty(TeakSettings.FirebaseAppId) ? null : new XElement("string", TeakSettings.FirebaseAppId, new XAttribute("name", "io_teak_firebase_app_id")),
                                 String.IsNullOrEmpty(TeakSettings.FirebaseApiKey) ? null : new XElement("string", TeakSettings.FirebaseApiKey, new XAttribute("name", "io_teak_firebase_api_key")),
                                 String.IsNullOrEmpty(TeakSettings.FirebaseProjectId) ? null : new XElement("string", TeakSettings.FirebaseProjectId, new XAttribute("name", "io_teak_firebase_project_id"))
                                )
                );
                doc.Save(Path.Combine(Application.dataPath, "Plugins/Android/res/values/teak.xml"));
            }
        }
    }

    [PostProcessBuild]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuildProject) {
        mRanThisBuild = false;
    }

    private static bool mRanThisBuild = false;
}
