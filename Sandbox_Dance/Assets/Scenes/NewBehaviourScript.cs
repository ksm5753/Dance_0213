using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Purchasing;
using UnityEngine.Events;

public class NewBehaviourScript : MonoBehaviour
{
    [Header("A")]
    public Text t;
    public IAPButton btnCoin;

    // Start is called before the first frame update
    void Start()
    {
        this.btnCoin.onPurchaseComplete.AddListener(new UnityAction<Product>((product) =>
        {
            t.text = "SUCCESS : " + product.transactionID;
        }));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
