/* Teak -- Copyright (C) 2014 GoCarrot Inc.
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

using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

public class TeakPostProcessScene
{
    [PostProcessScene]
    public static void OnPostprocessScene()
    {
        if(string.IsNullOrEmpty(TeakSettings.TeakAppId))
        {
            Debug.LogError("Teak App Id needs to be assigned in the Edit/Teak menu.");
        }
        if(string.IsNullOrEmpty(TeakSettings.TeakAppSecret))
        {
            Debug.LogError("Teak App Secret needs to be assigned in the Edit/Teak menu.");
        }

        if(!TeakSettings.AppValid)
        {
            Debug.LogWarning("Your Teak settings have not been validated. Click 'Validate Settings' in the Edit/Teak menu.");
        }

        if(PlayerSettings.iOS.exitOnSuspend)
        {
            Debug.LogWarning("Your app is set to exit when it is suspended on iOS (when the Home button is pushed). This will prevent Teak from tracking session times.");
        }
    }
}
