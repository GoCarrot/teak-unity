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
public partial class TeakNotification
{
    public bool Incentivized { get; set; }
    public string ScheduleName { get; set; }
    public string CreativeName { get; set; }

    public partial class Reply
    {
        public enum ReplyStatus
        {
            Ok,
            UnconfiguredKey,
            InvalidDevice,
            InternalError
        }

        public struct Notification
        {
            public string ScheduleId;
            public string CreativeId;
        }

        public ReplyStatus Status { get; set; }
        public List<Notification> Notifications { get; set; }
    }

    // Returns an id that can be used to cancel a scheduled notification
    public static IEnumerator ScheduleNotification(string creativeId, string defaultMessage, long delayInSeconds, System.Action<Reply> callback)
    {
        string data = null;
        string status = null;
#if UNITY_EDITOR || UNITY_WEBGL
        yield return null;
#elif UNITY_ANDROID
        AndroidJavaClass teakNotification = new AndroidJavaClass("io.teak.sdk.TeakNotification");
        AndroidJavaObject future = teakNotification.CallStatic<AndroidJavaObject>("scheduleNotification", creativeId, defaultMessage, delayInSeconds);
        if(future != null)
        {
            while(!future.Call<bool>("isDone")) yield return null;

            try
            {
                Dictionary<string, object> json = Json.Deserialize(future.Call<string>("get")) as Dictionary<string, object>;
                data = json["data"] as string;
                status = json["status"] as string;
            }
            catch
            {
                status = "error.internal";
                data = null;
            }
        }
#elif UNITY_IPHONE
        IntPtr notif = TeakNotificationSchedule_Retained(creativeId, defaultMessage, delayInSeconds);
        if(notif != IntPtr.Zero)
        {
            while(!TeakNotificationIsCompleted(notif)) yield return null;
            data = Marshal.PtrToStringAnsi(TeakNotificationGetTeakNotifId(notif));
            status = Marshal.PtrToStringAnsi(TeakNotificationGetStatus(notif));
            TeakRelease(notif);
        }
#endif
        callback(new Reply(status, data, creativeId));
    }

    // Cancel an existing notification
    public static IEnumerator CancelScheduledNotification(string scheduleId, System.Action<Reply> callback)
    {
        string data = null;
        string status = null;
#if UNITY_EDITOR || UNITY_WEBGL
        yield return null;
#elif UNITY_ANDROID
        AndroidJavaClass teakNotification = new AndroidJavaClass("io.teak.sdk.TeakNotification");
        AndroidJavaObject future = teakNotification.CallStatic<AndroidJavaObject>("cancelNotification", scheduleId);
        if(future != null)
        {
            while(!future.Call<bool>("isDone")) yield return null;
            try
            {
                Dictionary<string, object> json = Json.Deserialize(future.Call<string>("get")) as Dictionary<string, object>;
                data = json["data"] as string;
                status = json["status"] as string;
            }
            catch
            {
                status = "error.internal";
                data = null;
            }
        }
#elif UNITY_IPHONE
        IntPtr notif = TeakNotificationCancel_Retained(scheduleId);
        if(notif != IntPtr.Zero)
        {
            while(!TeakNotificationIsCompleted(notif)) yield return null;
            data = Marshal.PtrToStringAnsi(TeakNotificationGetTeakNotifId(notif));
            status = Marshal.PtrToStringAnsi(TeakNotificationGetStatus(notif));
            TeakRelease(notif);
        }
#endif
        callback(new Reply(status, data, null));
    }

    // Cancel all scheduled notifications
    public static IEnumerator CancelAllScheduledNotifications(System.Action<Reply> callback)
    {
        string data = null;
        string status = null;
#if UNITY_EDITOR || UNITY_WEBGL
        yield return null;
#elif UNITY_ANDROID
        AndroidJavaClass teakNotification = new AndroidJavaClass("io.teak.sdk.TeakNotification");
        AndroidJavaObject future = teakNotification.CallStatic<AndroidJavaObject>("cancelAll");
        if(future != null)
        {
            while(!future.Call<bool>("isDone")) yield return null;
            try
            {
                Dictionary<string, object> json = Json.Deserialize(future.Call<string>("get")) as Dictionary<string, object>;
                data = Json.Serialize(json["data"]);
                status = json["status"] as string;
            }
            catch
            {
                status = "error.internal";
                data = null;
            }
        }
#elif UNITY_IPHONE
        IntPtr notif = TeakNotificationCancelAll_Retained();
        if(notif != IntPtr.Zero)
        {
            while(!TeakNotificationIsCompleted(notif)) yield return null;
            data = Marshal.PtrToStringAnsi(TeakNotificationGetTeakNotifId(notif));
            status = Marshal.PtrToStringAnsi(TeakNotificationGetStatus(notif));
            TeakRelease(notif);
        }
#endif
        callback(new Reply(status, data, null));
    }

    /// @cond hide_from_doxygen
#if UNITY_IOS
    [DllImport ("__Internal")]
    private static extern IntPtr TeakNotificationSchedule_Retained(string creativeId, string message, long delay);

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
#endif
    /// @endcond

    /// @cond hide_from_doxygen
    public partial class Reply
    {
        public Reply(string status, string data, string creativeId = null)
        {
            this.Status = ReplyStatus.InternalError;
            switch(status)
            {
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
                List<object> replyList = Json.Deserialize(data) as List<object>;
                if (replyList != null) {
                    // Data contains array of pairs
                    this.Notifications = new List<Notification>();
                    foreach (object e in replyList) {
                        Dictionary<string, object> entry = e as Dictionary<string, object>;
                        this.Notifications.Add(new Notification { ScheduleId = entry["schedule_id"] as string, CreativeId = entry["creative_id"] as string });
                    }
                } else {
                    this.Notifications = new List<Notification>
                    {
                        new Notification { ScheduleId = data, CreativeId = creativeId }
                    };
                }
            }
        }
    }
    /// @endcond
}
