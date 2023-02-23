#region References
/// @cond hide_from_doxygen
// using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using TeakExtensions;
/// @endcond
#endregion

public partial class Teak {
    public class Channel {

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

        private static readonly ReadOnlyCollection<string> TypeToName = new ReadOnlyCollection<string>(
            new string[] {
                "push",
                "desktop_push",
                "platform_push",
                "email",
                "sms",
                "unknown"
            }
        );

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

        private static readonly ReadOnlyCollection<string> StateToName = new ReadOnlyCollection<string>(
            new string[] {
                "opt_out",
                "available",
                "opt_in",
                "absent",
                "unknown"
            }
        );

        /// <summary>
        /// Encapsulation of the state of a Teak marketing channel.
        /// </summary>
        public class Status {
            /// <summary>State of the marketing channel.</summary>
            public State State {
                get; private set;
            }

            /// <summary>The string version of the State of the marketing channel.</summary>
            public string StateName {
                get {
                    return StateToName[(int) this.State];
                }
            }

            /// <summary>`true` if there was a failure the last time a delivery was attempted to this channel.</summary>
            public bool DeliveryFault {
                get; private set;
            }

            private void Assignment(int stateAsInt, bool deliveryFault) {
                if (stateAsInt < 0 || stateAsInt > 4) stateAsInt = 4;
                this.State = (State) stateAsInt;
                this.DeliveryFault = deliveryFault;
            }

            /// <summary>Constructor</summary>
            public Status(State state, bool deliveryFault) {
                int stateAsInt = (int) state;
                Assignment(stateAsInt, deliveryFault);
            }

            /// <summary>Constructor</summary>
            public Status(Dictionary<string, object> json) {
                Assignment(StateName.IndexOf(json.Opt("state") as string),
                           Convert.ToBoolean(json.Opt("delivery_fault", "false")));
            }

            /// <summary>Dictionary representation of this object, suitable for JSON encoding.</summary>
            /// <returns>Dictionary representation of this object.</returns>
            public Dictionary<string, object> ToDictionary() {
                Dictionary<string, object> dict = new Dictionary<string, object>();
                dict.Add("state", this.StateName);
                dict.Add("delivery_fault", this.DeliveryFault);
                return dict;
            }
        }
    }

    /// <summary>
    /// Assign the opt-out state for a Teak marketing channel.
    /// </summary>
    public IEnumerator SetChannelState(Channel.State state, Channel.Type channel, System.Action<Channel.State> callback) {
        yield return null;
    }
}
