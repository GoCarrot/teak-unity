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

        /// <summary>True if the user has opted out of Teak email campaigns.</summary>
        public bool OptOutEmail { get; private set; }

        /// <summary>True if the user has opted out of Teak push notification campaigns.</summary>
        public bool OptOutPush { get; private set; }

        internal UserData(Dictionary<string, object> json) {
            this.AdditionalData = json["additionalData"] as Dictionary<string, object>;
            this.OptOutEmail = Convert.ToBoolean(json["optOutEmail"]);
            this.OptOutPush = Convert.ToBoolean(json["optOutPush"]);
        }
    }
}
