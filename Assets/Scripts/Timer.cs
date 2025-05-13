using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class Timer : MonoBehaviour
{
    //private float maxTime = 10;
    private float timeRemaining;
    private bool running = false;
    public GameObject txtobj;
    private TextMesh tm;
    private bool display = false;
    
    // Start is called before the first frame update
    void Start()
    {
        //setTime(maxTime);
        //timerIsRunning = true;
        tm = Instantiate(txtobj, new Vector3(0,3,5), Quaternion.identity).GetComponent<TextMesh>();
        tm.text = "";
    }

    // Update is called once per frame
    void Update()
    {
        if (running)
        {
            tm.color = Color.black;
            if (timeRemaining > 0f)
            {
                if (display)
                {
                    DisplayTime(timeRemaining);
                }
                timeRemaining -= Time.deltaTime;
            }
            else
            {
                Debug.Log("Time has run out!");
                timeRemaining = 0;
                if (display)
                {
                    DisplayTime(timeRemaining);
                }
                running = false;

                // Execute appropriate parent method
                
                Experimentscript exp = GameObject.Find("Experiment").GetComponent<Experimentscript>();
                
                //tm.text = "pre-timer.exptimerfinish()" + "  stage = " + exp.stage;
                exp.timerFinished();
                //tm.text = "post-timer.exptimerfinish()" + "  stage = " + exp.stage;

            }
        }
        else
        {
            tm.color = Color.red;
        }
    }

    public void setTime(float newTime)
    {
        timeRemaining = newTime;
        //maxTime = newTime;
    }

    public float getTime()
    {
        return timeRemaining;
    }

    public void setRunning(bool run)
    {
        running = run;
    }

    public bool getRunning()
    {
        return running;
    }

    /*
    public float getMaxTime()
    {
        return maxTime;
    }
    */
    
    void DisplayTime(float timeToDisplay)
    {
        //timeToDisplay += 1;

        //float minutes = Mathf.FloorToInt(timeToDisplay / 60); 
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);
        float milliSeconds = (timeToDisplay % 1) * 1000;

        tm.text = string.Format("{0:00}:{1:000}", seconds, milliSeconds);
    }
    
}
