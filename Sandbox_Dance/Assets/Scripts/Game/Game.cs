using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Data;
using UnityEngine.SceneManagement;
//--------------���� ��Ģ---------------//
// 1. �÷��̾�� ȭ���� ��ġ�ϸ� ������ �� �� ����
// 2. �������� �ڵ��ƺ��� ���� �� ������ �� ��� ����â�� ��
// 3. ������ ������ ª�� �ð��ȿ� Ǯ�����
// 4. ������ ������ ��� ������ ������ְ� �ƴϸ� ������ �״�� ���������(������ Ǫ�� ���ȿ� ��ü�ð��� ������)
// 5. ��ġ�� �ϰ� ���� ��� �������� ���ݾ� ���� �������� Ǯ�� ���� ���� �ܰ�� �Ѿ �ִϸ��̼��� �ٲ�

public class Game : MonoBehaviour
{
    [Header("�پ�� �ð� ����")]
    public List<float> timeSet = new List<float>(); // 0 : �ִ� �ð� 1 : �پ�� �ð�
    [SerializeField] float maxTime; // �ִ� �ð�
    [SerializeField] float playTime; // ���� �پ�� �ð�
    [SerializeField] GameObject timeBar; // ũ�Ⱑ �ٲ� ������Ʈ

    [SerializeField] float minusSpeed; // �ð��� ������ ���ǵ�
    [SerializeField] float plusSpeed; // �ð��� ������ ���ǵ�

    [Header("�÷��̾� ���� ���� ����")]
    [SerializeField] float needExp; // ���� ���� �ʿ��� ����ġ��
    [SerializeField] float gainExpSpeed; // ����ġ�� ��� �ӵ�
    [SerializeField] float actProcess; // �÷��̾��� ���� ���� ����ġ ����
    public int actLevel; // ���� �÷��̾��� �ൿ ����
    public int maxActLevel; // �ִ� ����
    [SerializeField] GameObject actBar; // �÷��̾��� ���� ���� ����ġ ��
    [SerializeField] GameObject[] students; // �л��� ���� ������Ʈ
    [SerializeField] Sprite[] studentSprite; // �л��� ������ (������ ��������)
    bool isDancing = false;

    [Header("������ ����")]
    [SerializeField] float[] rotateTimeRange; // �������� ���°� �ٲ�� �ֱ� ����{ 0 : �ּҽð�, 1 : �ִ� �ð�}
    [SerializeField] byte isWatching = 0; // �������� �Ĵٺ����� ����üũ {2 : ������, 1 : �ڵ��� ��, 0 : �ڵ�������}
    [SerializeField] GameObject SpineTeacher; // ������ ������ ������Ʈ

    [Header("����")]
    [SerializeField] Text resultText;
    [SerializeField] Text currentScoreText;
    public float score; // ���� ����
    [SerializeField] GameObject resultWin; // ���â
    public int finalPrice; // ���������� �޴� ��

    [Header("������ ����")]
    public bool isItemFog; // �������� ������ΰ�
    public bool isReviveOn; // ��Ȱ ������ ���� ����

    [SerializeField] float maxItemTime; // ������ ���� �ð�
    [SerializeField] float nowItemTime; // ������ ���� �ð�
    [SerializeField] byte useCount; // ���Ƚ��
    [SerializeField] byte maxUseCount; // �ִ� ��� Ƚ�� �Ŀ� �������� ����

    public int[] itemPrices; // 0 : ����������, 1 : �ð� ����, 2 : ��Ȱ
    [SerializeField] GameObject[] itemBtn; // 0 : ����������, 1 : ��Ȱ ...������ ��ư��

    public Toggle[] itemBuyBtns; // 0 : ����������, 1 : �ð� ����, 2 : ��Ȱ....������ ���� ��ư��

    [SerializeField] GameObject[] UIOBjs; // ������ ���۵Ǹ� ������ OBJ


