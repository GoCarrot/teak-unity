#region References
/// @cond hide_from_doxygen
using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using MiniJSON.Teak;
using TeakExtensions;
/// @endcond
#endregion

public partial class Teak {

    /// <summary>
    /// Teak Live Activity functionality.
    /// </summary>
    /// <remarks>
    /// Taps on a Live Activity that launch the app are tracked as clicks; the launching
    /// activity's <c>Activity.id</c> is surfaced as
    /// <see cref="TeakPostLaunchSummary.SystemActivityId"/> on
    /// <see cref="Teak.OnPostLaunchSummary"/>.
    /// </remarks>
    public partial class LiveActivity {

        /// <summary>Result of a call to a Live Activity API.</summary>
        public class Reply : IToJson {
            /// <summary>True if the call resulted in an error.</summary>
            public bool Error {
                get; private set;
            }

            /// <summary>A mapping of the argument or cause of the error to an array of strings explaining the errors.</summary>
            public Dictionary<string, List<string>> Errors {
                get; private set;
            }

            /// <summary>The number of pending updates canceled by <see cref="LiveActivity.CancelLiveActivityUpdates"/>, or null for other calls.</summary>
            public int? CanceledCount {
                get; private set;
            }

            /// <summary>The JSON received from the server.</summary>
            public Dictionary<string, object> Json {
                get; private set;
            }

            public Dictionary<string, object> toJson() {
                return this.Json;
            }

