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
    [Header("줄어들 시간 관련")]
    [SerializeField] float maxTime; // 최대 시간
    [SerializeField] float playTime; // 점점 줄어들 시간
    [SerializeField] GameObject timeBar; // 크기가 바뀔 오브젝트
    [SerializeField] float minusSpeed; // 시간이 떨어질 스피드
    [SerializeField] float plusSpeed; // 시간이 더해질 스피드

    [Header("플레이어 딴짓 레벨 관련")]
    [SerializeField] float needExp; // 레벨 업에 필요한 경험치양
    [SerializeField] float gainExpSpeed; // 경험치를 얻는 속도
    [SerializeField] float actProcess; // 플레이어의 현재 레벨 경험치 상태
    public int actLevel; // 현재 플레이어의 행동 레벨
    public int maxActLevel; // 최대 레벨
    [SerializeField] GameObject actBar; // 플레이어의 딴짓 레벨 경험치 바
    [SerializeField] GameObject []students; // 학생들 게임 오브젝트
    [SerializeField] Sprite[] studentSprite; // 학생들 사진들 (수정후 없어질것) 지
    bool isDancing = false;

    [Header("선생님 관련")]
    [SerializeField] float[] rotateTimeRange; // 선생님의 상태가 바뀌는 주기 범위{ 0 : 최소시간, 1 : 최대 시간}
    [SerializeField] byte isWatching = 0; // 선생님이 쳐다보는지 상태체크 {2 : 보는중, 1 : 뒤돌기 전, 0 : 뒤돌아있음}
    [SerializeField] GameObject SpineTeacher; // 선생님 스파인 오브젝트

    [Header("점수")]
    [SerializeField] Text resultText;
    [SerializeField] Text currentScoreText;
    public float score; // 현재 점수
    [SerializeField] GameObject resultWin; // 결과창

    [Header("아이템 관련")]
    public bool isItemFog; // 아이템이 사용중인가
    [SerializeField] float maxItemTime; // 아이템 지속 시간
    [SerializeField] float nowItemTime; // 아이템 현재 시간
    [SerializeField] byte useCount; // 사용횟수
    [SerializeField] byte maxUseCount; // 최대 사용 횟수 후에 지워질수 있음

    [SerializeField] int[] itemPrices; // 0 : 무적아이템, 1 : 시간 증가, 2 : 부활
    [SerializeField] GameObject [] itemBtn; // 0 : 무적아이템, 1 : 부활 ...아이템 버튼들

    [SerializeField] GameObject[] itemBuyBtns; // 0 : 무적아이템, 1 : 시간 증가, 2 : 부활....아이템 구매 버튼들


    // 게임 진행 관련
    [SerializeField] bool isPlaying = true; // 현재 게임이 진행중인지 {true = 진행중, false = 일시정지}

    private static Game instance = null;

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

    private void Start()
    {
        if(instance == null)
        {
            instance = this.GetComponent<Game>();
        }
    }

    void Update()
    {
        TimeManaging();
    }

    // 게임 전반적인 매니징
    void TimeManaging()
    {
        if (isPlaying)
        {
            if (!isItemFog)
            {
                if (Input.GetKeyDown(KeyCode.Mouse0))
                {
                    foreach (GameObject studenObj in students)
                    {
                        studenObj.GetComponent<Image>().sprite = studentSprite[actLevel];
                    }
                    isDancing = true;
                }

                if (Input.GetKeyUp(KeyCode.Mouse0))
                {
                    foreach (GameObject studenObj in students)
                    {
                        studenObj.GetComponent<Image>().sprite = studentSprite[4];
                    }
                    isDancing = false;
                }
            }

            else
            {
                nowItemTime += Time.deltaTime;
                if (nowItemTime > maxItemTime)
                {
                    nowItemTime = 0;
                    isItemFog = false;
                    isDancing = false;

                    float turnTime = Random.Range(rotateTimeRange[0], rotateTimeRange[1]);
                    Invoke("TeacherChange", turnTime);

                    foreach (GameObject studenObj in students)
                    {
                        //studenObj.GetComponent<StudentPos>().posChecker = false;
                        studenObj.GetComponent<Image>().sprite = studentSprite[4];
                        //studenObj.GetComponent<StudentPos>().changeCurrentPos();
                    }
                }
            }

            // 바의 크기 변경
            timeBar.GetComponent<RectTransform>().localScale = new Vector2((playTime / maxTime), 1); // 남은시간 바
            actBar.GetComponent<RectTransform>().localScale = new Vector2((actProcess / needExp), 1); // 경험치 바
            currentScoreText.text = Mathf.RoundToInt(score) + "0";

            score += Time.deltaTime;

            TouchChecker();
        }
    }

    void TouchChecker()
    {
        if (isDancing)
        {
            // 눌렀을 때 시간이 오르는 것과 딴짓 게이지가 오름
            playTime += plusSpeed * Time.deltaTime;
            actProcess +=  gainExpSpeed * Time.deltaTime;

            // 점수 오르기 딴짓 레벨에 따라 경험치 얻는 속도 달라짐
            if (actLevel != 0) score += actLevel * Time.deltaTime;
            else score += Time.deltaTime;

            // 딴짓 경험치가 필요 수치를 초과할 경우
            if (actProcess > needExp)
            {
                // 아직 최대 레벨이 아닐 경우 레벨을 올려주고 경험치 수치를 0으로 바꿔줌
                if (actLevel < maxActLevel)
                {
                    actLevel += 1;
                    foreach (GameObject studenObj in students)
                    {
                        studenObj.GetComponent<Image>().sprite = studentSprite[actLevel];
                    }
                    actProcess = 0;
                }

                // 최대 레벨에 도달했을 경우 그 이상으로 못올라가게 막아줌
                else
                {
                    actProcess = needExp;
                }
            }

            // 만약 선생님이 보고 있는 중이라면
            if (isWatching == 2 && !isItemFog)
            {
                // 게임을 잠시 멈춰주고 선생님의 상태변환도 멈춰준다.
                isPlaying = false;
                CancelInvoke("TeacherChange");

                Invoke("EndGame", 0.6f);
            }

            // 남은 시간이 최대시간을 초과할경우 그이상로 올라가지 못하도록 막아준다.
            if (playTime >= maxTime)
            {
                playTime = maxTime;
            }
        }

        else
        {
            // 남은시간과 경험치 수치가 점점 내려간다.
            playTime -= minusSpeed * Time.deltaTime;
            actProcess -= Time.deltaTime;

            // 만약 경험치 수치가 0보다 낮아지면
            if (actProcess <= 0)
            {
                //  현재 레벨이 0이 아니라면
                if (actLevel != 0)
                {
                    // 레벨을 낮춰주고 경험치 수치를 최대로 만들어준다.
                    actProcess = needExp;
                    actLevel--;
                }

                // 레벨이 0이라면 그 이하로 내려가지 않도록 막아준다.
                else
                {
                    actProcess = 0;
                }
            }

            // 남은 시간이 0보다 작아졌다면 게임오버 처리해줌
            if (playTime < 0 && isPlaying)
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
        float turnTime = Random.Range(rotateTimeRange[0], rotateTimeRange[1]);

        isWatching += 1;
        if(isWatching > 2)
        {
            isWatching = 0;
        }

        if(isWatching == 2)
        {
            turnTime = turnTime / 3;
        }

        SpineTeacher.GetComponent<SoldierT>().Move(isWatching);


        // turnTime 이후 다시 이 함수 실행
        Invoke("TeacherChange", turnTime);
    }

    // 게임 오버 결과 처리
    void EndGame()
    {
        resultWin.SetActive(true);
        score = Mathf.RoundToInt(score);
        resultText.text = score.ToString() + "0" + " 점!!";
        isPlaying = false;
        CancelInvoke("TeacherChange");
    }

    public void ContinueGame(GameObject reviveBtn)
    {
        float turnTime = Random.Range(rotateTimeRange[0], rotateTimeRange[1]);
        Invoke("TeacherChange", turnTime);
        playTime = playTime + 2f;
        isPlaying = true;
        isWatching = 0;
        SpineTeacher.GetComponent<SoldierT>().Move(0);
        resultWin.SetActive(false);
        reviveBtn.SetActive(false);
    }

    public void ItemBtn()
    {
        if(!isItemFog && useCount < maxUseCount)
        {
            useCount++;
            isWatching = 0;
            SpineTeacher.GetComponent<SoldierT>().Move(0);
            isItemFog = true;
            isDancing = true;
            foreach (GameObject studenObj in students)
            {
                studenObj.GetComponent<Image>().sprite = studentSprite[actLevel];
                //studenObj.GetComponent<StudentPos>().changeCurrentPos();
            }

            if(maxUseCount <= useCount)
            {
                itemBtn[0].SetActive(false);
            }
            CancelInvoke("TeacherChange");
        }
    }

    public void HomeBtn()
    {
        SceneManager.LoadScene("2. Lobby");
    }

    public void BuyItemBtn(int itemNum) // 0 : 무적아이템, 1 : 시간 증가, 2 : 부활
    {
        int playermoney = 500;
        if(playermoney >= itemPrices[itemNum])
        {
            // 이곳에 플레이어 돈을 없애주는 스크립트 작성
            switch (itemNum)
            {
                case 0:
                    itemBtn[0].SetActive(true);
                    break;

                case 1:
                    maxTime += 10;
                    break;

                case 2:
                    itemBtn[1].SetActive(true);
                    break;
            }

            itemBuyBtns[itemNum].SetActive(false);
            playermoney -= itemPrices[itemNum];
        }
    }
}
