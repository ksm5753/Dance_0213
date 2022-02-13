using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

public class SoldierT : MonoBehaviour
{
    public SkeletonGraphic skeletonAnimation;

    public void Move(byte check)
    {
        switch (check)
        {
            case 0:
                skeletonAnimation.AnimationState.SetAnimation(0, "Breathing", true);
                break;
            case 1:
                skeletonAnimation.AnimationState.SetAnimation(0, "Good", true);
                break;
            case 2:
                skeletonAnimation.AnimationState.SetAnimation(0, "Sad", true);
                break;
        }
    }
}
