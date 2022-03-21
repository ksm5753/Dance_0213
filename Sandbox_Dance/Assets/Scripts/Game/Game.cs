using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Data;
using UnityEngine.SceneManagement;
using Spine;
using Spine.Unity;
using UnityEngine.Timeline;
using UnityEngine.Playables;

public class Game : MonoBehaviour
{
    #region float 구조체
    [System.Serializable]
    public struct TypeFloat
    {
        public string structName;
        public InspectorArray[] inspecter;
    }
    [System.Serializable]
    public struct InspectorArray
    {
        public string name;
        public float variable;
    }
    #endregion

    [System.Serializable]
    public struct StudentsSpine 
    {
        public SkeletonDataAsset sit;
        public SkeletonDataAsset cheating;
        public SkeletonDataAsset dance;
    }

    public List<TypeFloat> typeFloat;
    public List<StudentsSpine> studentSpines;

    public List<int> setStudentNum;
    // 0 : 게임시간 (0 : 최대 시간, 1 : 현재 시간(줄어드는 시간), 2 : 줄어드는 속도, 3 : 늘어나는 속도)

    [Header("줄어들 시간 관련")]
    [SerializeField] GameObject timeBar; // 크기가 바뀔 오브젝트

    [Header("플레이어 딴짓 레벨 관련")]
    [SerializeField] GameObject actBar; // 플레이어의 딴짓 레벨 경험치 바
    [SerializeField] GameObject[] students; // 학생들 게임 오브젝트
    bool isDancing = false;
    Vector3 studentPos = new Vector3(0,0,0);

    [Header("선생님 관련")]
    [SerializeField] GameObject SpineTeacher; // 선생님 스파인 오브젝트

    [SerializeField] GameObject resultWin; // 결과창
    bool isNeedWatch = false; // 선생님 뒤돌아보고있는거 체크

    [Header("아이템 관련")]
    public bool isItemFog; // 아이템이 사용중인가
    public bool isReviveOn; // 부활 아이템 구매 여부
    public bool isFogOn; // 무적아이템을 구매했는가

    [SerializeField] GameObject[] itemBtn; // 0 : 무적아이템, 1 : 부활 ...아이템 버튼들

    public GameObject storeObj;
    public GameObject danceButton;

    public Toggle[] itemBuyBtns; // 0 : 무적아이템, 1 : 시간 증가, 2 : 부활....아이템 구매 버튼들

    public GameObject[] UIOBjs; // 게임이 시작되면 꺼야할 OBJ
    public GameObject[] resultMenuBtn;
    public GameObject tutoObj;
    public GameObject DoublePrice;

    [Header("점수")]
    public Text resultText;
    public Text[] pricesText; // 0 : 골드 1 : 루비
    public Text currentScoreText;
    public Text rankAdText;
    public GameObject checkHighScore;

    public float needMoney = 0;

    public float score; // 현재 점수
    public int bestScore;
    public int finalPrice; // 최종적으로 받는 돈

    // 결과창 순서 관련
    public GameObject[] resultObjs;
    int currentResultStatus;

    // 게임 진행 관련
    [SerializeField] bool isPlaying = true; // 현재 게임이 진행중인지 {true = 진행중, false = 일시정지}

    public static Game instance;

    #region 게임 시작 부터 끝까지

    public void TeacherMover()
    {
        SoundManager.Instance.bgmSource.Stop();
        typeFloat[0].inspecter[1].variable = typeFloat[0].inspecter[0].variable;
        SpineTeacher.GetComponent<TeacherMove>().TeacherState[1].gameObject.SetActive(true);
        SpineTeacher.GetComponent<TeacherMove>().TeacherState[0].gameObject.SetActive(false);
        for (int i = 0; i < itemBuyBtns.Length; i++)
        {
            if (itemBuyBtns[i].isOn)
            {
                needMoney += typeFloat[4].inspecter[i].variable;
            }
        }
        if (int.Parse(GameUI.GetInstance().coinText.text) >= needMoney)
        {
            BuyItemBtn();
            if (!isReviveOn)
            {
                itemBtn[1].SetActive(false);
            }
            storeObj.SetActive(false);
            SpineTeacher.GetComponent<PlayableDirector>().Play();
        }

        else
        {
            UIOBjs[3].SetActive(true);
            needMoney = 0;
        }
        danceButton.SetActive(false);
        itemBtn[0].SetActive(false);
        
    }

