using System.Xml.Linq;

[TeakTestFixture]
public class TeakAndroidAssetLibBuilderTests {

    [TeakTest]
    public void ForceDebugOutputTrue_IncludesBoolElement() {
        XDocument doc = TeakAndroidAssetLibBuilder.BuildTeakResourcesXml("app-id", "api-key", true, "legacy");
        string xml = doc.ToString();
        TeakAssert.IsTrue(xml.Contains("<bool name=\"io_teak_force_debug_output\">true</bool>"),
            "Expected XML to contain io_teak_force_debug_output bool element set to true");
    }

    [TeakTest]
    public void ForceDebugOutputFalse_OmitsBoolElement() {
        XDocument doc = TeakAndroidAssetLibBuilder.BuildTeakResourcesXml("app-id", "api-key", false, "legacy");
        string xml = doc.ToString();
        TeakAssert.IsFalse(xml.Contains("io_teak_force_debug_output"),
            "Expected XML to NOT contain io_teak_force_debug_output when false");
    }

    [TeakTest]
    public void AppIdAndApiKeyPresent() {
        XDocument doc = TeakAndroidAssetLibBuilder.BuildTeakResourcesXml("my-app", "my-key", false, "legacy");
        string xml = doc.ToString();
        TeakAssert.IsTrue(xml.Contains("my-app"), "Expected XML to contain app id");
        TeakAssert.IsTrue(xml.Contains("my-key"), "Expected XML to contain api key");
    }

    [TeakTest]
    public void ClaimModeLegacy_WritesLegacyString() {
        XDocument doc = TeakAndroidAssetLibBuilder.BuildTeakResourcesXml("app-id", "api-key", false, "legacy");
        string xml = doc.ToString();
        TeakAssert.IsTrue(xml.Contains("<string name=\"io_teak_reward_claim_mode\">legacy</string>"),
            "Expected XML to contain io_teak_reward_claim_mode=legacy");
    }

    [TeakTest]
    public void ClaimModeClientJwt_WritesClientJwtString() {
        XDocument doc = TeakAndroidAssetLibBuilder.BuildTeakResourcesXml("app-id", "api-key", false, "client_jwt");
        string xml = doc.ToString();
        TeakAssert.IsTrue(xml.Contains("<string name=\"io_teak_reward_claim_mode\">client_jwt</string>"),
            "Expected XML to contain io_teak_reward_claim_mode=client_jwt");
    }

    [TeakTest]
    public void ClaimModeServerJwt_WritesServerJwtString() {
        XDocument doc = TeakAndroidAssetLibBuilder.BuildTeakResourcesXml("app-id", "api-key", false, "server_jwt");
        string xml = doc.ToString();
        TeakAssert.IsTrue(xml.Contains("<string name=\"io_teak_reward_claim_mode\">server_jwt</string>"),
            "Expected XML to contain io_teak_reward_claim_mode=server_jwt");
    }
}
