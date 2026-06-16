using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

[TeakTestFixture]
public class TeakWebPushTests {

    static void DrainCoroutine(IEnumerator co) {
        while (co.MoveNext()) { }
    }

    static string NextCallbackID() {
        var m = typeof(Teak).GetMethod("NextCallbackID",
            BindingFlags.NonPublic | BindingFlags.Static);
        return (string)m.Invoke(null, null);
    }

    static Dictionary<string, Action<Dictionary<string, object>>> GetCallbackMap() {
        var f = typeof(Teak).GetField("teakOperationCallbackMap",
            BindingFlags.NonPublic | BindingFlags.Static);
        return f.GetValue(null) as Dictionary<string, Action<Dictionary<string, object>>>;
    }

    static void FireTeakOperationCallback(Teak teak, string jsonString) {
        var m = typeof(Teak).GetMethod("TeakOperationCallback",
            BindingFlags.NonPublic | BindingFlags.Instance);
        m.Invoke(teak, new object[] { jsonString });
    }

    [TeakTest]
    public void RegisterForNotificationsCompletesInEditorWithNullCallback() {
        var teak = new UnityEngine.GameObject("TeakForTest").AddComponent<Teak>();
        try {
            DrainCoroutine(teak.RegisterForNotifications(null));
        } finally {
            UnityEngine.Object.DestroyImmediate(teak.gameObject);
        }
    }

    [TeakTest]
    public void OperationCallbackDispatchesPermissionGrantedTrue() {
        var teak = new UnityEngine.GameObject("TeakForTest").AddComponent<Teak>();
        try {
            bool? result = null;
            string id = NextCallbackID();
            GetCallbackMap().Add(id, json => {
                result = json.ContainsKey("permissionGranted")
                    && json["permissionGranted"] is bool
                    && (bool)json["permissionGranted"];
            });

            var payload = MiniJSON.Teak.Json.Serialize(new Dictionary<string, object> {
                { "_callbackId", id },
                { "permissionGranted", true }
            });
            FireTeakOperationCallback(teak, payload);

            TeakAssert.IsNotNull(result, "callback should have fired");
            TeakAssert.IsTrue(result.Value, "permissionGranted=true should yield true");
            TeakAssert.IsFalse(GetCallbackMap().ContainsKey(id), "callback removed after dispatch");
        } finally {
            UnityEngine.Object.DestroyImmediate(teak.gameObject);
        }
    }

    [TeakTest]
    public void OperationCallbackDispatchesPermissionGrantedFalse() {
        var teak = new UnityEngine.GameObject("TeakForTest").AddComponent<Teak>();
        try {
            bool? result = null;
            string id = NextCallbackID();
            GetCallbackMap().Add(id, json => {
                result = json.ContainsKey("permissionGranted")
                    && json["permissionGranted"] is bool
                    && (bool)json["permissionGranted"];
            });

            var payload = MiniJSON.Teak.Json.Serialize(new Dictionary<string, object> {
                { "_callbackId", id },
                { "permissionGranted", false }
            });
            FireTeakOperationCallback(teak, payload);

            TeakAssert.IsNotNull(result, "callback should have fired");
            TeakAssert.IsFalse(result.Value, "permissionGranted=false should yield false");
        } finally {
            UnityEngine.Object.DestroyImmediate(teak.gameObject);
        }
    }
}
