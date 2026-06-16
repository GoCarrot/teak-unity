using System.Collections;

[TeakTestFixture]
public class TeakWebPushTests {

    static void DrainCoroutine(IEnumerator co) {
        while (co.MoveNext()) { }
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
}
