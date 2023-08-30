using System;
using System.Collections.Generic;

public class TeakPostLaunchSummary {
    /// <summary>The link used to launch the game, or ``null``.</summary>
    public string LaunchLink { get; private set; }

    /// <summary>The name of the schedule for the notification on the Teak Dashboard,
    /// or ``null`` if this launch was not from a notification.</summary>
    public string ScheduleName { get; private set; }

    /// <summary>The id of the schedule in the Teak CMS, or ``null`` if this launch was not from a notification.</summary>
    public string ScheduleId { get; private set; }

    /// <summary>The name of the link or notification on the Teak Dashboard,
    /// or ``null`` if this launch was not from a notification or link.</summary>
    public string CreativeName { get; private set; }

    /// <summary>The id of the link or notification in the Teak Dashboard,
    /// or ``null`` if this launch was not from a notification or link.</summary>
    public string CreativeId { get; private set; }

    /// <summary>Opaque reward identifier, or ``null`` if no reward.</summary>
    public string RewardId { get; private set; }

    /// <summary>
    /// The name of the Teak 'channel', one of: ``ios_push``, ``android_push``, ``fb_a2u``, ``email``,
    /// ``generic_link``, or ``null`` if this launch was not from a notification or link.
    /// </summary>
    public string ChannelName { get; private set; }

    /// <summary>The deep link used to launch the game, or ``null``.</summary>
    public string DeepLink { get; private set; }

    /// <summary>Opaque id uniquely identifying an individual notification or email send, or
    /// ``null`` if this launch is not from a notification or email..</summary>
    public string SourceSendId { get; private set; }

    /// <summary>OptOut category for this notification, or ``"teak"`` as the default.</summary>
    public string OptOutCategory { get; private set; }

    /// <summary>
    /// Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    public override string ToString() {
        string formatString = "{{ LaunchLink = '{0}', ScheduleName = '{1}', ScheduleId = '{2}', CreativeName = '{3}', CreativeId = '{4}', RewardId = '{5}', ChannelName = '{6}', DeepLink = '{7}', SourceSendId = '{8}', OptOutCategory = '{9}' }}";
        return string.Format(formatString,
                             this.LaunchLink,
                             this.ScheduleName,
                             this.ScheduleId,
                             this.CreativeName,
                             this.CreativeId,
                             this.RewardId,
                             this.ChannelName,
                             this.DeepLink,
                             this.SourceSendId,
                             this.OptOutCategory
                            );
    }

    /// @cond hide_from_doxygen
    public TeakPostLaunchSummary(Dictionary<string, object> json) {
        this.LaunchLink = json.ContainsKey("launch_link") ? json["launch_link"] as string : null;
        this.ScheduleName = json.ContainsKey("teakScheduleName") ? json["teakScheduleName"] as string : null;
        this.ScheduleId = json.ContainsKey("teakScheduleId") ? json["teakScheduleId"] as string : null;
        this.CreativeName = json.ContainsKey("teakCreativeName") ? json["teakCreativeName"] as string : null;
        this.CreativeId = json.ContainsKey("teakCreativeId") ? json["teakCreativeId"] as string : null;
        this.RewardId = json.ContainsKey("teakRewardId") ? json["teakRewardId"] as string : null;
        this.ChannelName = json.ContainsKey("teakChannelName") ? json["teakChannelName"] as string : null;
        this.DeepLink = json.ContainsKey("teakDeepLink") ? json["teakDeepLink"] as string : null;
        this.SourceSendId = json.ContainsKey("teakNotifId") ? json["teakNotifId"] as string : null;
        this.OptOutCategory = json.ContainsKey("teakOptOutCategory") ? json["teakOptOutCategory"] as string : null;
    }
    /// @endcond
}
