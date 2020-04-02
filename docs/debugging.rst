Debugging Your Code with Teak
=============================
.. highlight:: csharp

Sometimes things go wrong, Teak does its best to inform the user when things do. Teak also sends a steady stream of information about things which have gone properly.

While you can always go to the device logs to see this information, we've made it possible to programatically deal with events and/or format them into your own logging solution.

Getting Log Events from Teak
----------------------------
Teak communicates via semi-structured log events. You can view these in the device logs, but we also expose an event that you can use to listen for these logs inside Unity, as well as a wrapper class to help work with the log messages.

Listening for Log Events
^^^^^^^^^^^^^^^^^^^^^^^^
Create a handler for log events::

    void HandleLogEvent(Dictionary<string, object> logData) {
        Debug.Log(new TeakLogEvent(logData));
    }

And assign it to ``OnLogEvent`` ::

    Teak.Instance.OnLogEvent += HandleLogEvent;

TeakLogEvent
^^^^^^^^^^^^
    :RunId: A UUID which identifys the log session.

    :EventId: Sequencing number, unique to each ``RunId``.

    :TimeStamp: UNIX timestamp for the log event.

    :EventType: A string describing the reason for the log event.

    :LogLevel: A value that indicates success, or reason for the failure of the call:

        :VERBOSE: Not currently in use.

        :INFO: Log event is informing the user about what actions Teak is taking.

        :WARN: An indication of something which is wrong, or could go wrong, but is not currently causing issues.

        :ERROR: Something has gone wrong, and Teak is telling you about it.

    :EventData: Event-defined dictionary of additional information.
