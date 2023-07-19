#region References
/// @cond hide_from_doxygen
using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;

using MiniJSON.Teak;
using TeakExtensions;
/// @endcond
#endregion

public partial class Teak {
    /// <summary>Manages asynchronous calls to the native Teak SDK implementations.</summary>
    internal class Operation {

        /// <summary>Called when the result of IsDone becomes true; this must be pumped via calling IsDone.</summary>
        public event System.Action<Dictionary<string, object>, Exception> OnDone;

        /// <summary>Returns <code>true</code> if the operation is complete.</summary>
        public bool IsDone {
            get {
                if (this.isDone == true) { return true; }

#if UNITY_EDITOR
                this.isDone = true;
#elif UNITY_ANDROID
                this.isDone = future.Call<bool>("isDone");
#elif UNITY_IPHONE
                this.isDone = TeakOperationIsFinished(this.operation);
#elif UNITY_WEBGL
                this.isDone = this.markDoneOnNextCheck;
#endif
                if (this.isDone && this.OnDone != null) {
                    Dictionary<string, object> result = null;
                    Exception exception = null;
                    try {
                        result = this.Result;
                    } catch (Exception e) {
                        exception = e;
                    }
                    this.OnDone(result, exception);
                }
                return this.isDone;
            }
        }

        /// <summary><code>null</code> if the operation is not yet complete, otherwise the result of the operation as a JSON string.</summary>
        public string ResultJson {
            get {
                if (this.resultJson != null) {
                    return this.resultJson;
                }

                if (this.IsDone) {
#if UNITY_EDITOR
                    this.resultJson = "{}";
#elif UNITY_ANDROID
                    this.resultJson = future.Call<AndroidJavaObject>("get").Call<AndroidJavaObject>("toJSON").Call<string>("toString");
#elif UNITY_IPHONE
                    this.resultJson = TeakOperationGetResultJson(this.operation);
                    TeakRelease(this.operation);
#elif UNITY_WEBGL
                    this.resultJson = Json.Serialize(this.result);
#else
                    this.resultJson = "{}";
#endif
                }
                return this.resultJson;
            }
        }

        /// <summary><code>null</code> if the operation is not yet complete, otherwise the result of the operation as a dictionary.</summary>
        public Dictionary<string, object> Result {
            get {
                if (this.result != null) {
                    return this.result;
                }

                if (this.ResultJson != null) {
                    this.result = Json.Deserialize(this.ResultJson) as Dictionary<string, object>;
                }
                return this.result;
            }
        }

#if UNITY_EDITOR
#elif UNITY_ANDROID
        public Operation(Func<AndroidJavaObject> init) {
            this.future = init();
        }
#elif UNITY_IPHONE
        public Operation(Func<IntPtr> init) {
            this.operation = init();
        }
#elif UNITY_WEBGL
        public Operation(Action<string> init) {
            string callbackId = DateTime.Now.Ticks.ToString();
            teakOperationCallbackMap.Add(callbackId, json => {
                this.result = json;
                this.markDoneOnNextCheck = true;
            });
            init(callbackId);
        }
#endif

        private bool isDone = false;
        private string resultJson = null;
        private Dictionary<string, object> result = null;
#if UNITY_EDITOR
#elif UNITY_ANDROID
        private AndroidJavaObject future;
#elif UNITY_IPHONE
        private IntPtr operation;

        /// @cond hide_from_doxygen
        [DllImport ("__Internal")]
        private static extern bool TeakOperationIsFinished(IntPtr operation);

        [DllImport ("__Internal")]
        private static extern string TeakOperationGetResultJson(IntPtr operation);
        /// @endcond
#elif UNITY_WEBGL
        private bool markDoneOnNextCheck = false;
#endif
    }

    /// @cond hide_from_doxygen
    internal static Dictionary<string, System.Action<Dictionary<string, object>>> teakOperationCallbackMap = new Dictionary<string, System.Action<Dictionary<string, object>>>();
    void TeakOperationCallback(string jsonString) {
        try {
            Dictionary<string, object> json = Json.TryDeserialize(jsonString) as Dictionary<string, object>;
            if (json == null || !json.ContainsKey("_callbackId")) {
                return;
            }

            string callbackId = json["_callbackId"] as string;
            json.Remove("_callbackId");

            if (teakOperationCallbackMap.ContainsKey(callbackId)) {
                System.Action<Dictionary<string, object>> callback = teakOperationCallbackMap[callbackId];
                teakOperationCallbackMap.Remove(callbackId);
                callback(json);
            }
        } catch (Exception e) {
            Debug.LogError("[Teak] Error executing callback: " + jsonString + "\n\t" + e.ToString());
        }
    }

    public static void SafePerformCallback<T>(string method, System.Action<T> callback, T param) {
        try {
            if (callback != null) {
                callback(param);
            }
        } catch (Exception e) {
            if (param is Dictionary<string, object>) {
                Teak.Instance.ReportCallbackError(method, e, param as Dictionary<string, object>);
            } else if (param is IToJson) {
                Teak.Instance.ReportCallbackError(method, e, (param as IToJson).toJson());
            }
        }
    }
    /// @endcond
}
