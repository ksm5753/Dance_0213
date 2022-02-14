using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using BackEnd;
using UnityEngine.Purchasing;

public class InAppPurchaser : MonoBehaviour, IStoreListener
{
    public static InAppPurchaser instance;

    private static IStoreController storeController;
    private static IExtensionProvider extensionProvider;

    public const string FRESH_MAN = "com.touchtouch.sandbox_dance.freshman";
    public const string COIN10000 = "com.touchtouch.sandbox_dance.coin10000";
    public const string GRADUATE = "com.touchtouch.sandbox_dance.graduate";
    public const string DIA200 = "com.touchtouch.sandbox_dance.dia200";
    public const string DIA500 = "com.touchtouch.sandbox_dance.dia500";
    public const string DIA1200 = "com.touchtouch.sandbox_dance.dia1200";

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


        var module = StandardPurchasingModule.Instance();

        ConfigurationBuilder builder = ConfigurationBuilder.Instance(module);

        builder.AddProduct(FRESH_MAN, ProductType.NonConsumable);
        builder.AddProduct(GRADUATE, ProductType.NonConsumable);
        builder.AddProduct(DIA200, ProductType.Consumable);
        builder.AddProduct(DIA500, ProductType.Consumable);
        builder.AddProduct(DIA1200, ProductType.Consumable);
        builder.AddProduct(COIN10000, ProductType.Consumable);


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
                            // TODO : TEST 기능 (이미 구매한 상품이면 광고 지우기)
                            //AdsManager.instance.SetRemoveAds();
                            //BackEndServerManager.instance.SetRemoveAds();
                            switch (_productId)
                            {
                                case FRESH_MAN:
                                    PlayerPrefs.SetInt("isBuyNewStudent", 1);
                                    break;
                                case GRADUATE:
                                    PlayerPrefs.SetInt("isBuyOldStudent", 1);
                                    break;
                            }

                            LobbyUI.GetInstance().CheckBuyItems();
                        }
                        else
                        {
                            storeController.InitiatePurchase(p);
                        }
                    }
                }
                else
                {
                }
            }
            else
            {
            }
        }
        catch (Exception e)
        {
            print(e);
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
                    // TODO : TEST 기능 (이미 구매한 상품이면 광고 지우기)
                    //AdsManager.instance.SetRemoveAds();
                    //BackEndServerManager.instance.SetRemoveAds();
                }
                else
                {
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
        storeController = _sc;
        extensionProvider = _ep;
    }

    public void OnInitializeFailed(InitializationFailureReason reason)
    {
    }

    // ====================================================================================================
    #region 영수증 검증
    /* 
     *
	 */
    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        string id = string.Empty;
        string token = string.Empty;
        Param param = new Param();

        // 뒤끝 영수증 검증 처리    
        BackendReturnObject validation = null;
#if UNITY_ANDROID || UNITY_EDITOR
        validation = Backend.Receipt.IsValidateGooglePurchase(args.purchasedProduct.receipt, "receiptDescriptionGoogle");

        BackEnd.Game.Payment.GoogleReceiptData.FromJson(args.purchasedProduct.receipt, out id, out token);
        param.Add("productID", id);
        param.Add("token", token);

        param.Add("platform", "google");
#elif UNITY_IOS
        validation = Backend.Receipt.IsValidateApplePurchase(args.purchasedProduct.receipt, "receiptDescriptionApple");
#endif
        string msg = "";

        // 뒤끝 펑션 호출
        Backend.BFunc.InvokeFunction("receiptVaildate", param, callback => {
            if (callback.IsSuccess() == false)
            {
                return;
            }
            var result = callback.GetReturnValuetoJSON()["result"].ToString();
        });

        // 영수증 검증에 성공한 경우
        if (validation.IsSuccess())
        {
            // 구매 성공한 제품에 대한 id 체크하여 그에맞는 보상 
            // A consumable product has been purchased by this user.
            if (String.Equals(args.purchasedProduct.definition.id, TEST, StringComparison.Ordinal))
            {
                msg = string.Format("ProcessPurchase: PASS. Product: '{0}'", args.purchasedProduct.definition.id);
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
            Debug.Log(msg);
            Debug.Log(validation);

            //BackEndServerManager.instance.BuyRemoveAdsFailed();
        }

        // Return a flag indicating whether this product has completely been received, or if the application needs 
        // to be reminded of this purchase at next app launch. Use PurchaseProcessingResult.Pending when still 
        // saving purchased products to the cloud, and when that save is delayed.
        Backend.TBC.ChargeTBC(args.purchasedProduct.receipt, "파격 할인중!");
        return PurchaseProcessingResult.Complete;
    }
    #endregion

    // ====================================================================================================	

    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
    }

    // ==================================================
    public void BuyItem(int num)
    {
        switch (num)
        {
            case 0:
                BuyProductID(FRESH_MAN);
                break;
            case 1:
                BuyProductID(GRADUATE);
                break;
            case 2:
                BuyProductID(DIA200);
                break;
            case 3:
                BuyProductID(DIA500);
                break;
            case 4:
                BuyProductID(DIA1200);
                break;
            case 5:
                BuyProductID(COIN10000);
                break;
        }
    }
}
