SDK 21+ and Unity's Scary Android Permissions
=============================================
If you have recently updated Unity, or updated your ``targetSdkVersion`` to 21 or greater, you may notice that your game is now asking for Android permissions that you never requested.

.. image:: images/android-phone-state-permission.png

Woah, what!?

Make it Stop
------------
.. highlight:: xml

The "make and manage phone calls" permission is the ``READ_PHONE_STATE`` permission. This can be removed using the Android ``tools`` XML namespace.

Add this to your ``AndroidManifest.xml`` ::

    <uses-permission android:name="android.permission.READ_PHONE_STATE"
                     tools:node="remove" />

Make sure that the ``<manifest>`` tag in your ``AndroidManifest.xml`` contains ``xmlns:tools="http://schemas.android.com/tools"``.

The other permission is the ``WRITE_EXTERNAL_STORAGE`` permission.

.. image:: images/android-photos-permission.png

Can be suppressed by adding this to your ``AndroidManifest.xml`` inside your main ``<activity>`` ::

    <meta-data android:name="unityplayer.SkipPermissionsDialog" android:value="true" />

.. note:: If your game **does** require this permission, you will need to request it later in order to use it.
    
    We suggest only asking for permissions when the user initiates an action that would require the permission. If you need help with this, contact us.