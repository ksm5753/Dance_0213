using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
// Include Backend
using BackEnd;
using static BackEnd.SendQueue;
//  Include GPGS namespace
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine.SocialPlatforms;
using LitJson;
#if UNITY_IOS
using UnityEngine.SignInWithApple;
#endif

public class RankItem //Rank Class
{
    public string nickname { get; set; } // �г���
    public string score { get; set; }    // ����
    public string rank { get; set; }     // ��ũ
}

public class BackendServerManager : MonoBehaviour
{

    private static BackendServerManager instance;   // �ν��Ͻ�

    private string tempNickName;                        // ������ �г��� (id�� ����)
    public string myNickName { get; private set; } = string.Empty;  // �α����� ������ �г���
    public string myIndate { get; private set; } = string.Empty;    // �α����� ������ inDate
    private Action<bool, string> loginSuccessFunc = null;

    private const string BackendError = "statusCode : {0}\nErrorCode : {1}\nMessage : {2}";

    public string appleToken = ""; // SignInWithApple.cs���� ��ū���� ���� ���ڿ�

    public int myMoney;

    [SerializeField]string userIndate;
    string scoreIndate;

    string userInDateScore;


    public string rankUuid = "";


    //=================================================================================================
    #region ���� �ʱ�ȭ
    void Initialize()
    {
#if UNITY_ANDROID
        PlayGamesClientConfiguration config = new PlayGamesClientConfiguration
            .Builder()
            .RequestServerAuthCode(false)
            .RequestIdToken()
            .Build();
        PlayGamesPlatform.InitializeInstance(config);
        PlayGamesPlatform.DebugLogEnabled = true;

        PlayGamesPlatform.Activate();
#endif

        var bro = Backend.Initialize(true);

        if (bro.IsSuccess())
        {
#if UNITY_ANDROID //�ȵ���̵忡���� �۵�
            Debug.Log("GoogleHash - " + Backend.Utils.GetGoogleHash());
#endif
#if !UNITY_EDITOR //�ȵ���̵�, iOS ȯ�濡���� �۵�
            GetVersionInfo();
#endif
            LoginUI.GetInstance().TouchStart();
        }
        else Debug.LogError("�ڳ� �ʱ�ȭ ���� : " + bro);
    }
    #endregion

    #region ���� Ȯ�� (�����)
    private void GetVersionInfo()
    {
        Enqueue(Backend.Utils.GetLatestVersion, callback =>
        {
            if (callback.IsSuccess() == false)
            {
                Debug.LogError("���������� �ҷ����� �� �����Ͽ����ϴ�.\n" + callback);
                return;
            }

            var version = callback.GetReturnValuetoJSON()["version"].ToString();

            Version server = new Version(version);
            Version client = new Version(Application.version);

            var result = server.CompareTo(client);
            if (result == 0)
            {
                // 0 �̸� �� ������ ��ġ
                return;
            }
            else if (result < 0)
            {
                // 0 �̸��̸� server ������ client ���� ����
                // �˼��� �־��� ��� ���⿡ �ش�ȴ�.
                // ex) �˼����� 3.0.0, ���̺꿡 ���ǰ� �ִ� ���� 2.0.0, �ܼ� ���� 2.0.0
                return;
            }
            else
            {
                // 0���� ũ�� server ������ client ���� ����
                if (client == null)
                {
                    // Ŭ���̾�Ʈ�� null�� ��� ����ó��
                    Debug.LogError("Ŭ���̾�Ʈ ���������� null �Դϴ�.");
                    return;
                }
            }

            // ���� ������Ʈ �˾�
            //LoginUI.GetInstance().OpenUpdatePopup();
        });
    }
    #endregion

    #region ��ū���� �α���
    public void BackendTokenLogin(Action<bool, string> func)
    {
        Enqueue(Backend.BMember.LoginWithTheBackendToken, callback =>
        {
            if (callback.IsSuccess())
            {
                Debug.Log("��ū �α��� ����");
                loginSuccessFunc = func; //�α��� ���� ���� ������ �Է�

                OnBackendAuthorized(); //���� ���� �ҷ�����
                return;
            }
            else
            {
                Debug.Log("��ū �α��� ����\n" + callback.ToString());
                func(false, string.Empty); //���н� ��ū �ʱ�ȭ
                Backend.BMember.DeleteGuestInfo();
                LoginUI.GetInstance().TouchStart();
            }
        });
    }
    #endregion

