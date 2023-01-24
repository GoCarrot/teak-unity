#region References
/// @cond hide_from_doxygen
using System;
using System.Collections.Generic;

using TeakExtensions;
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

        private static Dictionary<string, object> EmptyDictionary = new Dictionary<string, object>();

        internal UserData(Dictionary<string, object> json) {
            this.AdditionalData = json.Opt("additionalData", EmptyDictionary) as Dictionary<string, object>;
            this.EmailStatus = new ChannelStatus(json.Opt("emailStatus", EmptyDictionary) as Dictionary<string, object>);
            this.PushStatus = new ChannelStatus(json.Opt("pushStatus", EmptyDictionary) as Dictionary<string, object>);
            this.SmsStatus = new ChannelStatus(json.Opt("smsStatus", EmptyDictionary) as Dictionary<string, object>);
            this.PushRegistration = json.Opt("pushRegistration", EmptyDictionary) as Dictionary<string, object>;
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