    public void StartGame()
    {
        SpineTeacher.GetComponent<TeacherMove>().TeacherState[1].gameObject.SetActive(false);
        SpineTeacher.GetComponent<TeacherMove>().TeacherState[0].gameObject.SetActive(true);
        danceButton.SetActive(true);
        if (isFogOn)
        {
            itemBtn[0].SetActive(true);
        }
        SoundManager.Instance.PlayBGM(2);
        ResetGame();
        Tuto();
    }
    void EndGame()
    {
        Time.timeScale = 1;

        if (!itemBtn[1].activeSelf)
        {
            SetEndGameStatus();
            SoundManager.Instance.bgmSource.Pause();
            SoundManager.Instance.gameBgm_2.Pause();
            resultWin.SetActive(true);

            score = Mathf.RoundToInt(score);
            finalPrice = Mathf.RoundToInt(score * 0.1f);

            resultText.text = score.ToString() + "0";

            pricesText[0].text = finalPrice.ToString();

            SoundManager.Instance.PlayEffect(1);
        }

        else
        {
            itemBtn[1].GetComponent< PlayableDirector>().Play();
        }
    }

    public void ReviveEnd()
    {
        ResetGame();
        itemBtn[1].SetActive(false);
    }

    public void SetEndGameStatus()
    {
        if (currentResultStatus == 0)
        {
            resultObjs[0].SetActive(true);
            resultObjs[1].SetActive(false);
            currentResultStatus = 1;
        }

        else
        {
            resultObjs[0].SetActive(false);
            resultObjs[1].SetActive(true);
        }
    }

    void ResetGame()
    {
        float turnTime = Random.Range(typeFloat[2].inspecter[0].variable, typeFloat[2].inspecter[1].variable);
        Invoke("TeacherChange", turnTime);
        isPlaying = true;
        typeFloat[2].inspecter[2].variable = 0;
        SpineTeacher.GetComponent<TeacherMove>().Move(0);
        resultWin.SetActive(false);
        ChangeStudentAct(false);
        isDancing = false;
    }

    public void PauseBtn(bool pauseOrPlay)
    {
        if (pauseOrPlay)
        {
            Time.timeScale = 0;
        }
        else
        {
            Time.timeScale = 1;
        }
    }

    public void Restart()
    {
        SceneChange("3. Game");
    }

    public void ContinueGame()
    {
        typeFloat[0].inspecter[1].variable = typeFloat[0].inspecter[1].variable + 2f;

        ResetGame();
        SoundManager.Instance.PlayBGM(0);

        itemBtn[1].SetActive(false);
    }

    public void GetDoublePrice()
    {
        SeeAdForCard.instance.isEarnDouble = true;
        SeeAdForCard.instance.UserChoseToWatchAd();
    }
    #endregion

    #region 학생과 선생님 애니메이션 조정
    void MakeStudentNum()
    {
        int randomNum = Random.Range(0, studentSpines.Count);
        if (setStudentNum.Contains(randomNum))
        {
            MakeStudentNum();
        }

        else
        {
            setStudentNum.Add(randomNum);
        }
    }

    void SetStudentAnime()
    {
        for(int i= 0; i < setStudentNum.Count; i++)
        {
            students[i].GetComponentsInChildren<RectTransform>()[1].gameObject.AddComponent<SkeletonGraphic>().skeletonDataAsset = studentSpines[setStudentNum[i]].sit;
            students[i].GetComponentsInChildren<RectTransform>()[2].gameObject.AddComponent<SkeletonGraphic>().skeletonDataAsset = studentSpines[setStudentNum[i]].cheating;
            students[i].GetComponentsInChildren<RectTransform>()[3].gameObject.AddComponent<SkeletonGraphic>().skeletonDataAsset = studentSpines[setStudentNum[i]].dance;

            students[i].GetComponentsInChildren<SkeletonGraphic>()[0].AnimationState.SetAnimation(0, "sit_b", true);
            students[i].GetComponentsInChildren<SkeletonGraphic>()[1].AnimationState.SetAnimation(0, "phone", true);
            students[i].GetComponentsInChildren<SkeletonGraphic>()[2].AnimationState.SetAnimation(0, "dance", true);

            students[i].GetComponentsInChildren<SkeletonGraphic>()[2].enabled = false;
            students[i].GetComponentsInChildren<SkeletonGraphic>()[1].enabled = false;
        }
    }

