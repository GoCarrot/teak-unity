#region References
/// @cond hide_from_doxygen
using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using MiniJSON.Teak;

#if UNITY_EDITOR
using System.IO;
using System.Net;
using System.Text;
#endif
/// @endcond
#endregion

/// <summary>
/// Interface for manipulating notifications from Teak.
/// </summary>
public partial class TeakNotification {
    public bool Incentivized { get; set; }
    public string ScheduleId { get; set; }
    public string CreativeId { get; set; }
    public string ChannelName { get; set; }
    public string RewardId { get; set; }
    public string DeepLink { get; set; }

    public override string ToString() {
        string formatString = "{{ Incentivized = '{0}', ScheduleId = '{1}', CreativeId = '{2}', ChannelName = '{3}', RewardId = '{4}', DeepLink = '{5}' }}";
        return string.Format(formatString,
            this.Incentivized,
            this.ScheduleId,
            this.CreativeId,
            this.ChannelName,
            this.RewardId,
            this.DeepLink);
    }

    public partial class Reply {
        public enum ReplyStatus {
            Ok,
            UnconfiguredKey,
            InvalidDevice,
            InternalError
        }

        public struct Notification {
            public string ScheduleId;
            public string CreativeId;

            public override string ToString() {
                string formatString = "{{ ScheduleId = '{0}', CreativeId = '{1}' }}";
                return string.Format(formatString,
                    this.ScheduleId,
                    this.CreativeId);
            }
        }

        public ReplyStatus Status { get; set; }
        public List<Notification> Notifications { get; set; }
    }

    // Returns an id that can be used to cancel a scheduled notification
    public static IEnumerator ScheduleNotification(string creativeId, string defaultMessage, long delayInSeconds, System.Action<Reply> callback) {
        if (Teak.Instance.Trace) {
            Debug.Log("[TeakNotification] ScheduleNotification(" + creativeId + ", " + defaultMessage + ", " + delayInSeconds + ")");
        }

#if UNITY_EDITOR
        yield return null;
#elif UNITY_ANDROID
        string data = null;
        string status = null;
        AndroidJavaClass teakNotification = new AndroidJavaClass("io.teak.sdk.TeakNotification");
        AndroidJavaObject future = teakNotification.CallStatic<AndroidJavaObject>("scheduleNotification", creativeId, defaultMessage, delayInSeconds);
        if (future != null) {
            while (!future.Call<bool>("isDone")) yield return null;

            try {
                Dictionary<string, object> json = Json.TryDeserialize(future.Call<string>("get")) as Dictionary<string, object>;
                data = json["data"] as string;
                status = json["status"] as string;
            } catch {
                status = "error.internal";
                data = null;
            }
        }

        SafePerformCallback("schedule_notification.local", callback, data, status, creativeId);
#elif UNITY_IPHONE
        string data = null;
        string status = null;
        IntPtr notif = TeakNotificationSchedule_Retained(creativeId, defaultMessage, delayInSeconds);
        if (notif != IntPtr.Zero) {
            while (!TeakNotificationIsCompleted(notif)) yield return null;
            data = Marshal.PtrToStringAnsi(TeakNotificationGetTeakNotifId(notif));
            status = Marshal.PtrToStringAnsi(TeakNotificationGetStatus(notif));
            TeakRelease(notif);
        }

        SafePerformCallback("schedule_notification.local", callback, data, status, creativeId);
#elif UNITY_WEBGL
        string callbackId = DateTime.Now.Ticks.ToString();
        webGlCallbackMap.Add(callbackId, callback);
        TeakNotificationSchedule(callbackId, creativeId, defaultMessage, delayInSeconds);
        yield return null;
#endif
    }

