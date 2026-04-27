[TeakTestFixture]
public class TeakClaimModeTests {

    [TeakTest]
    public void Legacy_MapsToLegacyString() {
        TeakAssert.AreEqual("legacy", TeakSettings.ClaimModeToNativeString(TeakClaimMode.Legacy));
    }

    [TeakTest]
    public void ClientJwt_MapsToClientJwtString() {
        TeakAssert.AreEqual("client_jwt", TeakSettings.ClaimModeToNativeString(TeakClaimMode.ClientJwt));
    }

    [TeakTest]
    public void ServerJwt_MapsToServerJwtString() {
        TeakAssert.AreEqual("server_jwt", TeakSettings.ClaimModeToNativeString(TeakClaimMode.ServerJwt));
    }
}
