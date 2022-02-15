using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Spine.Unity;

public class CardChanger : MonoBehaviour
{
    // ���� ��� ī�尡 ����ī�� �޸��� �����ִ� �ִϸ��̼Ǻ��� ���� ��ġ
    // ���� ī��� ī���� �޸鸸 �ٲ��ְ� �ִϸ��̼��� �����ָ�� �ִϸ��̼� ������ 1�ʵ� �ڵ����� ����
    // ��������� ī��� ī�忡 ��۰� �̹����� �ְ� ������ ���� �ִϸ��̼� �ϳ� ����ϰ� ������ �ٲ���
    // �̰��� 11�ѹ� �ݺ��Ǹ� �ڵ����� ������ ����ϰ� 3�ʵ� ����
    // ���� �ѹ� �ʱ�ȭ ��������� �ٽ� ����� On ���������

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
        smallCardsImage[cardIndex].GetComponent<Button>().onClick.AddListener(() => StartCoroutine(ActiveFalseSmallBack(cardIndex, 3.5f)));
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
        foreach (GameObject smallCardBack in smallCardsImage)
        {
            smallCardBack.GetComponentsInChildren<Image>()[2].enabled = false;
            smallCardBack.GetComponent<Button>().interactable = false;
        }
        maxNum = 1;
        SkipButton.gameObject.SetActive(false);
        Invoke("ActiveFalse", 1);
    }

    public void CheckCardStar(int cardNum)
    {
        if(cardNum > 95) cardStar = 5;

        else if(cardNum > 80) cardStar = 4;

        else if(cardNum > 60) cardStar = 3;

        else if (cardNum > 30) cardStar = 2;

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
    }

    void ActiveFalse() // ī�� ���� ����
    {
        foreach (GameObject smallCardBack in smallCardsImage)
        {
            smallCardBack.GetComponentsInChildren<Image>()[2].enabled = true;
            smallCardBack.GetComponent<Button>().interactable = true;
            smallCardBack.GetComponent<Button>().onClick.RemoveAllListeners();
        }

        this.gameObject.SetActive(false);
    }
    #endregion
}
