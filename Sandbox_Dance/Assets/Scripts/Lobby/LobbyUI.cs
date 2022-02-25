using BackEnd;
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
    public GameObject cardInfo;
    public GameObject getRubyInfo;
    public GameObject popUpUI;
    public GameObject drawCardUI;
    public GameObject detailPopUpUI;
    public GameObject closeAppUI;
    public Sprite[] cardOutLine;
    public Text adCount;
    public int testBOmb = 40; // 임시 확률

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

    private int currentDay = 0;

    public void ANG()
    {
        BackendServerManager.GetInstance().ANG();
    }


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
        cardInfo.SetActive(false);
        getRubyInfo.SetActive(false);
        popUpUI.SetActive(false);
        drawCardUI.SetActive(false);
        detailPopUpUI.SetActive(false);
        closeAppUI.SetActive(false);

        CheckBuyItems();
    }
    #endregion

    void Start()
    {
        Time.timeScale = 1;
        if (BackendServerManager.GetInstance() != null) setNickName(); //로비 들어오면 닉네임 설정

        settingPanel.GetComponentsInChildren<Toggle>()[0].isOn = PlayerPrefs.GetInt("BGM_Mute") == 1 ? true : false; //소리 설정
        settingPanel.GetComponentsInChildren<Toggle>()[1].isOn = PlayerPrefs.GetInt("Effect_Mute") == 1 ? true : false; // 효과음
        settingPanel.GetComponentsInChildren<Toggle>()[2].isOn = PlayerPrefs.GetInt("Vibrate_Mute") == 1 ? true : false; // 진동

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

    void Update()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            closeAppUI.SetActive(true);
        }
    }

    public void SetAdCount()
    {
        adCount.text = (5 - BackendServerManager.GetInstance().getAdviceCount()).ToString();
    }
    public void GameStart()
    {
        int randomNum = Random.Range(1, 101);
        if(randomNum <= testBOmb)
        {

        }

        else SceneManager.LoadScene("3. Game");
    }

    public void LogOut()
    {
        BackendServerManager.GetInstance().LogOut();
        SceneManager.LoadScene("1. Login");
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

    public void getInfoMoney() => BackendServerManager.GetInstance().GetMyMoney();

    public void BuyWithGold(int num) => BackendServerManager.GetInstance().BuyItems(num, true);

    public void BuyWithDiamond(int num) => BackendServerManager.GetInstance().BuyItems(num, false);

    public void getRankInfo() => BackendServerManager.GetInstance().getRank();

    public void QuitGame() => Application.Quit();

    public void ShowRankUI() => rankPanel.SetActive(true);

    public void DailyUser()
    {
        switch (System.DateTime.Now.DayOfWeek)
        {
            case System.DayOfWeek.Monday:
                currentDay = 0;
                break;
            case System.DayOfWeek.Tuesday:
                currentDay = 1;
                break;
            case System.DayOfWeek.Wednesday:
                currentDay = 2;
                break;
            case System.DayOfWeek.Thursday:
                currentDay = 3;
                break;
            case System.DayOfWeek.Friday:
                currentDay = 4;
                break;
            case System.DayOfWeek.Saturday:
                currentDay = 5;
                break;
            case System.DayOfWeek.Sunday:
                currentDay = 6;
                break;
        }
        BackendServerManager.GetInstance().setAdviceReset(currentDay);
        if (currentDay != BackendServerManager.GetInstance().getAdviceReset())
            BackendServerManager.GetInstance().setAdviceCount(5);
    }

    public void setCardInfo(int cardNum, int count, int reward)
    {
        int star = 0;
        if (cardNum > 95) star = 4;
        else if (cardNum > 74) star = 3;
        else if (cardNum > 54) star = 2;
        else if (cardNum > 29) star = 1;
        else star = 0;         

        if (count == 0)
        {
            Cards[cardNum].GetComponentsInChildren<Image>()[1].sprite = CardImage[0];
            Cards[cardNum].GetComponentsInChildren<Image>()[2].enabled = false;
            Cards[cardNum].GetComponentInChildren<Text>().enabled = false;
        }
        else
        {
            Cards[cardNum].GetComponentsInChildren<Image>()[1].sprite = CardImage[cardNum + 1];
            Cards[cardNum].GetComponentsInChildren<Image>()[2].enabled = true;
            Cards[cardNum].GetComponentsInChildren<Image>()[2].sprite = cardOutLine[star];
            Cards[cardNum].GetComponentInChildren<Text>().enabled = true;
        }

        Cards[cardNum].GetComponentInChildren<Text>().text = count.ToString();
        Cards[cardNum].GetComponent<Button>().onClick.AddListener(() => OpenCardInfo(cardNum, count.ToString(), reward, star));

        GameObject.Find("Scroll View").GetComponent<ScrollRect>().velocity = new Vector2(0, 0); //스크롤 하면서 다른 창으로 넘어갈 때 스크롤 저항 생김
        GameObject.Find("Content").GetComponent<RectTransform>().position = new Vector2(GameObject.Find("Content").GetComponent<RectTransform>().position.x, 0); //다른 창으로 넘길때 맨 위로 이동
    }

    public void OpenCardInfo(int cardNum, string count, int reward, int star)
    {
        if (count != "0")
        {
            cardInfo.SetActive(true);

            cardInfo.transform.GetChild(3).GetComponent<Text>().text = count;
            cardInfo.transform.GetChild(2).GetComponent<Image>().sprite = CardImage[cardNum + 1];
            cardInfo.GetComponentsInChildren<Image>()[3].sprite = cardOutLine[star];

            cardInfo.transform.GetChild(5).GetComponent<Button>().onClick.AddListener(() => DiscardCard(cardNum, int.Parse(count), reward));
            cardInfo.transform.GetChild(5).GetComponent<Button>().onClick.AddListener(() => SoundManager.Instance.PlayEffect(0));
        }
    }

    void DiscardCard(int cardNum, int cardCount, int reward)
    {
        print(reward);
        int getRubyNum = 0;

        if(cardCount >= 100)
        {
                if (reward == 0) getRubyNum = 19;
                else if (reward == 1) getRubyNum = 18;
                else if (reward == 2) getRubyNum = 15;
                else if (reward == 3) getRubyNum = 10;
                else if (reward == 4) getRubyNum = 0;
            reward = 4;
        }

        else if(cardCount >= 50 && cardCount < 100)
        {
                if (reward == 0) getRubyNum = 9;
                else if (reward == 1) getRubyNum = 8;
                else if (reward == 2) getRubyNum = 5;
                else if (reward == 3) getRubyNum = 0;
            reward = 3;
        }

        else if (cardCount >= 30 && cardCount < 50)
        {
                if (reward == 0) getRubyNum = 4;
                else if (reward == 1) getRubyNum = 1;
                else if (reward == 2) getRubyNum = 0;
            reward = 2;
        }

        else if (cardCount >= 10 && cardCount < 30)
        {
                if (reward == 0) getRubyNum = 1;
                else if (reward == 1) getRubyNum = 0;
            reward = 1;
        }


        // 이곳에 플레이어게 루비추가해주는 기능 추가
        // 플레이어 루비 += getRubyNum;
        getRubyInfo.SetActive(true);
        getRubyInfo.transform.GetChild(2).GetComponent<Text>().text = getRubyNum + " 개 획득";
        BackendServerManager.GetInstance().GetDiamond(getRubyNum);
        BackendServerManager.GetInstance().SetCard(cardNum + 1, string.Format("{0:D3}", cardCount) + "+" + reward);
        cardInfo.transform.GetChild(5).GetComponent<Button>().onClick.RemoveAllListeners();
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
