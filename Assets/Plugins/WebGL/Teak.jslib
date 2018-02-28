/* Teak -- Copyright (C) 2017 GoCarrot Inc.
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

mergeInto(LibraryManager.library, {
  TeakInitWebGL: function(ptr_appId, ptr_apiKey) {
    var appId = Pointer_stringify(ptr_appId);
    var apiKey = Pointer_stringify(ptr_apiKey);

    var doTeakInit = function() {
      (function(){window.teak=window.teak||[];window.teak.methods=["init","on","asyncInit","identify","trackEvent","postAction","postAchievement","postHighScore","canMakeFeedPost","popupFeedPost","reportNotificationClick","reportFeedClick","sendRequest","acceptRequest","loadInboxData", "claimReward", "setIsUnity", "scheduleNotification", "cancelNotification", "cancelAllNotifications"];window.teak.factory=function(e){return function(){var t=Array.prototype.slice.call(arguments);t.unshift(e);window.teak.push(t);return window.teak}};for(var e=0;e<window.teak.methods.length;e++){var t=window.teak.methods[e];if(!window.teak[t]){window.teak[t]=window.teak.factory(t)}}var n=document.createElement("script");n.type="text/javascript";n.async=true;n.src="//d2h7sc2qwu171k.cloudfront.net/teak.min.js";var r=document.getElementsByTagName("script")[0];r.parentNode.insertBefore(n,r)})()

      window.teak.init(appId, apiKey);
      window.teak.setIsUnity();
    };

    // Load jQuery, if it's not already loaded
    if (window.jQuery === undefined) {
      var s = document.createElement('script');
      s.src = "//ajax.googleapis.com/ajax/libs/jquery/1.7.2/jquery.js";
      s.onload = doTeakInit;
      document.head.appendChild(s);
    } else {
      doTeakInit();
    }
  },
  TeakIdentifyUser: function(ptr_userId) {
    var userId = Pointer_stringify(ptr_userId);
    window.teak.identify(userId);

    window.teak.claimReward(function(reply) {
      SendMessage("TeakGameObject", "RewardClaimAttempt", JSON.stringify(reply));
    });
  },
  TeakTrackEvent: function(ptr_actionId, ptr_objectTypeId, ptr_objectInstanceId) {
    var actionId = Pointer_stringify(ptr_actionId).trim();
    var objectTypeId = Pointer_stringify(ptr_objectTypeId).trim();
    var objectInstanceId = Pointer_stringify(ptr_objectInstanceId).trim();

    objectTypeId = objectTypeId.length === 0 ? undefined : objectTypeId;
    objectInstanceId = objectInstanceId.length === 0 ? undefined : objectInstanceId;

    window.teak.trackEvent(actionId, objectTypeId, objectInstanceId);
  },
  TeakDeepLinkTableInternal: {},
  TeakUnityRegisterRoute__deps: ['TeakDeepLinkTableInternal'],
  TeakUnityRegisterRoute: function(ptr_route, ptr_name, ptr_description) {
    var route = Pointer_stringify(ptr_route);
    var name = Pointer_stringify(ptr_name);
    var description = Pointer_stringify(ptr_description);

    _TeakDeepLinkTableInternal[route] = {
      name: name,
      description: description
    };
  },
  TeakUnityReadyForDeepLinks: function() {

  },
  TeakSetBadgeCount: function(count) {

  }
});
