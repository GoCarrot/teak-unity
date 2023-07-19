#region References
/// @cond hide_from_doxygen
using UnityEngine;

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;

using MiniJSON.Teak;
using TeakExtensions;
/// @endcond
#endregion

public partial class Teak {

    /// <summary>
    /// Teak Notification scheduling.
    /// </summary>
    public class Notification {
        /// <summary>Result of a function call working with notifications from the Teak server.</summary>
        public class Reply : IToJson {
            /// <summary>True if the call resulted in an error.</summary>
            public bool Error {
                get; private set;
            }

            // <summary>A mapping of the argument or cause of the error to an array of strings explaining the errors.</summary>
            public Dictionary<string, List<string>> Errors {
                get; private set;
            }

            /// <summary>The notification ids related to this call.</summary>
            public List<string> ScheduleIds {
                get; private set;
            }

            /// <summary>The JSON received from the server.</summary>
            public Dictionary<string, object> Json {
                get; private set;
            }

            public Dictionary<string, object> toJson() {
                return this.Json;
            }

            internal Reply(Dictionary<string, object> json) {
                this.Json = json;

                this.Error = Convert.ToBoolean(json.Opt("error", "true"));
                this.Errors = Teak.Utils.ParseErrorsFromReply(json);

                if (json.ContainsKey("data")) {
                    this.ScheduleIds = new List<string> { json["data"] as string };
                } else if (json.ContainsKey("schedule_ids")) {
                    List<object> scheduleIds = json["schedule_ids"] as List<object>;
                    if (scheduleIds != null) {
                        this.ScheduleIds = scheduleIds.Cast<string>().ToList();
                    }
                }
            }

            /// <summary>
            /// Returns a string that represents the current object.
            /// </summary>
            /// <returns>A string that represents the current object.</returns>
            public override string ToString() {
                return MiniJSON.Teak.Json.Serialize(this.Json);
            }

            /// @cond hide_from_doxygen
            public static Reply ReplyWithErrorForException(Exception e) {
                return new Reply(new Dictionary<string, object> {
                    {"error", true},
                    {
                        "errors", new Dictionary<string, object> {
                            {"unity", new string[] { e.ToString() }}
                        }
                    }
                });
            }
            /// @endcond

            /// <summary>Unknown Unity-related error</summary>
            public static Reply UndeterminedUnityError = new Reply(new Dictionary<string, object> {
                {"error", true},
                {
                    "errors", new Dictionary<string, object> {
                        {"unity", new string[] {"Undetermined error"}}
                    }
                }
            });
        }

        /// <summary>
        /// Schedule a notification to send to the logged in user for a future time.
        /// </summary>
        /// <remarks>
        /// \note The maximum delay for scheduling a notification is 30 days.
        /// </remarks>
        /// <param name="scheduleName">A value used to identify the message creative in the Teak CMS e.g. "daily_bonus".</param>
        /// <param name="delayInSeconds">The number of seconds from the current time before the notification should be sent.</param>
        /// <param name="personalizationData">Optional information which can be used for templating on the server; or null.</param>
        /// <param name="callback">The callback to be called after the notification is scheduled.</param>
        public static IEnumerator Schedule(string scheduleName, long delayInSeconds, Dictionary<string, object> personalizationData, System.Action<Reply> callback) {
            if (Teak.Instance.Trace) {
                Debug.Log("[Teak.Notification] Schedule(" + scheduleName + ", " + delayInSeconds + ", " + personalizationData == null ? "{}" : MiniJSON.Teak.Json.Serialize(personalizationData) + ")");
            }

            Reply reply = Reply.UndeterminedUnityError;
            Teak.Operation operation = null;

#if UNITY_EDITOR
#elif UNITY_WEBGL
            operation = new Teak.Operation(callbackId => {
                TeakNotificationScheduleWithPersonalization(callbackId, scheduleName, delayInSeconds,
                        personalizationData == null ? null : Json.Serialize(personalizationData));
            });
#elif UNITY_ANDROID
            operation = new Teak.Operation(() => {
                AndroidJavaClass teak = new AndroidJavaClass("io.teak.sdk.Teak$Notification");
                return teak.CallStatic<AndroidJavaObject>("schedule", scheduleName, delayInSeconds,
                        personalizationData == null ? null : Json.Serialize(personalizationData));
            });
#elif UNITY_IPHONE
            operation = new Teak.Operation(() => {
                return TeakNotificationSchedulePersonalizationData_Retained(scheduleName, delayInSeconds,
                        personalizationData == null ? null : Json.Serialize(personalizationData));
            });
#endif
            operation.OnDone += (result, exception) => {
                if (exception != null) {
                    reply = Reply.ReplyWithErrorForException(exception);
                } else {
                    reply = new Reply(result);
                }
                Teak.SafePerformCallback("teak.notification.schedule", callback, reply);
            };
            while (!operation.IsDone) { yield return null; }
        }

        /// @cond hide_from_doxygen
#if UNITY_IOS
        [DllImport ("__Internal")]
        private static extern IntPtr TeakNotificationSchedulePersonalizationData_Retained(string scheduleName, long delay, string personalizationDataJson);
#elif UNITY_WEBGL
        [DllImport ("__Internal")]
        private static extern void TeakNotificationScheduleWithPersonalization(string callbackId, string scheduleName, long delay, string personalizationDataJson);
#endif

        /// @endcond
    }
}
