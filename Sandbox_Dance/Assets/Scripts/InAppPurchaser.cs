using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using BackEnd;
using UnityEngine.Purchasing;

public class InAppPurchaser : MonoBehaviour, IStoreListener
{
    public Text text;
    public static InAppPurchaser instance;

    private static IStoreController storeController;
    private static IExtensionProvider extensionProvider;

    public const string TEST = "com.touchtouch.sandbox_dance.coin500";

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        InitializePurchasing();
    }

    bool IsInitialized()
    {
        return (storeController != null && extensionProvider != null);
    }
    
    void InitializePurchasing()
    {
        if (IsInitialized()) return;

        text.text = ("InAppPurchaser.cs - InitializePurchasing - START");

        var module = StandardPurchasingModule.Instance();

        ConfigurationBuilder builder = ConfigurationBuilder.Instance(module);
        // remove ads
        builder.AddProduct(TEST, ProductType.Consumable); //, new IDs {{ removeAds, GooglePlay.Name }});


        UnityPurchasing.Initialize(this, builder);
        Debug.Log("##### InitializePurchasing : Initialize");
    }

    void BuyProductID(string _productId)
    {
        try
        {
            if (IsInitialized())
            {
                Product p = storeController.products.WithID(_productId);

                if (p != null)
                {
                    if (p.availableToPurchase)
                    {
                        if (p.definition.type == ProductType.NonConsumable && p.hasReceipt)
                        {
                            text.text = ("이미 구매한 상품입니다.");
                            // TODO : TEST 기능 (이미 구매한 상품이면 광고 지우기)
                            //AdsManager.instance.SetRemoveAds();
                            //BackEndServerManager.instance.SetRemoveAds();
                        }
                        else
                        {
                            text.text = (string.Format("Purchasing product asychronously: '{0}'", p.definition.id));
                            storeController.InitiatePurchase(p);
                        }
                    }
                }
                else
                {
                    text.text = ("BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase");
                }
            }
            else
            {
                text.text = ("BuyProductID FAIL. Not initialized.");
            }
        }
        catch (Exception e)
        {
            text.text = ("BuyProductID: FAIL. Exception during purchase. " + e);
        }
    }

    public void PurchaseRestore()
    {
        // If Purchasing has not yet been set up ...
        if (!IsInitialized())
        {
            // ... report the situation and stop restoring. Consider either waiting longer, or retrying initialization.
            Debug.Log("RestorePurchases FAIL. Not initialized.");
            return;
        }
        // If we are running on an Apple device ...
        if (Application.platform == RuntimePlatform.IPhonePlayer ||
            Application.platform == RuntimePlatform.OSXPlayer)
        {
            // ... begin restoring purchases
            Debug.Log("RestorePurchases started ...");
            // Fetch the Apple store-specific subsystem.
            var apple = extensionProvider.GetExtension<IAppleExtensions>();
            // Begin the asynchronous process of restoring purchases. Expect a confirmation response in the Action<bool> below, and ProcessPurchase if there are previously purchased products to restore.
            apple.RestoreTransactions((result) =>
            {

                // The first phase of restoration. If no more responses are received on ProcessPurchase then no purchases are available to be restored.
                Debug.Log("RestorePurchases continuing: " + result + ". If no further messages, no purchases available to restore.");
                if (result)
                {
                    text.text = ("구매 복원 성공");
                    // TODO : TEST 기능 (이미 구매한 상품이면 광고 지우기)
                    //AdsManager.instance.SetRemoveAds();
                    //BackEndServerManager.instance.SetRemoveAds();
                }
                else
                {
                    text.text = ("구매 복원 실패");
                }
            });
        }
        // Otherwise ...
        else
        {
            // We are not running on an Apple device. No work is necessary to restore purchases.
            Debug.Log("RestorePurchases FAIL. Not supported on this platform. Current = " + Application.platform);
        }
    }

    public void OnInitialized(IStoreController _sc, IExtensionProvider _ep)
    {
        text.text = ("OnInitialized: PASS");
        storeController = _sc;
        extensionProvider = _ep;
    }

    public void OnInitializeFailed(InitializationFailureReason reason)
    {
        text.text = ("OnInitializeFailed : \n" + reason);
    }

    // ====================================================================================================
    #region 영수증 검증
    /* 
     *
	 */
    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        text.text = (args.purchasedProduct.availableToPurchase).ToString();
        // 뒤끝 영수증 검증 처리    
        BackendReturnObject validation = null;
#if UNITY_ANDROID || UNITY_EDITOR
        validation = Backend.Receipt.IsValidateGooglePurchase(args.purchasedProduct.receipt, "receiptDescriptionGoogle");
#elif UNITY_IOS
        validation = Backend.Receipt.IsValidateApplePurchase(args.purchasedProduct.receipt, "receiptDescriptionApple");
#endif
        string msg = "";

        // 영수증 검증에 성공한 경우
        if (validation.IsSuccess())
        {
            // 구매 성공한 제품에 대한 id 체크하여 그에맞는 보상 
            // A consumable product has been purchased by this user.
            if (String.Equals(args.purchasedProduct.definition.id, TEST, StringComparison.Ordinal))
            {
                msg = string.Format("ProcessPurchase: PASS. Product: '{0}'", args.purchasedProduct.definition.id);
                text.text = (msg);
                Debug.Log(msg);

                //AdsManager.instance.SetRemoveAds();
                //BackEndServerManager.instance.BuyRemoveAdsSuccess();
            }
        }
        // 영수증 검증에 실패한 경우 
        else
        {
            // Or ... an unknown product has been purchased by this user. Fill in additional products here....
            msg = string.Format("ProcessPurchase: FAIL. Unrecognized product: '{0}'", args.purchasedProduct.definition.id);
            text.text = (msg + ", " + validation);
            Debug.Log(msg);
            Debug.Log(validation);

            //BackEndServerManager.instance.BuyRemoveAdsFailed();
        }

        // Return a flag indicating whether this product has completely been received, or if the application needs 
        // to be reminded of this purchase at next app launch. Use PurchaseProcessingResult.Pending when still 
        // saving purchased products to the cloud, and when that save is delayed.
        return PurchaseProcessingResult.Complete;
    }
    #endregion

    // ====================================================================================================	

    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        text.text = ("구매 실패\n" + product.definition.storeSpecificId + "\n" + failureReason);
    }

    // ==================================================
    public void BuyRemoveAds()
    {
        BuyProductID(TEST);
    }
}
