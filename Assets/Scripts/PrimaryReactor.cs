using System.Collections;
using UnityEngine;

public class PrimaryReactor : MonoBehaviour
{
    public PrimaryButtonWatcher watcher;
    public bool IsPressed = false; // used to display button state in the Unity Inspector window

    // rotation code
    /*
    public Vector3 rotationAngle = new Vector3(45, 45, 45);
    public float rotationDuration = 0.25f; // seconds
    private Quaternion offRotation;
    private Quaternion onRotation;
    private Coroutine rotator;
    */

    void Start()
    {
        watcher.primaryButtonPress.AddListener(onPrimaryButtonEvent);

        // rotation code
        /*
        offRotation = this.transform.rotation;
        onRotation = Quaternion.Euler(rotationAngle) * offRotation;
        */
    }

    public void onPrimaryButtonEvent(bool pressed)
    {   
        
        // used to update the accessible field
        IsPressed = pressed;

        if (pressed)
        {
            watcher.gameObject.GetComponent<Experimentscript>().primaryButtonDown();
        }
        else
        {
            watcher.gameObject.GetComponent<Experimentscript>().primaryButtonUp();
        }

        // rotation code
        /*
        if (rotator != null)
            StopCoroutine(rotator);
        if (pressed)
            rotator = StartCoroutine(AnimateRotation(this.transform.rotation, onRotation));
        else
            rotator = StartCoroutine(AnimateRotation(this.transform.rotation, offRotation));
        */
    }

    // rotation code
    /*
    private IEnumerator AnimateRotation(Quaternion fromRotation, Quaternion toRotation)
    {
        float t = 0;
        while (t < rotationDuration)
        {
            transform.rotation = Quaternion.Lerp(fromRotation, toRotation, t / rotationDuration);
            t += Time.deltaTime;
            yield return null;
        }
    }
    */
}