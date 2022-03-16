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
    public string nickname { get; set; } // 닉네임
    public string score { get; set; }    // 점수
    public string rank { get; set; }     // 랭크
}

public class BackendServerManager : MonoBehaviour
{

    private static BackendServerManager instance;   // 인스턴스

    private string tempNickName;                        // 설정할 닉네임 (id와 동일)
    public string myNickName { get; private set; } = string.Empty;  // 로그인한 계정의 닉네임
    public string myIndate { get; private set; } = string.Empty;    // 로그인한 계정의 inDate
    private Action<bool, string> loginSuccessFunc = null;

    private const string BackendError = "statusCode : {0}\nErrorCode : {1}\nMessage : {2}";

    public string appleToken = ""; // SignInWithApple.cs에서 토큰값을 받을 문자열

    public int myMoney;

    [SerializeField]string userIndate;
    string scoreIndate;

    string userInDateScore;


    public string rankUuid = "";


    //=================================================================================================
    #region 서버 초기화
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
#if UNITY_ANDROID //안드로이드에서만 작동
            Debug.Log("GoogleHash - " + Backend.Utils.GetGoogleHash());
#endif
#if !UNITY_EDITOR //안드로이드, iOS 환경에서만 작동
            GetVersionInfo();
#endif
            LoginUI.GetInstance().TouchStart();
        }
        else Debug.LogError("뒤끝 초기화 실패 : " + bro);
    }
    #endregion

    #region 버전 확인 (모바일)
    private void GetVersionInfo()
    {
        Enqueue(Backend.Utils.GetLatestVersion, callback =>
        {
            if (callback.IsSuccess() == false)
            {
                Debug.LogError("버전정보를 불러오는 데 실패하였습니다.\n" + callback);
                return;
            }

            var version = callback.GetReturnValuetoJSON()["version"].ToString();

            Version server = new Version(version);
            Version client = new Version(Application.version);

            var result = server.CompareTo(client);
            if (result == 0)
            {
                // 0 이면 두 버전이 일치
                return;
            }
            else if (result < 0)
            {
                // 0 미만이면 server 버전이 client 이전 버전
                // 검수를 넣었을 경우 여기에 해당된다.
                // ex) 검수버전 3.0.0, 라이브에 운용되고 있는 버전 2.0.0, 콘솔 버전 2.0.0
                return;
            }
            else
            {
                // 0보다 크면 server 버전이 client 이후 버전
                if (client == null)
                {
                    // 클라이언트가 null인 경우 예외처리
                    Debug.LogError("클라이언트 버전정보가 null 입니다.");
                    return;
                }
            }

            // 버전 업데이트 팝업
            //LoginUI.GetInstance().OpenUpdatePopup();
        });
    }
    #endregion

    #region 토큰으로 로그인
    public void BackendTokenLogin(Action<bool, string> func)
    {
        Enqueue(Backend.BMember.LoginWithTheBackendToken, callback =>
        {
            if (callback.IsSuccess())
            {
                Debug.Log("토큰 로그인 성공");
                loginSuccessFunc = func; //로그인 성공 여부 데이터 입력

                OnBackendAuthorized(); //사전 유저 불러오기
                return;
            }
            else
            {
                Debug.Log("토큰 로그인 실패\n" + callback.ToString());
                func(false, string.Empty); //실패시 토큰 초기화
                Backend.BMember.DeleteGuestInfo();
                LoginUI.GetInstance().TouchStart();
            }
        });
    }
    #endregion

    #region 커스텀 로그인
    public void CustomLogin(string id, string pw, Action<bool, string> func)
    {
        Enqueue(Backend.BMember.CustomLogin, id, pw, callback =>
        {
            if (callback.IsSuccess())
            {
                Debug.Log("커스텀 로그인 성공");
                loginSuccessFunc = func; //로그인 여부 데이터 입력

                OnBackendAuthorized(); //사전 유저 정보 불러오기
                return;
            }

            Debug.Log("커스텀 로그인 실패\n" + callback);
            func(false, string.Format(BackendError,
                callback.GetStatusCode(), callback.GetErrorCode(), callback.GetMessage()));
        });
    }
    #endregion

    #region 커스텀 회원가입
    public void CustomSignIn(string id, string pw, Action<bool, string> func)
    {
        tempNickName = id;
        Enqueue(Backend.BMember.CustomSignUp, id, pw, callback =>
        {
            if (callback.IsSuccess())
            {
                Debug.Log("커스텀 회원가입 성공");
                loginSuccessFunc = func; //로그인 여부 데이터 입력

                OnBackendAuthorized(); //사전 유저 정보 불러오기
                return;
            }

            Debug.LogError("커스텀 회원가입 실패\n" + callback.ToString());
            func(false, string.Format(BackendError,
                callback.GetStatusCode(), callback.GetErrorCode(), callback.GetMessage()));
        });
    }
    #endregion

    #region 구글 페더레이션 로그인 / 회원가입
    public void GoogleAuthorizeFederation(Action<bool, string> func)
    {
#if UNITY_ANDROID //안드로이드 일 경우

        if (Social.localUser.authenticated) // 이미 gpgs 로그인이 된 경우
        {
            var token = GetFederationToken();
            if (token.Equals(string.Empty)) //토큰이 존재하지 않을 경우
            {
                Debug.LogError("GPGS 토큰이 존재하지 않습니다.");
                func(false, "GPGS 인증을 실패하였습니다.\nGPGS 토큰이 존재하지 않습니다.");
                return;
            }

            Enqueue(Backend.BMember.AuthorizeFederation, token, FederationType.Google, "gpgs 인증", callback =>
            {
                if (callback.IsSuccess())
                {
                    Debug.Log("GPGS 인증 성공");
                    loginSuccessFunc = func;

                    OnBackendAuthorized(); //사전 유저 정보 불러오기
                    return;
                }

                string ANG = "";
                switch (callback.GetErrorCode())
                {
                    case "403":

                        ANG = "차단 당한 유저입니다.";
                        break;
                }
                Debug.LogError("GPGS 인증 실패\n" + callback.ToString());
                func(false, string.Format(BackendError, ANG));
            });
        }

        else // gpgs 로그인을 해야하는 경우
        {
            Social.localUser.Authenticate((bool success) =>
            {
                if (success)
                {
                    var token = GetFederationToken();
                    if (token.Equals(string.Empty))
                    {
                        Debug.LogError("GPGS 토큰이 존재하지 않습니다.");
                        func(false, "GPGS 인증을 실패하였습니다.\nGPGS 토큰이 존재하지 않습니다.");
                        return;
                    }

                    Enqueue(Backend.BMember.AuthorizeFederation, token, FederationType.Google, "gpgs 인증", callback =>
                    {
                        if (callback.IsSuccess())
                        {
                            Debug.Log("GPGS 인증 성공");
                            loginSuccessFunc = func;

                            OnBackendAuthorized(); //사전 유저 정보 불러오기
                            return;
                        }

                        Debug.LogError("GPGS 인증 실패\n" + callback.ToString());
                        func(false, string.Format(BackendError,
                            callback.GetStatusCode(), callback.GetErrorCode(), callback.GetMessage()));
                    });
                }
                else
                {
                    Debug.LogError("GPGS 로그인 실패");
                    func(false, "GPGS 인증을 실패하였습니다.\nGPGS 로그인을 실패하였습니다.");
                    return;
                }
            });
        }
#endif
    }
    #endregion

    #region 애플 페더레이션 로그인 / 회원가입
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
        Debug.Log("애플 토큰으로 뒤끝에 로그인");
        Enqueue(Backend.BMember.AuthorizeFederation, appleToken, FederationType.Apple, "apple 인증", callback =>
        {
            if (callback.IsSuccess())
            {
                Debug.Log("Apple 인증 성공");

                OnBackendAuthorized();
                return;
            }

            Debug.LogError("Apple 인증 실패\n" + callback.ToString());
            loginSuccessFunc(false, string.Format(BackendError,
                callback.GetStatusCode(), callback.GetErrorCode(), callback.GetMessage()));
        });

    }
