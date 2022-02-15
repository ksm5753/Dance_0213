using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Spine.Unity;

public class CardChanger : MonoBehaviour
{
    // 단일 대상 카드가 여러카드 뒷면을 보여주는 애니메이션보다 위에 위치
    // 단일 카드는 카드의 뒷면만 바꿔주고 애니메이션을 정해주면됨 애니메이션 끝나고 1초뒤 자동으로 꺼줌
    // 여러대상의 카드는 카드에 토글과 이미지를 주고 누르면 단일 애니메이션 하나 재생하고 끝나면 바꿔줌
    // 이것이 11한번 반복되면 자동으로 마지막 토글하고 3초뒤 꺼줌
    // 끌때 한번 초기화 시켜줘야함 다시 토글을 On 시켜줘야함

    public GameObject[] oneOrMany; // 0 : 하나짜리, 1 : 여러개

    // 한개짜리 관리
    public SkeletonGraphic []OneImage; // 한개의 이미지 관리
    public GameObject SingleCard; // 한개의 카드

    // 11연 뽑 관련
    public int[] smallCardsNum; // 카드 개별 확인
    public GameObject[] smallCardsImage;
    public GameObject[] smallCardsEffects;
    public Sprite[] smallCardsOutLine; // 작은 카드용 겉면

    // 남은 확인해야할 카드
    public int maxNum = 1; // 봐야할 카드들
    public int cardStar = 1; // 카드의 등급

    // 스킵 버튼
    [SerializeField] Button SkipButton;

    public void OneCard(int cardNum) // 하나 뽑았을 때 실행
    {
        maxNum -= 1;
        oneOrMany[0].SetActive(true); // 하나 까기 켜주기
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

    public void SmallCards(int cardNum, int cardIndex) // 여러개 뽑았을 떄 실행, cardNum 나올 카드 이미지 번호, cardIndex 이 카드가 몇번째 카드인지
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

    #region 카드 꺼주는 코드들
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

    void ActiveFalse() // 카드 상태 리셋
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