    #region Ŀ���� �α���
    public void CustomLogin(string id, string pw, Action<bool, string> func)
    {
        Enqueue(Backend.BMember.CustomLogin, id, pw, callback =>
        {
            if (callback.IsSuccess())
            {
                Debug.Log("Ŀ���� �α��� ����");
                loginSuccessFunc = func; //�α��� ���� ������ �Է�

                OnBackendAuthorized(); //���� ���� ���� �ҷ�����
                return;
            }

            Debug.Log("Ŀ���� �α��� ����\n" + callback);
            func(false, string.Format(BackendError,
                callback.GetStatusCode(), callback.GetErrorCode(), callback.GetMessage()));
        });
    }
    #endregion

    #region Ŀ���� ȸ������
    public void CustomSignIn(string id, string pw, Action<bool, string> func)
    {
        tempNickName = id;
        Enqueue(Backend.BMember.CustomSignUp, id, pw, callback =>
        {
            if (callback.IsSuccess())
            {
                Debug.Log("Ŀ���� ȸ������ ����");
                loginSuccessFunc = func; //�α��� ���� ������ �Է�

                OnBackendAuthorized(); //���� ���� ���� �ҷ�����
                return;
            }

            Debug.LogError("Ŀ���� ȸ������ ����\n" + callback.ToString());
            func(false, string.Format(BackendError,
                callback.GetStatusCode(), callback.GetErrorCode(), callback.GetMessage()));
        });
    }
    #endregion

    #region ���� ������̼� �α��� / ȸ������
    public void GoogleAuthorizeFederation(Action<bool, string> func)
    {
#if UNITY_ANDROID //�ȵ���̵� �� ���

        if (Social.localUser.authenticated) // �̹� gpgs �α����� �� ���
        {
            var token = GetFederationToken();
            if (token.Equals(string.Empty)) //��ū�� �������� ���� ���
            {
                Debug.LogError("GPGS ��ū�� �������� �ʽ��ϴ�.");
                func(false, "GPGS ������ �����Ͽ����ϴ�.\nGPGS ��ū�� �������� �ʽ��ϴ�.");
                return;
            }

            Enqueue(Backend.BMember.AuthorizeFederation, token, FederationType.Google, "gpgs ����", callback =>
            {
                if (callback.IsSuccess())
                {
                    Debug.Log("GPGS ���� ����");
                    loginSuccessFunc = func;

                    OnBackendAuthorized(); //���� ���� ���� �ҷ�����
                    return;
                }

                string ANG = "";
                switch (callback.GetErrorCode())
                {
                    case "403":

                        ANG = "���� ���� �����Դϴ�.";
                        break;
                }
                Debug.LogError("GPGS ���� ����\n" + callback.ToString());
                func(false, string.Format(BackendError, ANG));
            });
        }

        else // gpgs �α����� �ؾ��ϴ� ���
        {
            Social.localUser.Authenticate((bool success) =>
            {
                if (success)
                {
                    var token = GetFederationToken();
                    if (token.Equals(string.Empty))
                    {
                        Debug.LogError("GPGS ��ū�� �������� �ʽ��ϴ�.");
                        func(false, "GPGS ������ �����Ͽ����ϴ�.\nGPGS ��ū�� �������� �ʽ��ϴ�.");
                        return;
                    }

                    Enqueue(Backend.BMember.AuthorizeFederation, token, FederationType.Google, "gpgs ����", callback =>
                    {
                        if (callback.IsSuccess())
                        {
                            Debug.Log("GPGS ���� ����");
                            loginSuccessFunc = func;

                            OnBackendAuthorized(); //���� ���� ���� �ҷ�����
                            return;
                        }

                        Debug.LogError("GPGS ���� ����\n" + callback.ToString());
                        func(false, string.Format(BackendError,
                            callback.GetStatusCode(), callback.GetErrorCode(), callback.GetMessage()));
                    });
                }
                else
                {
                    Debug.LogError("GPGS �α��� ����");
                    func(false, "GPGS ������ �����Ͽ����ϴ�.\nGPGS �α����� �����Ͽ����ϴ�.");
                    return;
                }
            });
        }
#endif
    }
    #endregion

