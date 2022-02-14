using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Spine.Unity;
using Spine;
using UnityEngine.Events;


public class CardInverser : MonoBehaviour
{

    [SerializeField]
    SkeletonGraphic sgBack;

    [SerializeField]
    SkeletonGraphic sgFront;

    [SerializeField]
    Image cardImage;

    [SerializeField]
    UnityEvent completeEvent = new UnityEvent();

    private Spine.AnimationState stateBack;
    private Spine.AnimationState stateFront;

    public GameObject [] elevenCards;

    public GameObject[] drawCount;

    private bool isInversed = false;

    public GameObject cardUI;

    public UnityEvent CompleteEvent { get => completeEvent;  }

    public int star = 1;
    public int cardNum = 10;

    private void Awake()
    {
        stateBack = sgBack.AnimationState;
        stateFront = sgFront.AnimationState;
    }

    public void OnEnable()
    {
        stateBack.SetAnimation(0, "card_o", false);
        stateFront.SetAnimation(0, "card_o", false);
    }

    public void OffOneUI(bool isOneOrEleven)
    {
        switch (isOneOrEleven)
        {
            case false:
                drawCount[0].SetActive(false);
                drawCount[1].SetActive(true);
                break;

            case true:
                drawCount[0].SetActive(true);
                drawCount[1].SetActive(false);
                break;
        }
    }

    public void SetCard(int cardNum)
    {
        isInversed = false;

        if(cardNum > 95)
        {
            star = 4;
        }

        else if(cardNum > 80)
        {
            star = 3;
        }

        else if(cardNum > 50)
        {
            star = 2;
        }

        else
        {
            star = 1;
        }
        cardImage.sprite = LobbyUI.GetInstance().CardImage[cardNum];
    }

    public void InverseCard()
    {
        SetCard(cardNum);
        if(!isInversed)
        {
            Debug.Log("BBB");
            isInversed = true;
            
            stateBack.SetAnimation(0, "card_" + (star).ToString(), false);
            stateFront.SetAnimation(0, "card_" + (star).ToString(), false);
        }
        transform.GetChild(1).GetComponent<Button>().interactable = false;
    }
    public void CloseWindowInvoke()
    {
        Debug.Log("AAA");
        InverseCard();
        Invoke("CloseWindow", 2);
    }

    void CloseWindow()
    {
        cardUI.SetActive(false);
    }
}
