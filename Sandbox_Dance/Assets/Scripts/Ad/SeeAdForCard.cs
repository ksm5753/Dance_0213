using UnityEngine.Events;
using UnityEngine;
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class SeeAdForCard : MonoBehaviour
{
    public static SeeAdForCard instance;
    private RewardedAd rewardedAd;
    public bool isStartGame;
    // Start is called before the first frame update
    void Start()
    {
        MobileAds.Initialize(initStatus => { });

        string adUnitId;
#if UNITY_ANDROID
        adUnitId = "ca-app-pub-3940256099942544/5224354917";
#elif UNITY_IPHONE
            adUnitId = "ca-app-pub-3940256099942544/1712485313";
#else
            adUnitId = "unexpected_platform";
#endif

        this.rewardedAd = new RewardedAd(adUnitId);

        // Called when an ad request has successfully loaded.
        this.rewardedAd.OnAdLoaded += HandleRewardedAdLoaded;
        // Called when an ad is shown.
        this.rewardedAd.OnAdOpening += HandleRewardedAdOpening;
        // Called when an ad request failed to show.
        this.rewardedAd.OnAdFailedToShow += HandleRewardedAdFailedToShow;
        // Called when the user should be rewarded for interacting with the ad.
        this.rewardedAd.OnUserEarnedReward += HandleUserEarnedReward;
        // Called when the ad is closed.
        this.rewardedAd.OnAdClosed += HandleRewardedAdClosed;

        // Create an empty ad request.
        AdRequest request = new AdRequest.Builder().Build();
        // Load the rewarded ad with the request.

        this.rewardedAd.LoadAd(request);

    }

    public void HandleRewardedAdLoaded(object sender, EventArgs args)
    {
        MonoBehaviour.print("HandleRewardedAdLoaded event received");
        if (SceneManager.GetActiveScene().name == "2. Lobby") { LobbyUI.GetInstance().isCanWatchAd = true; }
    }

    public void HandleRewardedAdFailedToLoad(object sender, AdErrorEventArgs args)
    {
        MonoBehaviour.print(
            "HandleRewardedAdFailedToLoad event received with message: "
                             + args.Message);
    }

    public void HandleRewardedAdOpening(object sender, EventArgs args)
    {
        MonoBehaviour.print("HandleRewardedAdOpening event received");
    }

    public void HandleRewardedAdFailedToShow(object sender, AdErrorEventArgs args)
    {
        MonoBehaviour.print(
            "HandleRewardedAdFailedToShow event received with message: "
                             + args.Message);
    }

    public void HandleRewardedAdClosed(object sender, EventArgs args)
    {
        MonoBehaviour.print("HandleRewardedAdClosed event received");
        // Create an empty ad request.
        AdRequest request = new AdRequest.Builder().Build();
        // Load the rewarded ad with the request.
        this.rewardedAd.LoadAd(request);
        if (!isStartGame) 
        {
            BackendServerManager.GetInstance().DrawCard(true);
        }

        else 
        {
            SceneManager.LoadScene("3. Game");
        }
    }

    public void HandleUserEarnedReward(object sender, Reward args)
    {
        string type = args.Type;
        double amount = args.Amount;
        MonoBehaviour.print(
            "HandleRewardedAdRewarded event received for "
                        + amount.ToString() + " " + type);
    }

    public void UserChoseToWatchAd()
    {
        if (!isStartGame)
        {
            int adNum = BackendServerManager.GetInstance().getAdviceCount(); // ÇÃ·¹ÀÌ¾îÀÇ ±¤°í º» È½¼ö)
            if (adNum != 5)
            {
                if (this.rewardedAd.IsLoaded())
                {
                    this.rewardedAd.Show();
                    if (!isStartGame)
                    {
                        adNum = BackendServerManager.GetInstance().getAdviceCount() + 1;
                        BackendServerManager.GetInstance().setAdviceCount(5 - adNum);
                        LobbyUI.GetInstance().SetAdCount();
                    }
                }
            }
        }

        else
        {
            if (this.rewardedAd.IsLoaded())
            {
                this.rewardedAd.Show();
            }

            else
            {
                Game.instance.SceneChange("3. Game");
            }
        }
    }

    void Awake()
    {
        if (instance == null) instance = this;
    }

}
