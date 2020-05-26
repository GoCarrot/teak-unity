Working with Teak in Unity
==========================
.. highlight:: csharp

Requesting Push Notification Permissions
----------------------------------------
In order to use push notifications on iOS, you will need to request permissions.

::

    UnityEngine.iOS.NotificationServices.RegisterForNotifications(UnityEngine.iOS.NotificationType.Alert |
        UnityEngine.iOS.NotificationType.Badge |  UnityEngine.iOS.NotificationType.Sound);

.. note:: We suggest that you don't simply ask for push permissions when the app starts. We'll be happy to talk with you to figure out what works best for your title.

Push Notifications and Local Notifications
------------------------------------------
Whenever your game is launched via a push notification, or local notification Teak will let you know by sending out an event to all listeners.

You can listen for that event during by first writing a listener function, for example::

    void MyOnLaunchedFromNotificationListener(TeakNotification notification)
    {
        Debug.Log("OnLaunchedFromNotification: " + notification.CreativeId + " - " + notification.ScheduleId + " Incentivized? " + notification.Incentivized);
    }

And then adding it to the ``Teak.Instance.OnLaunchedFromNotification`` event during ``Awake()`` in any ``MonoBehaviour``::

    void Awake()
    {
        Teak.Instance.OnLaunchedFromNotification += MyOnLaunchedFromNotificationListener;
    }

Rewards
-------
Whenever your game should grant a reward to a user Teak will let you know by sending out an event to all listeners.

You can listen for that event during by first writing a listener function, for example::

    void MyRewardListener(TeakReward reward)
    {
        switch (reward.Status) {
            case TeakReward.RewardStatus.GrantReward: {
                // The user has been issued this reward by Teak
                foreach(KeyValuePair<string, object> entry in reward.Reward)
                {
                    Debug.Log("[Teak Unity Cleanroom] OnReward -- Give the user " + entry.Value + " instances of " + entry.Key);
                }
            }
            break;

            case TeakReward.RewardStatus.SelfClick: {
                // The user has attempted to claim a reward from their own social post
            }
            break;

            case TeakReward.RewardStatus.AlreadyClicked: {
                // The user has already been issued this reward
            }
            break;

            case TeakReward.RewardStatus.TooManyClicks: {
                // The reward has already been claimed its maximum number of times globally
            }
            break;

            case TeakReward.RewardStatus.ExceedMaxClicksForDay: {
                // The user has already claimed their maximum number of rewards of this type for the day
            }
            break;

            case TeakReward.RewardStatus.Expired: {
                // This reward has expired and is no longer valid
            }
            break;

            case TeakReward.RewardStatus.InvalidPost: {
                //Teak does not recognize this reward id
            }
            break;
        }
    }

And then adding it to the ``Teak.Instance.OnReward`` event during ``Awake()`` in any ``MonoBehaviour``::

    void Awake()
    {
        Teak.Instance.OnReward += MyRewardListener;
    }

Working with Notifications
--------------------------
You can use Teak to schedule notifications for the future; delivered either to the current user, or other users.

.. note:: You get the full benefit of Teak's analytics, A/B testing, and Content Management System.

.. note:: All notification related methods are coroutines. You may need to wrap calls to them in StartCoroutine()

Callbacks
^^^^^^^^^
All notification related methods are coroutines, which use a callback to communicate the results back to the caller.

The ``TeakNotification.Reply`` class has two properties:
    :Status: A value that indicates success, or reason for the failure of the call:

        :Ok: The call was successful, and the notification has been scheduled for delivery.

        :UnconfiguredKey: The call could not be completed because Teak is unable to send a notification to the device due to a configuration setting. This can either be that the user has not granted push permissions on iOS, or that the Teak Dashboard does not have sending credentials suitable for the current device (i.e. Teak has not been provided with an FCM Sender ID/API Key, APNS certificate, or ADM Client ID/Client Secret).

        :InvalidDevice: The call could not be completed because Teak is not aware of the device scheduling the notification. This can happen if Teak was completely unable to get a push token for the device, which can occur due to intermittent failures in APNS/FCM/ADM, intermittent networking failures between the device and those services, or system modifications made on rooted devices.

        :InternalError: An unknown error occured, and the call should be retried.

    :Notifications: If the call was successful, a ``List`` containing the notification schedule ids that were created or canceled by the call.


