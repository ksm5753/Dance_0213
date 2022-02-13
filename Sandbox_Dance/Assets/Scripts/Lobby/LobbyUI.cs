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
    }
    #endregion

    void Start()
    {
        if (BackendServerManager.GetInstance() != null) setNickName(); //�κ� ������ �г��� ����

        settingPanel.GetComponentsInChildren<Toggle>()[0].isOn = PlayerPrefs.GetInt("BGM_Mute") == 1 ? true : false; //�Ҹ� ����
                                                                                                                     
        BackendServerManager.GetInstance().InitalizeGameData(); //���ӵ����� �ʱ⼳��
        
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

        GameObject.Find("Scroll View").GetComponent<ScrollRect>().velocity = new Vector2(0, 0); //��ũ�� �ϸ鼭 �ٸ� â���� �Ѿ �� ��ũ�� ���� ����
        GameObject.Find("Content").GetComponent<RectTransform>().position = new Vector2(GameObject.Find("Content").GetComponent<RectTransform>().position.x, 0); //�ٸ� â���� �ѱ涧 �� ���� �̵�
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