    public static IEnumerator ScheduleNotification(string creativeId, long delayInSeconds, string[] userIds, System.Action<Reply> callback) {
        if (Teak.Instance.Trace) {
            Debug.Log("[TeakNotification] ScheduleNotification(" + creativeId + ", " + delayInSeconds + ", " + userIds + ")");
        }

#if UNITY_EDITOR
        yield return null;
#elif UNITY_ANDROID
        string data = null;
        string status = null;
        AndroidJavaClass teakNotification = new AndroidJavaClass("io.teak.sdk.TeakNotification");
        AndroidJavaObject future = teakNotification.CallStatic<AndroidJavaObject>("scheduleNotification", creativeId, delayInSeconds, userIds);
        if (future != null) {
            while (!future.Call<bool>("isDone")) yield return null;

            try {
                Dictionary<string, object> json = Json.TryDeserialize(future.Call<string>("get")) as Dictionary<string, object>;
                data = json["data"] as string;
                status = json["status"] as string;
            } catch {
                status = "error.internal";
                data = null;
            }
        }

        SafePerformCallback("schedule_notification.long_distance", callback, data, status, creativeId);
#elif UNITY_IPHONE
        string data = null;
        string status = null;
        IntPtr notif = TeakNotificationScheduleLongDistance_Retained(creativeId, delayInSeconds, userIds, userIds.Length);
        if (notif != IntPtr.Zero) {
            while (!TeakNotificationIsCompleted(notif)) yield return null;
            data = Marshal.PtrToStringAnsi(TeakNotificationGetTeakNotifId(notif));
            status = Marshal.PtrToStringAnsi(TeakNotificationGetStatus(notif));
            TeakRelease(notif);
        }

        SafePerformCallback("schedule_notification.long_distance", callback, data, status, creativeId);
#elif UNITY_WEBGL
        string callbackId = DateTime.Now.Ticks.ToString();
        webGlCallbackMap.Add(callbackId, callback);
        TeakNotificationScheduleLongDistance(callbackId, creativeId, Json.Serialize(userIds), delayInSeconds);
        yield return null;
#endif
    }

    // Cancel an existing notification
    public static IEnumerator CancelScheduledNotification(string scheduleId, System.Action<Reply> callback) {
        if (Teak.Instance.Trace) {
            Debug.Log("[TeakNotification] CancelScheduledNotification(" + scheduleId + ")");
        }

#if UNITY_EDITOR
        yield return null;
#elif UNITY_ANDROID
        string data = null;
        string status = null;
        AndroidJavaClass teakNotification = new AndroidJavaClass("io.teak.sdk.TeakNotification");
        AndroidJavaObject future = teakNotification.CallStatic<AndroidJavaObject>("cancelNotification", scheduleId);
        if (future != null) {
            while (!future.Call<bool>("isDone")) yield return null;
            try {
                Dictionary<string, object> json = Json.TryDeserialize(future.Call<string>("get")) as Dictionary<string, object>;
                data = json["data"] as string;
                status = json["status"] as string;
            } catch {
                status = "error.internal";
                data = null;
            }
        }

        SafePerformCallback("cancel_notification", callback, data, status, null);
#elif UNITY_IPHONE
        string data = null;
        string status = null;
        IntPtr notif = TeakNotificationCancel_Retained(scheduleId);
        if (notif != IntPtr.Zero) {
            while (!TeakNotificationIsCompleted(notif)) yield return null;
            data = Marshal.PtrToStringAnsi(TeakNotificationGetTeakNotifId(notif));
            status = Marshal.PtrToStringAnsi(TeakNotificationGetStatus(notif));
            TeakRelease(notif);
        }

        SafePerformCallback("cancel_notification", callback, data, status, null);
#elif UNITY_WEBGL
        string callbackId = DateTime.Now.Ticks.ToString();
        webGlCallbackMap.Add(callbackId, callback);
        TeakNotificationCancel(callbackId, scheduleId);
        yield return null;
#endif
    }

    // Cancel all scheduled notifications
    public static IEnumerator CancelAllScheduledNotifications(System.Action<Reply> callback) {
        if (Teak.Instance.Trace) {
            Debug.Log("[TeakNotification] CancelAllScheduledNotifications()");
        }

#if UNITY_EDITOR
        yield return null;
#elif UNITY_ANDROID
        string data = null;
        string status = null;
        AndroidJavaClass teakNotification = new AndroidJavaClass("io.teak.sdk.TeakNotification");
        AndroidJavaObject future = teakNotification.CallStatic<AndroidJavaObject>("cancelAll");
        if (future != null) {
            while (!future.Call<bool>("isDone")) yield return null;
            try {
                Dictionary<string, object> json = Json.TryDeserialize(future.Call<string>("get")) as Dictionary<string, object>;
                data = Json.Serialize(json["data"]);
                status = json["status"] as string;
            } catch {
                status = "error.internal";
                data = null;
            }
        }

        SafePerformCallback("cancel_all_notifications", callback, data, status, null);
#elif UNITY_IPHONE
        string data = null;
        string status = null;
        IntPtr notif = TeakNotificationCancelAll_Retained();
        if (notif != IntPtr.Zero) {
            while (!TeakNotificationIsCompleted(notif)) yield return null;
            data = Marshal.PtrToStringAnsi(TeakNotificationGetTeakNotifId(notif));
            status = Marshal.PtrToStringAnsi(TeakNotificationGetStatus(notif));
            TeakRelease(notif);
        }

        SafePerformCallback("cancel_all_notifications", callback, data, status, null);
#elif UNITY_WEBGL
        string callbackId = DateTime.Now.Ticks.ToString();
        webGlCallbackMap.Add(callbackId, callback);
        TeakNotificationCancelAll(callbackId);
        yield return null;
#endif
    }