    void ChangeStudentAct(bool isDance)
    {
        if (isDance)
        {
            if (typeFloat[3].inspecter[2].variable == 3)
            {
                for (int i = 0; i < students.Length; i++)
                {
                    students[i].GetComponentsInChildren<SkeletonGraphic>()[0].enabled = false;
                    students[i].GetComponentsInChildren<SkeletonGraphic>()[1].enabled = false;
                    students[i].GetComponentsInChildren<SkeletonGraphic>()[2].enabled = true;
                    students[i].transform.SetSiblingIndex(2);
                    students[i].transform.localPosition = new Vector3(0, -300, 0);
                }
            }

            else
            {
                for (int i = 0; i < students.Length; i++)
                {
                    students[i].GetComponentsInChildren<SkeletonGraphic>()[0].enabled = false;
                    students[i].GetComponentsInChildren<SkeletonGraphic>()[1].enabled = true;
                    switch (typeFloat[3].inspecter[2].variable)
                    {
                        case 0:
                            students[i].GetComponentsInChildren<SkeletonGraphic>()[1].AnimationState.SetAnimation(0, "phone", true);
                            break;
                        case 1:
                            students[i].GetComponentsInChildren<SkeletonGraphic>()[1].AnimationState.SetAnimation(0, "eat 1", true);
                            break;
                        case 2:
                            students[i].GetComponentsInChildren<SkeletonGraphic>()[1].AnimationState.SetAnimation(0, "eat 2", true);
                            break;
                    }
                    students[i].GetComponentsInChildren<SkeletonGraphic>()[2].enabled = false;
                    students[i].transform.SetSiblingIndex(2);
                    students[i].transform.localPosition = studentPos;
                }
            }
        }

        else
        {
            for (int i = 0; i < students.Length; i++)
            {
                students[i].GetComponentsInChildren<SkeletonGraphic>()[0].enabled = true;
                students[i].GetComponentsInChildren<SkeletonGraphic>()[1].enabled = false;
                students[i].GetComponentsInChildren<SkeletonGraphic>()[2].enabled = false;
                students[i].transform.SetSiblingIndex(1);
                students[i].transform.localPosition = studentPos;
            }
        }
    }

    void TeacherChange() // 선생님의 상태변경{Invoke 로 계속 실행 시켜줌} 
    {
        // 다음 바뀔 시간 체크
        float turnTime = Random.Range(typeFloat[2].inspecter[0].variable, typeFloat[2].inspecter[1].variable);
        // 선생님 상태 변환

        if (typeFloat[2].inspecter[2].variable == 0)
        {
            typeFloat[2].inspecter[2].variable += 1;
        }

        else
        {
            if (typeFloat[2].inspecter[2].variable == 5)
            {
                typeFloat[2].inspecter[2].variable = 0;
            }

            else if (typeFloat[2].inspecter[2].variable >= 2 && typeFloat[2].inspecter[2].variable <= 4)
            {
                int randomNum = Random.Range(0, 4);
                if (randomNum != 0)
                {
                    typeFloat[2].inspecter[2].variable = 5;
                    Debug.Log("Watch");
                }
                else
                {
                    typeFloat[2].inspecter[2].variable = 0;
                }
            }

            else if (typeFloat[2].inspecter[2].variable == 1)
            {
                if (turnTime >= 2.5f)
                {
                    Debug.Log("ready");
                    typeFloat[2].inspecter[2].variable += 1;
                }

                else if (turnTime >= 1.5f)
                {
                    Debug.Log("ready 1");
                    typeFloat[2].inspecter[2].variable += 2;
                }

                else
                {
                    Debug.Log("ready 2");
                    typeFloat[2].inspecter[2].variable += 3;
                }
            }
        }

        SpineTeacher.GetComponent<TeacherMove>().Move((byte)typeFloat[2].inspecter[2].variable);
        // turnTime 이후 다시 이 함수 실행
        Invoke("TeacherChange", turnTime);
    }
    #endregion

