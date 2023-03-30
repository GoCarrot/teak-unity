using System;
using System.Collections.Generic;

/// <summary>
/// Describes a semi-structured log event that is reported from the native Teak SDK.
/// </summary>
public class TeakLogEvent {

    /// <summary>
    /// Log level, indicating the sevarity of the log message
    /// </summary>
    public enum Level {
        /// <summary>Verbose log event, these events are mostly debugging related.</summary>
        VERBOSE,

        /// <summary>Informational log event, used to inform the developer.</summary>
        INFO,

        /// <summary>Warning log event, this is something the developer should take notice of and fix.</summary>
        WARN,

        /// <summary>Error log event, an error has occurred and the developer should investigate.</summary>
        ERROR
    }

    /// <summary>A unique string that is assigned when the app starts and stays until the app is closed.</summary>
    public string RunId { get; private set; }

    /// <summary>This is a sequential number that, when combined with 'RunId', uniquely identifies this log event.</summary>
    public long   EventId { get; private set; }

    /// <summary>The time at which this log event was generated.</summary>
    public long   TimeStamp { get; private set; }

    /// <summary>The type of log event, assigned by the SDK.</summary>
    public string EventType { get; private set; }

    /// <summary>The log level.</summary>
    public Level  LogLevel { get; private set; }

    /// <summary>Semi-structured data containing the log event.</summary>
    public Dictionary<string, object> EventData { get; private set; }

    /// @cond hide_from_doxygen
    public TeakLogEvent(Dictionary<string, object> logData) {
        this.RunId = logData["run_id"] as string;
        this.EventId = (long) logData["event_id"];
        this.TimeStamp = (long) logData["timestamp"];
        this.EventType = logData["event_type"] as string;
        this.LogLevel = (Level) Enum.Parse(typeof(Level), logData["log_level"] as string, true);
        this.EventData = logData.ContainsKey("event_data") ? logData["event_data"] as Dictionary<string, object> : null;
    }
    /// @endcond

    /// <summary>
    /// Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    public override string ToString() {
        string formatString = "{{ RunId = '{0}', EventId = '{1}', TimeStamp = '{2}', EventType = '{3}', LogLevel = '{4}'{5} }}";
        string eventDataString = "";
        return string.Format(formatString,
                             this.RunId,
                             this.EventId,
                             this.TimeStamp,
                             this.EventType,
                             this.LogLevel,
                             eventDataString
                            );
    }
}
