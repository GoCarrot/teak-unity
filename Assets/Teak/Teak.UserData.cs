#region References
/// @cond hide_from_doxygen
using System;
using System.Collections.Generic;
/// @endcond
#endregion

public partial class Teak {
    /// <summary>
    ///
    /// </summary>
    public class UserData {
        /// <summary>Arbitrary, per-user, information sent from the Teak server.</summary>
        public Dictionary<string, object> AdditionalData { get; private set; }

        /// <summary></summary>
        public ChannelStatus EmailStatus { get; private set; }

        /// <summary></summary>
        public ChannelStatus PushStatus { get; private set; }

        /// <summary></summary>
        public ChannelStatus SmsStatus { get; private set; }

        /// <summary>Push registration information for the current user, if available.</summary>
        public Dictionary<string, object> PushRegistration { get; private set; }

        internal UserData(Dictionary<string, object> json) {
            this.AdditionalData = json["additionalData"] as Dictionary<string, object>;
            this.EmailStatus = new ChannelStatus(json["emailStatus"] as Dictionary<string, object>);
            this.PushStatus = new ChannelStatus(json["pushStatus"] as Dictionary<string, object>);
            this.SmsStatus = new ChannelStatus(json["smsStatus"] as Dictionary<string, object>);
            this.PushRegistration = json["pushRegistration"] as Dictionary<string, object>;
        }


        /// <summary>Dictionary representation of this object, suitable for JSON encoding.</summary>
        /// <returns>Dictionary representation of this object.</returns>
        public Dictionary<string, object> ToDictionary() {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict.Add("additionalData", this.AdditionalData);
            dict.Add("emailStatus", this.EmailStatus.ToDictionary());
            dict.Add("pushStatus", this.PushStatus.ToDictionary());
            dict.Add("smsStatus", this.SmsStatus.ToDictionary());
            dict.Add("pushRegistration", this.PushRegistration);
            return dict;
        }
    }
}