Scheduling a Local Notification
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
To schedule a notification from your game, use::

    IEnumerator TeakNotification.ScheduleNotification(string creativeId, string defaultMessage,
        long delayInSeconds, System.Action<TeakNotification.Reply> callback)

Parameters
    :creativeId: A value used to identify the message creative in the Teak CMS e.g. "daily_bonus"

    :defaultMessage: The text to use in the notification if there are no modifications in the Teak CMS.

    :delayInSeconds: The number of seconds from the current time before the notification should be sent.

    :callback: The callback to be called after the notification is scheduled

.. important:: The maximum delay for a Local Notification is 30 days.

Scheduling a Long-Distance Notification
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
To schedule a notification from your game, delivered to a different user of your game use::

    IEnumerator TeakNotification.ScheduleNotification(string creativeId, long delayInSeconds,
        string[] userIds, System.Action<TeakNotification.Reply> callback)

Parameters
    :creativeId: A value used to identify the message creative in the Teak CMS e.g. "daily_bonus"

    :delayInSeconds: The number of seconds from the current time before the notification should be sent.

    :userIds: An array of user ids to which the notification should be delivered

    :callback: The callback to be called after the notifications are scheduled

.. important:: The maximum delay for a Long-Distance Notification is 30 days.

Canceling a Notification
^^^^^^^^^^^^^^^^^^^^^^^^
To cancel a previously scheduled notification, use::

    IEnumerator TeakNotification.CancelScheduledNotification(string scheduledId,
        System.Action<TeakNotification.Reply> callback)

Parameters
    :scheduleId: Passing the id received from ``ScheduleNotification()`` will cancel that specific notification; passing the ``creativeId`` used to schedule the notification will cancel **all** scheduled notifications with that creative id for the user

    :callback: The callback to be called after the notification is canceled

Canceling all Local Notifications
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
To cancel all previously scheduled local notifications, use::

    IEnumerator TeakNotification.CancelAllScheduledNotifications(
        System.Action<TeakNotification.Reply> callback)

Parameters
    :callback: The callback to be called after the notifications are canceled

.. note:: This call is processed asynchronously. If you immediately call ``TeakNotification.ScheduleNotification()`` after calling ``TeakNotification.CancelAllScheduledNotifications()`` it is possible for your newly scheduled notification to also be canceled. We recommend waiting until the callback has fired before scheduling any new notifications.

.. _get-notification-state:

Determining if User Has Disabled Push Notifications
---------------------------------------------------
You can use Teak to get the state of push notifications for your app.

If notifications are disabled, you can prompt them to re-enable them on the settings page for the app, and use Teak to go directly the settings for your app.

Notification State
^^^^^^^^^^^^^^^^^^
To get the state of push notifications, use::

    NotificationState PushNotificationState

Return
    :UnableToDetermine: Unable to determine the notification state.

    :Enabled: Notifications are enabled, your app can send push notifications.

    :Disabled: Notifications are disabled, your app cannot send push notifications.

    :Provisional: Provisional notifications are enabled, your app can send notifications but they will only display in the Notification Center (iOS 12+ only).

    :NotRequested: The user has not been asked to authorize push notifications (iOS only).

Example::

    if (Teak.Instance.PushNotificationState == Teak.NotificationState.Disabled) {
        // Show a button that will let users open the settings
    }

Opening the Settings for Your App
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
If you want to show the settings for your app, use::

    bool OpenSettingsAppToThisAppsSettings()

This function will return ``false`` if Teak was not able to open the settings, ``true`` otherwise.

Example::

    // ...
    // When a user presses a button indicating they want to change their notification settings
    Teak.Instance.OpenSettingsAppToThisAppsSettings()

.. player-properties:

Player Properties
-----------------
Teak can store up to 16 numeric, and 16 string properties per player. These properties can then be used for targeting.

You do not need to register the property in the Teak Dashboard prior to sending them from your game, however you will need to register them in the Teak Dashboard before using them in targeting.

Numeric Property
^^^^^^^^^^^^^^^^
To set a numeric property, use::

    void SetNumericAttribute(string key, double value)

Example::

    Teak.Instance.SetNumericAttribute("coins", new_coin_balance);

String Property
^^^^^^^^^^^^^^^
To set a string property, use::

    void SetStringAttribute(string key, string value)

Example::

    Teak.Instance.SetStringAttribute("last_slot", "amazing_slot_name");

Analytics Events
----------------
Teak can be used to track analytics events which can then be used for targeting. These events are automatically batched by the Teak SDK, you do not need to perform your own batching.

