using System;
using System.Collections.Generic;

[TeakTestFixture]
public class TeakLiveActivityTests {

    [TeakTest]
    public void ReplyParsesOkStatus() {
        var reply = new Teak.LiveActivity.Reply(new Dictionary<string, object> {
            {"status", "ok"}
        });
        TeakAssert.IsFalse(reply.Error);
        TeakAssert.IsNull(reply.Errors);
        TeakAssert.IsNull(reply.CanceledCount);
    }

    [TeakTest]
    public void ReplyParsesErrorStatus() {
        var reply = new Teak.LiveActivity.Reply(new Dictionary<string, object> {
            {"status", "error"},
            {"errors", new Dictionary<string, object> {
                {"activityId", new List<object> {"activityId cannot be null or empty"}}
            }}
        });
        TeakAssert.IsTrue(reply.Error);
        TeakAssert.IsNotNull(reply.Errors);
        TeakAssert.AreEqual(1, reply.Errors["activityId"].Count);
        TeakAssert.AreEqual("activityId cannot be null or empty", reply.Errors["activityId"][0]);
    }

    [TeakTest]
    public void ReplyParsesCanceledCount() {
        var reply = new Teak.LiveActivity.Reply(new Dictionary<string, object> {
            {"status", "ok"},
            {"canceled", 3L}
        });
        TeakAssert.IsFalse(reply.Error);
        TeakAssert.IsNotNull(reply.CanceledCount);
        TeakAssert.AreEqual(3, reply.CanceledCount);
    }

    [TeakTest]
    public void ReplyCanceledCountAcceptsInt() {
        var reply = new Teak.LiveActivity.Reply(new Dictionary<string, object> {
            {"status", "ok"},
            {"canceled", 5}
        });
        TeakAssert.AreEqual(5, reply.CanceledCount);
    }

    [TeakTest]
    public void ReplyMissingStatusIsError() {
        var reply = new Teak.LiveActivity.Reply(new Dictionary<string, object>());
        TeakAssert.IsTrue(reply.Error);
    }

    [TeakTest]
    public void ReplyPreservesRawJson() {
        var source = new Dictionary<string, object> {
            {"status", "ok"},
            {"canceled", 2L}
        };
        var reply = new Teak.LiveActivity.Reply(source);
        TeakAssert.IsTrue(ReferenceEquals(source, reply.Json), "Reply should retain the source dictionary reference");
    }

    [TeakTest]
    public void ReplyWithErrorForExceptionIsError() {
        var reply = Teak.LiveActivity.Reply.ReplyWithErrorForException(new InvalidOperationException("boom"));
        TeakAssert.IsTrue(reply.Error);
        TeakAssert.IsNotNull(reply.Errors);
        TeakAssert.IsTrue(reply.Errors.ContainsKey("unity"));
    }

    [TeakTest]
    public void HexStringToBytesRoundTrip() {
        byte[] expected = new byte[] { 0xDE, 0xAD, 0xBE, 0xEF, 0x01, 0x23 };
        byte[] actual = Teak.LiveActivity.HexStringToBytes("deadbeef0123");
        TeakAssert.AreEqual(expected.Length, actual.Length);
        for (int i = 0; i < expected.Length; i++) {
            TeakAssert.AreEqual(expected[i], actual[i]);
        }
    }

    [TeakTest]
    public void HexStringToBytesAcceptsUppercase() {
        byte[] actual = Teak.LiveActivity.HexStringToBytes("DEADBEEF");
        TeakAssert.AreEqual(4, actual.Length);
        TeakAssert.AreEqual((byte)0xDE, actual[0]);
        TeakAssert.AreEqual((byte)0xEF, actual[3]);
    }

    [TeakTest]
    public void HexStringToBytesRejectsOddLength() {
        bool threw = false;
        try {
            Teak.LiveActivity.HexStringToBytes("abc");
        } catch (ArgumentException) {
            threw = true;
        }
        TeakAssert.IsTrue(threw, "expected ArgumentException for odd-length hex");
    }

    [TeakTest]
    public void HexStringToBytesRejectsNonHexChar() {
        bool threw = false;
        try {
            Teak.LiveActivity.HexStringToBytes("zz");
        } catch (ArgumentException) {
            threw = true;
        }
        TeakAssert.IsTrue(threw, "expected ArgumentException for non-hex char");
    }

    [TeakTest]
    public void HexStringToBytesRejectsNull() {
        bool threw = false;
        try {
            Teak.LiveActivity.HexStringToBytes(null);
        } catch (ArgumentNullException) {
            threw = true;
        }
        TeakAssert.IsTrue(threw, "expected ArgumentNullException for null");
    }

    // Drains a coroutine synchronously for editor-mode tests — all Teak coroutines finish
    // in a single step on editor/non-iOS platforms because the native path is skipped.
    static void DrainCoroutine(System.Collections.IEnumerator co) {
        while (co.MoveNext()) { }
    }

    [TeakTest]
    public void StartedLiveActivityHexOverloadInvokesErrorCallbackOnInvalidHex() {
        Teak.LiveActivity.Reply captured = null;
        var teak = new UnityEngine.GameObject("TeakForTest").AddComponent<Teak>();
        try {
            DrainCoroutine(teak.LiveActivities.StartedLiveActivity(
                "chest_timer", "zz", "sys-1", r => { captured = r; }));
            TeakAssert.IsNotNull(captured, "callback must fire even on bad hex");
            TeakAssert.IsTrue(captured.Error, "reply must be error for invalid hex");
            TeakAssert.IsNotNull(captured.Errors);
            TeakAssert.IsTrue(captured.Errors.ContainsKey("unity"));
        } finally {
            UnityEngine.Object.DestroyImmediate(teak.gameObject);
        }
    }

    [TeakTest]
    public void ScheduleLiveActivityUpdateInvokesErrorCallbackOnNullCustomData() {
        Teak.LiveActivity.Reply captured = null;
        var teak = new UnityEngine.GameObject("TeakForTest").AddComponent<Teak>();
        try {
            DrainCoroutine(teak.LiveActivities.ScheduleLiveActivityUpdate(
                "chest_timer", 60, null, null, r => { captured = r; }));
            TeakAssert.IsNotNull(captured, "callback must fire when customData is null");
            TeakAssert.IsTrue(captured.Error, "reply must be error for null customData");
            TeakAssert.IsNotNull(captured.Errors);
            TeakAssert.IsTrue(captured.Errors.ContainsKey("customData"));
        } finally {
            UnityEngine.Object.DestroyImmediate(teak.gameObject);
        }
    }

}
