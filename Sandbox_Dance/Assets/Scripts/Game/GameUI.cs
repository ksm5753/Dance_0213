using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    private static GameUI instance;

    public GameObject itemBuyPanel;
    public Text coinText;
    public Text diaText;


    void Start()
    {
        BackendServerManager.GetInstance().GetMyMoney();
    }

    public static GameUI GetInstance()
    {
        if (instance == null) return null;

        return instance;
    }

    void Awake()
    {
        if (!instance) instance = this;
    }
}