            /// @cond hide_from_doxygen
            public Reply(Dictionary<string, object> json) {
                this.Json = json;

                string status = json.Opt("status", "error") as string;
                this.Error = !"ok".Equals(status);
                this.Errors = Teak.Utils.ParseErrorsFromReply(json);

                if (json.ContainsKey("canceled")) {
                    try {
                        this.CanceledCount = Convert.ToInt32(json["canceled"]);
                    } catch (Exception) {
                        this.CanceledCount = null;
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

            public static Reply ReplyWithErrorForException(Exception e) {
                return new Reply(new Dictionary<string, object> {
                    {"status", "error"},
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
                {"status", "error"},
                {
                    "errors", new Dictionary<string, object> {
                        {"unity", new string[] {"Undetermined error"}}
                    }
                }
            });
        }

        /// <summary>
        /// Report a Live Activity push-to-update token to Teak.
        /// </summary>
        /// <remarks>
        /// Call this when a live activity starts and receives its push-to-update token, and
        /// again whenever the token rotates during the activity's lifetime.
        ///
        /// </remarks>
        /// <param name="activityId">A stable, game-chosen string naming the kind of live activity (e.g. "chest_timer").</param>
        /// <param name="pushToken">The push-to-update token bytes from ActivityKit's <c>Activity.pushTokenUpdates</c>.</param>
        /// <param name="systemActivityId">The OS-level per-instance activity identifier, from <c>Activity.id</c>.</param>
        /// <param name="callback">A callback invoked with the result of the call.</param>
        public static IEnumerator StartedLiveActivity(string activityId, byte[] pushToken, string systemActivityId, System.Action<Reply> callback) {
            int tokenLength = pushToken == null ? 0 : pushToken.Length;

            if (Teak.Instance.Trace) {
                Debug.Log("[Teak.LiveActivity] StartedLiveActivity(" + activityId + ", <" + tokenLength + " bytes>, " + systemActivityId + ")");
            }

#if !UNITY_EDITOR && UNITY_IPHONE
            Teak.Operation operation = new Teak.Operation(() => {
                return TeakStartedLiveActivity_Retained(activityId, pushToken, tokenLength, systemActivityId);
            });
            operation.OnDone += (result, exception) => {
                Reply reply;
                if (exception != null) {
                    reply = Reply.ReplyWithErrorForException(exception);
                } else {
                    reply = new Reply(result);
                }
                Teak.SafePerformCallback("teak.liveactivity.started", callback, reply);
            };
            while (!operation.IsDone) { yield return null; }
#else
            NoOpCallback(callback);
            yield break;
#endif
        }

        /// <summary>
        /// Report a Live Activity push-to-update token to Teak, using a hex-encoded token string.
        /// </summary>
        /// <remarks>
        /// Convenience overload for callers who already hold the token as a hex string
        /// (for example, persisted via <c>PlayerPrefs</c>). Decodes to bytes and delegates to
        /// <see cref="StartedLiveActivity(string, byte[], string, System.Action{Reply})"/>.
        /// </remarks>
        /// <param name="activityId">A stable, game-chosen string naming the kind of live activity.</param>
        /// <param name="pushTokenHex">The push-to-update token as a hex-encoded string.</param>
        /// <param name="systemActivityId">The OS-level per-instance activity identifier.</param>
        /// <param name="callback">A callback invoked with the result of the call.</param>
        public static IEnumerator StartedLiveActivity(string activityId, string pushTokenHex, string systemActivityId, System.Action<Reply> callback) {
            byte[] tokenBytes = null;
            Exception conversionError = null;
            try {
                tokenBytes = HexStringToBytes(pushTokenHex);
            } catch (Exception e) {
                conversionError = e;
            }

            if (conversionError != null) {
                Teak.SafePerformCallback("teak.liveactivity.started", callback, Reply.ReplyWithErrorForException(conversionError));
                yield break;
            }

            IEnumerator inner = StartedLiveActivity(activityId, tokenBytes, systemActivityId, callback);
            while (inner.MoveNext()) { yield return inner.Current; }
        }

        /// <summary>
        /// Schedule a single update to be delivered to a live activity at a future time.
        /// </summary>
        /// <remarks>
        /// Call between <see cref="StartedLiveActivity(string, byte[], string, System.Action{Reply})"/>
        /// and the activity ending. <paramref name="customData"/> is delivered as APNs
        /// <c>content-state</c>; <paramref name="systemData"/> carries Apple system fields
        /// (<c>event</c>, <c>stale-date</c>, <c>dismissal-date</c>).
        ///
        /// </remarks>
        /// <param name="activityId">A stable, game-chosen string naming the kind of live activity.</param>
        /// <param name="offset">Delay from server-now, in seconds, at which the update should be delivered.</param>
        /// <param name="customData">Game-defined content-state payload. Must contain only JSON-serializable values. Required.</param>
        /// <param name="systemData">Optional Apple system fields. May be null.</param>
        /// <param name="callback">A callback invoked with the result of the call.</param>
        public static IEnumerator ScheduleLiveActivityUpdate(string activityId, long offset, Dictionary<string, object> customData, Dictionary<string, object> systemData, System.Action<Reply> callback) {
            if (Teak.Instance.Trace) {
                Debug.Log("[Teak.LiveActivity] ScheduleLiveActivityUpdate(" + activityId + ", " + offset + ", " +
                          (customData == null ? "null" : "<customData>") + ", " +
                          (systemData == null ? "null" : "<systemData>") + ")");
            }

            if (customData == null) {
                Teak.SafePerformCallback("teak.liveactivity.schedule", callback, BuildFieldError("customData", "customData cannot be null"));
                yield break;
            }

            string customDataJson, systemDataJson;
            try {
                customDataJson = Json.Serialize(customData);
                systemDataJson = systemData == null ? null : Json.Serialize(systemData);
            } catch (Exception e) {
                Teak.SafePerformCallback("teak.liveactivity.schedule", callback, Reply.ReplyWithErrorForException(e));
                yield break;
            }

#if !UNITY_EDITOR && UNITY_IPHONE
            Teak.Operation operation = new Teak.Operation(() => {
                return TeakScheduleLiveActivityUpdate_Retained(activityId, offset, customDataJson, systemDataJson);
            });
            operation.OnDone += (result, exception) => {
                Reply reply;
                if (exception != null) {
                    reply = Reply.ReplyWithErrorForException(exception);
                } else {
                    reply = new Reply(result);
                }
                Teak.SafePerformCallback("teak.liveactivity.schedule", callback, reply);
            };
            while (!operation.IsDone) { yield return null; }
#else
            NoOpCallback(callback);
            yield break;
#endif
        }

        /// <summary>
        /// Cancel all pending scheduled updates for a live activity.
        /// </summary>
        /// <remarks>
        /// Scopes to the current user's updates for <paramref name="activityId"/>; updates
        /// scheduled for other users or other activity kinds are unaffected.
        ///
        /// The reply's <see cref="Reply.CanceledCount"/> carries the number of updates canceled
        /// by the server on success.
        ///
        /// </remarks>
        /// <param name="activityId">A stable, game-chosen string naming the kind of live activity whose pending updates should be canceled.</param>
        /// <param name="callback">A callback invoked with the result of the call.</param>
        public static IEnumerator CancelLiveActivityUpdates(string activityId, System.Action<Reply> callback) {
            if (Teak.Instance.Trace) {
                Debug.Log("[Teak.LiveActivity] CancelLiveActivityUpdates(" + activityId + ")");
            }

#if !UNITY_EDITOR && UNITY_IPHONE
            Teak.Operation operation = new Teak.Operation(() => {
                return TeakCancelLiveActivityUpdates_Retained(activityId);
            });
            operation.OnDone += (result, exception) => {
                Reply reply;
                if (exception != null) {
                    reply = Reply.ReplyWithErrorForException(exception);
                } else {
                    reply = new Reply(result);
                }
                Teak.SafePerformCallback("teak.liveactivity.cancel", callback, reply);
            };
            while (!operation.IsDone) { yield return null; }
#else
            NoOpCallback(callback);
            yield break;
#endif
        }

        /// @cond hide_from_doxygen
        private static void NoOpCallback(System.Action<Reply> callback) {
            Teak.SafePerformCallback("teak.liveactivity.noop", callback, new Reply(new Dictionary<string, object> {
                {"status", "ok"},
                {"noop", true}
            }));
        }

        // System.Convert.FromHexString landed in .NET 5; Unity 2022.3's .NET Standard 2.1
        // target does not include it, so we ship a minimal decoder.
        private static byte[] HexStringToBytes(string hex) {
            if (hex == null) {
                throw new ArgumentNullException("hex");
            }
            if (hex.Length % 2 != 0) {
                throw new ArgumentException("Hex string must have even length", "hex");
            }
            byte[] bytes = new byte[hex.Length / 2];
            for (int i = 0; i < bytes.Length; i++) {
                bytes[i] = (byte)((HexCharToInt(hex[i * 2]) << 4) | HexCharToInt(hex[i * 2 + 1]));
            }
            return bytes;
        }

        private static int HexCharToInt(char c) {
            if (c >= '0' && c <= '9') { return c - '0'; }
            if (c >= 'a' && c <= 'f') { return c - 'a' + 10; }
            if (c >= 'A' && c <= 'F') { return c - 'A' + 10; }
            throw new ArgumentException("Invalid hex character: " + c);
        }

        private static Reply BuildFieldError(string field, string message) {
            return new Reply(new Dictionary<string, object> {
                {"status", "error"},
                {
                    "errors", new Dictionary<string, object> {
                        {field, new string[] { message }}
                    }
                }
            });
        }

#if UNITY_IPHONE
        [DllImport ("__Internal")]
        private static extern IntPtr TeakStartedLiveActivity_Retained(
            string activityId,
            [MarshalAs(UnmanagedType.LPArray)] byte[] pushToken,
            int pushTokenLength,
            string systemActivityId);

        [DllImport ("__Internal")]
        private static extern IntPtr TeakScheduleLiveActivityUpdate_Retained(
            string activityId,
            long offset,
            string customDataJson,
            string systemDataJson);

        [DllImport ("__Internal")]
        private static extern IntPtr TeakCancelLiveActivityUpdates_Retained(string activityId);
#endif
        /// @endcond
    }
}
