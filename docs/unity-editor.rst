Unity Editor
============

Add Teak
--------

You can use Unity Package Manager (See :ref:`upm-usage`).

Or...

Download the latest *Teak.unitypackage* from https://sdks.teakcdn.com/unity/Teak.unitypackage

Import it into your project in Unity by going to the menu

    **Assets > Import Package > Custom Package**

Then select *Teak.unitypackage*

What This Does
^^^^^^^^^^^^^^

Importing the *Teak.unitypackage* will add the Unity scripts needed to interact with Teak, the native libraries required for Teak functionality, and the other files that Teak needs to function.

Configure Teak
--------------

Go to the Edit menu in Unity and select Teak

    **Edit > Teak**

This will open the Teak configuration in the Unity Inspector, put in your Teak App Id and Teak API Key.

.. note:: Make sure the Inspector tab is visible in your Unity window.

Your Teak App Id and API Key can be found in the Settings for your app on the Teak dashboard.

What This Does
^^^^^^^^^^^^^^

This stores the Teak App Id and API Key for use by Teak's runtime, and helpers.

Testing It
^^^^^^^^^^^^^^
Just click the **Validate Settings** button, and it will confirm that the settings are correct for your game, or tell you what is wrong.

Tell Teak how to Identify The Current User
------------------------------------------
.. highlight:: csharp

Call the ``IdentifyUser`` function and pass it a string which uniquely identifies the current user::

    void IdentifyUser(string userIdentifier, UserConfiguration userConfiguration)

Parameters
    :userIdentifier: Unique identifier for the user (255 characters or fewer).

    :userConfiguration: Additional user configuration:

        :Email: Email address

        :FacebookId: Facebook id

        :OptOutFacebook: Opts out of tracking Facebook token. Teak will no longer be able to correlate users across multiple devices.

        :OptOutIdfa: Opts out of collected the Id For Advertising (IDFA). Teak will no longer be able to sync this user to Facebook Ad Audiences.

        :OptOutPushKey: Opts out of collecting the push key. Teak will no longer be able to send Local Notifications or Push Notifications for your game.

Example::

    // ...
    // As soon as you know the id for the current user.
    UserConfiguration userConfiguration = new UserConfiguration {
        Email = "email@user.com"
    };
    Teak.Instance.IdentifyUser("user_123456", userConfiguration);

.. note:: The ``userIdentifier`` value should be the same as you send to your back-end for a user id.

Opting Out of Tracking
^^^^^^^^^^^^^^^^^^^^^^
If the user has opted out of data collection completely, do not call ``IdentifyUser``, and Teak will not track the user at all.

If the user has opted out of specific data collection, set the corrisponding opt-out to ``true`` in the user configuration.

What This Does
^^^^^^^^^^^^^^
Identifying the user will allow Teak to start sending information to the remote Teak service and allow Teak to function.

Testing It
^^^^^^^^^^
You can now run your game in the Unity Editor, and look in the Unity Log window.

You Should See
^^^^^^^^^^^^^^

::

    [Teak] IdentifyUser(): a unique user identifier
