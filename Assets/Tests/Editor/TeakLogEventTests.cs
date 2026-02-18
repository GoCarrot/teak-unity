using System.Collections.Generic;

[TeakTestFixture]
public class TeakLogEventTests {

    Dictionary<string, object> MakeLogData(Dictionary<string, object> extras = null) {
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

    [TeakTest]
    public void DeviceIdParsedFromLogData() {
        var evt = new TeakLogEvent(MakeLogData(new Dictionary<string, object> {
            {"device_id", "abc-123-device"}
        }));
        TeakAssert.AreEqual("abc-123-device", evt.DeviceId);
    }

    [TeakTest]
    public void AppIdParsedFromLogData() {
        var evt = new TeakLogEvent(MakeLogData(new Dictionary<string, object> {
            {"app_id", "12345"}
        }));
        TeakAssert.AreEqual("12345", evt.AppId);
    }

    [TeakTest]
    public void BundleIdParsedFromLogData() {
        var evt = new TeakLogEvent(MakeLogData(new Dictionary<string, object> {
            {"bundle_id", "com.example.game"}
        }));
        TeakAssert.AreEqual("com.example.game", evt.BundleId);
    }

    [TeakTest]
    public void SdkVersionParsedFromLogData() {
        var evt = new TeakLogEvent(MakeLogData(new Dictionary<string, object> {
            {"sdk_version", "4.3.9"}
        }));
        TeakAssert.AreEqual("4.3.9", evt.SdkVersion);
    }

    [TeakTest]
    public void ClientAppVersionParsedFromLogData() {
        var evt = new TeakLogEvent(MakeLogData(new Dictionary<string, object> {
            {"client_app_version", "100"}
        }));
        TeakAssert.AreEqual("100", evt.ClientAppVersion);
    }

    [TeakTest]
    public void ClientAppVersionNameParsedFromLogData() {
        var evt = new TeakLogEvent(MakeLogData(new Dictionary<string, object> {
            {"client_app_version_name", "1.2.3"}
        }));
        TeakAssert.AreEqual("1.2.3", evt.ClientAppVersionName);
    }

    [TeakTest]
    public void MissingFieldsAreNull() {
        var evt = new TeakLogEvent(MakeLogData());
        TeakAssert.IsNull(evt.DeviceId);
        TeakAssert.IsNull(evt.AppId);
        TeakAssert.IsNull(evt.BundleId);
        TeakAssert.IsNull(evt.SdkVersion);
        TeakAssert.IsNull(evt.ClientAppVersion);
        TeakAssert.IsNull(evt.ClientAppVersionName);
    }

    [TeakTest]
    public void ExistingPropertiesUnchanged() {
        var evt = new TeakLogEvent(MakeLogData(new Dictionary<string, object> {
            {"device_id", "dev-1"},
            {"event_data", new Dictionary<string, object> {{"key", "value"}}}
        }));
        TeakAssert.AreEqual("test-run-id", evt.RunId);
        TeakAssert.AreEqual(42L, evt.EventId);
        TeakAssert.AreEqual(1700000000L, evt.TimeStamp);
        TeakAssert.AreEqual("test.event", evt.EventType);
        TeakAssert.AreEqual(TeakLogEvent.Level.INFO, evt.LogLevel);
        TeakAssert.IsNotNull(evt.EventData);
    }
}
