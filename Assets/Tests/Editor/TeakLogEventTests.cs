using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public static class TeakLogEventTests {
    static int passed = 0;
    static int failed = 0;

    static void Assert(bool condition, string message) {
        if (condition) {
            passed++;
        } else {
            failed++;
            Debug.LogError("[FAIL] " + message);
        }
    }

    static void AssertEqual(object expected, object actual, string message) {
        Assert(object.Equals(expected, actual),
            message + " — expected: " + (expected ?? "null") + ", got: " + (actual ?? "null"));
    }

    public static void RunAll() {
        passed = 0;
        failed = 0;

        try {
            TestDeviceIdParsedFromLogData();
            TestAppIdParsedFromLogData();
            TestBundleIdParsedFromLogData();
            TestSdkVersionParsedFromLogData();
            TestClientAppVersionParsedFromLogData();
            TestClientAppVersionNameParsedFromLogData();
            TestDeviceIdNullWhenMissing();
            TestExistingPropertiesUnchanged();
        } catch (Exception e) {
            failed++;
            Debug.LogError("[FAIL] Exception: " + e);
        }

        Debug.Log(string.Format("[TeakLogEventTests] {0} passed, {1} failed", passed, failed));

        if (failed > 0) {
            EditorApplication.Exit(1);
        } else {
            EditorApplication.Exit(0);
        }
    }

    static Dictionary<string, object> MakeLogData(Dictionary<string, object> extras = null) {
        var data = new Dictionary<string, object> {
            {"run_id", "test-run-id"},
            {"event_id", 42L},
            {"timestamp", 1700000000L},
            {"event_type", "test.event"},
            {"log_level", "INFO"}
        };
        if (extras != null) {
            foreach (var kv in extras) {
                data[kv.Key] = kv.Value;
            }
        }
        return data;
    }

    static void TestDeviceIdParsedFromLogData() {
        var data = MakeLogData(new Dictionary<string, object> {
            {"device_id", "abc-123-device"}
        });
        var evt = new TeakLogEvent(data);
        AssertEqual("abc-123-device", evt.DeviceId, "DeviceId should be parsed from log data");
    }

    static void TestAppIdParsedFromLogData() {
        var data = MakeLogData(new Dictionary<string, object> {
            {"app_id", "12345"}
        });
        var evt = new TeakLogEvent(data);
        AssertEqual("12345", evt.AppId, "AppId should be parsed from log data");
    }

    static void TestBundleIdParsedFromLogData() {
        var data = MakeLogData(new Dictionary<string, object> {
            {"bundle_id", "com.example.game"}
        });
        var evt = new TeakLogEvent(data);
        AssertEqual("com.example.game", evt.BundleId, "BundleId should be parsed from log data");
    }

    static void TestSdkVersionParsedFromLogData() {
        var data = MakeLogData(new Dictionary<string, object> {
            {"sdk_version", "4.3.9"}
        });
        var evt = new TeakLogEvent(data);
        AssertEqual("4.3.9", evt.SdkVersion, "SdkVersion should be parsed from log data");
    }

    static void TestClientAppVersionParsedFromLogData() {
        var data = MakeLogData(new Dictionary<string, object> {
            {"client_app_version", "100"}
        });
        var evt = new TeakLogEvent(data);
        AssertEqual("100", evt.ClientAppVersion, "ClientAppVersion should be parsed from log data");
    }

    static void TestClientAppVersionNameParsedFromLogData() {
        var data = MakeLogData(new Dictionary<string, object> {
            {"client_app_version_name", "1.2.3"}
        });
        var evt = new TeakLogEvent(data);
        AssertEqual("1.2.3", evt.ClientAppVersionName, "ClientAppVersionName should be parsed from log data");
    }

    static void TestDeviceIdNullWhenMissing() {
        var data = MakeLogData();
        var evt = new TeakLogEvent(data);
        AssertEqual(null, evt.DeviceId, "DeviceId should be null when not in log data");
    }

    static void TestExistingPropertiesUnchanged() {
        var data = MakeLogData(new Dictionary<string, object> {
            {"device_id", "dev-1"},
            {"event_data", new Dictionary<string, object> {{"key", "value"}}}
        });
        var evt = new TeakLogEvent(data);
        AssertEqual("test-run-id", evt.RunId, "RunId should still work");
        AssertEqual(42L, evt.EventId, "EventId should still work");
        AssertEqual(1700000000L, evt.TimeStamp, "TimeStamp should still work");
        AssertEqual("test.event", evt.EventType, "EventType should still work");
        AssertEqual(TeakLogEvent.Level.INFO, evt.LogLevel, "LogLevel should still work");
        Assert(evt.EventData != null, "EventData should still be parsed");
    }
}