    #region ���� ������̼� �α��� / ȸ������
    public void AppleAuthorizeFederation(Action<bool, string> func)
    {
#if UNITY_IOS
        loginSuccessFunc = func;
        var siwa = gameObject.GetComponent<SignInWithApple>();
        siwa.Login(AppleFedeCallback);
#endif
    }

#if UNITY_IOS
    private void AppleFedeCallback(SignInWithApple.CallbackArgs args)
    {
        Debug.Log("���� ��ū���� �ڳ��� �α���");
        Enqueue(Backend.BMember.AuthorizeFederation, appleToken, FederationType.Apple, "apple ����", callback =>
        {
            if (callback.IsSuccess())
            {
                Debug.Log("Apple ���� ����");

                OnBackendAuthorized();
                return;
            }

            Debug.LogError("Apple ���� ����\n" + callback.ToString());
            loginSuccessFunc(false, string.Format(BackendError,
                callback.GetStatusCode(), callback.GetErrorCode(), callback.GetMessage()));
        });

    }
#endif
    #endregion

    #region ���� ��ū �ޱ�
    private string GetFederationToken()
    {
#if UNITY_ANDROID
        if (!PlayGamesPlatform.Instance.localUser.authenticated)
        {
            Debug.LogError("GPGS�� ���ӵǾ����� �ʽ��ϴ�. PlayGamesPlatform.Instance.localUser.authenticated :  fail");
            return string.Empty;
        }
        // ���� ��ū �ޱ�
        string _IDtoken = PlayGamesPlatform.Instance.GetIdToken();
        tempNickName = PlayGamesPlatform.Instance.GetUserDisplayName();
        Debug.Log(tempNickName);
        return _IDtoken;

#elif UNITY_IOS
        return string.Empty;
#else
        return string.Empty;
#endif
    }
    #endregion

    #region �г��� �� �Է�
    public void UpdateNickname(string nickname, Action<bool, string> func)
    {
        Enqueue(Backend.BMember.UpdateNickname, nickname, bro =>
        {
            // �г����� ������ ��ġ���� ������ �ȵ�
            if (!bro.IsSuccess())
            {
                Debug.LogError("�г��� ���� ����\n" + bro.ToString());
                func(false, string.Format(BackendError,
                    bro.GetStatusCode(), bro.GetErrorCode(), bro.GetMessage()));
                return;
            }
            loginSuccessFunc = func;
            OnBackendAuthorized(); //���� ���� ���� �ҷ�����
        });
    }
    #endregion

    #region �Խ�Ʈ �α���
    public void GuestLogin(Action<bool, string> func)
    {
        Enqueue(Backend.BMember.GuestLogin, callback =>
        {
            if (callback.IsSuccess())
            {
                Debug.Log("�Խ�Ʈ �α��� ����");
                loginSuccessFunc = func;

                OnBackendAuthorized(); //���� ���� ���� �ҷ�����
                return;
            }

            Debug.Log("�Խ�Ʈ �α��� ����\n" + callback);

            string ANG = "";
            switch (callback.GetErrorCode())
            {
                case "403":
                    ANG = "���� ���� ���̵��Դϴ�.";
                    break;
            }
            print("ERROR" + ANG);
            func(false, string.Format(BackendError, ANG));
        });
    }
    #endregion

