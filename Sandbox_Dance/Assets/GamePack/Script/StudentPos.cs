using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StudentPos : MonoBehaviour
{
    // ---------구현 규칙-----------
    // 1. 학생들은 딴짓을 할때마다 책상위로 올라간다.
    // 2. 만약 모두 올라가기 전에 바뀌면 그자리에서 다시 올라가거나 내려간다,

    [SerializeField] Vector3[] TargetPoses; // 목표 위치
    [SerializeField] float changeTime = 0; // 점점 바뀔 시간
    [SerializeField]float moveSpeed = 1f; // 움직직이는 스피드
    public bool posChecker; // 현재 가야할 장소 체크
    [SerializeField] Vector3 currentPos;

    void Start()
    {
        changeCurrentPos();
        TargetPoses[0] = transform.position;
        TargetPoses[1] = TargetPoses[0] + TargetPoses[1];
        TargetPoses[2] = TargetPoses[0] + TargetPoses[2];
    }

    void Update()
    {
        posChanager();
        upTime();

        if (Application.isEditor)
        {
            if (!Game.Instance.isItemFog)
            {
                if (Input.GetKeyUp(KeyCode.Mouse0))
                {
                    posChecker = false;
                    changeCurrentPos();
                }

                // 누르고 있는 상태가 아니라면
                else if (Input.GetKeyDown(KeyCode.Mouse0))
                {
                    posChecker = true;
                    changeCurrentPos();
                }
            }

            else
            {
                posChecker = true;
            }
        }

        // 아닐경우
        else
        {
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Stationary)
                {
                    posChecker = true;
                    changeCurrentPos();
                }

                else if (touch.phase == TouchPhase.Canceled)
                {
                    posChecker = true;
                    changeCurrentPos();
                }
            }
        }
    }

    public void changeCurrentPos() 
    {
        currentPos = transform.position;
        changeTime = 0;
    }

    public void upTime()
    {
        changeTime += Time.deltaTime * moveSpeed;
    }

    void posChanager()
    {
        if (posChecker)
        {
            if (Game.Instance.typeFloat[3].inspecter[2].variable < Game.Instance.typeFloat[3].inspecter[3].variable)
            {
                transform.position = Vector3.Lerp(currentPos, TargetPoses[1], changeTime);
            }
            else
            {
                transform.position = Vector3.Lerp(currentPos, TargetPoses[2], changeTime);
            }
        }

        else
        {
            transform.position = Vector3.Lerp(currentPos, TargetPoses[0], changeTime);
        }
    }
}
