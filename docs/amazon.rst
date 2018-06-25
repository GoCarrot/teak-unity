.. include:: global.rst

Amazon
======
If you are shipping on Amazon devices, there are a few extra steps required to support Amazon Device Messaging (ADM).

Please note that GCM (Google Cloud Messaging) does not work on Amazon devices. In order to use push notifications on Amazon devices, these steps are required.

Follow Amazon's Set Up Documentation
------------------------------------
Amazon's documentation for ADM is not authored with Unity in mind, so we have supplimented their integration steps with Unity instructions.

The Teak SDK will also automatically try to debug problems with ADM at runtime (in development builds).

Don't hesitate to contact us if you are confused by any of these steps.

Credentials and API Key
^^^^^^^^^^^^^^^^^^^^^^^
First, you will need Amazon credentials and an Amazon API key.

https://developer.amazon.com/docs/adm/obtain-credentials.html

Once you have completed these steps, go to your apps on the Amazon Developer site, and look at the Security Profiles https://developer.amazon.com/iba-sp/overview.html

Click on the security profile for your app.

Copy the value in the Amazon security profile for **Client ID** into the Teak Mobile Settings for your app for **ADM Client ID**.

Copy the value in the Amazon security profile for **Client Secret** into the Teak Mobile Settings for your app for **ADM Client Secret**.

This gives Teak the information it needs to send ADM messages to your app.

Update Your AndroidManifest.xml
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
https://developer.amazon.com/docs/adm/integrate-your-app.html#update-your-app-manifest

Follow steps 1, 2, 3, and 4.

In step 3, you should specify ``android:required="false"`` as Teak will gracefully handle cases when ADM is not available.

In step 4, these are the values you should use:

* [YOUR SERVICE NAME] = io.teak.sdk.push.ADMPushProvider
* [YOUR RECEIVER NAME] = io.teak.sdk.push.ADMPushProvider$MessageAlertReceiver

Store Your API Key as an Asset
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
https://developer.amazon.com/docs/adm/integrate-your-app.html#store-your-api-key-as-an-asset

When this documentation references the ``assets`` folder, it means ``Assets/Plugins/Android/assets``.

Give it a Try
-------------
You should now have what you need to test ADM support.

Keep an eye on the debug console, if you see::

    Add this to your <application> in AndroidManifest.xml in order to use ADM: <amazon:enable-feature android:name="com.amazon.device.messaging" android:required="false" />

Revisit step 3 in https://developer.amazon.com/docs/adm/integrate-your-app.html#update-your-app-manifest

If you see log output with ``event_type`` of ``amazon.adm.registration_error`` Teak is trying to determine why ADM registration has failed. If you see::

    Unable to find 'api_key.txt' in assets [...]

``api_key.txt`` is not in the Android assets. Revisit https://developer.amazon.com/docs/adm/integrate-your-app.html#store-your-api-key-as-an-asset

If you see::

    Whitespace found in 'api_key.txt'

There is whitespace somewhere in the contents of ``api_key.txt``, this will prevent Amazon's SDK from reading the key. Remove the whitespace, it is usually a trailing newline.

If you see::

    Potentially malformed contents of 'api_key.txt', does not contain three sections delimited by '.'

The contents of ``api_key.txt`` are incorrect. Revisit https://developer.amazon.com/docs/adm/integrate-your-app.html#store-your-api-key-as-an-asset

If you see::

    Package name mismatch in 'api_key.txt'

The package name of your app does not match the package name inside ``api_key.txt``. The ``api_key.txt`` must be generated for the package name of your app.

If you see::

    App signature SHA-256 does not match api_key.txt
    App signature MD5 does not match api_key.txt

The signature your app was signed with does not match any of the signatures in ``api_key.txt``. Revisit step 8 in https://developer.amazon.com/docs/adm/obtain-credentials.html
