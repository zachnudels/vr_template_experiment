using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class btnscript : MonoBehaviour
{   
    public Image img;
    // Start is called before the first frame update
    void Start()
    {
        img = transform.parent.gameObject.GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void onSelect()
    {
        string name = gameObject.name;
        switch (name)
        {
            case "red_btn":
                img.color = Color.red;
                break;
            case "green_btn":
                img.color = Color.green;
                break;
            default:
                break;
        }
    }
}