    // ���� ���� ����
    [SerializeField] bool isPlaying = true; // ���� ������ ���������� {true = ������, false = �Ͻ�����}

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
        if (instance == null)
        {
            instance = this.GetComponent<Game>();
        }
    }

    public void Initialize()
    {
        foreach (GameObject i in UIOBjs)
        {
            i.SetActive(false);
        }
    }

    void Update()
    {
        TimeManaging();
    }

    // ���� �������� �Ŵ�¡
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
                        studenObj.GetComponent<Image>().sprite = studentSprite[4];
                    }
                }
            }

            // ���� ũ�� ����
            timeBar.GetComponent<RectTransform>().localScale = new Vector2((playTime / maxTime), 1); // �����ð� ��
            actBar.GetComponent<RectTransform>().localScale = new Vector2((actProcess / needExp), 1); // ����ġ ��
            currentScoreText.text = Mathf.RoundToInt(score) + "0";

            score += Time.deltaTime;

            TouchChecker();
        }
    }

    void TouchChecker()
    {
        if (isDancing)
        {
            // ������ �� �ð��� ������ �Ͱ� ���� �������� ����
            playTime += plusSpeed * Time.deltaTime;
            actProcess += gainExpSpeed * Time.deltaTime;

            // ���� ������ ���� ������ ���� ����ġ ��� �ӵ� �޶���
            if (actLevel != 0) score += actLevel * Time.deltaTime * 10;
            else score += Time.deltaTime * 10;

            // ���� ����ġ�� �ʿ� ��ġ�� �ʰ��� ���
            if (actProcess > needExp)
            {
                // ���� �ִ� ������ �ƴ� ��� ������ �÷��ְ� ����ġ ��ġ�� 0���� �ٲ���
                if (actLevel < maxActLevel)
                {
                    actLevel += 1;
                    foreach (GameObject studenObj in students)
                    {
                        studenObj.GetComponent<Image>().sprite = studentSprite[actLevel];
                    }
                    actProcess = 0;
                }

                // �ִ� ������ �������� ��� �� �̻����� ���ö󰡰� ������
                else
                {
                    actProcess = needExp;
                }
            }

            // ���� �������� ���� �ִ� ���̶��
            if (isWatching == 2 && !isItemFog)
            {
                // ������ ��� �����ְ� �������� ���º�ȯ�� �����ش�.
                isPlaying = false;
                CancelInvoke("TeacherChange");

                Invoke("EndGame", 0.6f);
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
            actProcess -= Time.deltaTime;

            // ���� ����ġ ��ġ�� 0���� ��������
            if (actProcess <= 0)
            {
                //  ���� ������ 0�� �ƴ϶��
                if (actLevel != 0)
                {
                    // ������ �����ְ� ����ġ ��ġ�� �ִ�� ������ش�.
                    actProcess = needExp;
                    actLevel--;
                }

                // ������ 0�̶�� �� ���Ϸ� �������� �ʵ��� �����ش�.
                else
                {
                    actProcess = 0;
                }
            }

            // ���� �ð��� 0���� �۾����ٸ� ���ӿ��� ó������
            if (playTime < 0 && isPlaying)
            {
                isPlaying = false;
                EndGame();
                Debug.Log("���� ����");
            }
        }
    }

    void TeacherChange() // �������� ���º���{Invoke �� ��� ���� ������} 
    {
        // ���� �ٲ� �ð� üũ
        float turnTime = Random.Range(rotateTimeRange[0], rotateTimeRange[1]);
        isWatching += 1;
        if (isWatching > 2)
        {
            isWatching = 0;
        }
        SpineTeacher.GetComponent<SoldierT>().Move(isWatching);
        // turnTime ���� �ٽ� �� �Լ� ����
        Invoke("TeacherChange", turnTime);
    }

    // ���� ���� ��� ó��
    void EndGame()
    {
        resultWin.SetActive(true);
        score = Mathf.RoundToInt(score);
        finalPrice = Mathf.RoundToInt(score / 100);
        resultText.text = "����� ���� : " + score.ToString() + '\n' + "���� ��� : " + finalPrice;
        isPlaying = false;
        CancelInvoke("TeacherChange");
    }

    public void ContinueGame()
    {
        float turnTime = Random.Range(rotateTimeRange[0], rotateTimeRange[1]);
        Invoke("TeacherChange", turnTime);
        playTime = playTime + 2f;
        isPlaying = true;
        isWatching = 0;
        SpineTeacher.GetComponent<SoldierT>().Move(0);
        resultWin.SetActive(false);
    }

    public void StartGame()
    {
        BuyItemBtn();
        float turnTime = Random.Range(rotateTimeRange[0], rotateTimeRange[1]);
        Invoke("TeacherChange", turnTime);
        playTime = playTime + 2f;
        isPlaying = true;
        isWatching = 0;
        SpineTeacher.GetComponent<SoldierT>().Move(0);
        resultWin.SetActive(false);
        if (!isReviveOn)
        {
            itemBtn[1].SetActive(false);
        }
    }

    public void ItemBtn()
    {
        if (!isItemFog && useCount < maxUseCount)
        {
            SpineTeacher.GetComponent<SoldierT>().Move(0);
            useCount++;
            isWatching = 0;
            isItemFog = true;
            isDancing = true;
            foreach (GameObject studenObj in students)
            {
                studenObj.GetComponent<Image>().sprite = studentSprite[actLevel];
            }

            if (maxUseCount <= useCount)
            {
                itemBtn[0].SetActive(false);
            }
            CancelInvoke("TeacherChange");
        }
    }

    public void HomeBtn()
    {
        SceneManager.LoadScene("2. Lobby");
        // �̰��� finalPrice ��ŭ�� ��带 �÷��̾�� ����
    }

    #region ������ ����
    public void BuyItemBtn() // 0 : ����������, 1 : �ð� ����, 2 : ��Ȱ
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
                BackendServerManager.GetInstance().BuyInGameItem(itemPrices[0]);
                break;
            case 1:
                maxTime += 10;
                print("B");
                BackendServerManager.GetInstance().BuyInGameItem(itemPrices[1]);

                break;
            case 2:
                isReviveOn = true;
                print("C");
                BackendServerManager.GetInstance().BuyInGameItem(itemPrices[2]);
                break;
        }
    }
    #endregion
}
