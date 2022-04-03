using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

public class TeacherMove : MonoBehaviour
{
    public SkeletonGraphic[] TeacherState; // 0 : �ڵ������� �ڵ�����, 1 : �ڵ��ƺ� 2 : ������ 3 : ȭ��
    // Start is called before the first frame update

    public void SetBreath()
    {
        TeacherState[2].AnimationState.SetAnimation(0, "Breathing", true);
    }

    public void SetMove()
    {
        TeacherState[0].gameObject.SetActive(false);
        TeacherState[1].gameObject.SetActive(false);
        TeacherState[2].gameObject.SetActive(true);
        TeacherState[3].gameObject.SetActive(false);
        TeacherState[2].AnimationState.SetAnimation(0, "Walking", true);
    }

    public void SetAngry()
    {
        TeacherState[0].gameObject.SetActive(false);
        TeacherState[1].gameObject.SetActive(false);
        TeacherState[2].gameObject.SetActive(false);
        TeacherState[3].gameObject.SetActive(true);
        TeacherState[3].AnimationState.SetAnimation(0, "angry", true);
    }

    public void Move(byte check)
    {
        if (check <= 4)
        {
            TeacherState[1].gameObject.SetActive(false);
            TeacherState[0].gameObject.SetActive(true);
        }

        else
        {
            TeacherState[0].gameObject.SetActive(false);
            TeacherState[1].gameObject.SetActive(true);
        }
        switch (check)
        {
            case 0:
                TeacherState[0].AnimationState.SetAnimation(0, "basic", true);
                break;
            case 2:
                TeacherState[0].AnimationState.SetAnimation(0, "ready", true);
                break;
            case 3:
                TeacherState[0].AnimationState.SetAnimation(0, "ready2", true);
                break;
            case 4:
                TeacherState[0].AnimationState.SetAnimation(0, "ready3", true);
                break;

            case 5:
                TeacherState[1].AnimationState.SetAnimation(0, "Breathing", true);
                break;

            case 6:
                TeacherState[1].AnimationState.SetAnimation(0, "Sad", true);
                break;
        }
    }
}
