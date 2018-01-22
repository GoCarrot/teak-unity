It Broke
========
Well, crap.

Here's a list of common errors, and what causes them.

iOS
---
:ref:`ios-edit-info-plist`.

Android
-------
.. highlight:: xml
If You See
^^^^^^^^^^
::

    java.lang.RuntimeException: Failed to find R.string.io_teak_api_key

or::

    java.lang.RuntimeException: Failed to find R.string.io_teak_app_id

or::

    R.string.io_teak_gcm_sender_id not present, push notifications disabled.

This means that the XML values for Teak are not present. You need to :ref:`android-edit-teak-xml`.

If You See
^^^^^^^^^^
::

    java.util.ServiceConfigurationError: io.teak.sdk.InstanceIDListenerService not found in AndroidManifest

This means that the InstanceIDListenerService is not in your AndroidManifest.xml, add the following to your AndroidManifest.xml in the ``<application>`` section::

    <service android:name="io.teak.sdk.InstanceIDListenerService" android:exported="false" >
        <intent-filter>
            <action android:name="com.google.android.gms.iid.InstanceID" />
        </intent-filter>
    </service>

If You See
^^^^^^^^^^
::

    java.lang.ClassNotFoundException: android.support.v4.content.LocalBroadcastManager

You are missing the dependency ``com.android.support:support-core-utils:26+``

If You See
^^^^^^^^^^
::

    java.lang.ClassNotFoundException: android.support.v4.app.NotificationManagerCompat

You are missing the dependency ``com.android.support:support-compat:26+``

If You See
^^^^^^^^^^
::

    java.lang.ClassNotFoundException: com.google.android.gms.common.GooglePlayServicesUtil

You are missing the dependencies ``com.google.android.gms:play-services-base:10+`` and ``com.google.android.gms:play-services-basement:10+``

If You See
^^^^^^^^^^
::

    java.lang.ClassNotFoundException: com.google.android.gms.gcm.GoogleCloudMessaging

You are missing the dependency ``com.google.android.gms:play-services-gcm:10+``

If You See
^^^^^^^^^^
::

    java.lang.ClassNotFoundException: com.google.android.gms.iid.InstanceIDListenerService

You are missing the dependency ``com.google.android.gms:play-services-iid:10+``

