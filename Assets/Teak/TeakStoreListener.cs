#if UNITY_PURCHASING
using UnityEngine;
using UnityEngine.Purchasing;

using System;
using System.Collections;
using System.Collections.Generic;

#if !TEAK_NOT_AVAILABLE
using MiniJSON.Teak;
#endif // !TEAK_NOT_AVAILABLE

public class TeakStoreListener : IStoreListener {
    public IStoreListener AttachedStoreListener { get; private set; }

    public TeakStoreListener(IStoreListener hostedListener) {
        if (hostedListener == null) throw new ArgumentNullException("hostedListener");
        this.AttachedStoreListener = hostedListener;
    }

    #region IStoreListener
    public void OnInitialized(IStoreController controller, IExtensionProvider extensions) {
        this.AttachedStoreListener.OnInitialized(controller, extensions);
    }

    public void OnInitializeFailed(InitializationFailureReason error) {
        this.AttachedStoreListener.OnInitializeFailed(error);
    }

    public void OnPurchaseFailed(Product item, PurchaseFailureReason r) {
        this.AttachedStoreListener.OnPurchaseFailed(item, r);
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs e) {
#if !UNITY_EDITOR && UNITY_ANDROID && !TEAK_NOT_AVAILABLE
        try {
            Dictionary<string, object> receipt = Json.Deserialize(e.purchasedProduct.receipt) as Dictionary<string,object>;
            if ("GooglePlay".Equals(receipt["Store"])) {
                Dictionary<string, object> receiptPayload = Json.Deserialize(receipt["Payload"] as string) as Dictionary<string,object>;
                Dictionary<string, object> receiptPayloadJson = Json.Deserialize(receiptPayload["json"] as string) as Dictionary<string,object>;
                string receiptPayloadJsonString = Json.Serialize(receiptPayloadJson);

                AndroidJavaClass teak = new AndroidJavaClass("io.teak.sdk.Teak");
                teak.CallStatic("pluginPurchaseSucceeded", receiptPayloadJsonString, "unityiap");
            }
        } finally {
        }
#endif // UNITY_ANDROID && !TEAK_NOT_AVAILABLE
        return this.AttachedStoreListener.ProcessPurchase(e);
    }
    #endregion IStoreListener
}
#endif // UNITY_PURCHASING