    /// @cond hide_from_doxygen
    private static void SafePerformCallback(string method, System.Action<Reply> callback, string data, string status, string creativeId) {
        try {
            callback(new Reply(status, data, creativeId));
        } catch (Exception e) {
            Dictionary<string, object> extras = new Dictionary<string, object>();
            extras["data"] = data;
            extras["status"] = status;
            if (creativeId != null) {
                extras["creative_id"] = creativeId;
            }
            Teak.Instance.ReportCallbackError(method, e, extras);
        }
    }

#if UNITY_IOS
    [DllImport ("__Internal")]
    private static extern IntPtr TeakNotificationSchedule_Retained(string creativeId, string message, long delay);

    [DllImport ("__Internal")]
    private static extern IntPtr TeakNotificationScheduleLongDistance_Retained(string creativeId, long delayInSeconds, string[] userIds, int lenUserIds);

    [DllImport ("__Internal")]
    private static extern IntPtr TeakNotificationCancel_Retained(string scheduleId);

    [DllImport ("__Internal")]
    private static extern IntPtr TeakNotificationCancelAll_Retained();

    [DllImport ("__Internal")]
    private static extern void TeakRelease(IntPtr obj);

    [DllImport ("__Internal")]
    private static extern bool TeakNotificationIsCompleted(IntPtr notif);

    [DllImport ("__Internal")]
    private static extern IntPtr TeakNotificationGetTeakNotifId(IntPtr notif);

    [DllImport ("__Internal")]
    private static extern IntPtr TeakNotificationGetStatus(IntPtr notif);
#elif UNITY_WEBGL
    [DllImport ("__Internal")]
    private static extern void TeakNotificationSchedule(string callbackId, string creativeId, string message, long delay);

    [DllImport ("__Internal")]
    private static extern IntPtr TeakNotificationScheduleLongDistance(string callbackId, string creativeId, string jsonUserIds, long delay);

    [DllImport ("__Internal")]
    private static extern void TeakNotificationCancel(string callbackId, string scheduleId);

    [DllImport ("__Internal")]
    private static extern void TeakNotificationCancelAll(string callbackId);

    private static Dictionary<string, System.Action<Reply>> webGlCallbackMap = new Dictionary<string, System.Action<Reply>>();
    public static void WebGLCallback(string callbackId, string status, string data, string creativeId) {
        if (webGlCallbackMap.ContainsKey(callbackId)) {
            System.Action<Reply> callback = webGlCallbackMap[callbackId];
            webGlCallbackMap.Remove(callbackId);
            callback(new Reply(status, data, creativeId));
        }
    }
#endif
    /// @endcond

    /// @cond hide_from_doxygen
    public partial class Reply {
        public Reply(string status, string data, string creativeId = null) {
            this.Status = ReplyStatus.InternalError;
            switch (status) {
                case "ok":
                    this.Status = ReplyStatus.Ok;
                    break;
                case "unconfigured_key":
                    this.Status = ReplyStatus.UnconfiguredKey;
                    break;
                case "invalid_device":
                    this.Status = ReplyStatus.InvalidDevice;
                    break;
            }

            if (this.Status == ReplyStatus.Ok) {
                List<object> replyList = Json.TryDeserialize(data) as List<object>;
                if (replyList != null) {
                    // Data contains array of pairs
                    this.Notifications = new List<Notification>();
                    foreach (object e in replyList) {
                        Dictionary<string, object> entry = e as Dictionary<string, object>;
                        if (entry != null) {
                            this.Notifications.Add(new Notification { ScheduleId = entry["schedule_id"].ToString(), CreativeId = entry["creative_id"] as string });
                        } else {
                            this.Notifications.Add(new Notification { ScheduleId = e as string, CreativeId = creativeId });
                        }
                    }
                } else {
                    this.Notifications = new List<Notification> {
                        new Notification { ScheduleId = data, CreativeId = creativeId }
                    };
                }
            }
        }
    }
    /// @endcond
}