    #region ���� ���� ���� �ҷ�����
    private void OnBackendAuthorized()
    {
        Enqueue(Backend.BMember.GetUserInfo, callback =>
        {
            if (!callback.IsSuccess())
            {
                Debug.LogError("���� ���� �ҷ����� ����\n" + callback);
                loginSuccessFunc(false, string.Format(BackendError,
                callback.GetStatusCode(), callback.GetErrorCode(), callback.GetMessage()));
                return;
            }
            Debug.Log("��������\n" + callback);

            var info = callback.GetReturnValuetoJSON()["row"];
            

            if (info["nickname"] == null) //�г��� ���� ���� ��� �г��� ���� UI����
            {
                LoginUI.GetInstance().ActiveNickNameObject();
                return;
            }
            myNickName = info["nickname"].ToString();
            myIndate = info["inDate"].ToString();

            if (loginSuccessFunc != null) //�α��� ���������Ƿ� ��Ī ����Ʈ �� �ҷ���
            {
                LoginUI.GetInstance().SuccessLogin(loginSuccessFunc);
            }
        });
    }
    #endregion

    #region �α׾ƿ�
    public void LogOut()
    {
        Backend.BMember.Logout();
    }
    #endregion
    //=================================================================================================
    #region ����
    public int getAdviceReset()
    {
        string ad = "";
        var bro = Backend.GameData.GetMyData("User", new Where());
        if (bro.IsSuccess())
        {
            ad = bro.GetReturnValuetoJSON()["rows"][0]["ADReset"]["N"].ToString();
        }
        return int.Parse(ad);
    }

    public void setAdviceReset(int num)
    {
        Param param = new Param();
        param.Add("ADReset", num);
        Backend.GameData.UpdateV2("User", userIndate, Backend.UserInDate, param);
    }
    public int getAdviceCount()
    {
        string ad = "";
        var bro = Backend.GameData.GetMyData("User", new Where());
        if (bro.IsSuccess())
        {
            ad = bro.GetReturnValuetoJSON()["rows"][0]["AD"]["N"].ToString();
        }
        return int.Parse(ad);
    }

    public void setAdviceCount(int num)
    {
        Param param = new Param();
        param.Add("AD", 5 - num);
        Backend.GameData.UpdateV2("User", userIndate, Backend.UserInDate, param);
    }
    #endregion
    //=================================================================================================
    #region ��ũ �ý���

    #region ���� ���
    internal void InsertScore(int _score)
    {
        Param param = new Param();
        param.Add("score", _score);

        Backend.GameData.Insert("score", param, insertScoreBro =>
        {
            Debug.Log("InsertScore - " + insertScoreBro);
            userInDateScore = insertScoreBro.GetInDate();

            Enqueue(Backend.URank.User.UpdateUserScore, rankUuid, "score", userInDateScore, param, updateScoreBro =>
            {
                Debug.Log("UpdateUserScore - " + updateScoreBro);
            });
        });
    }

    private void UpdateScore(int _score)
    {

        // ������ ������ ������ ����
        Param param = new Param();
        param.Add("score", _score);

        Backend.URank.User.UpdateUserScore(rankUuid, "score", userInDateScore, param, updateScoreBro =>
        {
            Debug.Log("UpdateUserScore - " + updateScoreBro);
            if (updateScoreBro.IsSuccess())
            {
            }
            else
            {
            }
        });
    }
    public void UpdateScore2(int _score)
    {
        Enqueue(Backend.GameData.Get, "score", new Where(), myScoreBro =>
        {
            Debug.Log("�÷��̾� ���� ���� - " + myScoreBro.ToString());
            if (myScoreBro.IsSuccess())
            {
                JsonData userData = myScoreBro.GetReturnValuetoJSON()["rows"];
                // ���� ���ھ �����ϴ� ���
                if (userData.Count > 0)
                {
                    userInDateScore = userData[0]["inDate"]["S"].ToString();
                    print("�̹� ���� : " + (int.Parse(userData[0]["score"]["N"].ToString()) < _score));

                    // ���� ���ھ� update
                    if (int.Parse(userData[0]["score"]["N"].ToString()) < _score)
                    {
                        print(_score);
                        UpdateScore(_score);
                    }
                }
                // ���� ���ھ �������� �ʴ� ���
                else
                {
                    // ���� ���ھ� insert
                    InsertScore(_score);
                }
            }
            else
            {
                InsertScore(_score);
            }
        });
    }
    public void UpdateScores(int point)
    {
        var bro = Backend.GameData.GetMyData("score", new Where());
        //bro.GetReturnValuetoJSON["rows"][0]["score"]["N"]
        string myScore = "0";
        print(myRankData.score == null);
        myScore = myRankData.score == null ? myRankData.score : "0";

        if (point > int.Parse(myScore))
        {
            Param param = new Param();
            param.Add("score", point);

            Enqueue(Backend.GameData.Insert, "score", param, insertScoreBro =>
            {
                Debug.Log("InsertScore - " + insertScoreBro);
                scoreIndate = insertScoreBro.GetInDate();

                Enqueue(Backend.URank.User.UpdateUserScore, rankUuid, "score", scoreIndate, param, updateScoreBro =>
                {
                    if (updateScoreBro.IsSuccess())
                        Debug.Log("UpdateUserScore - " + updateScoreBro);
                });
            });
        }
        
    }
    #endregion


