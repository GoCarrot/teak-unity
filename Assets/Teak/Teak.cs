#region License
/* Teak -- Copyright (C) 2016 GoCarrot Inc.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
#endregion

#region References
/// @cond hide_from_doxygen
using UnityEngine;

using System;
using System.Text;
using System.Reflection;
using System.Runtime.InteropServices;

using MiniJSON.Teak;
using System.Collections.Generic;
/// @endcond
#endregion

/// <summary>
/// A MonoBehaviour which can be attached to a Unity GameObject to
/// provide access to Teak functionality.
/// </summary>
public partial class Teak : MonoBehaviour
{
    /// <summary>
    /// Gets the <see cref="Teak"/> singleton.
    /// </summary>
    /// <value> The <see cref="Teak"/> singleton.</value>
    public static Teak Instance
    {
        get
        {
            if(mInstance == null)
            {
                mInstance = FindObjectOfType(typeof(Teak)) as Teak;

                if(mInstance == null)
                {
                    GameObject teakGameObject = GameObject.Find("TeakGameObject");
                    if(teakGameObject == null)
                    {
                        teakGameObject = new GameObject("TeakGameObject");
                        teakGameObject.AddComponent<Teak>();
                    }
                    mInstance = teakGameObject.GetComponent<Teak>();
                }
            }
            return mInstance;
        }
    }

    /// <summary>Teak SDK version.</summary>
    public static string Version
    {
        get
        {
            return TeakVersion.Version;
        }
    }

    public static string AppId
    {
        get;
        set;
    }

    public static string APIKey
    {
        get;
        set;
    }

    /// <summary>The user identifier for the current user.</summary>
    public string UserId
    {
        get;
        private set;
    }

    /// <summary>
    /// Value provided to IdentifyUser to opt out of collecting an IDFA for this specific user.
    /// </summary>
    /// <remarks>
    /// If you prevent Teak from collecting the Identifier For Advertisers (IDFA),
    /// Teak will no longer be able to add this user to Facebook Ad Audiences.
    /// </remarks>
    public const string OptOutIdfa = "opt_out_idfa";

    /// <summary>
    /// Value provided to IdentifyUser to opt out of collecting a Push Key for this specific user.
    /// </summary>
    /// <remarks>
    /// If you prevent Teak from collecting the Push Key, Teak will no longer be able
    /// to send Local Notifications or Push Notifications for this user.
    /// </remarks>
    public const string OptOutPushKey = "opt_out_push_key";

    /// <summary>
    /// Value provided to IdentifyUser to opt out of collecting a Facebook Access Token for this specific user.
    /// </summary>
    /// <remarks>
    /// If you prevent Teak from collecting the Facebook Access Token,
    /// Teak will no longer be able to correlate this user across multiple devices.
    /// </remarks>

    public const string OptOutFacebook = "opt_out_facebook";

    /// <summary>
    /// Tell Teak how it should identify the current user.
    /// </summary>
    /// <remarks>
    /// This should be the same way you identify the user in your backend.
    /// </remarks>
    /// <param name="userIdentifier">An identifier which is unique for the current user.</param>
    /// <param name="optOut">A list containing zero or more of: OptOutIdfa, OptOutPushKey, OptOutFacebook</param>
    public void IdentifyUser(string userIdentifier, List<string> optOut = null)
    {
        if (optOut == null) optOut = new List<string>();

        this.UserId = userIdentifier;

#if UNITY_EDITOR
        Debug.Log("[Teak] IdentifyUser(): " + userIdentifier);
#elif UNITY_ANDROID
        AndroidJavaClass teak = new AndroidJavaClass("io.teak.sdk.Teak");
        teak.CallStatic("identifyUser", userIdentifier, optOut.ToArray());
#elif UNITY_IPHONE || UNITY_WEBGL
        TeakIdentifyUser(userIdentifier, Json.Serialize(optOut));
#endif
    }

    /// <summary>
    /// Track an arbitrary event in Teak.
    /// </summary>
    /// <param name="actionId">The identifier for the action, e.g. 'complete'.</param>
    /// <param name="objectTypeId">The type of object that is being posted, e.g. 'quest'.</param>
    /// <param name="objectInstanceId">The specific instance of the object, e.g. 'gather-quest-1'</param>
    public void TrackEvent(string actionId, string objectTypeId, string objectInstanceId)
    {
#if UNITY_EDITOR
        Debug.Log("[Teak] TrackEvent(): " + actionId + " - " + objectTypeId + " - " + objectInstanceId);
#elif UNITY_ANDROID
        AndroidJavaClass teak = new AndroidJavaClass("io.teak.sdk.Teak");
        teak.CallStatic("trackEvent", actionId, objectTypeId, objectInstanceId);
#elif UNITY_IPHONE || UNITY_WEBGL
        TeakTrackEvent(actionId, objectTypeId, objectInstanceId);
#endif
    }

    /// <summary>
    /// An event which gets fired when the app is launched via a push notification.
    /// </summary>
    public event System.Action<TeakNotification> OnLaunchedFromNotification;

    /// <summary>
    /// An event which gets fired when a Teak Reward has been processed (successfully or unsuccessfully).
    /// </summary>
    public event System.Action<TeakReward> OnReward;

    /// <summary>
    /// Method used to register a deep link route.
    /// </summary>
    /// <param name="route">The route for this deep link.</param>
    /// <param name="name">The name of this deep link, used in the Teak dashboard.</param>
    /// <param name="description">A description for what this deep link does, used in the Teak dashboard.</param>
    /// <param name="action">A function, or lambda to execute when this deep link is invoked via a notification or web link.</param>
    public void RegisterRoute(string route, string name, string description, Action<Dictionary<string, object>> action)
    {
        mDeepLinkRoutes[route] = action;
#if UNITY_EDITOR
        Debug.Log("[Teak] RegisterRoute(): " + route + " - " + name + " - " + description);
#elif UNITY_ANDROID
        AndroidJavaClass teakUnity = new AndroidJavaClass("io.teak.sdk.wrapper.unity.TeakUnity");
        teakUnity.CallStatic("registerRoute", route, name, description);
#elif UNITY_IPHONE || UNITY_WEBGL
        TeakUnityRegisterRoute(route, name, description);
#endif
    }

    /// <summary>
    /// Method to set the number displayed on the icon of the app on the home screen.
    /// </summary>
    /// <param name="count">The number to display on the icon of the app on the home screen, or 0 to clear.</param>
    /// <returns>True if Teak was able to set the badge count, false otherwise.</returns>
    public bool SetBadgeCount(int count)
    {
#if UNITY_EDITOR
        Debug.Log("[Teak] SetBadgeCount(" + count + ")");
        return true;
#elif UNITY_ANDROID
        AndroidJavaClass teak = new AndroidJavaClass("io.teak.sdk.Teak");
        return teak.CallStatic<bool>("setApplicationBadgeNumber", count);
#elif UNITY_IPHONE || UNITY_WEBGL
        TeakSetBadgeCount(count);
        return true;
#endif
    }

    /// <summary>
    /// Test if notifications are enabled.
    /// </summary>
    /// <returns>false if notifications have been disabled, true if they are enabled, or Teak could not determine the status.</returns>
    public bool AreNotificationsEnabled()
    {
#if UNITY_EDITOR
        Debug.Log("[Teak] AreNotificationsEnabled()");
        return true;
#elif UNITY_WEBGL
        return true;
#elif UNITY_ANDROID
        AndroidJavaClass teak = new AndroidJavaClass("io.teak.sdk.Teak");
        return !teak.CallStatic<bool>("userHasDisabledNotifications");
#elif UNITY_IPHONE
        return !TeakHasUserDisabledPushNotifications();
#endif
    }

    /// <summary>
    /// Open the settings for your app.
    /// </summary>
    /// <returns>false if Teak was unable to open the settings for your app, true otherwise.</returns>
    public bool OpenSettingsAppToThisAppsSettings()
    {
#if UNITY_EDITOR
        Debug.Log("[Teak] OpenSettingsAppToThisAppsSettings()");
        return false;
#elif UNITY_WEBGL
        return false;
#elif UNITY_ANDROID
        AndroidJavaClass teak = new AndroidJavaClass("io.teak.sdk.Teak");
        return !teak.CallStatic<bool>("openSettingsAppToThisAppsSettings");
#elif UNITY_IPHONE
        return TeakOpenSettingsAppToThisAppsSettings();
#endif
    }

    /// <summary>
    /// Assign a numeric value to a user profile attribute
    /// </summary>
    /// <param name="key">The name of the numeric attribute.</param>
    /// <param name="value">The value for the numeric attribute.</param>
    public void SetNumericAttribute(string key, double value)
    {
#if UNITY_EDITOR
        Debug.Log("[Teak] SetNumericAttribute(" + key + ", " + value + ")");
#elif UNITY_ANDROID
        AndroidJavaClass teak = new AndroidJavaClass("io.teak.sdk.Teak");
        teak.CallStatic("setNumericAttribute", key, value);
#elif UNITY_IPHONE || UNITY_WEBGL
        TeakSetNumericAttribute(key, value);
#endif
    }

    /// <summary>
    /// Assign a string value to a user profile attribute
    /// </summary>
    /// <param name="key">The name of the string attribute.</param>
    /// <param name="value">The value for the string attribute.</param>
    public void SetStringAttribute(string key, string value)
    {
#if UNITY_EDITOR
        Debug.Log("[Teak] SetStringAttribute(" + key + ", " + value + ")");
#elif UNITY_ANDROID
        AndroidJavaClass teak = new AndroidJavaClass("io.teak.sdk.Teak");
        teak.CallStatic("setStringAttribute", key, value);
#elif UNITY_IPHONE || UNITY_WEBGL
        TeakSetStringAttribute(key, value);
#endif
    }

    /// <summary>
    /// Get Teak's configuration data about the current device.
    /// </summary>
    /// <returns>A dictionary containing device info, or null if it's not ready</returns>
    public Dictionary<string, object> GetDeviceConfiguration()
    {
#if UNITY_EDITOR || UNITY_WEBGL
        return new Dictionary<string, object>();
#elif UNITY_ANDROID
        AndroidJavaClass teak = new AndroidJavaClass("io.teak.sdk.Teak");
        return Json.Deserialize(teak.CallStatic<string>("getDeviceConfiguration")) as Dictionary<string,object>;
#elif UNITY_IPHONE
        string configuration = Marshal.PtrToStringAnsi(TeakGetDeviceConfiguration());
        return Json.Deserialize(configuration) as Dictionary<string,object>;
#endif
    }

    /// <summary>
    /// Get Teak's configuration data about the current app.
    /// </summary>
    /// <returns>A dictionary containing app info, or null if it's not ready</returns>
    public Dictionary<string, object> GetAppConfiguration()
    {
#if UNITY_EDITOR || UNITY_WEBGL
        return new Dictionary<string, object>();
#elif UNITY_ANDROID
        AndroidJavaClass teak = new AndroidJavaClass("io.teak.sdk.Teak");
        return Json.Deserialize(teak.CallStatic<string>("getAppConfiguration")) as Dictionary<string,object>;
#elif UNITY_IPHONE
        string configuration = Marshal.PtrToStringAnsi(TeakGetAppConfiguration());
        return Json.Deserialize(configuration) as Dictionary<string,object>;
#endif
    }

    /// <summary>
    /// Register for Provisional Push Notifications.
    /// </summary>
    /// <remarks>
    /// This method only has any effect on iOS devices running iOS 12 or higher.
    /// </remarks>
    /// <returns>true if the device was an iOS 12+ device</returns>
    public bool RegisterForProvisionalNotifications()
    {
#if !UNITY_EDITOR && UNITY_IPHONE
        return TeakRequestProvisionalPushAuthorization();
#else
        return false;
#endif
    }

    /// @cond hide_from_doxygen
    private static Teak mInstance;
    Dictionary<string, Action<Dictionary<string, object>>> mDeepLinkRoutes = new Dictionary<string, Action<Dictionary<string, object>>>();
    /// @endcond

    /// @cond hide_from_doxygen
#if UNITY_ANDROID
    private void Prime31PurchaseSucceded<T>(T purchase)
    {
        try {
            PropertyInfo originalJson = purchase.GetType().GetProperty("originalJson");
            AndroidJavaClass teak = new AndroidJavaClass("io.teak.sdk.Teak");
            teak.CallStatic("pluginPurchaseSucceeded", originalJson.GetValue(purchase, null), "prime31");
        } catch (Exception  ignored) {
        }
    }

    private void Prime31PurchaseFailed(string error, int errorCode)
    {
        try {
            AndroidJavaClass teak = new AndroidJavaClass("io.teak.sdk.Teak");
            teak.CallStatic("pluginPurchaseFailed", errorCode, "prime31");
        } catch (Exception  ignored) {
        }
    }

    private void OpenIABPurchaseSucceded<T>(T purchase)
    {
        try {
            MethodInfo serialize = purchase.GetType().GetMethod("Serialize");
            AndroidJavaClass teak = new AndroidJavaClass("io.teak.sdk.Teak");
            Dictionary<string, object> json = Json.Deserialize(serialize.Invoke(purchase, null)) as Dictionary<string, object>;
            teak.CallStatic("pluginPurchaseSucceeded", Json.Serialize(json["originalJson"]), "openiab");
        } catch (Exception  ignored) {
        }
    }

    private void OpenIABPurchaseFailed(int errorCode, string error)
    {
        try {
            AndroidJavaClass teak = new AndroidJavaClass("io.teak.sdk.Teak");
            teak.CallStatic("pluginPurchaseFailed", errorCode, "openiab");
        } catch (Exception  ignored) {
        }
    }

#elif UNITY_IPHONE || UNITY_WEBGL
    [DllImport ("__Internal")]
    private static extern void TeakIdentifyUser(string userId, string optOut);

    [DllImport ("__Internal")]
    private static extern void TeakTrackEvent(string actionId, string objectTypeId, string objectInstanceId);

    [DllImport ("__Internal")]
    private static extern void TeakUnityRegisterRoute(string route, string name, string description);

    [DllImport ("__Internal")]
    private static extern void TeakUnityReadyForDeepLinks();

    [DllImport ("__Internal")]
    private static extern void TeakSetBadgeCount(int count);

    [DllImport ("__Internal")]
    private static extern void TeakSetNumericAttribute(string key, double value);

    [DllImport ("__Internal")]
    private static extern void TeakSetStringAttribute(string key, string value);
#endif

#if UNITY_IPHONE
    [DllImport ("__Internal")]
    private static extern bool TeakHasUserDisabledPushNotifications();

    [DllImport ("__Internal")]
    private static extern bool TeakOpenSettingsAppToThisAppsSettings();

    [DllImport ("__Internal")]
    private static extern bool TeakRequestProvisionalPushAuthorization();
#endif

#if UNITY_WEBGL
    [DllImport ("__Internal")]
    private static extern string TeakInitWebGL(string appId, string apiKey);
#elif UNITY_IPHONE
    [DllImport ("__Internal")]
    private static extern IntPtr TeakGetAppConfiguration();

    [DllImport ("__Internal")]
    private static extern IntPtr TeakGetDeviceConfiguration();
#endif
    /// @endcond

    #region UnitySendMessage
    /// @cond hide_from_doxygen
    void NotificationLaunch(string jsonString)
    {
        Dictionary<string, object> json = Json.Deserialize(jsonString) as Dictionary<string, object>;
        json.Remove("teakReward");
        json.Remove("teakDeepLink");

        if (OnLaunchedFromNotification != null) {
            OnLaunchedFromNotification(new TeakNotification {
                Incentivized = (json["incentivized"] is bool) ? (bool) json["incentivized"] : false,
                ScheduleId = json["teakScheduleName"] as string,
                CreativeId = json["teakCreativeName"] as string,
                RewardId = json.ContainsKey("teakRewardId") ? json["teakRewardId"] as string : null
            });
        }
    }

    void RewardClaimAttempt(string jsonString)
    {
        Dictionary<string, object> json = Json.Deserialize(jsonString) as Dictionary<string, object>;

        if (OnReward != null) {
            OnReward(new TeakReward(json));
        }
    }

    void DeepLink(string jsonString)
    {
        Dictionary<string, object> json = Json.Deserialize(jsonString) as Dictionary<string, object>;
        string route = json["route"] as string;
        if (mDeepLinkRoutes.ContainsKey(route))
        {
            try
            {
                mDeepLinkRoutes[route](json["parameters"] as Dictionary<string, object>);
            }
            catch(Exception e)
            {
                Debug.LogError("[Teak] Error executing Action for route: " + route + "\n" + e.ToString());
            }
        }
        else
        {
            Debug.LogError("[Teak] Unable to find Action for route: " + route);
        }
    }

#if UNITY_WEBGL
    void NotificationCallback(string jsonString)
    {
        try
        {
            Dictionary<string, object> json = Json.Deserialize(jsonString) as Dictionary<string, object>;
            string callbackId = json["callbackId"] as string;
            string status = json["status"] as string;
            string creativeId = json.ContainsKey("creativeId") ? json["creativeId"] as string : null;
            string data = json.ContainsKey("data") ? (json["data"] is string ? json["data"] as string : Json.Serialize(json["data"])) : null;
            TeakNotification.WebGLCallback(callbackId, status, data, creativeId);
        }
        catch(Exception e)
        {
            Debug.LogError("[Teak] Error executing callback for notification data: " + jsonString + "\n" + e.ToString());
        }
    }
#endif
    /// @endcond
    #endregion

    #region MonoBehaviour
    /// @cond hide_from_doxygen
    void Awake()
    {
        Debug.Log("[Teak] Unity SDK Version: " + Teak.Version);
        DontDestroyOnLoad(this);

        string appId = null;
        string apiKey = null;
#if UNITY_EDITOR
#elif UNITY_WEBGL
        appId = (string.IsNullOrEmpty(Teak.AppId) ? TeakSettings.AppId : Teak.AppId);
        apiKey = (string.IsNullOrEmpty(Teak.APIKey) ? TeakSettings.APIKey : Teak.APIKey);
        TeakInitWebGL(appId, apiKey);
#else
        appId = GetAppConfiguration()["appId"] as string;
        apiKey = GetAppConfiguration()["apiKey"] as string;
#endif
        if (appId != null) Teak.AppId = appId;
        if (apiKey != null) Teak.APIKey = apiKey;
    }

    void Start()
    {
#if UNITY_EDITOR
        // Nothing currently
#elif UNITY_ANDROID
        AndroidJavaClass teakUnity = new AndroidJavaClass("io.teak.sdk.wrapper.unity.TeakUnity");
        teakUnity.CallStatic("readyForDeepLinks");
#elif UNITY_IPHONE || UNITY_WEBGL
        TeakUnityReadyForDeepLinks();
#endif

#if UNITY_ANDROID
        // Try and find an active store plugin
        Type onePF = Type.GetType("OpenIABEventManager, Assembly-CSharp-firstpass");
        if(onePF == null) onePF = Type.GetType("OpenIABEventManager, Assembly-CSharp");

        Type prime31 = Type.GetType("Prime31.GoogleIABManager, Assembly-CSharp-firstpass");
        if(prime31 == null) prime31 = Type.GetType("Prime31.GoogleIABManager, Assembly-CSharp");

        if(onePF != null)
        {
            Debug.Log("[Teak] Found OpenIAB, adding event handlers.");
            EventInfo successEvent = onePF.GetEvent("purchaseSucceededEvent");
            EventInfo failEvent = onePF.GetEvent("purchaseFailedEvent");

            Type purchase = Type.GetType("OnePF.Purchase, Assembly-CSharp-firstpass");
            if(purchase == null) purchase = Type.GetType("OnePF.Purchase, Assembly-CSharp");

            MethodInfo magic = GetType().GetMethod("OpenIABPurchaseSucceded", BindingFlags.NonPublic | BindingFlags.Instance).MakeGenericMethod(purchase);
            Delegate successDelegate = Delegate.CreateDelegate(successEvent.EventHandlerType, this, magic);
            object[] successHandlerArgs = { successDelegate };
            successEvent.GetAddMethod().Invoke(null, successHandlerArgs);

            Delegate failDelegate = Delegate.CreateDelegate(failEvent.EventHandlerType, this, "OpenIABPurchaseFailed");
            object[] failHandlerArgs = { failDelegate };
            failEvent.GetAddMethod().Invoke(null, failHandlerArgs);
        }
        else if(prime31 != null)
        {
            Debug.Log("[Teak] Found Prime31, adding event handlers.");

            EventInfo successEvent = prime31.GetEvent("purchaseSucceededEvent");
            EventInfo failEvent = prime31.GetEvent("purchaseFailedEvent");

            Type purchase = Type.GetType("Prime31.GooglePurchase, Assembly-CSharp-firstpass");
            if(purchase == null) purchase = Type.GetType("Prime31.GooglePurchase, Assembly-CSharp");

            MethodInfo magic = GetType().GetMethod("Prime31PurchaseSucceded", BindingFlags.NonPublic | BindingFlags.Instance).MakeGenericMethod(purchase);
            Delegate successDelegate = Delegate.CreateDelegate(successEvent.EventHandlerType, this, magic);
            object[] successHandlerArgs = { successDelegate };
            successEvent.GetAddMethod().Invoke(null, successHandlerArgs);

            Delegate failDelegate = Delegate.CreateDelegate(failEvent.EventHandlerType, this, "Prime31PurchaseFailed");
            object[] failHandlerArgs = { failDelegate };
            failEvent.GetAddMethod().Invoke(null, failHandlerArgs);
        }
        else
        {
#if UNITY_PURCHASING
            Debug.Log("[Teak] Found Unity IAP, use TeakStoreListener to wrap IStoreListener.");
#else
            Debug.LogWarning("[Teak] No known store plugin found.");
#endif
        }
#endif
    }

    void OnApplicationQuit()
    {
        Destroy(this);
    }
    /// @endcond
    #endregion
}
