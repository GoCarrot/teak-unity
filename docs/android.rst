.. include:: global.rst

Android
=======
Dependencies
------------
.. _android-dependencies:

The following dependencies are required by Teak

* ``com.google.android.gms:play-services-ads:16+``
* ``com.google.android.gms:play-services-base:16+``
* ``com.google.android.gms:play-services-basement:16+``
* ``com.google.firebase:firebase-messaging:17+``
* ``com.android.support:support-core-utils:26.1+``
* ``com.android.support:support-compat:26.1+``

.. note:: If you use the `Play Services Resolver plugin for Unity <https://github.com/googlesamples/unity-jar-resolver>`_, these dependencies should be taken care of automatically.

Tell Teak to Auto-Initialize
----------------------------
.. highlight:: xml

Teak can take advantage of Firebase's initialization of ContentProviders to auto-initialize. This is the easiest way to integrate Teak with your Android target.

All you need to do is add this to the main activity in your AndroidManifest.xml::

    <meta-data android:name="io_teak_initialize" android:value="true"/>

.. note:: This should work just fine for most games. You can go right to :ref:`skip-to-edit-teak-xml`

Make Teak your Main Activity
^^^^^^^^^^^^^^^^^^^^^^^^^^^^
.. highlight:: xml

If you can't use auto-initialization, for some reason, edit your ``Assets/Plugins/Android/AndroidManifest.xml`` and change the main activity from::

    <activity android:name="com.unity3d.player.UnityPlayerActivity" android:label="@string/app_name">

to::

    <activity android:name="io.teak.sdk.wrapper.unity.TeakUnityPlayerActivity" android:label="@string/app_name">

You can now :ref:`skip-to-edit-teak-xml`

If there's a reason that you can't do this, then follow the steps to Add Teak to your Main Activity.

What About Shortcuts?
^^^^^^^^^^^^^^^^^^^^^
.. highlight:: xml

To preserve app shortcuts, add the following below (not inside) the ``<activity>`` you just changed::

    <activity-alias android:name="com.unity3d.player.UnityPlayerActivity" android:targetActivity="io.teak.sdk.wrapper.unity.TeakUnityPlayerActivity" >
        <intent-filter>
            <action android:name="android.intent.action.MAIN" />
            <category android:name="android.intent.category.LAUNCHER" />
        </intent-filter>
    </activity-alias>

This creates an 'activity alias' which tells Android, "Our main activity used to be com.unity3d.player.UnityPlayerActivity, but now it's io.teak.sdk.wrapper.unity.TeakUnityPlayerActivity"

Subclass TeakUnityPlayerActivity
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
.. highlight:: java

If you can't change your main activity, and instad have your own custom activity that subclasses ``UnityPlayerActivity`` then simply change::

    extends UnityPlayerActivity

to::

    extends TeakUnityPlayerActivity

Otherwise Add Teak to your Main Activity
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
.. highlight:: java

If you can't subclass ``TeakUnityPlayerActivity``, then you need to add the Teak initialization calls into your custom activity.

Import Teak into your main activity::

    import io.teak.sdk.Teak;
    import io.teak.sdk.wrapper.unity;

Call Teak.onCreate **before** the call to super.onCreate, then call TeakUnity.initialize **after** the call to super.onCreate::

    protected void onCreate(Bundle savedInstanceState) {
        Teak.onCreate(this);
        super.onCreate(savedInstanceState);
        TeakUnity.initialize();
        // ... etc
    }

Call setIntent()::

    protected void onNewIntent(Intent intent) {
        super.onNewIntent(intent);
        setIntent(intent); // << Add this line
    }

.. note:: You only need to call setIntent() if your custom activity does not inherit from ``UnityPlayerActivity``, otherwise it is done by ``UnityPlayerActivity``.

What This Does
^^^^^^^^^^^^^^
This lets Teak hook into the Android app lifecycle and configure itself, listen for Facebook logins, billing events, and begin sending information to the Teak Service.

Testing It
^^^^^^^^^^
Run your game on an Android device, and look at the Android debug log output.

You Should See
^^^^^^^^^^^^^^
.. highlight:: json

