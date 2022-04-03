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
    // 게임 시간 관련
    public float maxTime = 15; // 게임 최대 시간
    public float playTime = 15; // 현재 게임 시간
    public float minusSpeed = 1.8f; // 시간이 떨어지는 속도
    public float plusSpeed = 1.3f; // 시간이 추가되는 속도

    // 무적 아이템 관련
    public float maxItemTime = 3; // 아이템 지속시간
    public float currentTime = 0; // 현재 아이템 지속시간
    public int maxItemUseCount = 1; // 아이템 최대 사용횟수
    public int itemUseCount = 0; // 아이템 사용횟수

    // 선생님 시간 관련
    public float rotateMinTime = 0.8f; // 선생님이 상태가 변경되는 최소 시간
    public float rotateMaxTime = 1.2f; // 선생님이 상태가 변경되는 최대 시간
    public int teacherState = 0; // 선생님이 보고 있는지 체크

    // 경험치 관련
    public float maxExp = 10; // 최대 경험치
    public float currentExp = 0; // 현재 경험치
    public int currenLevel = 0; // 현재 레벨
    public int maxLevel = 3; // 최대 레벨
    public float gainSpeed = 5; // 얻는 속도

    [System.Serializable]
    public struct StudentsSpine // 학생들 스파인 관련
    {
        public SkeletonDataAsset sit;
        public SkeletonDataAsset cheating;
        public SkeletonDataAsset dance;
    }

    public List<StudentsSpine> studentSpines;

    public List<int> setStudentNum; // 학생들 수 체크 용

    [Header("줄어들 시간 관련")]
    [SerializeField] GameObject timeBar; // 크기가 바뀔 오브젝트

    [Header("플레이어 딴짓 레벨 관련")]
    [SerializeField] GameObject actBar; // 플레이어의 딴짓 레벨 경험치 바
    [SerializeField] GameObject[] students; // 학생들 게임 오브젝트
    bool isDancing = false; // 현재 플레이어가 춤을 추고 있는지
    Vector3 studentPos = new Vector3(0,0,0);

    [Header("선생님 관련")]
    [SerializeField] GameObject SpineTeacher; // 선생님 스파인 오브젝트

    [SerializeField] GameObject resultWin; // 결과창

    [Header("아이템 관련")]
    public bool isItemFog; // 아이템이 사용중인가
    public bool isReviveOn; // 부활 아이템 구매 여부
    public bool isFogOn; // 무적아이템을 구매했는가
    public float needMoney = 0; // 아이템을 구매 할떄 최종적으로 필요한 돈
    public int[] itemprice; // 아이템 가격

    [SerializeField] GameObject[] itemBtn; // 0 : 무적아이템, 1 : 부활 ...아이템 버튼들 부활은 버튼이 아니라 타임라인 애니메이션 용임

    public GameObject storeObj; // 상점 창
    public GameObject danceButton; // 춤추기 버튼

    public Toggle[] itemBuyBtns; // 0 : 무적아이템, 1 : 시간 증가, 2 : 부활....아이템 구매 버튼들

    public GameObject[] UIOBjs; // 게임이 시작되면 꺼야할 OBJ
    public GameObject[] resultMenuBtn; // 결과창 메뉴 버튼들 0 : 메인화면으로 1 : 재시작
    public GameObject tutoObj; // 튜토리얼 오브젝트
    public GameObject DoublePrice; // 두배 버튼

    [Header("점수")]
    public Text resultText; // 최종 점수 텍스트
    public Text[] pricesText; // 0 : 골드 1 : 루비
    public Text currentScoreText; // 현재 점수 텍스트
    public Text rankAdText; // 새로운 랭킹을 등록할때 나오는 텍스트
    public GameObject checkHighScore; // 새로운 랭킹을 등록할 때 나올 도장 이미지

    public float score; // 현재 점수
    public int bestScore;
    public int finalPrice; // 최종적으로 받는 돈

    // 결과창 순서 관련
    public GameObject[] resultObjs;
    int currentResultStatus;
    public PlayableDirector[] gameEndTimeLine; // 0 : 선생님 화, 1 : 타임오버

    // 게임 진행 관련
    [SerializeField] bool isPlaying = true; // 현재 게임이 진행중인지 {true = 진행중, false = 일시정지}

    public static Game instance;

    #region 게임 시작 부터 끝까지

    public void TeacherMover() // 처음 게임이 시작 될때 선생님이 교실로 들어올떄 사용
    {
        playTime = maxTime; // 게임 시작시 현재 시간을 최대 시간으로 바꿔줌

        // 선생님이 걸어가고 있는 것들 빼고 다른 스파인은 모두 꺼줌
        SpineTeacher.GetComponent<TeacherMove>().TeacherState[1].gameObject.SetActive(false); // 선생님 뒤 상태
        SpineTeacher.GetComponent<TeacherMove>().TeacherState[0].gameObject.SetActive(false); // 선생님 보고 있는 상태 
        SpineTeacher.GetComponent<TeacherMove>().TeacherState[2].gameObject.SetActive(true); // 선생님이 걷고 있는 상태
        SpineTeacher.GetComponent<TeacherMove>().TeacherState[3].gameObject.SetActive(false); // 선생님이 화난 상태

        // 아이템 구매 버튼들 상태 확인 후 필요한 돈 +
        for (int i = 0; i < itemBuyBtns.Length; i++)
        {
            if (itemBuyBtns[i].isOn)
            {
                needMoney += itemprice[i];
            }
        }

        // 만약 플레이어의 돈이 충분하다면 게임시작
        if (int.Parse(GameUI.GetInstance().coinText.text) >= needMoney)
        {
            SoundManager.Instance.bgmSource.Stop();
            BuyItemBtn();
            itemBtn[1].SetActive(false);
            storeObj.SetActive(false);
            danceButton.SetActive(false);
            itemBtn[0].SetActive(false);
            SpineTeacher.GetComponent<PlayableDirector>().Play(); // 이 타임라인에서 선생님이 목적지에 도착한 후 게임이 본격적으로 시작하는 스크립트를 실행함
        }

        // 부족하다면 경고창
        else
        {
            UIOBjs[3].SetActive(true);
            needMoney = 0;
        }      
    }

    public void StartGame() // 본격적으로 게임을 시작
    {
        // 선생님이 뒤돌아 보고 있는 상태 빼고 모두 꺼줌
        SpineTeacher.GetComponent<TeacherMove>().TeacherState[1].gameObject.SetActive(false); // 선생님 뒤 상태
        SpineTeacher.GetComponent<TeacherMove>().TeacherState[0].gameObject.SetActive(true); // 선생님 보고 있는 상태 
        SpineTeacher.GetComponent<TeacherMove>().TeacherState[2].gameObject.SetActive(false); // 선생님이 걷고 있는 상태
        SpineTeacher.GetComponent<TeacherMove>().TeacherState[3].gameObject.SetActive(false); // 선생님이 화난 상태

        // 춤추기 버튼 활성화
        danceButton.SetActive(true);

        // 만약 무적아이템을 구매했다면 아이템 버튼을 켜줌
        if (isFogOn)
        {
            itemBtn[0].SetActive(true);
        }
        
        // 게임 BGM 플레이 시작
        SoundManager.Instance.PlayBGM(2);

        // 게임 상태 리셋
        ResetGame();

        // 만약 처음 게임을 한다면 튜토리얼 창을 열어줌
        Tuto();
    }

    //  게임이 끝날때 실행, 
    public void EndGame()
    {
        Time.timeScale = 1;

        ChangeStudentAct(false); // 학생들이 춤추고 있지 않은 상태로 바꿔줌

        // 만약 부활 아이템을 구매한 상태가 아닐경우 게임을 끝내줌
        if (!isReviveOn)
        {
            SetEndGameStatus(); // 게임 결과 상태를 체크
            resultWin.SetActive(true); // 게임 결과창을 열어줌

            score = Mathf.RoundToInt(score); // 현재 점수를 체크
            finalPrice = Mathf.RoundToInt(score * 0.1f); // 얻을 골드 체크

            resultText.text = score.ToString(); //최종 점수를 텍스트에 적어줌

            pricesText[0].text = finalPrice.ToString(); // 최종적으로 얻은 돈을 텍스트에 적어줌
        }
    }

    public void EndMusic() // 게임이 끝날때 음악을 꺼줌
    {
        SoundManager.Instance.bgmSource.Pause();
        SoundManager.Instance.gameBgm_2.Pause();
        danceButton.SetActive(false);
    }

    void ReviveFunc() // 부활 체크
    {
        itemBtn[1].SetActive(true); // 부활 아이템 이펙트를 켜줌
        itemBtn[1].GetComponent<PlayableDirector>().Play(); // 부활 아이템 타임라인 켜줌
        SoundManager.Instance.bgmSource.Play(); // 게임 음악을 다시켜줌
        SoundManager.Instance.gameBgm_2.Pause(); // 게임을 잠시 멈춰줌
        isReviveOn = false; // 아이템 상태를 비활성화해줌
    }

    public void ReviveEnd() // 부활 아이템 타임라인 끝날떄 실행
    {
        ResetGame(); // 게임 상태 리셋
        itemBtn[1].SetActive(false); // 부활 아이템 이펙트 꺼줌
    }

    public void SetEndGameStatus() // 게임 종료 상태 체크용
    {
        if (currentResultStatus == 0) // 광고 버튼 활성화
        {
            resultObjs[0].SetActive(true);
            resultObjs[1].SetActive(false);
            currentResultStatus = 1;
        }

        else // 랭킹창과 나머지 버튼들 활성화
        {
            resultObjs[0].SetActive(false);
            resultObjs[1].SetActive(true);
            resultObjs[1].transform.GetChild(2).gameObject.SetActive(false); // x 버튼 비활성화
        }
    }

    void ResetGame() // 게임 상태 초기화용(첫 게임 시작시와 부활 시 사용)
    {
        // 선생님이 행동 시간을 랜덤으로 얻어서 적용해줌
        float turnTime = Random.Range(rotateMinTime, rotateMaxTime);
        Invoke("TeacherChange", turnTime);

        isPlaying = true; // 현재 게임을 하고 있는 것으로 변경
        teacherState = 0; // 선생님은 뒤돈 상태로 변경
        SpineTeacher.GetComponent<TeacherMove>().Move(0); // 선생님의 애니메이션 상태를 뒤돈 상태로 변경
        resultWin.SetActive(false); // 게임 결과창을 꺼줌
        ChangeStudentAct(false); // 학생들을 앉아있는 상태로 변경
        SpineTeacher.GetComponent<TeacherMove>().TeacherState[3].gameObject.SetActive(false); // 선생님이 화나있는 상태 꺼줌
        isDancing = false; // 현재 춤을 추고 있는것을 비활성화 상태로
        danceButton.SetActive(true); // 춤추기 버튼 활성화
    }

    public void PauseBtn(bool pauseOrPlay) // 게임 일시정지
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

    public void Restart() // 게임 재시작
    {
        SceneChange("3. Game");
    }

    public void ContinueGame() // 계속해서 플에이
    {
        currentTime = currentTime + 2f;

        ResetGame();
        SoundManager.Instance.bgmSource.Play();

        itemBtn[1].SetActive(false);
    }

    public void GetDoublePrice() // 광고로 두배 보상 얻기
    {
        SeeAdForCard.instance.isEarnDouble = true;
        SeeAdForCard.instance.UserChoseToWatchAd();
    }
    #endregion

    #region 학생과 선생님 애니메이션 조정
    void MakeStudentNum() // 학생들을 랜덤으로 생성 시켜줌
    {
        int randomNum = Random.Range(0, studentSpines.Count); // 학생들 랜덤 선택
        if (setStudentNum.Contains(randomNum)) // 만약 이미 있는 학생이라면 다시 선택
        {
            MakeStudentNum();
        }

        else // 없다면 넣어줌
        {
            setStudentNum.Add(randomNum);
        }
    }

    void SetStudentAnime() // 학생들 스파인 적용
    {
        for(int i= 0; i < setStudentNum.Count; i++) // MakeStudentNum() 에서 얻은 학생들의 스파인을 적용
        {
            students[i].GetComponentsInChildren<RectTransform>()[1].gameObject.AddComponent<SkeletonGraphic>().skeletonDataAsset = studentSpines[setStudentNum[i]].sit;
            students[i].GetComponentsInChildren<RectTransform>()[2].gameObject.AddComponent<SkeletonGraphic>().skeletonDataAsset = studentSpines[setStudentNum[i]].cheating;
            students[i].GetComponentsInChildren<RectTransform>()[3].gameObject.AddComponent<SkeletonGraphic>().skeletonDataAsset = studentSpines[setStudentNum[i]].dance;

            // 모든 스파인 애니메이션을 초기화
            students[i].GetComponentsInChildren<SkeletonGraphic>()[0].AnimationState.SetAnimation(0, "sit_b", true);
            students[i].GetComponentsInChildren<SkeletonGraphic>()[1].AnimationState.SetAnimation(0, "phone", true);
            students[i].GetComponentsInChildren<SkeletonGraphic>()[2].AnimationState.SetAnimation(0, "dance", true);

            // 앉아있는 애니메이션을 제외하고는 모두 꺼줌
            students[i].GetComponentsInChildren<SkeletonGraphic>()[2].enabled = false;
            students[i].GetComponentsInChildren<SkeletonGraphic>()[1].enabled = false;
        }
    }

    // 학생들의 애니메이션을 바꿔줌
    void ChangeStudentAct(bool isDance)
    {
        if (isDance)
        {
            if (currenLevel == 3) // 레벨이 최대치라면 춤추고 있는 애니메이션으로 변경
            {
                for (int i = 0; i < students.Length; i++)
                {
                    students[i].GetComponentsInChildren<SkeletonGraphic>()[0].enabled = false;
                    students[i].GetComponentsInChildren<SkeletonGraphic>()[1].enabled = false;
                    students[i].GetComponentsInChildren<SkeletonGraphic>()[2].enabled = true;
                    int ranNum = Random.Range(0, 2);
                    if (ranNum == 0)
                    {
                        students[i].GetComponentsInChildren<SkeletonGraphic>()[2].AnimationState.SetAnimation(0, "dance", true);
                    }

                    else
                    {
                        students[i].GetComponentsInChildren<SkeletonGraphic>()[2].AnimationState.SetAnimation(0, "dance2", true);
                    }
                    students[i].transform.SetSiblingIndex(2);
                    students[i].transform.localPosition = new Vector3(0, -300, 0);
                }
            }

            // 아니라면 레벨에 따라 다른 애니메이션 적용
            else
            {
                for (int i = 0; i < students.Length; i++)
                {
                    students[i].GetComponentsInChildren<SkeletonGraphic>()[0].enabled = false;
                    students[i].GetComponentsInChildren<SkeletonGraphic>()[1].enabled = true;
                    switch (currenLevel)
                    {
                        case 2:
                            students[i].GetComponentsInChildren<SkeletonGraphic>()[1].AnimationState.SetAnimation(0, "phone", true);
                            break;
                        case 0:
                            int ranNum1 = Random.Range(0, 2);
                            if(ranNum1 == 0)
                            {
                                students[i].GetComponentsInChildren<SkeletonGraphic>()[1].AnimationState.SetAnimation(0, "eat 1", true);
                            }

                            else
                            {
                                students[i].GetComponentsInChildren<SkeletonGraphic>()[1].AnimationState.SetAnimation(0, "eat 1-1", true);
                            }
                            break;
                        case 1:
                            int ranNum2 = Random.Range(0, 2);
                            if (ranNum2 == 0)
                            {
                                students[i].GetComponentsInChildren<SkeletonGraphic>()[1].AnimationState.SetAnimation(0, "eat 2", true);
                            }

                            else
                            {
                                if (setStudentNum[i] == 2)
                                {
                                    students[i].GetComponentsInChildren<SkeletonGraphic>()[1].AnimationState.SetAnimation(0, "eat 2-2", true);
                                }

                                else
                                {
                                    students[i].GetComponentsInChildren<SkeletonGraphic>()[1].AnimationState.SetAnimation(0, "eat 2-1", true);
                                }
                            }
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
        float turnTime = Random.Range(rotateMinTime, rotateMaxTime);

        // 선생님이 뒤돈 상태라면 보기 전 상태로 변경 시켜줌
        if (teacherState == 0)
        {
            teacherState += 1;
        }

        else
        {
            // 선생님이 보고 있는 상태라면 뒤돌아 있는 상태로 변경
            if (teacherState == 5)
            {
                teacherState = 0;
            }

            // 뒤돌기 전 상태 후에 뒤를 돌아 볼지 혹은 다시 뒤도는 상태로 변경 할 지 확률 체크
            else if (teacherState >= 2 && teacherState <= 4)
            {
                int randomNum = Random.Range(0, 4);
                if (randomNum != 0)
                {
                    teacherState = 5;
                }
                else
                {
                    teacherState = 0;
                }
            }

            // 뒤돌기 전 상태라면 시간에 따라 다른 애니메이션 적용
            else if (teacherState == 1)
            {
                if (turnTime >= 2.5f)
                {
                    teacherState += 1;
                }

                else if (turnTime >= 1.5f)
                {
                    teacherState += 2;
                }

                else
                {
                    teacherState += 3;
                }
            }
        }

        // 선생님의 스파인 상태를 변경 시켜줌
        SpineTeacher.GetComponent<TeacherMove>().Move((byte)teacherState);

        // turnTime 이후 다시 이 함수 실행
        Invoke("TeacherChange", turnTime);
    }
    #endregion

    #region 게임 매니져
    void TimeManaging() // 게임의 시간의 지남체크
    {
        if (isPlaying) // 플레이하고 있는 상태일 경우
        {
            if (isItemFog) // 만약 무적 상태가 켜져 있을 경우
            {
                currentTime += Time.deltaTime; // 아이템 사용시간을 올려줌
                if (currentTime > maxItemTime) // 아이템 사용시간이 최대시간을 초과할 경우 아이템 상태를 해제 해줌
                {
                    currentTime = 0;
                    isItemFog = false;
                    isDancing = false;

                    float turnTime = Random.Range(rotateMinTime, rotateMaxTime);
                    Invoke("TeacherChange", turnTime); // 선생님 다시 뒤돌 준비
                    BgmManage(false); // 학생들 수업 듣고 있는 bgm 켜줌
                    ChangeStudentAct(false); // 학생들 앉아잇는 상태로 변경
                }
            }

            timeBar.GetComponent<RectTransform>().localScale = new Vector2((playTime / maxTime), 1); // 남은시간 바
            actBar.GetComponent<RectTransform>().localScale = new Vector2((currentExp / maxExp), 1); // 경험치 
            currentScoreText.text = Mathf.RoundToInt(score).ToString(); // 점수 텍스트

            if (currenLevel!= 0) score += currenLevel * Time.deltaTime; // 점수는 현재 딴짓 레벨에 얻는 속도가 달라짐
            else score += Time.deltaTime;

            TouchChecker();
        }
    }

    void TouchChecker()
    {
        if (isDancing)
        {
            // 눌렀을 때 시간이 오르는 것과 딴짓 게이지가 오름
            playTime += plusSpeed * Time.deltaTime;
            currentExp += gainSpeed * Time.deltaTime;

            // 점수 오르기 딴짓 레벨에 따라 점수 얻는 속도 달라짐
            if (currenLevel != 0) score += currenLevel * Time.deltaTime;
            else score += Time.deltaTime;

            // 딴짓 경험치가 필요 수치를 초과할 경우
            if (currentExp > maxExp)
            {
                // 아직 최대 레벨이 아닐 경우 레벨을 올려주고 경험치 수치를 0으로 바꿔줌
                if (currenLevel < maxLevel)
                {
                    currenLevel += 1;
                    currentExp = 0; // 현재 경험치 수치 0 으로 바꿔줌

                    ChangeStudentAct(true);
                }

                // 최대 레벨에 도달했을 경우 그 이상으로 못올라가게 막아줌 (최대 레벨은 0 ~ 3 까지)
                else
                {
                    currentExp = maxExp;
                }
            }

            // 만약 선생님이 보고 있는 중이라면
            if (teacherState == 5 && !isItemFog)
            {
                // 게임을 잠시 멈춰주고 선생님의 상태변환도 멈춰준다.
                isPlaying = false;
                SoundManager.Instance.Vibrate();
                CancelInvoke("TeacherChange");
                SpineTeacher.GetComponent<TeacherMove>().Move(6);
                if (!isReviveOn)
                {
                    MakeResult();
                    gameEndTimeLine[0].Play();
                }

                else
                {
                    ReviveFunc();
                }
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
            currentExp -= Time.deltaTime;

            if (currentExp <= 0) // 만약 경험치 수치가 0보다 낮아지면
            {
                if (currenLevel != 0)  //  현재 레벨이 0이 아니라면
                {
                    // 레벨을 낮춰주고 경험치 수치를 최대로 만들어준다.
                    currentExp = maxExp;
                    currenLevel --;
                }

                // 레벨이 0이라면 그 이하로 내려가지 않도록 막아준다.
                else
                {
                    currentExp = 0;
                }
            }

            // 남은 시간이 0보다 작아졌다면 게임오버 처리해줌
            if (playTime < 0 && isPlaying)
            {
                isPlaying = false;
                SoundManager.Instance.Vibrate();
                CancelInvoke("TeacherChange");
                if (!isReviveOn)
                {
                    MakeResult();
                    gameEndTimeLine[1].Play();
                }

                else
                {
                    playTime += 2;
                    ReviveFunc();
                }
            }
        }
    }

    void MakeResult() // 게임결과 만들어줌
    {
        if (!itemBtn[1].activeSelf)
        {
            print((int)score);
            score = Mathf.RoundToInt(score);
            finalPrice = Mathf.RoundToInt(score * 0.1f);
            BackendServerManager.GetInstance().GiveMoeny(finalPrice);
            BackendServerManager.GetInstance().UpdateScore2((int)score);
        }
    }

    public void DanceButton(bool isDance) // 춤추기 버튼
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

    void BgmManage(bool isDance) // 현재 배경음을 바꿔줌
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
    public void ActivateItem(int itemNum) // 아이템을 켜줌
    {
        switch (itemNum)
        {
            case 0:
                isFogOn = true;
                BackendServerManager.GetInstance().BuyInGameItem(itemprice[0]);
                break;
            case 1:
                maxTime = maxTime + 10;
                BackendServerManager.GetInstance().BuyInGameItem(itemprice[1]);

                break;
            case 2:
                isReviveOn = true;
                BackendServerManager.GetInstance().BuyInGameItem(itemprice[2]);
                break;
        }
    }
    public void ItemBtn() // 무적 아이템 버튼
    {
        if (!isItemFog && itemUseCount < maxItemUseCount)
        {
            SpineTeacher.GetComponent<TeacherMove>().Move(0); // 선생님의 상태를 뒤돈 상태로
            itemUseCount++; // 아이템 사용 횟수 +
            teacherState = 0; // 선생님의 상태를 뒤돈 상태로
            isItemFog = true; // 무적 상태 on
            isDancing = true; // 춤추고 있는 상태로 변경
            ChangeStudentAct(true); // 학생들 스파인을 딴짓으로 변경
            BgmManage(isDancing); // bgm  을 딴짓할때의 bgm 으로 변경
            if (maxItemUseCount <= itemUseCount) // 만약 아이템 사용횟수가 최대라면 아이템을 꺼줌
            {
                itemBtn[0].SetActive(false);
            }
            CancelInvoke("TeacherChange");
        }
    }

    #endregion

    public void getRankInfo() => BackendServerManager.GetInstance().getRank();

    public void Tuto() // 만약 게임을 처음 시작할 경우 튜토리얼을 게임 시간을 멈추고 띄어줌
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
        // 맨처음에 학생들 선택
        for(int i = 0; i < students.Length; i++)
        {
            MakeStudentNum();
        }

        // 학생들 애니메이션 세팅
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
