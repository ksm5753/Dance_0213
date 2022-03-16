using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadeText : MonoBehaviour
{
    float full = 1;
    bool isTrigger = false;
    void OnEnable()
    {
        isTrigger = true;
        full = 1;
    }

    void OnDisable()
    {
        isTrigger = false;
    }

    private void Update()
    {
        if (isTrigger == true)
        {
            full -= Time.deltaTime * 0.5f;
            gameObject.GetComponent<Text>().color = new Color(255, 255, 255, full);
            if (full <= 0)
            {
                gameObject.SetActive(false);
            }
        }
    }
}
