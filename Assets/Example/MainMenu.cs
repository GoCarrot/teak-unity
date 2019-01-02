using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using MiniJSON.Teak;

public class MainMenu : MonoBehaviour {
    public int buttonHeight = 250;

#if UNITY_IOS
    string pushTokenString = null;
#endif
    string teakUserId = null;
    string teakSdkVersion = null;
    string teakDeepLinkLaunch = null;
    string teakScheduledNotification = null;

    void Awake() {
        Teak.Instance.RegisterRoute("/store/:sku", "Store", "Open the store to an SKU", (Dictionary<string, object> parameters) => {
            Debug.Log("Got store deep link: " + Json.Serialize(parameters));
        });
    }

    void Start() {
        teakUserId = SystemInfo.deviceUniqueIdentifier;
        teakSdkVersion = "Teak SDK Version: " + Teak.Version;

        Teak.Instance.IdentifyUser(teakUserId);

        Teak.Instance.OnLaunchedFromNotification += OnLaunchedFromNotification;
        Teak.Instance.OnReward += OnReward;
    }

    void OnApplicationPause(bool isPaused) {
        if (isPaused) {
            // Pause
        } else {
            // Resume
        }
    }

    void OnLaunchedFromNotification(TeakNotification notification) {
        Debug.Log("OnLaunchedFromNotification: " + notification.CreativeId);
        teakScheduledNotification = null; // To get the UI back
    }

    void OnReward(TeakReward reward) {
        Debug.Log("OnReward: " + Json.Serialize(reward.Reward));
    }

#if UNITY_IOS
    void FixedUpdate() {
        if (pushTokenString == null) {
            byte[] token = UnityEngine.iOS.NotificationServices.deviceToken;
            if (token != null) {
                // Teak will take care of storing this automatically
                pushTokenString = System.BitConverter.ToString(token).Replace("-", "").ToLower();
            }
        }
    }
#endif

    void OnGUI() {
        GUILayout.BeginArea(new Rect(10, 10, Screen.width - 20, Screen.height - 20));

        GUILayout.Label(teakSdkVersion);
        GUILayout.Label(teakUserId);
        GUILayout.Label(teakDeepLinkLaunch);

#if UNITY_IOS
        if (pushTokenString != null) {
            GUILayout.Label("Push Token: " + pushTokenString);
        } else {
            if (GUILayout.Button("Request Push Notifications", GUILayout.Height(buttonHeight))) {
                UnityEngine.iOS.NotificationServices.RegisterForNotifications(UnityEngine.iOS.NotificationType.Alert |  UnityEngine.iOS.NotificationType.Badge |  UnityEngine.iOS.NotificationType.Sound);
            }
        }
#endif

        if (teakScheduledNotification == null) {
            if (GUILayout.Button("Simple Notification", GUILayout.Height(buttonHeight))) {
                StartCoroutine(TeakNotification.ScheduleNotification("test_none", "Simple push notification", 10, (TeakNotification.Reply reply) => {
                    if (reply.Status == TeakNotification.Reply.ReplyStatus.Ok) {
                        teakScheduledNotification = reply.Notifications[0].ScheduleId;
                    }
                }));
            }
        } else {
            if (GUILayout.Button("Cancel Notification " + teakScheduledNotification, GUILayout.Height(buttonHeight))) {
                StartCoroutine(TeakNotification.CancelScheduledNotification(teakScheduledNotification, (TeakNotification.Reply reply) => {
                    teakScheduledNotification = null;
                }));
            }
        }

        GUILayout.EndArea();
    }
}
