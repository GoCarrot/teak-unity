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

    static void DrainCoroutine(System.Collections.IEnumerator co) {
        while (co.MoveNext()) { }
    }

    [TeakTest]
    public void RegisterPushToStartTokenHexOverloadDoesNotThrowOnInvalidHex() {
        var teak = new UnityEngine.GameObject("TeakForTest").AddComponent<Teak>();
        try {
            Teak.LiveActivity.RegisterPushToStartToken("zz");
        } finally {
            UnityEngine.Object.DestroyImmediate(teak.gameObject);
        }
    }

    [TeakTest]
    public void RegisterPushToStartTokenHexOverloadDoesNotThrowOnNull() {
        var teak = new UnityEngine.GameObject("TeakForTest").AddComponent<Teak>();
        try {
            Teak.LiveActivity.RegisterPushToStartToken((string)null);
        } finally {
            UnityEngine.Object.DestroyImmediate(teak.gameObject);
        }
    }

    [TeakTest]
    public void StartedLiveActivityHexOverloadInvokesErrorCallbackOnInvalidHex() {
        Teak.LiveActivity.Reply captured = null;
        var teak = new UnityEngine.GameObject("TeakForTest").AddComponent<Teak>();
        try {
            DrainCoroutine(Teak.LiveActivity.StartedLiveActivity(
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
            DrainCoroutine(Teak.LiveActivity.ScheduleLiveActivityUpdate(
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
