using System.Collections.Generic;

[TeakTestFixture]
public class TeakPostLaunchSummaryTests {

    [TeakTest]
    public void SystemActivityIdParsedFromLaunchData() {
        var summary = new TeakPostLaunchSummary(new Dictionary<string, object> {
            {"teakSystemActivityId", "ABCD-1234"}
        });
        TeakAssert.AreEqual("ABCD-1234", summary.SystemActivityId);
    }

    [TeakTest]
    public void SystemActivityIdNullWhenAbsent() {
        var summary = new TeakPostLaunchSummary(new Dictionary<string, object>());
        TeakAssert.IsNull(summary.SystemActivityId);
    }
}