    static string dash = "-";
    [HideInInspector]
    public List<RankItem> rankTopList = new List<RankItem>();
    [HideInInspector]
    public RankItem myRankData = new RankItem();

    public RankItem EmptyRank = new RankItem
    {
        nickname = dash,
        score = dash,
        rank = dash
    };

    #region ��ŷ ���� ��������
    public void getRank()
    {
        RankItem item;
        int rankCount = 10;

        Enqueue(Backend.URank.User.GetRankList, rankUuid, rankCount, callback =>
        {
            if(callback.IsSuccess())
            {
                //�� ��ũ ��������
                getMyRank();

                //��ũ ����
                JsonData rankData = callback.GetReturnValuetoJSON()["rows"];
                rankTopList.Clear();
                for(int i = 0; i < rankData.Count; i++)
                {
                    if(rankData[i] != null)
                    {
                        item = new RankItem
                        {
                            nickname = rankData[i].Keys.Contains("nickname") ? rankData[i]["nickname"]["S"].ToString() : dash,
                            rank = rankData[i].Keys.Contains("rank") ? rankData[i]["rank"]["N"].ToString() : dash,
                            score = rankData[i].Keys.Contains("score") ? rankData[i]["score"]["N"].ToString() : dash
                        };
                        rankTopList.Add(item);
                    }
                }
            }
            else
            {
                print("getRank() - " + callback);
            }
        });
    }

    // ������ ���̸� ��ŷ �������� (UUID����)
    public void getMyRank()
    {
        Backend.URank.User.GetMyRank(rankUuid, 0, myRankBro =>
        {
            if (myRankBro.IsSuccess())
            {
                JsonData rankData = myRankBro.GetReturnValuetoJSON()["rows"];
                if (rankData[0] != null)
                {
                    
                    myRankData = new RankItem
                    {
                        nickname = rankData[0].Keys.Contains("nickname") ? rankData[0]["nickname"]["S"].ToString() : dash,
                        rank = rankData[0].Keys.Contains("rank") ? rankData[0]["rank"]["N"].ToString() : dash,
                        score = rankData[0].Keys.Contains("score") ? rankData[0]["score"]["N"].ToString() : dash
                    };
                }
                else
                {
                    myRankData = EmptyRank;
                }
            }
            else
            {
                myRankData = EmptyRank;
                Debug.Log("getMyRank() - " + myRankBro);
            }

            //Rank UI
            LobbyUI.GetInstance().ShowRankUI();
        });


    }
    #endregion

    #endregion


    //=================================================================================================
    #region ī�� �ý���

    public void SetCard(int cardNum, string cardReward)
    {
        Param param = new Param();
        param.Add("Card " + string.Format("{0:D3}", cardNum), cardReward);
        Backend.GameData.Update("Option5", new Where(), param);
    }

    #region ī�� �̱�

    public void DrawCard(bool isOne)
    {
        Enqueue(Backend.Probability.GetProbabilitys, "4044", isOne ? 1 : 11, callback =>
        {
            for (int i = 0; i < (isOne ? 1 : 11); i++)
            {
                string data = callback.GetReturnValuetoJSON()["elements"][i]["itemID"]["S"].ToString().Split('i')[1];
                print("Card " + data);
                string cardNum = "";
                string cardNum2 = "";

                int n = i;

                var bro = Backend.GameData.GetMyData("Option5", new Where());
                if (bro.IsSuccess())
                {
                    foreach (JsonData row in BackendReturnObject.Flatten(bro.Rows()))
                    {
                        string[] ANG = row["Card " + string.Format("{0:D3}", data)].ToString().Split('+');
                        print(int.Parse(ANG[0]));
                        cardNum = string.Format("{0:D3}", (int.Parse(ANG[0]) + 1));
                        cardNum2 = int.Parse(ANG[1]).ToString();
                    }
                }
                else print("A : " + callback);

                print("Card " + data + ", " + cardNum + "+" + cardNum2);
                Param param = new Param();
                param.Add("Card " + data, cardNum + "+" + cardNum2);


                Enqueue(Backend.GameData.Update, "Option5", new Where(), param, callback =>
                {
                    if (callback.IsSuccess())
                    {
                        LobbyUI.GetInstance().drawCardUI.SetActive(true);
                        if (isOne)
                        {
                            LobbyUI.GetInstance().drawCardUI.GetComponent<CardChanger>().maxNum = 1;
                            LobbyUI.GetInstance().drawCardUI.GetComponent<CardChanger>().oneOrMany[1].SetActive(false);
                            LobbyUI.GetInstance().drawCardUI.GetComponent<CardChanger>().oneOrMany[0].SetActive(true);
                            LobbyUI.GetInstance().drawCardUI.GetComponent<CardChanger>().OneCard(int.Parse(data.ToString()));
                            LobbyUI.GetInstance().drawCardUI.GetComponent<CardChanger>().SetBackEffectOnOff(false);
                        }

                        else
                        {
                            LobbyUI.GetInstance().drawCardUI.GetComponent<CardChanger>().maxNum = 11;
                            LobbyUI.GetInstance().drawCardUI.GetComponent<CardChanger>().oneOrMany[0].SetActive(false);
                            LobbyUI.GetInstance().drawCardUI.GetComponent<CardChanger>().oneOrMany[1].SetActive(true);
                            LobbyUI.GetInstance().drawCardUI.GetComponent<CardChanger>().smallCardsNum[n] = int.Parse(data);
                            LobbyUI.GetInstance().drawCardUI.GetComponent<CardChanger>().SmallCards(int.Parse(data), n);
                        }
                    }
                    else print("DrawCard() - " + callback);
                });
            }
        });
    }
    public void DrawCards(bool isOne)
    {

        Enqueue(Backend.Probability.GetProbabilitys, "4044", isOne ? 1 : 11, callback =>
        {
            for (int i = 0; i < (isOne ? 1 : 11); i++)
            {
                var data = callback.GetReturnValuetoJSON()["elements"][i]["itemID"]["S"].ToString();
                Param param = new Param();

                param.AddCalculation("Card " + data.ToString().Split('i')[1], GameInfoOperator.addition, 1);
                int n = i;
                Backend.GameData.UpdateWithCalculation("Option5", new Where(), param, callback =>
                {
                    if (callback.IsSuccess())
                    {
                        LobbyUI.GetInstance().drawCardUI.SetActive(true);
                        if (isOne)
                        {
                            LobbyUI.GetInstance().drawCardUI.GetComponent<CardChanger>().maxNum = 1;
                            LobbyUI.GetInstance().drawCardUI.GetComponent<CardChanger>().oneOrMany[1].SetActive(false);
                            LobbyUI.GetInstance().drawCardUI.GetComponent<CardChanger>().oneOrMany[0].SetActive(true);
                            LobbyUI.GetInstance().drawCardUI.GetComponent<CardChanger>().OneCard(int.Parse(data.ToString().Split('i')[1]));
                        }

                        else
                        {
                            LobbyUI.GetInstance().drawCardUI.GetComponent<CardChanger>().maxNum = 11;
                            LobbyUI.GetInstance().drawCardUI.GetComponent<CardChanger>().oneOrMany[0].SetActive(false);
                            LobbyUI.GetInstance().drawCardUI.GetComponent<CardChanger>().oneOrMany[1].SetActive(true);
                            LobbyUI.GetInstance().drawCardUI.GetComponent<CardChanger>().smallCardsNum[n] = int.Parse(data.ToString().Split('i')[1]);
                            LobbyUI.GetInstance().drawCardUI.GetComponent<CardChanger>().SmallCards(int.Parse(data.ToString().Split('i')[1]), n);
                        }
                    }
                    else print("DrawCard() - " + callback);
                });
            }
        });
    }
    #endregion