Event Format
^^^^^^^^^^^^
Teak events are a tuple of values, 'action', 'object type' and 'object instance'. For example: ['LevelUp', 'Fishing', '13'].

Object instance, and object type are optional, but if you provide an object instance, you must also provide an object type, for example ['FishCaught', null, '13'] is not allowed, but ['FishCaught', 'Salmon'] is allowed.

Tracking an Event
^^^^^^^^^^^^^^^^^
To track that an event occurred, use::

    void TrackEvent(string actionId, string objectTypeId, string objectInstanceId)

Example::

    Teak.Instance.TrackEvent("LevelUp", "Fishing", "13");

Incrementing Events
^^^^^^^^^^^^^^^^^^^
Incremented events are used for analytics which grow over time. You cannot provide negative values.

To increment an event, use::

    void IncrementEvent(string actionId, string objectTypeId, string objectInstanceId, ulong count)

Examples::

    Teak.Instance.IncrementEvent("coin_sink", "slot", "Happy Land Slots", 25000);
    Teak.Instance.IncrementEvent("spin", "slot", "Happy Land Slots", 1);
    // <after the spin happens>
    Teak.Instance.IncrementEvent("coin_source", "slot", "Happy Land Slots", 1000000);

Deep Links
----------
Deep Linking with Teak is based on routes, which act like URLs. These routes allow you to specify variables

You can add routes using::

    void RegisterRoute(string route, string name, string description, Action<Dictionary<string, object>> action)

For example::

    void Awake()
    {
        Teak.Instance.RegisterRoute("/store/:sku", "Store", "Open the store to an SKU", (Dictionary<string, object> parameters) => {
            // Any URL query parameters, or path parameters will be contained in the dictionary
            Debug.Log("Open the store to this sku - " + parameters["sku"]);
        });
    }

Parameters
    :route: The route definition to register

    :name: The name of the route, this will be used in the Teak Dashboard

    :description: The description of the route, this will be used in the Teak Dashboard

    :action: The method to execute when the app is opened via a deep link to this route

.. important:: You need to register your deep link routes before you call ``IdentifyUser``.

How Routes Work
^^^^^^^^^^^^^^^
Routes work like URLs where parts of the path can be a variable. In the example above, the route is ``/store/:sku``. Variables in the path are designated with ``:``. So, in the route ``/store/:sku`` there is a variable named ``sku``.

This means that if the deep link used to launch the app was ``/store/io.teak.test.dollar`` was used to open the app, it would call the function and assign the value ``io.teak.test.dollar`` to the key ``sku`` in the dictionary that is passed in.

This dictionary will also contain any URL query parameters. For example::

    /store/io.teak.test.dollar?campaign=email

In this link, the value ``io.teak.test.dollar`` would be assigned to the key ``sku``, and the value ``email`` would be assigned to the key ``campaign``.

.. The route system that Teak uses is very flexible, let's look at a slightly more complicated example.

.. What if we wanted to make a deep link which opened the game to a specific slot machine.

When Are Deep Links Executed
^^^^^^^^^^^^^^^^^^^^^^^^^^^^
Deep links are passed to an application as part of the launch. The Teak SDK holds onto the deep link information and waits until your app has finished launching, and initializing.

Deep links will get processed the sooner of:

* Your app calls ``IdentifyUser``
* Your app calls ``ProcessDeepLinks``

``ProcessDeepLinks`` is provided so that you can signify that deep links should be processed earlier than your call to ``IdentifyUser`` or so that you can still process deep links in the case of a user opting out of tracking.

Logout
------
You can log out the current user using ``Logout``. If the user is logged out, Teak will not process deep links or rewards until a user is logged in, via ``IdentifyUser``.

Preprocessor Defines
--------------------
Teak sets some preprocessor defines for your use in ``Teak/Editor/TeakPreProcessDefiner.cs``.

    :TEAK_2_0_OR_NEWER: The Teak SDK version is at least 2.0

    :TEAK_2_1_OR_NEWER: The Teak SDK version is at least 2.1

    :TEAK_2_2_OR_NEWER: The Teak SDK version is at least 2.2

    :TEAK_2_3_OR_NEWER: The Teak SDK version is at least 2.3

    :TEAK_3_0_OR_NEWER: The Teak SDK version is at least 3.0

    :TEAK_3_1_OR_NEWER: The Teak SDK version is at least 3.1

    :TEAK_3_2_OR_NEWER: The Teak SDK version is at least 3.2
