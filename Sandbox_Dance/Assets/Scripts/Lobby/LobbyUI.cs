using BackEnd;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    [Header("Setting Panel")]
    public GameObject settingPanel;
    public Text nickNameText;

    [Space(15f)]
    [Header("Store Panel")]
    public GameObject storePanel;
    public Text goldText2;
    public Text diamondText2;

    [Space(15f)]
    [Header("Rank Panel")]
    public GameObject rankPanel;

    [Space(15f)]
    [Header("Collection Panel")]
    public GameObject collectionPanel;
    public ToggleGroup collectionGroup;
    public Text gameNameText;

    public GameObject[] Cards;
    public Sprite[] CardImage;

    // 내가 추가한거(이태호)
    public GameObject CardInfo;
    public GameObject getRubyInfo;
    public GameObject popUpUI;
    public GameObject drawCardUI;
    public GameObject detailPopUpUI;

    [Space(15f)]
    [Header("DrawCard Panel")]
    public GameObject drawCardPanel;
    public Text goldText;
    public Text diamondText;

    [Space(15f)]
    [Header("Others")]
    public GameObject loadingObject;
    public GameObject errorObject;

    private static LobbyUI instance;


    #region 초기화 (ScaleCtrl -> Initialize)
    public void Initialize()
    {
        loadingObject.SetActive(false);
        errorObject.SetActive(false);

        settingPanel.SetActive(false);
        storePanel.SetActive(false);
        rankPanel.SetActive(false);
        collectionPanel.SetActive(false);
        drawCardPanel.SetActive(false);
        CardInfo.SetActive(false);
        getRubyInfo.SetActive(false);
        popUpUI.SetActive(false);
        drawCardUI.SetActive(false);
        detailPopUpUI.SetActive(false);

        CheckBuyItems();
    }
    #endregion

    void Start()
    {
        if (BackendServerManager.GetInstance() != null) setNickName(); //로비 들어오면 닉네임 설정

        settingPanel.GetComponentsInChildren<Toggle>()[0].isOn = PlayerPrefs.GetInt("BGM_Mute") == 1 ? true : false; //소리 설정

        BackendServerManager.GetInstance().InitalizeGameData(); //게임데이터 초기설정

        if (!PlayerPrefs.HasKey("isBuyNewStudent"))
        {
            PlayerPrefs.SetInt("isBuyNewStudent", 0);
        }

        if (!PlayerPrefs.HasKey("isBuyOldStudent"))
        {
            PlayerPrefs.SetInt("isBuyOldStudent", 0);
        }

    }

    #region 닉네임 설정
    private void setNickName()
    {
        var name = BackendServerManager.GetInstance().myNickName;
        if (name.Equals(string.Empty))
        {
            Debug.Log("닉네임 불러오기 실패");
            name = "Error";
        }

        nickNameText.text = name;
    }
    #endregion

    public void getInfoMoney()
    {
        BackendServerManager.GetInstance().GetMyMoney();
    }

    public void BuyWithGold(int num)
    {
        BackendServerManager.GetInstance().BuyItems(num, true);
    }

    public void BuyWithDiamond(int num)
    {
        BackendServerManager.GetInstance().BuyItems(num, false);
    }

    public void GameStart()
    {
        SceneManager.LoadScene("3. Game");
    }

    public void getRankInfo()
    {
        BackendServerManager.GetInstance().getRank();
    }

    public void ShowRankUI()
    {
        rankPanel.SetActive(true);
    }

    public void setCardInfo(int cardNum, string count)
    {
        if (count == "0")
        {
            Cards[cardNum].GetComponentsInChildren<Image>()[1].sprite = CardImage[0];
            Cards[cardNum].GetComponentInChildren<Text>().enabled = false;
        }
        else
        {
            Cards[cardNum].GetComponentsInChildren<Image>()[1].sprite = CardImage[cardNum + 1];
            Cards[cardNum].GetComponentInChildren<Text>().enabled = true;
        }

        Cards[cardNum].GetComponentInChildren<Text>().text = count.ToString();
        Cards[cardNum].GetComponent<Button>().onClick.AddListener(() => OpenCardInfo(cardNum, count));

        GameObject.Find("Scroll View").GetComponent<ScrollRect>().velocity = new Vector2(0, 0); //스크롤 하면서 다른 창으로 넘어갈 때 스크롤 저항 생김
        GameObject.Find("Content").GetComponent<RectTransform>().position = new Vector2(GameObject.Find("Content").GetComponent<RectTransform>().position.x, 0); //다른 창으로 넘길때 맨 위로 이동
    }

    public void OpenCardInfo(int cardNum, string count)
    {
        if (count != "0")
        {
            CardInfo.SetActive(true);

            CardInfo.transform.GetChild(3).GetComponent<Text>().text = count;
            CardInfo.transform.GetChild(2).GetComponent<Image>().sprite = CardImage[cardNum + 1];

            CardInfo.transform.GetChild(5).GetComponent<Button>().onClick.AddListener(() => DiscardCard(int.Parse(count)));
        }
    }

    void DiscardCard(int cardCount)
    {
        int getRubyNum = 0;

        if (cardCount > 100) getRubyNum = 10;

        else if (cardCount > 50) getRubyNum = 5;


        else if (cardCount > 30) getRubyNum = 3;


        else if (cardCount > 10) getRubyNum = 1;


        // 이곳에 플레이어게 루비추가해주는 기능 추가
        // 플레이어 루비 += getRubyNum;
        getRubyInfo.SetActive(true);
        getRubyInfo.transform.GetChild(2).GetComponent<Text>().text = cardCount + " 개 획득";
    }


    //카드
    public void CollectionValue()
    {
        switch (collectionGroup.GetFirstActiveToggle().name)
        {
            case "MenuToggle_0":
                gameNameText.text = "급식왕";
                BackendServerManager.GetInstance().GetUserCards(1);
                break;
            case "MenuToggle_1":
                gameNameText.text = "급식왕2";
                BackendServerManager.GetInstance().GetUserCards(2);
                break;
            case "MenuToggle_2":
                gameNameText.text = "급식왕3";
                BackendServerManager.GetInstance().GetUserCards(3);
                break;
            case "MenuToggle_3":
                gameNameText.text = "급식왕4";
                BackendServerManager.GetInstance().GetUserCards(4);
                break;
            case "MenuToggle_4":
                gameNameText.text = "선생님 몰래";
                BackendServerManager.GetInstance().GetUserCards(5);
                break;
            default:
                break;
        }
    }

    public void CheckBuyItems()
    {
        if(PlayerPrefs.GetInt("isBuyNewStudent") == 1)
        {
            storePanel.transform.GetChild(1).GetChild(0).GetChild(0).GetChild(0).GetChild(0).gameObject.SetActive(false);
        }

        if(PlayerPrefs.GetInt("isBuyOldStudent") == 1)
        {
            storePanel.transform.GetChild(1).GetChild(0).GetChild(0).GetChild(0).GetChild(1).gameObject.SetActive(false);
        }
    }


    public static LobbyUI GetInstance()
    {
        if (instance == null) return null;

        return instance;
    }

    void Awake()
    {
        if (!instance) instance = this;
    }
}
