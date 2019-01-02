Badging Your App
================
.. highlight:: csharp

You can set the badge count on your app's icon using::

    bool SetBadgeCount(int count)

Parameters
    :count: The number to assign as a badge to the application's icon

This function will return ``true`` if it was able to set the badge count for your app's icon, or ``false`` otherwise.

On Android, this functionality uses the `ShortcutBadger library by Leo Sin <https://github.com/leolin310148/ShortcutBadger>`_.

Known Issues
------------

* October 11, 2018

    Google Nexus and Pixel devices are not supported, because the launcher used does not support badging.

    Samsung devices running Android 8+ are not supported (`GitHub Issue <https://github.com/leolin310148/ShortcutBadger/pull/268>`_).

    :Since: All Versions
    :Fixed: Can't Fix
