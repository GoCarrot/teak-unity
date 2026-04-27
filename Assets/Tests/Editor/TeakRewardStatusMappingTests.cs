using System.Collections.Generic;

[TeakTestFixture]
public class TeakRewardStatusMappingTests {

    private static TeakReward MakeReward(string status) {
        var json = new Dictionary<string, object> {
            { "teakRewardId", "reward-id" },
            { "status", status }
        };
        return new TeakReward(json);
    }

    [TeakTest]
    public void PlayerIneligible_MapsToPlayerIneligible() {
        TeakReward reward = MakeReward("player_ineligible");
        TeakAssert.AreEqual(TeakReward.RewardStatus.PlayerIneligible, reward.Status);
    }

    [TeakTest]
    public void NoRewardAvailable_MapsToNoRewardAvailable() {
        TeakReward reward = MakeReward("no_reward_available");
        TeakAssert.AreEqual(TeakReward.RewardStatus.NoRewardAvailable, reward.Status);
    }

    [TeakTest]
    public void ClaimModeUnsupported_MapsToClaimModeUnsupported() {
        TeakReward reward = MakeReward("claim_mode_unsupported");
        TeakAssert.AreEqual(TeakReward.RewardStatus.ClaimModeUnsupported, reward.Status);
    }

    [TeakTest]
    public void GrantReward_StillMapsCorrectly() {
        var json = new Dictionary<string, object> {
            { "teakRewardId", "reward-id" },
            { "status", "grant_reward" },
            { "reward", new Dictionary<string, object> { { "coins", 100 } } }
        };
        TeakReward reward = new TeakReward(json);
        TeakAssert.AreEqual(TeakReward.RewardStatus.GrantReward, reward.Status);
    }

    [TeakTest]
    public void UnknownStatus_StaysInternalError() {
        TeakReward reward = MakeReward("some_unknown_value");
        TeakAssert.AreEqual(TeakReward.RewardStatus.InternalError, reward.Status);
    }
}
