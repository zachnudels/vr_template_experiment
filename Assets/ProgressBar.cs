using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProgressBar : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateLevel(float amount) {
        Debug.Log("Updating by " + amount);
        if (transform.localScale.x <= 1 && amount < 0 ||
            transform.localScale.x > 100 && amount > 0 ||
            amount == 0) {
            return;
        }

        float translate = amount * 5;

        transform.localScale += Vector3.right * amount;
        transform.localPosition += Vector3.right * translate;
    }


}
