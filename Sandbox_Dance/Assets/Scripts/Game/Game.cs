using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Data;
using UnityEngine.SceneManagement;

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

    #region 학생들 스프라이트 변경
    [System.Serializable]
    public struct StudentImageSet
    {
        public string studentName;
        public StudentImageList[] image;
    }

    [System.Serializable]
    public struct StudentImageList
    {
        public string imagePosName;
        public Sprite studentPosImage;
    }
    #endregion

    public List<TypeFloat> typeFloat;
    public List<StudentImageSet> studentImages;

    public List<int> setStudentNum;
    // 0 : 게임시간 (0 : 최대 시간, 1 : 현재 시간(줄어드는 시간), 2 : 줄어드는 속도, 3 : 늘어나는 속도)

    [Header("줄어들 시간 관련")]
    [SerializeField] GameObject timeBar; // 크기가 바뀔 오브젝트

    [Header("플레이어 딴짓 레벨 관련")]
    [SerializeField] GameObject actBar; // 플레이어의 딴짓 레벨 경험치 바
    [SerializeField] GameObject[] students; // 학생들 게임 오브젝트
    bool isDancing = false;

    [Header("선생님 관련")]
    [SerializeField] GameObject SpineTeacher; // 선생님 스파인 오브젝트

    [SerializeField] GameObject resultWin; // 결과창

    [Header("아이템 관련")]
    public bool isItemFog; // 아이템이 사용중인가
    public bool isReviveOn; // 부활 아이템 구매 여부

    [SerializeField] GameObject[] itemBtn; // 0 : 무적아이템, 1 : 부활 ...아이템 버튼들

    public GameObject storeObj;

    public Toggle[] itemBuyBtns; // 0 : 무적아이템, 1 : 시간 증가, 2 : 부활....아이템 구매 버튼들

    public GameObject[] UIOBjs; // 게임이 시작되면 꺼야할 OBJ
    public GameObject[] resultMenuBtn;

    [Header("점수")]
    public Text resultText;
    public Text[] pricesText; // 0 : 골드 1 : 루비
    public Text currentScoreText;

    public float needMoney = 0;

    public float score; // 현재 점수
    public int bestScore;
    public int finalPrice; // 최종적으로 받는 돈

    // 게임 진행 관련
    [SerializeField] bool isPlaying = true; // 현재 게임이 진행중인지 {true = 진행중, false = 일시정지}


    public static Game instance;

    void Update()
    {
        TimeManaging();
        if (Input.GetKey(KeyCode.Escape))
        {
            UIOBjs[2].SetActive(true);
        }
    }

    void MakeStudentNum()
    {
        int randomNum = Random.Range(0, studentImages.Count);
        if (setStudentNum.Contains(randomNum))
        {
            MakeStudentNum();
        }

        else
        {
            setStudentNum.Add(randomNum);
        }
    }

    public void DanceButton(bool isDance)
    {
        if (isPlaying)
        {
            if (isDance)
            {
                for(int i = 0; i < students.Length; i++)
                {
                    students[i].GetComponent<Image>().sprite = studentImages[setStudentNum[i]].image[(int)typeFloat[3].inspecter[2].variable].studentPosImage;
                    students[i].transform.SetSiblingIndex(2);
                }
                SoundManager.Instance.PlayBGM(1);
                isDancing = true;
            }

            else
            {
                for (int i = 0; i < students.Length; i++)
                {
                    students[i].GetComponent<Image>().sprite = studentImages[setStudentNum[i]].image[4].studentPosImage;
                    students[i].transform.SetSiblingIndex(1);
                }
                SoundManager.Instance.PlayBGM(0);
                isDancing = false;
            }
        }
    }

    // 게임 전반적인 매니징
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

                    for (int i = 0; i < students.Length; i++)
                    {
                        students[i].GetComponent<Image>().sprite = studentImages[setStudentNum[i]].image[4].studentPosImage;
                    }
                }
            }

            timeBar.GetComponent<RectTransform>().localScale = new Vector2((typeFloat[0].inspecter[1].variable / typeFloat[0].inspecter[0].variable), 1); // 남은시간 바
            actBar.GetComponent<RectTransform>().localScale = new Vector2((typeFloat[3].inspecter[1].variable / typeFloat[3].inspecter[0].variable), 1); // 경험치 바
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

                    for (int i = 0; i < students.Length; i++)
                    {
                        students[i].GetComponent<Image>().sprite = studentImages[setStudentNum[i]].image[(int)typeFloat[3].inspecter[2].variable].studentPosImage;
                    }
                }

                // 최대 레벨에 도달했을 경우 그 이상으로 못올라가게 막아줌 (최대 레벨은 0 ~ 3 까지)
                else
                {
                    typeFloat[3].inspecter[1].variable = typeFloat[3].inspecter[0].variable;
                }
            }

            // 만약 선생님이 보고 있는 중이라면
            if (typeFloat[2].inspecter[2].variable == 2 && !isItemFog)
            {
                // 게임을 잠시 멈춰주고 선생님의 상태변환도 멈춰준다.
                isPlaying = false;
                SoundManager.Instance.Vibrate();
                CancelInvoke("TeacherChange");
                SpineTeacher.GetComponent<TeacherMove>().Move(3);
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

    void TeacherChange() // 선생님의 상태변경{Invoke 로 계속 실행 시켜줌} 
    {
        // 다음 바뀔 시간 체크
        float turnTime = Random.Range(typeFloat[2].inspecter[0].variable, typeFloat[2].inspecter[1].variable);

        // 선생님 상태 변환
        typeFloat[2].inspecter[2].variable += 1;

        if (typeFloat[2].inspecter[2].variable > 2)
        {
            typeFloat[2].inspecter[2].variable = 0;
        }
        SpineTeacher.GetComponent<TeacherMove>().Move((byte)typeFloat[2].inspecter[2].variable);
        // turnTime 이후 다시 이 함수 실행
        Invoke("TeacherChange", turnTime);
    }

    // 게임 오버 결과 처리
    void EndGame()
    {
        SoundManager.Instance.bgmSource.Pause();
        resultWin.SetActive(true);

        score = Mathf.RoundToInt(score);
        finalPrice = Mathf.RoundToInt(score * 0.1f);

        resultText.text = score.ToString() +"0";

        pricesText[0].text = finalPrice.ToString();

        SoundManager.Instance.PlayEffect(1);

        if (!itemBtn[1].activeSelf)
        {
            resultMenuBtn[0].SetActive(true);
            resultMenuBtn[1].SetActive(true);
        }

        else
        {
            resultMenuBtn[0].SetActive(false);
            resultMenuBtn[1].SetActive(false);
        }
    }

    void MakeResult()
    {
        if (!itemBtn[1].activeSelf)
        {
            score = Mathf.RoundToInt(score);
            finalPrice = Mathf.RoundToInt(score * 0.1f);
            BackendServerManager.GetInstance().GiveMoeny(finalPrice);
            if(score > bestScore)
            {
                bestScore = (int)score * 10;
                BackendServerManager.GetInstance().UpdateScore2((int)score * 10);
            }
        }
    }

    public void SceneChange(string SceneName)
    {
        SceneManager.LoadScene(SceneName);
    }

    public void ContinueGame()
    {
        typeFloat[0].inspecter[1].variable = typeFloat[0].inspecter[1].variable + 2f;

        ResetGame();
        SoundManager.Instance.PlayBGM(0);

        itemBtn[1].SetActive(false);
    }

    public void StartGame()
    {
        typeFloat[0].inspecter[1].variable = typeFloat[0].inspecter[0].variable;
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
            ResetGame();
            storeObj.SetActive(false);
            if (!isReviveOn)
            {
                itemBtn[1].SetActive(false);
            }
        }

        else
        {
            UIOBjs[3].SetActive(true);
            needMoney = 0;
        }
    }

    public void ItemBtn()
    {
        if (!isItemFog && typeFloat[1].inspecter[2].variable < typeFloat[1].inspecter[3].variable)
        {
            SpineTeacher.GetComponent<TeacherMove>().Move(0);
            typeFloat[1].inspecter[2].variable ++;
            typeFloat[2].inspecter[2].variable = 0;
            isItemFog = true;
            isDancing = true;
            for (int i =0; i < students.Length; i++)
            {
                students[i].GetComponent<Image>().sprite = studentImages[setStudentNum[i]].image[(int)typeFloat[3].inspecter[2].variable].studentPosImage;
            }

            if (typeFloat[1].inspecter[3].variable <= typeFloat[1].inspecter[2].variable)
            {
                itemBtn[0].SetActive(false);
            }
            CancelInvoke("TeacherChange");
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
        for (int i = 0; i < students.Length; i++)
        {
            students[i].GetComponent<Image>().sprite = studentImages[setStudentNum[i]].image[4].studentPosImage;
            students[i].transform.SetSiblingIndex(1);
        }
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
                itemBtn[0].SetActive(true);
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
    #endregion

    public static Game Instance()
    {
        if (instance == null) return null;

        return instance;
    }

    //해상도 초기화
    public void Initialize()
    {
        foreach (GameObject i in UIOBjs)
        {
            i.SetActive(false);
        }
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
    }
}
