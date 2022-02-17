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

    // ���� �߰��Ѱ�(����ȣ)
    public GameObject CardInfo;
    public GameObject getRubyInfo;
    public GameObject popUpUI;
    public GameObject drawCardUI;
    public GameObject detailPopUpUI;
    public GameObject CloseAppUI;
    public Sprite[] cardOutLine;

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


    #region �ʱ�ȭ (ScaleCtrl -> Initialize)
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
        CloseAppUI.SetActive(false);

        CheckBuyItems();
    }
    #endregion

    void Start()
    {
        if (BackendServerManager.GetInstance() != null) setNickName(); //�κ� ������ �г��� ����

        settingPanel.GetComponentsInChildren<Toggle>()[0].isOn = PlayerPrefs.GetInt("BGM_Mute") == 1 ? true : false; //�Ҹ� ����

        BackendServerManager.GetInstance().InitalizeGameData(); //���ӵ����� �ʱ⼳��

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
            CloseAppUI.SetActive(true);
        }
    }

    #region �г��� ����
    private void setNickName()
    {
        var name = BackendServerManager.GetInstance().myNickName;
        if (name.Equals(string.Empty))
        {
            Debug.Log("�г��� �ҷ����� ����");
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

    public void QuitGame()
    {
        Application.Quit();
    }

    public void ShowRankUI()
    {
        rankPanel.SetActive(true);
    }

    public void setCardInfo(int cardNum, string count)
    {
        int star = 0;
        if (cardNum > 95) star = 4;
        else if (cardNum > 74) star = 3;
        else if (cardNum > 54) star = 2;
        else if (cardNum > 29) star = 1;
        else star = 0;         

        if (count == "0")
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
        Cards[cardNum].GetComponent<Button>().onClick.AddListener(() => OpenCardInfo(cardNum, count, star));

        GameObject.Find("Scroll View").GetComponent<ScrollRect>().velocity = new Vector2(0, 0); //��ũ�� �ϸ鼭 �ٸ� â���� �Ѿ �� ��ũ�� ���� ����
        GameObject.Find("Content").GetComponent<RectTransform>().position = new Vector2(GameObject.Find("Content").GetComponent<RectTransform>().position.x, 0); //�ٸ� â���� �ѱ涧 �� ���� �̵�
    }

    public void OpenCardInfo(int cardNum, string count, int star)
    {
        if (count != "0")
        {
            CardInfo.SetActive(true);

            CardInfo.transform.GetChild(3).GetComponent<Text>().text = count;
            CardInfo.transform.GetChild(2).GetComponent<Image>().sprite = CardImage[cardNum + 1];
            CardInfo.GetComponentsInChildren<Image>()[3].sprite = cardOutLine[star];

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


        // �̰��� �÷��̾�� ����߰����ִ� ��� �߰�
        // �÷��̾� ��� += getRubyNum;
        getRubyInfo.SetActive(true);
        getRubyInfo.transform.GetChild(2).GetComponent<Text>().text = getRubyNum + " �� ȹ��";
    }


    //ī��
    public void CollectionValue()
    {
        switch (collectionGroup.GetFirstActiveToggle().name)
        {
            case "MenuToggle_0":
                gameNameText.text = "�޽Ŀ�";
                BackendServerManager.GetInstance().GetUserCards(1);
                break;
            case "MenuToggle_1":
                gameNameText.text = "�޽Ŀ�2";
                BackendServerManager.GetInstance().GetUserCards(2);
                break;
            case "MenuToggle_2":
                gameNameText.text = "�޽Ŀ�3";
                BackendServerManager.GetInstance().GetUserCards(3);
                break;
            case "MenuToggle_3":
                gameNameText.text = "�޽Ŀ�4";
                BackendServerManager.GetInstance().GetUserCards(4);
                break;
            case "MenuToggle_4":
                gameNameText.text = "������ ����";
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