    #region 게임 매니져
    // 게임의 시간의 지남체크
    void TimeManaging()
    {
        if (isPlaying)
        {
             if(isItemFog)
            {
                typeFloat[1].inspecter[1].variable += Time.deltaTime;
                if (typeFloat[1].inspecter[1].variable > typeFloat[1].inspecter[0].variable)
                {
                    typeFloat[1].inspecter[1].variable = 0;
                    isItemFog = false;
                    isDancing = false;

                    float turnTime = Random.Range(typeFloat[2].inspecter[0].variable, typeFloat[2].inspecter[1].variable);
                    Invoke("TeacherChange", turnTime);
                    BgmManage(false);
                    ChangeStudentAct(false);
                }
            }

            timeBar.GetComponent<RectTransform>().localScale = new Vector2((typeFloat[0].inspecter[1].variable / typeFloat[0].inspecter[0].variable), 1); // 남은시간 바
            actBar.GetComponent<RectTransform>().localScale = new Vector2((typeFloat[3].inspecter[1].variable / typeFloat[3].inspecter[0].variable), 1); // 경험치 
            currentScoreText.text = Mathf.RoundToInt(score).ToString() + "0";

            if (typeFloat[3].inspecter[2].variable != 0) score += typeFloat[3].inspecter[2].variable * Time.deltaTime;
            else score += Time.deltaTime;

            TouchChecker();
        }
    }

    void TouchChecker()
    {
        if (isDancing)
        {
            // 눌렀을 때 시간이 오르는 것과 딴짓 게이지가 오름
            typeFloat[0].inspecter[1].variable += typeFloat[0].inspecter[3].variable * Time.deltaTime;
            typeFloat[3].inspecter[1].variable += typeFloat[3].inspecter[4].variable * Time.deltaTime;

            // 점수 오르기 딴짓 레벨에 따라 경험치 얻는 속도 달라짐
            if (typeFloat[3].inspecter[2].variable != 0) score += typeFloat[3].inspecter[2].variable * Time.deltaTime;
            else score += Time.deltaTime;

            // 딴짓 경험치가 필요 수치를 초과할 경우
            if (typeFloat[3].inspecter[1].variable > typeFloat[3].inspecter[0].variable)
            {
                // 아직 최대 레벨이 아닐 경우 레벨을 올려주고 경험치 수치를 0으로 바꿔줌
                if (typeFloat[3].inspecter[2].variable < typeFloat[3].inspecter[3].variable)
                {
                    typeFloat[3].inspecter[2].variable += 1;
                    typeFloat[3].inspecter[1].variable = 0; // 현재 경험치 수치 0 으로 바꿔줌

                    ChangeStudentAct(true);
                }

                // 최대 레벨에 도달했을 경우 그 이상으로 못올라가게 막아줌 (최대 레벨은 0 ~ 3 까지)
                else
                {
                    typeFloat[3].inspecter[1].variable = typeFloat[3].inspecter[0].variable;
                }
            }

            // 만약 선생님이 보고 있는 중이라면
            if (typeFloat[2].inspecter[2].variable == 5 && !isItemFog)
            {
                // 게임을 잠시 멈춰주고 선생님의 상태변환도 멈춰준다.
                isPlaying = false;
                SoundManager.Instance.Vibrate();
                CancelInvoke("TeacherChange");
                SpineTeacher.GetComponent<TeacherMove>().Move(6);
                MakeResult();
                Invoke("EndGame", 0.3f);
            }

            // 남은 시간이 최대시간을 초과할경우 그이상로 올라가지 못하도록 막아준다.
            if (typeFloat[0].inspecter[1].variable >= typeFloat[0].inspecter[0].variable)
            {
                typeFloat[0].inspecter[1].variable = typeFloat[0].inspecter[0].variable;
            }
        }

        else
        {
            // 남은시간과 경험치 수치가 점점 내려간다.
            typeFloat[0].inspecter[1].variable -= typeFloat[0].inspecter[2].variable * Time.deltaTime;
            typeFloat[3].inspecter[1].variable -= Time.deltaTime;

            if (typeFloat[3].inspecter[1].variable <= 0) // 만약 경험치 수치가 0보다 낮아지면
            {
                if (typeFloat[3].inspecter[2].variable != 0)  //  현재 레벨이 0이 아니라면
                {
                    // 레벨을 낮춰주고 경험치 수치를 최대로 만들어준다.
                    typeFloat[3].inspecter[1].variable = typeFloat[3].inspecter[0].variable;
                    typeFloat[3].inspecter[2].variable--;
                }

                // 레벨이 0이라면 그 이하로 내려가지 않도록 막아준다.
                else
                {
                    typeFloat[3].inspecter[1].variable = 0;
                }
            }

            // 남은 시간이 0보다 작아졌다면 게임오버 처리해줌
            if (typeFloat[0].inspecter[1].variable < 0 && isPlaying)
            {
                isPlaying = false;
                SoundManager.Instance.Vibrate();
                CancelInvoke("TeacherChange");
                MakeResult();
                Invoke("EndGame", 0.3f);
            }
        }
    }
    void MakeResult()
    {
        if (!itemBtn[1].activeSelf)
        {
            print((int)score * 10);
            score = Mathf.RoundToInt(score);
            finalPrice = Mathf.RoundToInt(score * 0.1f);
            BackendServerManager.GetInstance().GiveMoeny(finalPrice);
            BackendServerManager.GetInstance().UpdateScore2((int)score * 10);
        }
    }
    #endregion

