using System.Xml.Linq;

[TeakTestFixture]
public class TeakAndroidAssetLibBuilderTests {

    [TeakTest]
    public void ForceDebugOutputTrue_IncludesBoolElement() {
        XDocument doc = TeakAndroidAssetLibBuilder.BuildTeakResourcesXml("app-id", "api-key", true);
        string xml = doc.ToString();
        TeakAssert.IsTrue(xml.Contains("<bool name=\"io_teak_force_debug_output\">true</bool>"),
            "Expected XML to contain io_teak_force_debug_output bool element set to true");
    }

    [TeakTest]
    public void ForceDebugOutputFalse_OmitsBoolElement() {
        XDocument doc = TeakAndroidAssetLibBuilder.BuildTeakResourcesXml("app-id", "api-key", false);
        string xml = doc.ToString();
        TeakAssert.IsFalse(xml.Contains("io_teak_force_debug_output"),
            "Expected XML to NOT contain io_teak_force_debug_output when false");
    }

    [TeakTest]
    public void AppIdAndApiKeyPresent() {
        XDocument doc = TeakAndroidAssetLibBuilder.BuildTeakResourcesXml("my-app", "my-key", false);
        string xml = doc.ToString();
        TeakAssert.IsTrue(xml.Contains("my-app"), "Expected XML to contain app id");
        TeakAssert.IsTrue(xml.Contains("my-key"), "Expected XML to contain api key");
    }
}
