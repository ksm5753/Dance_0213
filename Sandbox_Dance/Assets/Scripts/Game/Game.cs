using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Data;
using UnityEngine.SceneManagement;
//--------------게임 규칙---------------//
// 1. 플레이어는 화면을 터치하면 딴짓을 할 수 있음
// 2. 선생님이 뒤돌아보고 있을 때 딴짓을 할 경우 문제창이 뜸
// 3. 간단한 문제를 짧은 시간안에 풀어야함
// 4. 정답을 맞췄을 경우 게임을 계속해주고 아니면 게임을 그대로 끝내줘야함(문제를 푸는 동안엔 전체시간은 멈춰줌)
// 5. 터치를 하고 있을 경우 게이지가 조금씩 차고 게이지가 풀로 차면 다음 단계로 넘어가 애니메이션이 바뀜

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

    public List<TypeFloat> typeFloat;

    [Header("줄어들 시간 관련")]
    [SerializeField] GameObject timeBar; // 크기가 바뀔 오브젝트

    [Header("플레이어 딴짓 레벨 관련")]
    
    [SerializeField] GameObject actBar; // 플레이어의 딴짓 레벨 경험치 바
    [SerializeField] GameObject[] students; // 학생들 게임 오브젝트
    [SerializeField] Sprite[] studentSprite; // 학생들 사진들 (수정후 없어질것)
    bool isDancing = false;

    [Header("선생님 관련")]
    [SerializeField] GameObject SpineTeacher; // 선생님 스파인 오브젝트

    [SerializeField] GameObject resultWin; // 결과창

    [Header("아이템 관련")]
    public bool isItemFog; // 아이템이 사용중인가
    public bool isReviveOn; // 부활 아이템 구매 여부

    [SerializeField] GameObject[] itemBtn; // 0 : 무적아이템, 1 : 부활 ...아이템 버튼들

    public Toggle[] itemBuyBtns; // 0 : 무적아이템, 1 : 시간 증가, 2 : 부활....아이템 구매 버튼들

    [SerializeField] GameObject[] UIOBjs; // 게임이 시작되면 꺼야할 OBJ

    [Header("점수")]
    public Text resultText;
    public Text currentScoreText;

    public float score; // 현재 점수
    public int finalPrice; // 최종적으로 받는 돈

    // 게임 진행 관련
    [SerializeField] bool isPlaying = true; // 현재 게임이 진행중인지 {true = 진행중, false = 일시정지}

    private static Game instance = null;

    void Update()
    {
        TimeManaging();
    }

    public void DanceButton(bool isDance)
    {
        if (isDance)
        {
            foreach (GameObject studenObj in students)
            {
                studenObj.GetComponent<Image>().sprite = studentSprite[(int)typeFloat[3].inspecter[2].variable];
            }
            isDancing = true;
        }

        else
        {
            foreach (GameObject studenObj in students)
            {
                studenObj.GetComponent<Image>().sprite = studentSprite[4];
            }
            isDancing = false;
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

                    foreach (GameObject studenObj in students)
                    {
                        studenObj.GetComponent<Image>().sprite = studentSprite[4];
                    }
                }
            }

            timeBar.GetComponent<RectTransform>().localScale = new Vector2((typeFloat[0].inspecter[1].variable / typeFloat[0].inspecter[0].variable), 1); // 남은시간 바
            actBar.GetComponent<RectTransform>().localScale = new Vector2((typeFloat[3].inspecter[1].variable / typeFloat[3].inspecter[0].variable), 1); // 경험치 바
            currentScoreText.text = Mathf.RoundToInt(score).ToString() + "0";

            score += typeFloat[3].inspecter[2].variable * Time.deltaTime;

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
            else score += Time.deltaTime * 10;

            // 딴짓 경험치가 필요 수치를 초과할 경우
            if (typeFloat[3].inspecter[1].variable > typeFloat[3].inspecter[0].variable)
            {
                // 아직 최대 레벨이 아닐 경우 레벨을 올려주고 경험치 수치를 0으로 바꿔줌
                if (typeFloat[3].inspecter[2].variable < typeFloat[3].inspecter[3].variable)
                {
                    typeFloat[3].inspecter[2].variable += 1;
                    foreach (GameObject studenObj in students)
                    {
                        studenObj.GetComponent<Image>().sprite = studentSprite[(int)typeFloat[3].inspecter[2].variable];
                    }
                    typeFloat[3].inspecter[1].variable = 0;
                }

                // 최대 레벨에 도달했을 경우 그 이상으로 못올라가게 막아줌
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
                CancelInvoke("TeacherChange");

                Invoke("EndGame", 0.6f);
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

            // 만약 경험치 수치가 0보다 낮아지면
            if (typeFloat[3].inspecter[1].variable <= 0)
            {
                //  현재 레벨이 0이 아니라면
                if (typeFloat[3].inspecter[2].variable != 0)
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
                EndGame();
                Debug.Log("게임 오버");
            }
        }
    }

    void TeacherChange() // 선생님의 상태변경{Invoke 로 계속 실행 시켜줌} 
    {
        // 다음 바뀔 시간 체크
        float turnTime = Random.Range(typeFloat[2].inspecter[0].variable, typeFloat[2].inspecter[1].variable);
        typeFloat[2].inspecter[2].variable += 1;
        if (typeFloat[2].inspecter[2].variable > 2)
        {
            typeFloat[2].inspecter[2].variable = 0;
        }
        SpineTeacher.GetComponent<SoldierT>().Move((byte)typeFloat[2].inspecter[2].variable);
        // turnTime 이후 다시 이 함수 실행
        Invoke("TeacherChange", turnTime);
    }

    // 게임 오버 결과 처리
    void EndGame()
    {
        resultWin.SetActive(true);
        score = Mathf.RoundToInt(score);
        finalPrice = Mathf.RoundToInt(score / 10);
        resultText.text = "당신의 점수 : " + score.ToString() +"0"+ '\n' + "얻은 골드 : " + finalPrice;
        isPlaying = false;
        CancelInvoke("TeacherChange");
    }

    public void ContinueGame()
    {
        typeFloat[0].inspecter[1].variable = typeFloat[0].inspecter[1].variable + 2f;
        ResetGame();
    }

    public void StartGame()
    {
        BuyItemBtn();
        typeFloat[0].inspecter[1].variable = typeFloat[0].inspecter[0].variable;
        ResetGame();
        if (!isReviveOn)
        {
            itemBtn[1].SetActive(false);
        }
    }

    public void ItemBtn()
    {
        if (!isItemFog && typeFloat[1].inspecter[2].variable < typeFloat[1].inspecter[3].variable)
        {
            SpineTeacher.GetComponent<SoldierT>().Move(0);
            typeFloat[1].inspecter[2].variable ++;
            typeFloat[2].inspecter[2].variable = 0;
            isItemFog = true;
            isDancing = true;
            foreach (GameObject studenObj in students)
            {
                studenObj.GetComponent<Image>().sprite = studentSprite[(int)typeFloat[3].inspecter[2].variable];
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
        SpineTeacher.GetComponent<SoldierT>().Move(0);
        resultWin.SetActive(false);
    }

    public void HomeBtn()
    {
        SceneManager.LoadScene("2. Lobby");
        // 이곳에 finalPrice 만큼의 골드를 플레이어에게 지급
    }

    #region 아이템 관련
    public void BuyItemBtn() // 0 : 무적아이템, 1 : 시간 증가, 2 : 부활
    {
        for(int i = 0; i < itemBuyBtns.Length; i++)
        {
            if (itemBuyBtns[i].isOn == false)
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
                print("A");
                BackendServerManager.GetInstance().BuyInGameItem((int)typeFloat[4].inspecter[0].variable);
                break;
            case 1:
                typeFloat[0].inspecter[0].variable += 10;
                print("B");
                BackendServerManager.GetInstance().BuyInGameItem((int)typeFloat[4].inspecter[1].variable);

                break;
            case 2:
                isReviveOn = true;
                print("C");
                BackendServerManager.GetInstance().BuyInGameItem((int)typeFloat[4].inspecter[2].variable);
                break;
        }
    }
    #endregion

    public static Game Instance
    {
        get
        {
            if (null == instance)
            {
                return null;
            }
            return instance;
        }
    }
    public void Initialize()
    {
        foreach (GameObject i in UIOBjs)
        {
            i.SetActive(false);
        }
    }

    private void Start()
    {
        if (instance == null)
        {
            instance = this.GetComponent<Game>();
        }
    }
}
