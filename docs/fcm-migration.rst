.. highlight:: xml

Migrating to FCM and Teak 2.0
=============================
GCM was deprecated April 10, 2018, and will be removed "as soon as April 11, 2019" `according to Google <https://developers.google.com/cloud-messaging/faq>`_.

:Since: 2.0.0

The Teak SDK no longer supports GCM, but we're going to make your migration to FCM as painless as possible.

Don't Panic
-----------
* It's going to be ok
* All of the push tokens Teak has collected will keep working
* You don't have to let Google collect analytics
* You can always ask us for help

Import your GCM project as a Firebase project
---------------------------------------------
Follow these instructions to import your project to Firebase: https://developers.google.com/cloud-messaging/android/android-migrate-fcm#import-your-gcm-project-as-a-firebase-project

.. note:: You only need to follow step One of the instructions.

    This doc describes everything else you need to do.

Your live game will continue to work after this step, this step just adds Firebase, so there will not be a disruption in service.

Simplify your AndroidManifest.xml
---------------------------------
.. highlight:: xml

Deleted code is debugged code, and we get to delete code!

Remove these permissions from your AndroidManifest.xml::

    <permission android:name="<your-package-name>.permission.C2D_MESSAGE"
                android:protectionLevel="signature" />
    <uses-permission android:name="<your-package-name>.permission.C2D_MESSAGE" />

Remove the Teak receiver from your AndroidManifest.xml::

    <receiver android:name="io.teak.sdk.Teak" android:exported="false">
        <intent-filter>
            <action android:name="YOUR_ANDROID_BUNDLE_ID.intent.TEAK_NOTIFICATION_OPENED" />
            <action android:name="YOUR_ANDROID_BUNDLE_ID.intent.TEAK_NOTIFICATION_CLEARED" />
            <category android:name="YOUR_ANDROID_BUNDLE_ID" />
        </intent-filter>
    </receiver>

Remove the Teak GCM Instance ID Listener Service from your AndroidManifest.xml::

    <service android:name="io.teak.sdk.InstanceIDListenerService" android:exported="false" >
        <intent-filter>
            <action android:name="com.google.android.gms.iid.InstanceID" />
        </intent-filter>
    </service>

All


Add the FCM dependency, remove the GCM dependency
-------------------------------------------------
If you use the `Play Services Resolver plugin for Unity <https://github.com/googlesamples/unity-jar-resolver>`_, these dependencies should be taken care of automatically.

We tested using version 1.2.95 of the Play Services Resolver plugin.

.. note:: If you use other SDKs (Upsight, Leanplum, et. al.) they could be relying on GCM. You will need to make sure they support FCM, or remove them, since GCM and FCM can not live side-by-side.

If you do not currently use the Play Services Resolver plugin, we strongly suggest that you start. It turns your Android dependancies from AARs and JARs that have been copy-pasted into your repository into code.

If you really cannot use the Play Services Resolver plugin, make sure your dependencies are updated to :ref:`android-dependencies`.

Configuration
-------------
If you use Firebase's ``google-services.json`` and its accompanying Gradle plugin (or something else that turns ``google-services.json`` into XML resources), then you are all set.

Otherwise, simply add 

Optionally Disable FirebaseInitProvider
---------------------------------------
.. highlight:: xml

If Teak is the only thing in your game that uses Firebase, and you don't like seeing the log message::

You can disable it, put this into your AndroidManifest.xml::

    <provider android:name="com.google.firebase.provider.FirebaseInitProvider"
              android:authorities="${applicationId}.firebaseinitprovider"
              tools:node="remove" />

(https://firebase.googleblog.com/2017/03/take-control-of-your-firebase-init-on.html)

Optionally Disable Google's Automatic Analytics Collection
----------------------------------------------------------
.. highlight:: xml

Don't want to send your purchase and session data to Google? You don't have to!

https://firebase.google.com/support/guides/disable-analytics#permanently_deactivate_collection

Add this line to your AndroidManifest.xml::

    <meta-data android:name="firebase_analytics_collection_deactivated" android:value="true" />

