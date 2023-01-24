#region References
/// @cond hide_from_doxygen
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using TeakExtensions;
/// @endcond
#endregion

public partial class Teak {
    /// <summary>
    /// Encapsulation of the state of a Teak marketing channel.
    /// </summary>
    public class ChannelStatus {
        /// <summary>Enum of possible state values for a Teak marketing channel.</summary>
        public enum ChannelState : int {
            OptOut = 0,
            Available = 1,
            OptIn = 2,
            Absent = 3,
            Unknown = 4
        }

        private static readonly ReadOnlyCollection<string> ChannelStatusName = new ReadOnlyCollection<string>(
            new string[] {
                "opt_out",
                "available",
                "opt_in",
                "absent",
                "unknown"
            }
        );

        /// <summary>State of the marketing channel.</summary>
        public ChannelState State {
            get; private set;
        }

        /// <summary>The string version of the State of the marketing channel.</summary>
        public string StateName {
            get {
                return ChannelStatusName[(int) this.State];
            }
        }

        /// <summary>`true` if there was a failure the last time a delivery was attempted to this channel.</summary>
        public bool DeliveryFault {
            get; private set;
        }

        private void Assignment(int stateAsInt, bool deliveryFault) {
            if (stateAsInt < 0 || stateAsInt > 4) stateAsInt = 4;
            this.State = (ChannelState) stateAsInt;
            this.DeliveryFault = deliveryFault;
        }

        /// <summary>Constructor</summary>
        public ChannelStatus(ChannelState state, bool deliveryFault) {
            int stateAsInt = (int) state;
            Assignment(stateAsInt, deliveryFault);
        }

        /// <summary>Constructor</summary>
        public ChannelStatus(Dictionary<string, object> json) {
            Assignment(ChannelStatusName.IndexOf(json.Opt("state") as string),
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