    #region 아이템 관련
    public void BuyItemBtn() // 0 : 무적아이템, 1 : 시간 증가, 2 : 부활
    {
        for(int i = 0; i < itemBuyBtns.Length; i++)
        {
            if (itemBuyBtns[i].isOn == true)
            {
                ActivateItem(i);
            }
        }
    }
    public void ActivateItem(int itemNum)
    {
        switch (itemNum)
        {
            case 0:
                isFogOn = true;
                BackendServerManager.GetInstance().BuyInGameItem((int)typeFloat[4].inspecter[0].variable);
                break;
            case 1:
                typeFloat[0].inspecter[0].variable = typeFloat[0].inspecter[0].variable + 10;
                BackendServerManager.GetInstance().BuyInGameItem((int)typeFloat[4].inspecter[1].variable);

                break;
            case 2:
                isReviveOn = true;
                BackendServerManager.GetInstance().BuyInGameItem((int)typeFloat[4].inspecter[2].variable);
                break;
        }
    }
    public void ItemBtn()
    {
        if (!isItemFog && typeFloat[1].inspecter[2].variable < typeFloat[1].inspecter[3].variable)
        {
            SpineTeacher.GetComponent<TeacherMove>().Move(0);
            typeFloat[1].inspecter[2].variable++;
            typeFloat[2].inspecter[2].variable = 0;
            isItemFog = true;
            isDancing = true;
            ChangeStudentAct(true);
            BgmManage(isDancing);
            if (typeFloat[1].inspecter[3].variable <= typeFloat[1].inspecter[2].variable)
            {
                itemBtn[0].SetActive(false);
            }
            CancelInvoke("TeacherChange");
        }
    }

    #endregion

    public void Tuto()
    {
        if (!PlayerPrefs.HasKey("Tutocheck"))
        {
            Time.timeScale = 0;
            PlayerPrefs.SetInt("Tutocheck", 1);
            tutoObj.SetActive(true);
        }
    }

    public void SceneChange(string SceneName)
    {
        SceneManager.LoadScene(SceneName);
    }
    public void DanceButton(bool isDance)
    {
        if (isPlaying && !isItemFog)
        {
            BgmManage(isDance);
            if (isDance)
            {
                ChangeStudentAct(true);
                isDancing = true;
            }

            else
            {
                ChangeStudentAct(false);
                isDancing = false;
            }
        }
    }

    void BgmManage(bool isDance)
    {
        switch (isDance)
        {
            case false:
                SoundManager.Instance.bgmSource.Play();
                SoundManager.Instance.gameBgm_2.Pause();
                break;

            case true:
                SoundManager.Instance.bgmSource.Pause();
                SoundManager.Instance.gameBgm_2.Play();
                break;
        }
    }

    //해상도 초기화
    public void Initialize()
    {
        foreach (GameObject i in UIOBjs)
        {
            i.SetActive(false);
        }
        tutoObj.SetActive(false);
        rankAdText.gameObject.SetActive(false);
        studentPos = students[0].transform.localPosition;
        checkHighScore.SetActive(false);
    }
    public static Game Instance()
    {
        if (instance == null) return null;

        return instance;
    }

    void Awake()
    {
        if (instance == null) instance = this;
    }

    void Start()
    {
        for(int i = 0; i < students.Length; i++)
        {
            MakeStudentNum();
        }
        SetStudentAnime();
    }

    void Update()
    {
        TimeManaging();
        if (Input.GetKey(KeyCode.Escape))
        {
            UIOBjs[2].SetActive(true);
        }
    }
}
