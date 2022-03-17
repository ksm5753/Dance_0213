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
    public bool isEarnDouble;
    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(this.gameObject);
        MobileAds.Initialize(initStatus => {});
        RequestAd();
    }

    void RequestAd()
    {
        string adUnitId;
#if UNITY_ANDROID
        adUnitId = "ca-app-pub-3940256099942544/5224354917";
#elif UNITY_IPHONE
        adUnitId = "ca-app-pub-3940256099942544/5224354917";
#else
        adUnitId = "unexpected_platform";
#endif

        this.rewardedAd = new RewardedAd(adUnitId);

        // Called when an ad request has successfully loaded.
        rewardedAd.OnAdLoaded += HandleRewardedAdLoaded;
        // Called when an ad is shown.
        rewardedAd.OnAdOpening += HandleRewardedAdOpening;
        // Called when an ad request failed to show.
        rewardedAd.OnAdFailedToShow += HandleRewardedAdFailedToShow;
        // Called when the user should be rewarded for interacting with the ad.
        rewardedAd.OnUserEarnedReward += HandleUserEarnedReward;
        // Called when the ad is closed.
        rewardedAd.OnAdClosed += HandleRewardedAdClosed;

        // Create an empty ad request.
        AdRequest request = new AdRequest.Builder().Build();
        // Load the rewarded ad with the request.
        this.rewardedAd.LoadAd(request);
    }

    public void HandleRewardedAdLoaded(object sender, EventArgs args)
    {
        if (SceneManager.GetActiveScene().name == "2. Lobby") { LobbyUI.GetInstance().isCanWatchAd = true; }
    }

    public void HandleRewardedAdFailedToLoad(object sender, AdErrorEventArgs args)
    {
        
    }

    public void HandleRewardedAdOpening(object sender, EventArgs args)
    {
        RequestAd();
    }

    public void HandleRewardedAdFailedToShow(object sender, AdErrorEventArgs args)
    {
    }

    public void HandleRewardedAdClosed(object sender, EventArgs args)
    {
        MonoBehaviour.print("HandleRewardedAdClosed event received");
        if (isEarnDouble)
        {
            BackendServerManager.GetInstance().GiveMoeny(Game.instance.finalPrice);
            Game.instance.finalPrice = Game.instance.finalPrice * 2;
            Game.instance.pricesText[0].text = Game.instance.finalPrice.ToString();
            Game.instance.DoublePrice.SetActive(false);
            isEarnDouble = false;
        }

        else
        {
            if (!isStartGame)
            {
                BackendServerManager.GetInstance().DrawCard(true);
            }

            else
            {
                SceneManager.LoadScene("3. Game");
            }
        }
    }

    public void HandleUserEarnedReward(object sender, Reward args)
    {
        string type = args.Type;
        double amount = args.Amount;
    }

    public void UserChoseToWatchAd()
    {
        if (SceneManager.GetActiveScene().name == "2. Lobby")
        {
            if (!isStartGame)
            {
                int adNum = BackendServerManager.GetInstance().getAdviceCount(); // ÇÃ·¹ÀÌ¾îÀÇ ±¤°í º» È½¼ö)
                if (adNum != 5)
                {
                    if (rewardedAd.IsLoaded())
                    {
                        adNum = BackendServerManager.GetInstance().getAdviceCount() + 1;
                        BackendServerManager.GetInstance().setAdviceCount(5 - adNum);
                        LobbyUI.GetInstance().SetAdCount();
                        this.rewardedAd.Show();
                    }
                }
            }

            else
            {
                if (rewardedAd.IsLoaded())
                {
                    this.rewardedAd.Show();
                }

                else
                {
                    Game.instance.SceneChange("3. Game");
                }
            }
        }

        else
        {
            if (isEarnDouble)
            {
                if (rewardedAd.IsLoaded())
                {
                    this.rewardedAd.Show();
                }

                else
                {
                    Game.instance.DoublePrice.SetActive(false);
                }
            }

            else
            {
                isStartGame = true;
                if (rewardedAd.IsLoaded())
                {
                    this.rewardedAd.Show();
                }

                else
                {
                    Game.instance.SceneChange("3. Game");
                }
            }
        }
    }

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(this.gameObject);
    }

}
