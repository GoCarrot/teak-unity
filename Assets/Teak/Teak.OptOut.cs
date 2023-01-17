#region References
/// @cond hide_from_doxygen
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
/// @endcond
#endregion
/*
public partial class Teak {
    public class ChannelCategory {
        // TODO: Should the 'Default' category be a static or will it come from server?

        public string Identifier {
            get;
            private set;
        }

        public string Name {
            get;
            private set;
        }
    }

    /// <summary>
    /// Used to denote what channel(s) an opt-out request applies to.
    /// </summary>
    public enum CampaignChannel {
        Push  = 0,
        Sms   = 1,
        Email = 2
    }

    // Used to convert CampaignChannel enum to string.
    private static readonly ReadOnlyCollection<string> ChannelName = new ReadOnlyCollection<string>(new string[] { "push", "sms", "email"});

    // These should all retry 3 times before saying there's an error.
    // In as much as is possible, we should try to make this as fire-and-forget as possible
    // for each developer to know.

    public IEnumerator SetChannelState(int channelEnum, int stateEnum) {
        // Errors: Invalid State
        yield return null;
    }

    public IEnumerator SetCategoryEnabled(string categoryStruct, int channelEnum, bool enabled) {
        yield return null;
    }

    // Null or empty-string will remove the email address
    public IEnumerator SetEmailAddress(string emailOrNull) {
        yield return null;
    }

    // DEPRECATED:
    // public static IEnumerator ScheduleNotification(string scheduleName, string defaultMessage, long delayInSeconds, System.Action<Reply> callback) {
    // This function will be deprecated, and updated to use the 'Default' category

    // Replaced with:
    public static IEnumerator ScheduleNotification(string scheduleName, long delayInSeconds, System.Action<Reply> callback) {
        yield return null;
    }
}
*/
