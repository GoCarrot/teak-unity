#region License
/* Teak -- Copyright (C) 2016 GoCarrot Inc.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
#endregion

#region References
/// @cond hide_from_doxygen
using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;

using MiniJSON.Teak;
/// @endcond
#endregion

public class TeakReward {
    public enum RewardStatus {
        GrantReward,
        SelfClick,
        AlreadyClicked,
        TooManyClicks,
        ExceedMaxClicksForDay,
        Expired,
        InvalidPost,
        InternalError
    }

    public RewardStatus Status { get; set; }
    public Dictionary<string, object> Reward { get; set; }
    public string ScheduleId { get; set; }
    public string CreativeId { get; set; }
    public string RewardId { get; set; }

    public TeakReward(Dictionary<string, object> json) {
        this.RewardId = json["teakRewardId"] as string;
        this.Status = RewardStatus.InternalError;
        switch (json["status"] as string) {
            case "grant_reward": {
                // The user has been issued this reward by Teak
                try {
                    this.Status = RewardStatus.GrantReward;
                    this.Reward = json["reward"] as Dictionary<string, object>;

                    // Optional
                    if (json.ContainsKey("teakScheduleName")) this.ScheduleId = json["teakScheduleName"] as string;
                    if (json.ContainsKey("teakCreativeName")) this.CreativeId = json["teakCreativeName"] as string;
                } catch {
                    this.Status = RewardStatus.InternalError;
                }
            }
            break;

            case "self_click": {
                // The user has attempted to claim a reward from their own social post
                this.Status = RewardStatus.SelfClick;
            }
            break;

            case "already_clicked": {
                // The user has already been issued this reward
                this.Status = RewardStatus.AlreadyClicked;
            }
            break;

            case "too_many_clicks": {
                // The reward has already been claimed its maximum number of times globally
                this.Status = RewardStatus.TooManyClicks;
            }
            break;

            case "exceed_max_clicks_for_day": {
                // The user has already claimed their maximum number of rewards of this type for the day
                this.Status = RewardStatus.ExceedMaxClicksForDay;
            }
            break;

            case "expired": {
                // This reward has expired and is no longer valid
                this.Status = RewardStatus.Expired;
            }
            break;

            case "invalid_post": {
                //Teak does not recognize this reward id
                this.Status = RewardStatus.InvalidPost;
            }
            break;
        }
    }
}
