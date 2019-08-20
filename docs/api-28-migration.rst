Migration to API 28
===================
Starting November 1st, 2019 Google will require that you target API 28 for app updates on the Google Play Store.

:Since: 3.0.0

The Teak SDK now requires that ``targetSdkVersion`` be at least 28.

This means that you will also need to update your Andoid Support libraries to be 28, or switch to AndroidX. Teak will use either the ``support-v4`` or AndroidX libraries, whichever are present in your build.

In addition, the following dependencies will also need to be updated:

    * ``com.google.android.gms:play-services-ads``, ``com.google.android.gms:play-services-base`` and ``com.google.android.gms:play-services-basement`` should be updated to at least ``17.1.0``

    * ``com.google.firebase:firebase-messaging`` should be updated to at least ``19.0.1``

.. note:: If you are using the Play Services Resolver plugin for Unity, these versions should be updated for you automatically.

If you have any questions, don't hesitate to reach out and ask.
