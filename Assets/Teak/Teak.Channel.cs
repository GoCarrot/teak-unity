#region References
/// @cond hide_from_doxygen
using UnityEngine;

using System;
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
    /// Teak Marketing Channel Functionality
    /// </summary>
    public partial class Channel {

        /// <summary>Teak Marketing Channel Type</summary>
        public enum Type : int {
            /// <summary>Push notification channel for mobile devices</summary>
            MobilePush = 0,

            /// <summary>Push notification channel for desktop devices</summary>
            DesktopPush = 1,

            /// <summary>Push notification channel for the current platform</summary>
            PlatformPush = 2,

            /// <summary>Email channel</summary>
            Email = 3,

            /// <summary>SMS channel</summary>
            SMS = 4,

            /// <summary>Unknown</summary>
            Unknown = 5
        }

        /// <summary>Enum of possible state values for a Teak marketing channel.</summary>
        public enum State : int {
            /// <summary>The user has opted out of this channel</summary>
            OptOut = 0,

            /// <summary>This channel is available, but has not be explicitly opted-in.</summary>
            Available = 1,

            /// <summary>The user has opted in to this channel</summary>
            OptIn = 2,

            /// <summary>This channel does not exist for this user</summary>
            Absent = 3,

            /// <summary>Unknown</summary>
            Unknown = 4
        }

        /// @cond hide_from_doxygen
        internal static readonly ReadOnlyCollection<string> TypeToName = new ReadOnlyCollection<string>(
        new string[] {
            "push",
            "desktop_push",
            "platform_push",
            "email",
            "sms",
            "unknown"
        }
        );

        internal static readonly ReadOnlyCollection<string> StateToName = new ReadOnlyCollection<string>(
        new string[] {
            "opt_out",
            "available",
            "opt_in",
            "absent",
            "unknown"
        }
        );
        /// @endcond

        /// <summary>
        /// Encapsulation of the state of a Teak marketing channel.
        /// </summary>
        public class Status {
            /// <summary>State of the marketing channel.</summary>
            public State State {
                get; private set;
            }

            /// <summary>The string version of the state of the marketing channel.</summary>
            public string StateName {
                get {
                    return StateToName[(int) this.State];
                }
            }

            /// <summary>The states of categories within the marketing channel.</summary>
            public Dictionary<string, string> Categories {
                get; private set;
            }

            /// <summary>The category state within the marketing channel.</summary>
            public State this[string category] {
                get {
                    if (this.Categories == null || !this.Categories.ContainsKey(category)) {
                        return State.Unknown;
                    }
                    int stateAsInt = StateName.IndexOf(this.Categories[category]);
                    if (stateAsInt < 0 || stateAsInt > 4) { stateAsInt = 4; }
                    return (State) stateAsInt;
                }
            }

            /// <summary>`true` if there was a failure the last time a delivery was attempted to this channel.</summary>
            public bool DeliveryFault {
                get; private set;
            }

            internal Status(Dictionary<string, object> json) {
                int stateAsInt = StateName.IndexOf(json.Opt("state", "unknown") as string);

                if (stateAsInt < 0 || stateAsInt > 4) { stateAsInt = 4; }
                this.State = (State) stateAsInt;
                this.DeliveryFault = Convert.ToBoolean(json.Opt("delivery_fault", "false"));

                if (json.ContainsKey("categories")) {
                    Dictionary<string, object> dict = json["categories"] as Dictionary<string, object>;
                    this.Categories = new Dictionary<string, string>();
                    if (dict != null) {
                        foreach (KeyValuePair<string, object> keyValuePair in dict) {
                            this.Categories.Add(keyValuePair.Key, keyValuePair.Value.ToString());
                        }
                    }
                }
            }

            /// <summary>Dictionary representation of this object, suitable for JSON encoding.</summary>
            /// <returns>Dictionary representation of this object.</returns>
            public Dictionary<string, object> ToDictionary() {
                Dictionary<string, object> dict = new Dictionary<string, object>();
                dict.Add("state", this.StateName);
                dict.Add("categories", this.Categories);
                dict.Add("delivery_fault", this.DeliveryFault);
                return dict;
            }
        }

        /// <summary>Reply to a <see cref="Teak.SetChannelState"/> call.</summary>
        public class Reply : IToJson {
            /// <summary>True if the call resulted in an error.</summary>
            public bool Error {
                get; private set;
            }

            /// <summary>The server-state of the marketing channel after the call.</summary>
            public State State {
                get; private set;
            }

            /// <summary>The type of marketing channel.</summary>
            public Type Channel {
                get; private set;
            }

            /// <summary>The category of the marketing channel.</summary>
            public string Category {
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

                bool error = false;
                bool.TryParse(json.Opt("error", "false") as string, out error);
                this.Error = error;

                int idx = StateToName.IndexOf(json.Opt("state", "unknown").ToString());
                this.State = (idx > -1) ? (Teak.Channel.State) idx : Teak.Channel.State.Unknown;

                idx = TypeToName.IndexOf(json.Opt("channel", "unknown").ToString());
                this.Channel = (idx > -1) ? (Teak.Channel.Type) idx : Teak.Channel.Type.Unknown;

                this.Category = json.Opt("category", null) as string;
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
                    {"state", "unknown"},
                    {"channel", "unknown"},
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
                {"state", "unknown"},
                {"channel", "unknown"},
                {
                    "errors", new Dictionary<string, object> {
                        {"unity", new string[] {"Undetermined error"}}
                    }
                }
            });
        }
    }

    /// <summary>
    /// Assign the opt-out state for a Teak marketing channel.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// \note You may only assign the values <see cref="Teak.Channel.Type.OptOut"/> and <see cref="Teak.Channel.Type.Available"/> to Push Channels; OptIn is not allowed.
    /// <param name="channel">The channel to which the new state is being assigned.</param>
    /// <param name="state">The opt-out state to assign to the marketing channel.</param>
    /// <param name="callback">A callback by which you will be informed of the result of the method.</param>
    public IEnumerator SetChannelState(Channel.Type channel, Channel.State state, System.Action<Channel.Reply> callback) {
        string stateAsString = Channel.StateToName[(int) state];
        string typeAsString = Channel.TypeToName[(int) channel];

        if (Teak.Instance.Trace) {
            Debug.Log("[Teak.Channel] SetChannelState(" + stateAsString + ", " + typeAsString + ")");
        }

        Channel.Reply reply = Channel.Reply.UndeterminedUnityError;
        Teak.Operation operation = null;

#if UNITY_EDITOR
#elif UNITY_WEBGL
        operation = new Teak.Operation(callbackId => {
            TeakSetStateForChannel_CallbackId(stateAsString, typeAsString, callbackId);
        });
#elif UNITY_ANDROID
        operation = new Teak.Operation(() => {
            AndroidJavaClass teak = new AndroidJavaClass("io.teak.sdk.Teak");
            return teak.CallStatic<AndroidJavaObject>("setChannelState", typeAsString, stateAsString);
        });
#elif UNITY_IPHONE
        operation = new Teak.Operation(() => {
            return TeakSetStateForChannel_Retained(stateAsString, typeAsString);
        });
#endif
        operation.OnDone += (result, exception) => {
            if (exception != null) {
                reply = Channel.Reply.ReplyWithErrorForException(exception);
            } else {
                reply = new Channel.Reply(result);
            }
            Teak.SafePerformCallback("teak.channel.setchannelstate", callback, reply);
        };
        while (!operation.IsDone) { yield return null; }
    }

    /// <summary>
    /// Assign the opt-out state for a category on a Teak marketing channel.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// \note You may only assign the values <see cref="Teak.Channel.Type.OptOut"/> and <see cref="Teak.Channel.Type.Available"/> to categories.
    /// <param name="channel">The channel to which the new state is being assigned.</param>
    /// <param name="category">The category in the channel to which the new state is being assigned.</param>
    /// <param name="state">The opt-out state to assign to the category.</param>
    /// <param name="callback">A callback by which you will be informed of the result of the method.</param>
    public IEnumerator SetCategoryState(Channel.Type channel, string category, Channel.State state, System.Action<Channel.Reply> callback) {
        string stateAsString = Channel.StateToName[(int) state];
        string typeAsString = Channel.TypeToName[(int) channel];

        if (Teak.Instance.Trace) {
            Debug.Log("[Teak.Channel] SetCategoryState(" + stateAsString + ", " + category + ", " + typeAsString + ")");
        }

        Channel.Reply reply = Channel.Reply.UndeterminedUnityError;
        Teak.Operation operation = null;
#if UNITY_EDITOR
#elif UNITY_WEBGL
        operation = new Teak.Operation(callbackId => {
            TeakSetCategoryForChannel_CallbackId(stateAsString, typeAsString, category, callbackId);
        });
#elif UNITY_ANDROID
        operation = new Teak.Operation(() => {
            AndroidJavaClass teak = new AndroidJavaClass("io.teak.sdk.Teak");
            return teak.CallStatic<AndroidJavaObject>("setCategoryState", typeAsString, category, stateAsString);
        });
#elif UNITY_IPHONE
        operation = new Teak.Operation(() => {
            return TeakSetStateForCategory_Retained(stateAsString, typeAsString, category);
        });
#endif
        operation.OnDone += (result, exception) => {
            if (exception != null) {
                reply = Channel.Reply.ReplyWithErrorForException(exception);
            } else {
                reply = new Channel.Reply(result);
            }
            Teak.SafePerformCallback("teak.channel.setcategorystate", callback, reply);
        };
        while (!operation.IsDone) { yield return null; }
    }

    /// @cond hide_from_doxygen
#if UNITY_WEBGL
    [DllImport ("__Internal")]
    private static extern void TeakSetStateForChannel_CallbackId(string state, string channel, string callbackId);
    [DllImport ("__Internal")]
    private static extern void TeakSetCategoryForChannel_CallbackId(string state, string channel, string category, string callbackId);
#elif UNITY_IPHONE
    [DllImport ("__Internal")]
    private static extern IntPtr TeakSetStateForChannel_Retained(string state, string channel);

    [DllImport ("__Internal")]
    private static extern IntPtr TeakSetStateForCategory_Retained(string state, string channel, string category);
#endif
    /// @endcond
}
