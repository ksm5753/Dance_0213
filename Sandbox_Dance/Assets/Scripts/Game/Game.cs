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
    #region float ����ü
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
    // 0 : ���ӽð� (0 : �ִ� �ð�, 1 : ���� �ð�(�پ��� �ð�), 2 : �پ��� �ӵ�, 3 : �þ�� �ӵ�)

    [Header("�پ�� �ð� ����")]
    [SerializeField] GameObject timeBar; // ũ�Ⱑ �ٲ� ������Ʈ

    [Header("�÷��̾� ���� ���� ����")]
    [SerializeField] GameObject actBar; // �÷��̾��� ���� ���� ����ġ ��
    [SerializeField] GameObject[] students; // �л��� ���� ������Ʈ
    bool isDancing = false;
    Vector3 studentPos = new Vector3(0,0,0);

    [Header("������ ����")]
    [SerializeField] GameObject SpineTeacher; // ������ ������ ������Ʈ

    [SerializeField] GameObject resultWin; // ���â
    bool isNeedWatch = false; // ������ �ڵ��ƺ����ִ°� üũ

    [Header("������ ����")]
    public bool isItemFog; // �������� ������ΰ�
    public bool isReviveOn; // ��Ȱ ������ ���� ����
    public bool isFogOn; // ������������ �����ߴ°�

    [SerializeField] GameObject[] itemBtn; // 0 : ����������, 1 : ��Ȱ ...������ ��ư��

    public GameObject storeObj;
    public GameObject danceButton;

    public Toggle[] itemBuyBtns; // 0 : ����������, 1 : �ð� ����, 2 : ��Ȱ....������ ���� ��ư��

    public GameObject[] UIOBjs; // ������ ���۵Ǹ� ������ OBJ
    public GameObject[] resultMenuBtn;
    public GameObject tutoObj;
    public GameObject DoublePrice;

    [Header("����")]
    public Text resultText;
    public Text[] pricesText; // 0 : ��� 1 : ���
    public Text currentScoreText;
    public Text rankAdText;
    public GameObject checkHighScore;

    public float needMoney = 0;

    public float score; // ���� ����
    public int bestScore;
    public int finalPrice; // ���������� �޴� ��

    // ���â ���� ����
    public GameObject[] resultObjs;
    int currentResultStatus;

    // ���� ���� ����
    [SerializeField] bool isPlaying = true; // ���� ������ ���������� {true = ������, false = �Ͻ�����}

    public static Game instance;

    #region ���� ���� ���� ������

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

    #region �л��� ������ �ִϸ��̼� ����
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

    void TeacherChange() // �������� ���º���{Invoke �� ��� ���� ������} 
    {
        // ���� �ٲ� �ð� üũ
        float turnTime = Random.Range(typeFloat[2].inspecter[0].variable, typeFloat[2].inspecter[1].variable);
        // ������ ���� ��ȯ

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
        // turnTime ���� �ٽ� �� �Լ� ����
        Invoke("TeacherChange", turnTime);
    }
    #endregion

    #region ���� �Ŵ���
    // ������ �ð��� ����üũ
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

            timeBar.GetComponent<RectTransform>().localScale = new Vector2((typeFloat[0].inspecter[1].variable / typeFloat[0].inspecter[0].variable), 1); // �����ð� ��
            actBar.GetComponent<RectTransform>().localScale = new Vector2((typeFloat[3].inspecter[1].variable / typeFloat[3].inspecter[0].variable), 1); // ����ġ 
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
            // ������ �� �ð��� ������ �Ͱ� ���� �������� ����
            typeFloat[0].inspecter[1].variable += typeFloat[0].inspecter[3].variable * Time.deltaTime;
            typeFloat[3].inspecter[1].variable += typeFloat[3].inspecter[4].variable * Time.deltaTime;

            // ���� ������ ���� ������ ���� ����ġ ��� �ӵ� �޶���
            if (typeFloat[3].inspecter[2].variable != 0) score += typeFloat[3].inspecter[2].variable * Time.deltaTime;
            else score += Time.deltaTime;

            // ���� ����ġ�� �ʿ� ��ġ�� �ʰ��� ���
            if (typeFloat[3].inspecter[1].variable > typeFloat[3].inspecter[0].variable)
            {
                // ���� �ִ� ������ �ƴ� ��� ������ �÷��ְ� ����ġ ��ġ�� 0���� �ٲ���
                if (typeFloat[3].inspecter[2].variable < typeFloat[3].inspecter[3].variable)
                {
                    typeFloat[3].inspecter[2].variable += 1;
                    typeFloat[3].inspecter[1].variable = 0; // ���� ����ġ ��ġ 0 ���� �ٲ���

                    ChangeStudentAct(true);
                }

                // �ִ� ������ �������� ��� �� �̻����� ���ö󰡰� ������ (�ִ� ������ 0 ~ 3 ����)
                else
                {
                    typeFloat[3].inspecter[1].variable = typeFloat[3].inspecter[0].variable;
                }
            }

            // ���� �������� ���� �ִ� ���̶��
            if (typeFloat[2].inspecter[2].variable == 5 && !isItemFog)
            {
                // ������ ��� �����ְ� �������� ���º�ȯ�� �����ش�.
                isPlaying = false;
                SoundManager.Instance.Vibrate();
                CancelInvoke("TeacherChange");
                SpineTeacher.GetComponent<TeacherMove>().Move(6);
                MakeResult();
                Invoke("EndGame", 0.3f);
            }

            // ���� �ð��� �ִ�ð��� �ʰ��Ұ�� ���̻�� �ö��� ���ϵ��� �����ش�.
            if (typeFloat[0].inspecter[1].variable >= typeFloat[0].inspecter[0].variable)
            {
                typeFloat[0].inspecter[1].variable = typeFloat[0].inspecter[0].variable;
            }
        }

        else
        {
            // �����ð��� ����ġ ��ġ�� ���� ��������.
            typeFloat[0].inspecter[1].variable -= typeFloat[0].inspecter[2].variable * Time.deltaTime;
            typeFloat[3].inspecter[1].variable -= Time.deltaTime;

            if (typeFloat[3].inspecter[1].variable <= 0) // ���� ����ġ ��ġ�� 0���� ��������
            {
                if (typeFloat[3].inspecter[2].variable != 0)  //  ���� ������ 0�� �ƴ϶��
                {
                    // ������ �����ְ� ����ġ ��ġ�� �ִ�� ������ش�.
                    typeFloat[3].inspecter[1].variable = typeFloat[3].inspecter[0].variable;
                    typeFloat[3].inspecter[2].variable--;
                }

                // ������ 0�̶�� �� ���Ϸ� �������� �ʵ��� �����ش�.
                else
                {
                    typeFloat[3].inspecter[1].variable = 0;
                }
            }

            // ���� �ð��� 0���� �۾����ٸ� ���ӿ��� ó������
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