    #region ī�� ���� ��������
    public void GetUserCards(int num)
    {
        Enqueue(Backend.GameData.GetMyData, "Option" + num, new Where(), callback =>
        {
            if (callback.IsSuccess())
            {
                foreach (JsonData row in BackendReturnObject.Flatten(callback.Rows()))
                {
                    for (int i = 0; i < 100; i++)
                    {
                        string[] ANG = row["Card " + string.Format("{0:D3}", i + 1)].ToString().Split('+');

                        LobbyUI.GetInstance().setCardInfo(i, int.Parse(string.Format("{0:#,0}", ANG[0])), int.Parse(ANG[1]));
                    }
                }

            }
            else print("GetUserCards() - " + callback);
        });
    }
    #endregion

    #endregion

    //=================================================================================================

    #region ���� ���

    #region ���� ������ �ִ� ��ȭ ����
    public void GetMyMoney()
    {
        Backend.GameData.GetMyData("User", new Where(), callback =>
        {
            if (callback.IsSuccess())
            {
                var gold = callback.GetReturnValuetoJSON()["rows"][0]["Gold"]["N"];
                var diamond = callback.GetReturnValuetoJSON()["rows"][0]["Diamond"]["N"];

                if(SceneManager.GetActiveScene().name == "2. Lobby")
                {
                    if (LobbyUI.GetInstance().drawCardPanel.activeSelf)
                    {
                        LobbyUI.GetInstance().goldText.text = gold.ToString();
                        LobbyUI.GetInstance().diamondText.text = diamond.ToString();
                    }
                    else if (LobbyUI.GetInstance().storePanel.activeSelf)
                    {
                        LobbyUI.GetInstance().goldText2.text = gold.ToString();
                        LobbyUI.GetInstance().diamondText2.text = diamond.ToString();
                    }
                }

                else if (SceneManager.GetActiveScene().name == "3. Game")
                {
                    if (GameUI.GetInstance().itemBuyPanel.activeSelf)
                    {
                        GameUI.GetInstance().coinText.text = gold.ToString();
                        GameUI.GetInstance().diaText.text = diamond.ToString();
                    }
                }

            }
            else print("GetMyMoney() - " + callback);
        });
    }

    #endregion

    public void GiveMoeny(int num)
    {
        Enqueue(Backend.GameData.GetMyData, "User", new Where(),bro => 
        {
            if (bro.IsSuccess())
            {
                var money = bro.GetReturnValuetoJSON()["rows"][0]["Gold"]["N"].ToString();

                Param param = new Param();

                param.Add("Gold", int.Parse(money) + num);

                Backend.GameData.UpdateV2("User", userIndate, Backend.UserInDate, param);
            }
        });
    }

    #region ������ ����

    public void BuyInGameItem(int num)
    {
        Backend.GameData.GetMyData("User", new Where(), callback =>
        {
            if (callback.IsSuccess())
            {
                var money = callback.GetReturnValuetoJSON()["rows"][0]["Gold"]["N"].ToString();
                switch (num)
                {
                    case 100:
                        if (int.Parse(money) >= 100)
                        {
                            Param param = new Param();
                            param.Add("Gold", int.Parse(money) - 100);
                            Backend.GameData.UpdateV2("User", userIndate, Backend.UserInDate, param);
                        }
                        else
                        {
                            print("�� ����");
                        }
                        break;

                    case 200:
                        if (int.Parse(money) >= 200)
                        {
                            Param param = new Param();
                            param.Add("Gold", int.Parse(money) - 200);
                            Backend.GameData.UpdateV2("User", userIndate, Backend.UserInDate, param);
                        }
                        else
                        {
                            print("�� ����");
                        }
                        break;

                    case 300:
                        if (int.Parse(money) >= 300)
                        {
                            Param param = new Param();
                            param.Add("Gold", int.Parse(money) - 300);
                            Backend.GameData.UpdateV2("User", userIndate, Backend.UserInDate, param);
                        }
                        else
                        {
                            print("�� ����");
                        }
                        break;
                }

                GetMyMoney();
            }
        });
    }
    public void BuyItems(int num, bool isGold)
    {
        Backend.GameData.GetMyData("User", new Where(), callback =>
        {
            if (callback.IsSuccess())
            {
                var money = callback.GetReturnValuetoJSON()["rows"][0][isGold ? "Gold" : "Diamond"]["N"].ToString();

                if (int.Parse(money) >= num)
                {
                    print("�����Ͽ����ϴ�.");

                    Param param = new Param();
                    param.Add(isGold ? "Gold" : "Diamond", int.Parse(money) - num);

                    Enqueue(Backend.GameData.UpdateV2, "User", userIndate, Backend.UserInDate, param, callback =>
                    {
                        if (callback.IsSuccess())
                        {
                            print("thanks");
                            if (isGold)
                            {
                                if (num == 200) DrawCard(true);
                                else DrawCard(false);
                            }
                            else
                            {
                                if (num == 20) DrawCard(true);
                                else DrawCard(false);
                            }
                            GetMyMoney();
                        }
                    });
                }
                else
                {
                    LobbyUI.GetInstance().popUpUI.SetActive(true);
                    print("���� �����մϴ�.");
                }
            }
            else print("BuyItems() - " + callback);
        });
    }
    #endregion

    #region ���̾� ȹ��
    public void GetDiamond(int num)
    {
        var bro = Backend.GameData.GetMyData("User", new Where());
            if (bro.IsSuccess())
            {
                var money = bro.GetReturnValuetoJSON()["rows"][0]["Diamond"]["N"].ToString();

                Param param = new Param();
                param.Add("Diamond", int.Parse(money) + num);

                Backend.GameData.Update("User", new Where(), param);
            }
    }
    #endregion

    #endregion

    //=================================================================================================


    #region ���� ó�� �ʱ�ȭ
    public void InitalizeGameData()
    {
        var bro = Backend.GameData.Get("User", new Where());
        if (bro.IsSuccess())
        {
            JsonData userData = bro.GetReturnValuetoJSON()["rows"];
            if (userData.Count == 0)
            {
                Param param = new Param();

                param.Add("Gold", 0);
                param.Add("Diamond", 0);

                param.Add("AD", 0);
                param.Add("ADReset", 0);

                Enqueue(Backend.GameData.Insert, "User", param, (callback) =>
                {
                    if (callback.IsSuccess()) print("����1");
                    else print("����");
                });

                Param param2 = new Param();
                for (int i = 0; i < 100; i++)
                {
                    param2.Add("Card " + string.Format("{0:D3}", (i + 1)), "000+0");
                }

                for (int i = 1; i <= 5; i++)
                {
                    Enqueue(Backend.GameData.Insert, "Option" + i, param2, (callback) =>
                    {
                        if (callback.IsSuccess()) print("����2");
                        else print("����");
                    });
                }
                InitalizeGameData();
                return;
            }
            userIndate = userData[0]["inDate"]["S"].ToString();
        }
    }
    #endregion

    void Awake()
    {
        if (instance != null)
        {
            Destroy(instance);
        }
        instance = this;
        // ��� ������ ����
        DontDestroyOnLoad(this.gameObject);
    }

    public static BackendServerManager GetInstance()
    {
        if (instance == null)
        {
            Debug.LogError("BackendServerManager �ν��Ͻ��� �������� �ʽ��ϴ�.");
            return null;
        }

        return instance;
    }

    void Start()
    {
        Initialize(); //���� �ʱ�ȭ
    }

    void Update()
    {
        //�񵿱��Լ� Ǯ��
        Backend.AsyncPoll();
    }

}