::

    {
      "event_type":"teak.state",
      "log_level":"INFO",
      "timestamp":"<timestamp>",
      "event_data": {
        "state":"Created",
        "old_state":"Allocated"
      },
      "event_id":1,
      "sdk_version": {
        "unity":"<unity-sdk-version>",
        "android":"<android-sdk-version>"
      },
      "run_id":"<some-guid>"
    }

And many other Teak log entries.

.. note:: If You Don't See Teak debug log messages, check to make sure your game is being built in debug mode.

If You See
^^^^^^^^^^
    java.lang.RuntimeException: Failed to find R.string.io_teak_api_key

It means that the ``res/values/teak.xml`` file was not found. See below...

.. _skip-to-edit-teak-xml:
.. _android-edit-teak-xml:

Edit res/values/teak.xml
------------------------
.. highlight:: xml

The Teak Editor scripts for Unity will manage this file for you.

If your build environment needs to provide the file itself, this is what it should look like::

    <?xml version="1.0" encoding="utf-8"?>
    <resources>
        <string name="io_teak_app_id">YOUR_TEAK_APP_ID</string>
        <string name="io_teak_api_key">YOUR_TEAK_API_KEY</string>
        <string name="io_teak_gcm_sender_id">YOUR_GCM_SENDER_ID</string>
        <string name="io_teak_firebase_app_id">YOUR_FIREBASE_APPLICATION_ID</string>
    </resources>

.. note:: Replace ``YOUR_TEAK_APP_ID``, ``YOUR_TEAK_API_KEY``, ``YOUR_GCM_SENDER_ID`` and, ``YOUR_FIREBASE_APPLICATION_ID`` with your game's values.

Your Teak App Id and API Key can be found in the Settings for your app on the Teak dashboard.

Your Firebase Application Id and GCM Sender Id can be found in your Firebase dashboard: <https://teak.readthedocs.io/en/latest/>

What This Does
^^^^^^^^^^^^^^
This provides Teak with the credentials needed to send information to the Teak Service.

Set Notification Icons for your Game
------------------------------------
To specify the icon displayed in the system tray, and at the top of the notification, specify these resources.

You will need two versions of this file. One located in ``values`` and the other located in ``values-v21``::

    <?xml version="1.0" encoding="utf-8"?>
    <resources>
        <!-- The tint-color for your silouette icon, format is: 0xAARRGGBB -->
        <integer name="io_teak_notification_accent_color">0xfff15a29</integer>

        <!-- Icons should be 144x144, PNG with transparency -->
        <drawable name="io_teak_small_notification_icon">@drawable/YOUR_ICON_FILE_NAME</drawable>
    </resources>

The file in ``values`` should specify a full-color icon, for devices running less than Android 5, and the file in ``values-v21`` should specify a white and transparent PNG for Android 5 and above.

.. _android-set-up-deep-linking:

Setting Up Deep Linking
-----------------------
.. highlight:: xml

Add the following to the ``<activity>`` section of your ``Assets/Plugins/Android/AndroidManifest.xml``::

    <intent-filter>
        <action android:name="android.intent.action.VIEW" />
        <category android:name="android.intent.category.DEFAULT" />
        <category android:name="android.intent.category.BROWSABLE" />
        <data android:scheme="http" android:host="YOUR_SUBDOMAIN.jckpt.me" />
        <data android:scheme="https" android:host="YOUR_SUBDOMAIN.jckpt.me" />
    </intent-filter>
    <intent-filter>
        <action android:name="android.intent.action.VIEW" />
        <category android:name="android.intent.category.DEFAULT" />
        <category android:name="android.intent.category.BROWSABLE" />
        <data android:scheme="teakYOUR_TEAK_APP_ID" android:host="*" />
    </intent-filter>

.. note:: Replace ``YOUR_TEAK_APP_ID`` with your Teak App Id and ``YOUR_SUBDOMAIN`` with your Teak Subdomain.

Your Teak App Id and Teak Subdomain can be found in the Settings for your app on the Teak dashboard.

What This Does
^^^^^^^^^^^^^^
This tells Android to look for deep link URLs created by Teak.
