.. include:: global.rst

Changelog
=========
.. highlight:: csharp

The changelog for the Unity SDK does not include the native SDK bug fixes. If a version is not listed in the Unity changelist, it means there were no Unity-specific changes made in that version.

1.0.0
-----
:Breaking Changes:
    * None
:New Features:
    * Notification launch callbacks now work on WebGL for Facebook Canvas
    * Deep link launch callbacks now work on WebGL
:Bug Fixes:
    * A WebGL scheduled/canceled notification could sometimes not trigger a callback, this has been fixed

0.19.0
------
:Breaking Changes:
    * None
:New Features:
    * ``RegisterForProvisionalNotifications()``
:Bug Fixes:
    * None

0.15.0
------
:Breaking Changes:
    * None
:New Features:
    * None
:Bug Fixes:
    * If another plugin is using ``IMPL_APP_CONTROLLER_SUBCLASS`` it now works

0.14.0
------
:Breaking Changes:
    * None
:New Features:
    * None
:Bug Fixes:
    * ``OnReward`` and ``OnLaunchedFromNotification`` null checks (this didn’t seem to affect anyone, but still was a good fix)
    * WebGL error with ``TeakDeepLinkTableInternal`` fixed

0.13.8
------
:Breaking Changes:
    * None
:New Features:
    * Now supports Play Services Resolver plugin
    * ``RewardId`` added to ``TeakReward``
    * ``SetBadgeCount()``
:Bug Fixes:
    * None

0.13.2
------
:Breaking Changes:
    * Now using AAR for Android, remove these files
        * Assets/Editor/TeakPackageBuilder.cs
        * Assets/Plugins/Android/res/layout/teak_big_notif_image_text.xml
        * Assets/Plugins/Android/res/layout/teak_notif_no_title.xml
        * Assets/Plugins/Android/res/values/teak_styles.xml
        * Assets/Plugins/Android/res/values-v21/teak_styles.xml
:New Features:
    * None
:Bug Fixes:
    * None


0.13.1
------
:Breaking Changes:
    * None
:New Features:
    * New setting (default off) controls if Teak does build post processing. Now it does not unless you enable it in Settings.
:Bug Fixes:
    * None

0.13.0
------
:Breaking Changes:
    * ``io.teak.sdk.TeakUnityPlayerNativeActivity`` renamed to ``io.teak.sdk.wrapper.unity.TeakUnityPlayerNativeActivity``
    * ``io.teak.sdk.TeakUnityPlayerActivity`` renamed to ``io.teak.sdk.wrapper.unity.TeakUnityPlayerActivity``
:New Features:
    * None
:Bug Fixes:
    * None

0.12.8
------
:Breaking Changes:
    * ``TeakNotification`` callbacks are now ``(string, string)``
:New Features:
    * Added ``CancelAllScheduledNotifications()``
:Bug Fixes:
    * None

0.12.7
------
:Breaking Changes:
    * ``TeakUnityPlayerNativeActivity`` deprecated in favor of ``TeakUnityPlayerActivity``
:New Features:
    * None
:Bug Fixes:
    * WebGL builds will now build cleanly

0.12.6
------
:Breaking Changes:
    * None
:New Features:
    * None
:Bug Fixes:
    * Fix for Prime31/OpenIAB not being in default locations (firstpass assembly)

0.12.1
------
:Breaking Changes:
    * None
:New Features:
    * Unity 5 Compatibility
:Bug Fixes:
    * None

0.12.0
------
:Breaking Changes:
    * None
:New Features:
    * Remote logging
    * Remote exception tracking
:Bug Fixes:
    * Fixed an absolute path issue during iOS post process when BuildPipeline.BuildPlayer was used to build