#endif
    #endregion

    #region 구글 토큰 받기
    private string GetFederationToken()
    {
#if UNITY_ANDROID
        if (!PlayGamesPlatform.Instance.localUser.authenticated)
        {
            Debug.LogError("GPGS에 접속되어있지 않습니다. PlayGamesPlatform.Instance.localUser.authenticated :  fail");
            return string.Empty;
        }
        // 유저 토큰 받기
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

    #region 닉네임 값 입력
    public void UpdateNickname(string nickname, Action<bool, string> func)
    {
        Enqueue(Backend.BMember.UpdateNickname, nickname, bro =>
        {
            // 닉네임이 없으면 매치서버 접속이 안됨
            if (!bro.IsSuccess())
            {
                Debug.LogError("닉네임 생성 실패\n" + bro.ToString());
                func(false, string.Format(BackendError,
                    bro.GetStatusCode(), bro.GetErrorCode(), bro.GetMessage()));
                return;
            }
            loginSuccessFunc = func;
            OnBackendAuthorized(); //유저 사전 정보 불러오기
        });
    }
    #endregion

    #region 게스트 로그인
    public void GuestLogin(Action<bool, string> func)
    {
        Enqueue(Backend.BMember.GuestLogin, callback =>
        {
            if (callback.IsSuccess())
            {
                Debug.Log("게스트 로그인 성공");
                loginSuccessFunc = func;

                OnBackendAuthorized(); //유저 사전 정보 불러오기
                return;
            }

            Debug.Log("게스트 로그인 실패\n" + callback);

            string ANG = "";
            switch (callback.GetErrorCode())
            {
                case "403":
                    ANG = "차단 당한 아이디입니다.";
                    break;
            }
            print("ERROR" + ANG);
            func(false, string.Format(BackendError, ANG));
        });
    }
    #endregion

    #region 실제 유저 정보 불러오기
    private void OnBackendAuthorized()
    {
        Enqueue(Backend.BMember.GetUserInfo, callback =>
        {
            if (!callback.IsSuccess())
            {
                Debug.LogError("유저 정보 불러오기 실패\n" + callback);
                loginSuccessFunc(false, string.Format(BackendError,
                callback.GetStatusCode(), callback.GetErrorCode(), callback.GetMessage()));
                return;
            }
            Debug.Log("유저정보\n" + callback);

            var info = callback.GetReturnValuetoJSON()["row"];
            

            if (info["nickname"] == null) //닉네임 값이 없을 경우 닉네임 적는 UI띄우기
            {
                LoginUI.GetInstance().ActiveNickNameObject();
                return;
            }
            myNickName = info["nickname"].ToString();
            myIndate = info["inDate"].ToString();

            if (loginSuccessFunc != null) //로그인 성공했으므로 매칭 리스트 값 불러옴
            {
                LoginUI.GetInstance().SuccessLogin(loginSuccessFunc);
            }
        });
    }
    #endregion

    #region 로그아웃
    public void LogOut()
    {
        Backend.BMember.Logout();
    }
    #endregion
    //=================================================================================================
    #region 광고
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
    #region 랭크 시스템

    #region 점수 등록
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

        // 서버로 삽입할 데이터 생성
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
            Debug.Log("플레이어 점수 정보 - " + myScoreBro.ToString());
            if (myScoreBro.IsSuccess())
            {
                JsonData userData = myScoreBro.GetReturnValuetoJSON()["rows"];
                // 유저 스코어가 존재하는 경우
                if (userData.Count > 0)
                {
                    userInDateScore = userData[0]["inDate"]["S"].ToString();
                    print("이미 존재 : " + (int.Parse(userData[0]["score"]["N"].ToString()) < _score));

                    // 유저 스코어 update
                    if (int.Parse(userData[0]["score"]["N"].ToString()) < _score)
                    {
                        print(_score);
                        UpdateScore(_score);
                    }
                }
                // 유저 스코어가 존재하지 않는 경우
                else
                {
                    // 유저 스코어 insert
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

    #region 랭킹 정보 가져오기
    public void getRank()
    {
        RankItem item;
        int rankCount = 10;

        Enqueue(Backend.URank.User.GetRankList, rankUuid, rankCount, callback =>
        {
            if(callback.IsSuccess())
            {
                //내 랭크 가져오기
                getMyRank();

                //랭크 저장
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

    // 접속한 게이머 랭킹 가져오기 (UUID지정)
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
    #region 카드 시스템

    public void SetCard(int cardNum, string cardReward)
    {
        Param param = new Param();
        param.Add("Card " + string.Format("{0:D3}", cardNum), cardReward);
        Backend.GameData.Update("Option5", new Where(), param);
    }

    #region 카드 뽑기

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

    #region 카드 정보 가져오기
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

    #region 상점 요소

    #region 현재 가지고 있는 재화 정보
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

    #region 아이템 구매

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
                            print("돈 없음");
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
                            print("돈 없음");
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
                            print("돈 없음");
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
                    print("구매하였습니다.");

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
                    print("돈이 부족합니다.");
                }
            }
            else print("BuyItems() - " + callback);
        });
    }
    #endregion

    #region 다이아 획득
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


    #region 게임 처음 초기화
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
                    if (callback.IsSuccess()) print("성공1");
                    else print("실패");
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
                        if (callback.IsSuccess()) print("성공2");
                        else print("실패");
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
        // 모든 씬에서 유지
        DontDestroyOnLoad(this.gameObject);
    }

    public static BackendServerManager GetInstance()
    {
        if (instance == null)
        {
            Debug.LogError("BackendServerManager 인스턴스가 존재하지 않습니다.");
            return null;
        }

        return instance;
    }

    void Start()
    {
        Initialize(); //서버 초기화
    }

    void Update()
    {
        //비동기함수 풀링
        Backend.AsyncPoll();
    }

}