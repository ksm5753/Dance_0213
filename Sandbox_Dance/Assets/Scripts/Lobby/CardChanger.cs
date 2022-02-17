using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Spine.Unity;

public class CardChanger : MonoBehaviour
{
    public GameObject[] oneOrMany; // 0 : �ϳ�¥��, 1 : ������

    // �Ѱ�¥�� ����
    public SkeletonGraphic []OneImage; // �Ѱ��� �̹��� ����
    public GameObject SingleCard; // �Ѱ��� ī��

    // 11�� �� ����
    public int[] smallCardsNum; // ī�� ���� Ȯ��
    public GameObject[] smallCardsImage;
    public GameObject[] smallCardsEffects;
    public Sprite[] smallCardsOutLine; // ���� ī��� �Ѹ�

    // ���� Ȯ���ؾ��� ī��
    public int maxNum = 1; // ������ ī���
    public int cardStar = 1; // ī���� ���

    int nowCardIndex;

    // ��ŵ ��ư
    [SerializeField] Button SkipButton;

    public void OneCard(int cardNum) // �ϳ� �̾��� �� ����
    {
        maxNum -= 1;
        oneOrMany[0].SetActive(true); // �ϳ� ��� ���ֱ�
        CheckCardStar(cardNum);
        SetBackEffect(SingleCard);
        OneImage[0].GetComponentInChildren<Image>().sprite = LobbyUI.GetInstance().CardImage[cardNum];

        foreach (SkeletonGraphic cardSet in OneImage)
        {
            cardSet.AnimationState.SetAnimation(0, "card_" + cardStar, false);
        }

        if (oneOrMany[1].activeSelf == true)
        {
            SkipButton.gameObject.SetActive(false);
            Invoke("ActiveFalseOneCard", 3.5f);
        }

        if (maxNum == 0)
        {
            Invoke("ActiveFalse", 3.8f);
        }
    }

    public void SmallCards(int cardNum, int cardIndex) // ������ �̾��� �� ����, cardNum ���� ī�� �̹��� ��ȣ, cardIndex �� ī�尡 ���° ī������
    {
        CheckCardStar(smallCardsNum[cardIndex]);
        SetBackEffect(smallCardsEffects[cardIndex]);
        SkipButton.gameObject.SetActive(true);
        smallCardsImage[cardIndex].GetComponentsInChildren<Image>()[0].sprite = LobbyUI.GetInstance().CardImage[cardNum];
        smallCardsImage[cardIndex].GetComponent<Button>().onClick.AddListener(() => OneCard(cardNum));
        smallCardsImage[cardIndex].GetComponent<Button>().onClick.AddListener(() => SetCardIndex(cardIndex));
        smallCardsImage[cardIndex].GetComponent<Button>().onClick.AddListener(() => StartCoroutine(ActiveFalseSmallBack(cardIndex, 3.5f)));
    }

    void SetCardIndex(int cardIndex)
    {
        nowCardIndex = cardIndex;
        smallCardsImage[cardIndex].GetComponent<Button>().interactable = false;
    }

    void SmallCardsBackSet()
    {
        for(int i = 0; i < smallCardsImage.Length; i++)
        {
            CheckCardStar(smallCardsNum[i]);
            SetBackEffect(smallCardsImage[i]);
        }
    }

    public void Skip()
    {
        for(int i = 0; i < smallCardsImage.Length; i++)
        {
            cardStar = smallCardsNum[i];
            CheckCardStar(cardStar);
            smallCardsImage[i].GetComponentsInChildren<Image>()[2].enabled = false;
            smallCardsImage[i].GetComponent<Button>().interactable = false;
            smallCardsImage[i].GetComponentsInChildren<Image>()[1].sprite = smallCardsOutLine[cardStar];
        }
        SkipButton.gameObject.SetActive(false);
        Invoke("ActiveFalse", 1);
    }

    public void SkipOne()
    {
        foreach (SkeletonGraphic cardSet in OneImage)
        {
            cardSet.AnimationState.SetAnimation(0, "card_" + cardStar + "-1", false);
        }

        if (oneOrMany[1].activeSelf == false)
        {
            CancelInvoke("ActiveFalse");
            Invoke("ActiveFalse", 0.5f);
        }
        else
        {
            StopAllCoroutines();
            StartCoroutine(ActiveFalseSmallBack(nowCardIndex, 0.5f));
            CancelInvoke("ActiveFalseOneCard");
            Invoke("ActiveFalseOneCard", 0.5f);
        }
    }

    public void CheckCardStar(int cardNum)
    {
        if (cardNum > 95) cardStar = 5;
        else if (cardNum > 74) cardStar = 4;
        else if (cardNum > 54) cardStar = 3;
        else if (cardNum > 29) cardStar = 2;
        else cardStar = 1;
    }

    public void SetBackEffect(GameObject targetCard)
    {
        for(int i = 0; i < 4; i++)
        {
            targetCard.transform.GetComponentsInChildren<ParticleSystem>()[i].Stop();
        }

        if (cardStar == 3 || cardStar == 4)
        {
            targetCard.transform.GetComponentsInChildren<ParticleSystem>()[3].Play();
            targetCard.transform.GetComponentsInChildren<ParticleSystem>()[2].Play();
        }

        if(cardStar == 5)
        {
            targetCard.transform.GetComponentsInChildren<ParticleSystem>()[0].Play();
            targetCard.transform.GetComponentsInChildren<ParticleSystem>()[1].Play();
        }
    }

    public void PauseButton()
    {
        Time.timeScale = 0;
    }

    #region ī�� ���ִ� �ڵ��
    IEnumerator ActiveFalseSmallBack(int cardIndex, float time)
    {
        yield return new WaitForSeconds(time);
        smallCardsImage[cardIndex].GetComponentsInChildren<Image>()[1].sprite = smallCardsOutLine[cardStar];
        smallCardsImage[cardIndex].GetComponentsInChildren<Image>()[2].enabled = false;
    }

    void ActiveFalseOneCard()
    {
        oneOrMany[0].SetActive(false);
        SkipButton.gameObject.SetActive(true);
        SingleCard.GetComponent<Button>().interactable = true;
    }

    void ActiveFalse() // ī�� ���� ����
    {
        foreach (GameObject smallCardBack in smallCardsImage)
        {
            smallCardBack.GetComponentsInChildren<Image>()[2].enabled = true;
            smallCardBack.GetComponent<Button>().interactable = true;
            smallCardBack.GetComponent<Button>().onClick.RemoveAllListeners();
        }
        SingleCard.GetComponent<Button>().interactable = true;

        this.gameObject.SetActive(false);
    }
    #endregion
}
