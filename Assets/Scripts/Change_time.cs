using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class Change_time : MonoBehaviour
{
    Transform hourHand;
    Transform minuteHand;
    Transform secondHand;

    // Start is called before the first frame update
    void Start()
    {
        hourHand = transform.Find("Clock_Analog_A_Hour");
        minuteHand = transform.Find("Clock_Analog_A_Minute");
        secondHand = transform.Find("Clock_Analog_A_Second");

        List<float> rotations = Rotations();


        hourHand.Rotate(new Vector3(rotations[0], 0, 0), Space.Self);
        minuteHand.Rotate(new Vector3(rotations[1], 0, 0), Space.Self);
        secondHand.Rotate(new Vector3(rotations[2], 0, 0), Space.Self);


    }

    // Update is called once per frame
    void Update()
    {
        List<float> rotations = Rotations();

        //Debug.Log(rotations[0] + "," + rotations[1] + "," + rotations[2]);

        hourHand.rotation = Quaternion.Euler(-rotations[0]+180, 90, -180);
        //Debug.Log(hourHand.rotation.eulerAngles);
        minuteHand.rotation = Quaternion.Euler(-rotations[1]+180, 90, -180);
        secondHand.rotation = Quaternion.Euler(-rotations[2]+180, 90, -180);

        

        //if (secondHand.rotation.eulerAngles.x - 90 < 0.1)
        //{
        //    Debug.Log(secondHand.rotation.eulerAngles);
        //    Debug.Log(secondTarget.eulerAngles);
        //}

        // Dampen towards the target rotation
        //if (hourHand.rotation != hourTarget)
        //{
        //    hourHand.rotation = Quaternion.Slerp(hourHand.rotation, hourTarget, Time.deltaTime * smooth);
        //}

        //if (minuteHand.rotation != minuteTarget)
        //{
        //    minuteHand.rotation = Quaternion.Slerp(minuteHand.rotation, minuteTarget, Time.deltaTime * smooth);
        //}

        //if (secondHand.rotation != secondTarget)
        //{
        //    secondHand.rotation = Quaternion.Slerp(secondTarget, secondHand.rotation, Time.deltaTime * smooth);
        //}
        

    }

    // Get Rotations
    List<float> Rotations()
    {
        DateTime currentTime = DateTime.Now;
        List<float> rotations = new List<float>
        {
            currentTime.Hour % 12 * 30,
            currentTime.Minute * 6,
            currentTime.Second * 6
        };
        return rotations;


    }

    // Rotate Hands
    void UpdateRotations()
    {
       
    }

}
