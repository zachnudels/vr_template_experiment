using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stage : MonoBehaviour
{
    new string name;
    int number;
    float duration;
    Dictionary<string, string> tags;

    public Stage(string name="", int number=1, float duration=0f)
    {
        this.name = name;
        this.number = number;
        this.duration = duration;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Begin()
    {
        print("Stage begin");
        if (duration == 0f)
        {
            End();
        }
    }

    public void End()
    {
        print("stage end");
    }

}
