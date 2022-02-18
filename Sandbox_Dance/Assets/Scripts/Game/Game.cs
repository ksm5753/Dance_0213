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

    public List<TypeFloat> typeFloat;
    // 0 : ���ӽð� (0 : �ִ� �ð�, 1 : ���� �ð�(�پ��� �ð�), 2 : �پ��� �ӵ�, 3 : �þ�� �ӵ�)

    [Header("�پ�� �ð� ����")]
    [SerializeField] GameObject timeBar; // ũ�Ⱑ �ٲ� ������Ʈ

    [Header("�÷��̾� ���� ���� ����")]
    [SerializeField] GameObject actBar; // �÷��̾��� ���� ���� ����ġ ��
    [SerializeField] GameObject[] students; // �л��� ���� ������Ʈ
    [SerializeField] Sprite[] studentSprite; // �л��� ������ (������ ��������)
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

    [Header("����")]
    public Text resultText;
    public Text[] pricesText; // 0 : ��� 1 : ���
    public Text currentScoreText;

    public float needMoney = 0;

    public float score; // ���� ����
    public int finalPrice; // ���������� �޴� ��

    // ���� ���� ����
    [SerializeField] bool isPlaying = true; // ���� ������ ���������� {true = ������, false = �Ͻ�����}

    private static Game instance = null;

    void Update()
    {
        TimeManaging();
        if (Input.GetKey(KeyCode.Escape))
        {
            UIOBjs[2].SetActive(true);
        }
    }

    public void DanceButton(bool isDance)
    {
        if (isPlaying)
        {
            if (isDance)
            {
                foreach (GameObject studenObj in students)
                {
                    studenObj.GetComponent<Image>().sprite = studentSprite[(int)typeFloat[3].inspecter[2].variable];
                }
                SoundManager.Instance.PlayBGM(1);
                isDancing = true;
            }

            else
            {
                foreach (GameObject studenObj in students)
                {
                    studenObj.GetComponent<Image>().sprite = studentSprite[4];
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

                    foreach (GameObject studenObj in students)
                    {
                        studenObj.GetComponent<Image>().sprite = studentSprite[4];
                    }
                }
            }

            timeBar.GetComponent<RectTransform>().localScale = new Vector2((typeFloat[0].inspecter[1].variable / typeFloat[0].inspecter[0].variable), 1); // �����ð� ��
            actBar.GetComponent<RectTransform>().localScale = new Vector2((typeFloat[3].inspecter[1].variable / typeFloat[3].inspecter[0].variable), 1); // ����ġ ��
            currentScoreText.text = Mathf.RoundToInt(score).ToString() + "0";

            if (typeFloat[3].inspecter[2].variable != 0) score += typeFloat[3].inspecter[2].variable * Time.deltaTime;
            else score += Time.deltaTime * 10;

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
            else score += Time.deltaTime * 10;

            // ���� ����ġ�� �ʿ� ��ġ�� �ʰ��� ���
            if (typeFloat[3].inspecter[1].variable > typeFloat[3].inspecter[0].variable)
            {
                // ���� �ִ� ������ �ƴ� ��� ������ �÷��ְ� ����ġ ��ġ�� 0���� �ٲ���
                if (typeFloat[3].inspecter[2].variable < typeFloat[3].inspecter[3].variable)
                {
                    typeFloat[3].inspecter[2].variable += 1;
                    typeFloat[3].inspecter[1].variable = 0; // ���� ����ġ ��ġ 0 ���� �ٲ���

                    foreach (GameObject studenObj in students)
                    {
                        studenObj.GetComponent<Image>().sprite = studentSprite[(int)typeFloat[3].inspecter[2].variable];
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

                Invoke("EndGame", 0.6f);
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
                EndGame();
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
        SpineTeacher.GetComponent<SoldierT>().Move((byte)typeFloat[2].inspecter[2].variable);
        // turnTime ���� �ٽ� �� �Լ� ����
        Invoke("TeacherChange", turnTime);
    }

    // ���� ���� ��� ó��
    void EndGame()
    {
        SoundManager.Instance.bgmSource.Pause();
        resultWin.SetActive(true);
        score = Mathf.RoundToInt(score);
        finalPrice = Mathf.RoundToInt(score / 10);
        resultText.text = score.ToString() +"0";
        pricesText[0].text = finalPrice.ToString();
        SoundManager.Instance.PlayEffect(1);
        isPlaying = false;
        CancelInvoke("TeacherChange");
    }

    public void ContinueGame()
    {
        typeFloat[0].inspecter[1].variable = typeFloat[0].inspecter[1].variable + 2f;
        ResetGame();
        SoundManager.Instance.PlayBGM(0);
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

    public void SceneChange(string SceneName)
    {
        BackendServerManager.GetInstance().GiveMoney(finalPrice);
        SceneManager.LoadScene(SceneName);
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
