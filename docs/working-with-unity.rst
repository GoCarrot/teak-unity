.. include:: global.rst

Working with Notifications, Rewards and Deep Links inside Unity
===============================================================
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

        :Ok: The call was successful.

        :UnconfiguredKey: The call could not be completed because the device does not have a push key associated with it.

        :InvalidDevice: The call could not be completed because Teak has not registered the device.

        :InternalError: An unknown error occured, the call may be retried.

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

Canceling a Notification
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
To cancel a previously scheduled notification, use::

    IEnumerator TeakNotification.CancelScheduledNotification(string scheduledId,
        System.Action<TeakNotification.Reply> callback)

Parameters
    :scheduleId: The id received from ``ScheduleNotification()``

    :callback: The callback to be called after the notification is canceled

Canceling all Local Notifications
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
To cancel all previously scheduled local notifications, use::

    IEnumerator TeakNotification.CancelAllScheduledNotifications(
        System.Action<TeakNotification.Reply> callback)

Parameters
    :callback: The callback to be called after the notifications are canceled

.. note:: This call is processed asynchronously. If you immediately call ``TeakNotification.ScheduleNotification()`` after calling ``TeakNotification.CancelAllScheduledNotifications()`` it is possible for your newly scheduled notification to also be canceled. We recommend waiting until the callback has fired before scheduling any new notifications.

Determining if User Has Disabled Push Notifications
---------------------------------------------------
You can use Teak to determine if a user has disabled push notifications for your app.

If notifications are disabled, you can prompt them to re-enable them on the settings page for the app, and use Teak to go directly the settings for your app.

Are Notifications Enabled?
^^^^^^^^^^^^^^^^^^^^^^^^^^^
To determine if notifications are enabled, use::

    bool AreNotificationsEnabled()

This function will return ``false`` if notifications are disabled, or ``true`` if notifications are enabled, or Teak could not determine the status.

Example::

    if (!Teak.Instance.AreNotificationsEnabled()) {
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

Deep Links
----------
Deep Linking with Teak is based on routes, which act like URLs. These routes allow you to specify variables

You can add routes during the ``Awake()`` function of any ``MonoBehaviour`` using::

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

How Routes Work
^^^^^^^^^^^^^^^
Routes work like URLs where parts of the path can be a variable. In the example above, the route is ``/store/:sku``. Variables in the path are designated with ``:``. So, in the route ``/store/:sku`` there is a variable named ``sku``.

This means that if the deep link used to launch the app was ``/store/io.teak.test.dollar`` was used to open the app, it would call the function and assign the value ``io.teak.test.dollar`` to the key ``sku`` in the dictionary that is passed in.

This dictionary will also contain any URL query parameters. For example::

    /store/io.teak.test.dollar?campaign=email

In this link, the value ``io.teak.test.dollar`` would be assigned to the key ``sku``, and the value ``email`` would be assigned to the key ``campaign``.

.. The route system that Teak uses is very flexible, let's look at a slightly more complicated example.

.. What if we wanted to make a deep link which opened the game to a specific slot machine.
