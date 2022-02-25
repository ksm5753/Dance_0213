using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

public class TeacherMove : MonoBehaviour
{
    public SkeletonGraphic[] TeacherState; // 0 : 뒤돌아있음 뒤돌기전, 1 : 뒤돌아봄
    // Start is called before the first frame update
    public void Move(byte check)
    {
        if (check == 0 || check == 1)
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
            case 1:
                TeacherState[0].AnimationState.SetAnimation(0, "ready", true);
                break;
            case 2:
                TeacherState[1].AnimationState.SetAnimation(0, "Breathing", true);
                break;
            case 3:
                TeacherState[1].AnimationState.SetAnimation(0, "Sad", true);
                break;
        }
    }
}
