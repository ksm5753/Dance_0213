using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Data;
using UnityEngine.SceneManagement;

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

    #region �л��� ��������Ʈ ����
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
    // 0 : ���ӽð� (0 : �ִ� �ð�, 1 : ���� �ð�(�پ��� �ð�), 2 : �پ��� �ӵ�, 3 : �þ�� �ӵ�)

    [Header("�پ�� �ð� ����")]
    [SerializeField] GameObject timeBar; // ũ�Ⱑ �ٲ� ������Ʈ

    [Header("�÷��̾� ���� ���� ����")]
    [SerializeField] GameObject actBar; // �÷��̾��� ���� ���� ����ġ ��
    [SerializeField] GameObject[] students; // �л��� ���� ������Ʈ
    bool isDancing = false;

    [Header("������ ����")]
    [SerializeField] GameObject SpineTeacher; // ������ ������ ������Ʈ

    [SerializeField] GameObject resultWin; // ���â

    [Header("������ ����")]
    public bool isItemFog; // �������� ������ΰ�
    public bool isReviveOn; // ��Ȱ ������ ���� ����

    [SerializeField] GameObject[] itemBtn; // 0 : ����������, 1 : ��Ȱ ...������ ��ư��

    public GameObject storeObj;

    public Toggle[] itemBuyBtns; // 0 : ����������, 1 : �ð� ����, 2 : ��Ȱ....������ ���� ��ư��

    public GameObject[] UIOBjs; // ������ ���۵Ǹ� ������ OBJ
    public GameObject[] resultMenuBtn;

    [Header("����")]
    public Text resultText;
    public Text[] pricesText; // 0 : ��� 1 : ���
    public Text currentScoreText;

    public float needMoney = 0;

    public float score; // ���� ����
    public int bestScore;
    public int finalPrice; // ���������� �޴� ��

    // ���� ���� ����
    [SerializeField] bool isPlaying = true; // ���� ������ ���������� {true = ������, false = �Ͻ�����}


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

    // ���� �������� �Ŵ�¡
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

            timeBar.GetComponent<RectTransform>().localScale = new Vector2((typeFloat[0].inspecter[1].variable / typeFloat[0].inspecter[0].variable), 1); // �����ð� ��
            actBar.GetComponent<RectTransform>().localScale = new Vector2((typeFloat[3].inspecter[1].variable / typeFloat[3].inspecter[0].variable), 1); // ����ġ ��
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

                    for (int i = 0; i < students.Length; i++)
                    {
                        students[i].GetComponent<Image>().sprite = studentImages[setStudentNum[i]].image[(int)typeFloat[3].inspecter[2].variable].studentPosImage;
                    }
                }

                // �ִ� ������ �������� ��� �� �̻����� ���ö󰡰� ������ (�ִ� ������ 0 ~ 3 ����)
                else
                {
                    typeFloat[3].inspecter[1].variable = typeFloat[3].inspecter[0].variable;
                }
            }

            // ���� �������� ���� �ִ� ���̶��
            if (typeFloat[2].inspecter[2].variable == 2 && !isItemFog)
            {
                // ������ ��� �����ְ� �������� ���º�ȯ�� �����ش�.
                isPlaying = false;
                SoundManager.Instance.Vibrate();
                CancelInvoke("TeacherChange");
                SpineTeacher.GetComponent<TeacherMove>().Move(3);
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

    void TeacherChange() // �������� ���º���{Invoke �� ��� ���� ������} 
    {
        // ���� �ٲ� �ð� üũ
        float turnTime = Random.Range(typeFloat[2].inspecter[0].variable, typeFloat[2].inspecter[1].variable);

        // ������ ���� ��ȯ
        typeFloat[2].inspecter[2].variable += 1;

        if (typeFloat[2].inspecter[2].variable > 2)
        {
            typeFloat[2].inspecter[2].variable = 0;
        }
        SpineTeacher.GetComponent<TeacherMove>().Move((byte)typeFloat[2].inspecter[2].variable);
        // turnTime ���� �ٽ� �� �Լ� ����
        Invoke("TeacherChange", turnTime);
    }

    // ���� ���� ��� ó��
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

    //�ػ� �ʱ�ȭ
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
