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
    // ���� �ð� ����
    public float maxTime = 15; // ���� �ִ� �ð�
    public float playTime = 15; // ���� ���� �ð�
    public float minusSpeed = 1.8f; // �ð��� �������� �ӵ�
    public float plusSpeed = 1.3f; // �ð��� �߰��Ǵ� �ӵ�

    // ���� ������ ����
    public float maxItemTime = 3; // ������ ���ӽð�
    public float currentTime = 0; // ���� ������ ���ӽð�
    public int maxItemUseCount = 1; // ������ �ִ� ���Ƚ��
    public int itemUseCount = 0; // ������ ���Ƚ��

    // ������ �ð� ����
    public float rotateMinTime = 0.8f; // �������� ���°� ����Ǵ� �ּ� �ð�
    public float rotateMaxTime = 1.2f; // �������� ���°� ����Ǵ� �ִ� �ð�
    public int teacherState = 0; // �������� ���� �ִ��� üũ

    // ����ġ ����
    public float maxExp = 10; // �ִ� ����ġ
    public float currentExp = 0; // ���� ����ġ
    public int currenLevel = 0; // ���� ����
    public int maxLevel = 3; // �ִ� ����
    public float gainSpeed = 5; // ��� �ӵ�

    [System.Serializable]
    public struct StudentsSpine // �л��� ������ ����
    {
        public SkeletonDataAsset sit;
        public SkeletonDataAsset cheating;
        public SkeletonDataAsset dance;
    }

    public List<StudentsSpine> studentSpines;

    public List<int> setStudentNum; // �л��� �� üũ ��

    [Header("�پ�� �ð� ����")]
    [SerializeField] GameObject timeBar; // ũ�Ⱑ �ٲ� ������Ʈ

    [Header("�÷��̾� ���� ���� ����")]
    [SerializeField] GameObject actBar; // �÷��̾��� ���� ���� ����ġ ��
    [SerializeField] GameObject[] students; // �л��� ���� ������Ʈ
    bool isDancing = false; // ���� �÷��̾ ���� �߰� �ִ���
    Vector3 studentPos = new Vector3(0,0,0);

    [Header("������ ����")]
    [SerializeField] GameObject SpineTeacher; // ������ ������ ������Ʈ

    [SerializeField] GameObject resultWin; // ���â

    [Header("������ ����")]
    public bool isItemFog; // �������� ������ΰ�
    public bool isReviveOn; // ��Ȱ ������ ���� ����
    public bool isFogOn; // ������������ �����ߴ°�
    public float needMoney = 0; // �������� ���� �ҋ� ���������� �ʿ��� ��
    public int[] itemprice; // ������ ����

    [SerializeField] GameObject[] itemBtn; // 0 : ����������, 1 : ��Ȱ ...������ ��ư�� ��Ȱ�� ��ư�� �ƴ϶� Ÿ�Ӷ��� �ִϸ��̼� ����

    public GameObject storeObj; // ���� â
    public GameObject danceButton; // ���߱� ��ư

    public Toggle[] itemBuyBtns; // 0 : ����������, 1 : �ð� ����, 2 : ��Ȱ....������ ���� ��ư��

    public GameObject[] UIOBjs; // ������ ���۵Ǹ� ������ OBJ
    public GameObject[] resultMenuBtn; // ���â �޴� ��ư�� 0 : ����ȭ������ 1 : �����
    public GameObject tutoObj; // Ʃ�丮�� ������Ʈ
    public GameObject DoublePrice; // �ι� ��ư

    [Header("����")]
    public Text resultText; // ���� ���� �ؽ�Ʈ
    public Text[] pricesText; // 0 : ��� 1 : ���
    public Text currentScoreText; // ���� ���� �ؽ�Ʈ
    public Text rankAdText; // ���ο� ��ŷ�� ����Ҷ� ������ �ؽ�Ʈ
    public GameObject checkHighScore; // ���ο� ��ŷ�� ����� �� ���� ���� �̹���

    public float score; // ���� ����
    public int bestScore;
    public int finalPrice; // ���������� �޴� ��

    // ���â ���� ����
    public GameObject[] resultObjs;
    int currentResultStatus;
    public PlayableDirector[] gameEndTimeLine; // 0 : ������ ȭ, 1 : Ÿ�ӿ���

    // ���� ���� ����
    [SerializeField] bool isPlaying = true; // ���� ������ ���������� {true = ������, false = �Ͻ�����}

    public static Game instance;

    #region ���� ���� ���� ������

    public void TeacherMover() // ó�� ������ ���� �ɶ� �������� ���Ƿ� ���Ë� ���
    {
        playTime = maxTime; // ���� ���۽� ���� �ð��� �ִ� �ð����� �ٲ���

        // �������� �ɾ�� �ִ� �͵� ���� �ٸ� �������� ��� ����
        SpineTeacher.GetComponent<TeacherMove>().TeacherState[1].gameObject.SetActive(false); // ������ �� ����
        SpineTeacher.GetComponent<TeacherMove>().TeacherState[0].gameObject.SetActive(false); // ������ ���� �ִ� ���� 
        SpineTeacher.GetComponent<TeacherMove>().TeacherState[2].gameObject.SetActive(true); // �������� �Ȱ� �ִ� ����
        SpineTeacher.GetComponent<TeacherMove>().TeacherState[3].gameObject.SetActive(false); // �������� ȭ�� ����

        // ������ ���� ��ư�� ���� Ȯ�� �� �ʿ��� �� +
        for (int i = 0; i < itemBuyBtns.Length; i++)
        {
            if (itemBuyBtns[i].isOn)
            {
                needMoney += itemprice[i];
            }
        }

        // ���� �÷��̾��� ���� ����ϴٸ� ���ӽ���
        if (int.Parse(GameUI.GetInstance().coinText.text) >= needMoney)
        {
            SoundManager.Instance.bgmSource.Stop();
            BuyItemBtn();
            itemBtn[1].SetActive(false);
            storeObj.SetActive(false);
            danceButton.SetActive(false);
            itemBtn[0].SetActive(false);
            SpineTeacher.GetComponent<PlayableDirector>().Play(); // �� Ÿ�Ӷ��ο��� �������� �������� ������ �� ������ ���������� �����ϴ� ��ũ��Ʈ�� ������
        }

        // �����ϴٸ� ���â
        else
        {
            UIOBjs[3].SetActive(true);
            needMoney = 0;
        }      
    }

    public void StartGame() // ���������� ������ ����
    {
        // �������� �ڵ��� ���� �ִ� ���� ���� ��� ����
        SpineTeacher.GetComponent<TeacherMove>().TeacherState[1].gameObject.SetActive(false); // ������ �� ����
        SpineTeacher.GetComponent<TeacherMove>().TeacherState[0].gameObject.SetActive(true); // ������ ���� �ִ� ���� 
        SpineTeacher.GetComponent<TeacherMove>().TeacherState[2].gameObject.SetActive(false); // �������� �Ȱ� �ִ� ����
        SpineTeacher.GetComponent<TeacherMove>().TeacherState[3].gameObject.SetActive(false); // �������� ȭ�� ����

        // ���߱� ��ư Ȱ��ȭ
        danceButton.SetActive(true);

        // ���� ������������ �����ߴٸ� ������ ��ư�� ����
        if (isFogOn)
        {
            itemBtn[0].SetActive(true);
        }
        
        // ���� BGM �÷��� ����
        SoundManager.Instance.PlayBGM(2);

        // ���� ���� ����
        ResetGame();

        // ���� ó�� ������ �Ѵٸ� Ʃ�丮�� â�� ������
        Tuto();
    }

    //  ������ ������ ����, 
    public void EndGame()
    {
        Time.timeScale = 1;

        ChangeStudentAct(false); // �л����� ���߰� ���� ���� ���·� �ٲ���

        // ���� ��Ȱ �������� ������ ���°� �ƴҰ�� ������ ������
        if (!isReviveOn)
        {
            SetEndGameStatus(); // ���� ��� ���¸� üũ
            resultWin.SetActive(true); // ���� ���â�� ������

            score = Mathf.RoundToInt(score); // ���� ������ üũ
            finalPrice = Mathf.RoundToInt(score * 0.1f); // ���� ��� üũ

            resultText.text = score.ToString(); //���� ������ �ؽ�Ʈ�� ������

            pricesText[0].text = finalPrice.ToString(); // ���������� ���� ���� �ؽ�Ʈ�� ������
        }
    }

    public void EndMusic() // ������ ������ ������ ����
    {
        SoundManager.Instance.bgmSource.Pause();
        SoundManager.Instance.gameBgm_2.Pause();
        danceButton.SetActive(false);
    }

    void ReviveFunc() // ��Ȱ üũ
    {
        itemBtn[1].SetActive(true); // ��Ȱ ������ ����Ʈ�� ����
        itemBtn[1].GetComponent<PlayableDirector>().Play(); // ��Ȱ ������ Ÿ�Ӷ��� ����
        SoundManager.Instance.bgmSource.Play(); // ���� ������ �ٽ�����
        SoundManager.Instance.gameBgm_2.Pause(); // ������ ��� ������
        isReviveOn = false; // ������ ���¸� ��Ȱ��ȭ����
    }

    public void ReviveEnd() // ��Ȱ ������ Ÿ�Ӷ��� ������ ����
    {
        ResetGame(); // ���� ���� ����
        itemBtn[1].SetActive(false); // ��Ȱ ������ ����Ʈ ����
    }

    public void SetEndGameStatus() // ���� ���� ���� üũ��
    {
        if (currentResultStatus == 0) // ���� ��ư Ȱ��ȭ
        {
            resultObjs[0].SetActive(true);
            resultObjs[1].SetActive(false);
            currentResultStatus = 1;
        }

        else // ��ŷâ�� ������ ��ư�� Ȱ��ȭ
        {
            resultObjs[0].SetActive(false);
            resultObjs[1].SetActive(true);
            resultObjs[1].transform.GetChild(2).gameObject.SetActive(false); // x ��ư ��Ȱ��ȭ
        }
    }

    void ResetGame() // ���� ���� �ʱ�ȭ��(ù ���� ���۽ÿ� ��Ȱ �� ���)
    {
        // �������� �ൿ �ð��� �������� �� ��������
        float turnTime = Random.Range(rotateMinTime, rotateMaxTime);
        Invoke("TeacherChange", turnTime);

        isPlaying = true; // ���� ������ �ϰ� �ִ� ������ ����
        teacherState = 0; // �������� �ڵ� ���·� ����
        SpineTeacher.GetComponent<TeacherMove>().Move(0); // �������� �ִϸ��̼� ���¸� �ڵ� ���·� ����
        resultWin.SetActive(false); // ���� ���â�� ����
        ChangeStudentAct(false); // �л����� �ɾ��ִ� ���·� ����
        SpineTeacher.GetComponent<TeacherMove>().TeacherState[3].gameObject.SetActive(false); // �������� ȭ���ִ� ���� ����
        isDancing = false; // ���� ���� �߰� �ִ°��� ��Ȱ��ȭ ���·�
        danceButton.SetActive(true); // ���߱� ��ư Ȱ��ȭ
    }

    public void PauseBtn(bool pauseOrPlay) // ���� �Ͻ�����
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

    public void Restart() // ���� �����
    {
        SceneChange("3. Game");
    }

    public void ContinueGame() // ����ؼ� �ÿ���
    {
        currentTime = currentTime + 2f;

        ResetGame();
        SoundManager.Instance.bgmSource.Play();

        itemBtn[1].SetActive(false);
    }

    public void GetDoublePrice() // ����� �ι� ���� ���
    {
        SeeAdForCard.instance.isEarnDouble = true;
        SeeAdForCard.instance.UserChoseToWatchAd();
    }
    #endregion

    #region �л��� ������ �ִϸ��̼� ����
    void MakeStudentNum() // �л����� �������� ���� ������
    {
        int randomNum = Random.Range(0, studentSpines.Count); // �л��� ���� ����
        if (setStudentNum.Contains(randomNum)) // ���� �̹� �ִ� �л��̶�� �ٽ� ����
        {
            MakeStudentNum();
        }

        else // ���ٸ� �־���
        {
            setStudentNum.Add(randomNum);
        }
    }

    void SetStudentAnime() // �л��� ������ ����
    {
        for(int i= 0; i < setStudentNum.Count; i++) // MakeStudentNum() ���� ���� �л����� �������� ����
        {
            students[i].GetComponentsInChildren<RectTransform>()[1].gameObject.AddComponent<SkeletonGraphic>().skeletonDataAsset = studentSpines[setStudentNum[i]].sit;
            students[i].GetComponentsInChildren<RectTransform>()[2].gameObject.AddComponent<SkeletonGraphic>().skeletonDataAsset = studentSpines[setStudentNum[i]].cheating;
            students[i].GetComponentsInChildren<RectTransform>()[3].gameObject.AddComponent<SkeletonGraphic>().skeletonDataAsset = studentSpines[setStudentNum[i]].dance;

            // ��� ������ �ִϸ��̼��� �ʱ�ȭ
            students[i].GetComponentsInChildren<SkeletonGraphic>()[0].AnimationState.SetAnimation(0, "sit_b", true);
            students[i].GetComponentsInChildren<SkeletonGraphic>()[1].AnimationState.SetAnimation(0, "phone", true);
            students[i].GetComponentsInChildren<SkeletonGraphic>()[2].AnimationState.SetAnimation(0, "dance", true);

            // �ɾ��ִ� �ִϸ��̼��� �����ϰ�� ��� ����
            students[i].GetComponentsInChildren<SkeletonGraphic>()[2].enabled = false;
            students[i].GetComponentsInChildren<SkeletonGraphic>()[1].enabled = false;
        }
    }

    // �л����� �ִϸ��̼��� �ٲ���
    void ChangeStudentAct(bool isDance)
    {
        if (isDance)
        {
            if (currenLevel == 3) // ������ �ִ�ġ��� ���߰� �ִ� �ִϸ��̼����� ����
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

            // �ƴ϶�� ������ ���� �ٸ� �ִϸ��̼� ����
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

    void TeacherChange() // �������� ���º���{Invoke �� ��� ���� ������} 
    {
        // ���� �ٲ� �ð� üũ
        float turnTime = Random.Range(rotateMinTime, rotateMaxTime);

        // �������� �ڵ� ���¶�� ���� �� ���·� ���� ������
        if (teacherState == 0)
        {
            teacherState += 1;
        }

        else
        {
            // �������� ���� �ִ� ���¶�� �ڵ��� �ִ� ���·� ����
            if (teacherState == 5)
            {
                teacherState = 0;
            }

            // �ڵ��� �� ���� �Ŀ� �ڸ� ���� ���� Ȥ�� �ٽ� �ڵ��� ���·� ���� �� �� Ȯ�� üũ
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

            // �ڵ��� �� ���¶�� �ð��� ���� �ٸ� �ִϸ��̼� ����
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

        // �������� ������ ���¸� ���� ������
        SpineTeacher.GetComponent<TeacherMove>().Move((byte)teacherState);

        // turnTime ���� �ٽ� �� �Լ� ����
        Invoke("TeacherChange", turnTime);
    }
    #endregion

    #region ���� �Ŵ���
    void TimeManaging() // ������ �ð��� ����üũ
    {
        if (isPlaying) // �÷����ϰ� �ִ� ������ ���
        {
            if (isItemFog) // ���� ���� ���°� ���� ���� ���
            {
                currentTime += Time.deltaTime; // ������ ���ð��� �÷���
                if (currentTime > maxItemTime) // ������ ���ð��� �ִ�ð��� �ʰ��� ��� ������ ���¸� ���� ����
                {
                    currentTime = 0;
                    isItemFog = false;
                    isDancing = false;

                    float turnTime = Random.Range(rotateMinTime, rotateMaxTime);
                    Invoke("TeacherChange", turnTime); // ������ �ٽ� �ڵ� �غ�
                    BgmManage(false); // �л��� ���� ��� �ִ� bgm ����
                    ChangeStudentAct(false); // �л��� �ɾ��մ� ���·� ����
                }
            }

            timeBar.GetComponent<RectTransform>().localScale = new Vector2((playTime / maxTime), 1); // �����ð� ��
            actBar.GetComponent<RectTransform>().localScale = new Vector2((currentExp / maxExp), 1); // ����ġ 
            currentScoreText.text = Mathf.RoundToInt(score).ToString(); // ���� �ؽ�Ʈ

            if (currenLevel!= 0) score += currenLevel * Time.deltaTime; // ������ ���� ���� ������ ��� �ӵ��� �޶���
            else score += Time.deltaTime;

            TouchChecker();
        }
    }

    void TouchChecker()
    {
        if (isDancing)
        {
            // ������ �� �ð��� ������ �Ͱ� ���� �������� ����
            playTime += plusSpeed * Time.deltaTime;
            currentExp += gainSpeed * Time.deltaTime;

            // ���� ������ ���� ������ ���� ���� ��� �ӵ� �޶���
            if (currenLevel != 0) score += currenLevel * Time.deltaTime;
            else score += Time.deltaTime;

            // ���� ����ġ�� �ʿ� ��ġ�� �ʰ��� ���
            if (currentExp > maxExp)
            {
                // ���� �ִ� ������ �ƴ� ��� ������ �÷��ְ� ����ġ ��ġ�� 0���� �ٲ���
                if (currenLevel < maxLevel)
                {
                    currenLevel += 1;
                    currentExp = 0; // ���� ����ġ ��ġ 0 ���� �ٲ���

                    ChangeStudentAct(true);
                }

                // �ִ� ������ �������� ��� �� �̻����� ���ö󰡰� ������ (�ִ� ������ 0 ~ 3 ����)
                else
                {
                    currentExp = maxExp;
                }
            }

            // ���� �������� ���� �ִ� ���̶��
            if (teacherState == 5 && !isItemFog)
            {
                // ������ ��� �����ְ� �������� ���º�ȯ�� �����ش�.
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

            // ���� �ð��� �ִ�ð��� �ʰ��Ұ�� ���̻�� �ö��� ���ϵ��� �����ش�.
            if (playTime >= maxTime)
            {
                playTime = maxTime;
            }
        }

        else
        {
            // �����ð��� ����ġ ��ġ�� ���� ��������.
            playTime -= minusSpeed * Time.deltaTime;
            currentExp -= Time.deltaTime;

            if (currentExp <= 0) // ���� ����ġ ��ġ�� 0���� ��������
            {
                if (currenLevel != 0)  //  ���� ������ 0�� �ƴ϶��
                {
                    // ������ �����ְ� ����ġ ��ġ�� �ִ�� ������ش�.
                    currentExp = maxExp;
                    currenLevel --;
                }

                // ������ 0�̶�� �� ���Ϸ� �������� �ʵ��� �����ش�.
                else
                {
                    currentExp = 0;
                }
            }

            // ���� �ð��� 0���� �۾����ٸ� ���ӿ��� ó������
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

    void MakeResult() // ���Ӱ�� �������
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

    public void DanceButton(bool isDance) // ���߱� ��ư
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

    void BgmManage(bool isDance) // ���� ������� �ٲ���
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

    #region ������ ����
    public void BuyItemBtn() // 0 : ����������, 1 : �ð� ����, 2 : ��Ȱ
    {
        for(int i = 0; i < itemBuyBtns.Length; i++)
        {
            if (itemBuyBtns[i].isOn == true)
            {
                ActivateItem(i);
            }
        }
    }
    public void ActivateItem(int itemNum) // �������� ����
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
    public void ItemBtn() // ���� ������ ��ư
    {
        if (!isItemFog && itemUseCount < maxItemUseCount)
        {
            SpineTeacher.GetComponent<TeacherMove>().Move(0); // �������� ���¸� �ڵ� ���·�
            itemUseCount++; // ������ ��� Ƚ�� +
            teacherState = 0; // �������� ���¸� �ڵ� ���·�
            isItemFog = true; // ���� ���� on
            isDancing = true; // ���߰� �ִ� ���·� ����
            ChangeStudentAct(true); // �л��� �������� �������� ����
            BgmManage(isDancing); // bgm  �� �����Ҷ��� bgm ���� ����
            if (maxItemUseCount <= itemUseCount) // ���� ������ ���Ƚ���� �ִ��� �������� ����
            {
                itemBtn[0].SetActive(false);
            }
            CancelInvoke("TeacherChange");
        }
    }

    #endregion

    public void getRankInfo() => BackendServerManager.GetInstance().getRank();

    public void Tuto() // ���� ������ ó�� ������ ��� Ʃ�丮���� ���� �ð��� ���߰� �����
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

    //�ػ� �ʱ�ȭ
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
        // ��ó���� �л��� ����
        for(int i = 0; i < students.Length; i++)
        {
            MakeStudentNum();
        }

        // �л��� �ִϸ��̼� ����
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
