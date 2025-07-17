using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TVDynamics : MonoBehaviour
{

    bool outOfView;
    float speed;
    Transform eyes;
    public GameObject TVPrefab;
    GameObject TV;
    float downOffset = 0.65f;

    public bool TVInitialized = false;


    static float velocity = 0.005f;

    // Start is called before the first frame update
    void Start() {
        this.speed = 0f;

        // Find eyes depending on simulation or not
        
    }

    public void Initialize() {
        try {
            if (GameObject.Find("Simulate").activeInHierarchy) {
                eyes = GameObject.Find("Camera Offset").transform;
            }
        }
        catch {
            try {
                if (GameObject.Find("Player").activeInHierarchy) {
                    eyes = GameObject.Find("VRCamera").transform;
                }
            }
            catch {
                Debug.LogError("Could not find camera");
            }
        }

        TV = Instantiate(TVPrefab,
            new Vector3(eyes.position.x, eyes.position.y - downOffset, 1.95f),
            //new Vector3(-0.1f, 1.7f - downOffset, 2.15f),
            Quaternion.Euler(0f, 180f, 0f),
            this.transform);

        outOfView = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (speed != 0) {
            TV.transform.Translate(new Vector3(0f, this.speed, 0f));
            if (outOfView && TV.transform.position.y <= eyes.position.y - downOffset) {
                outOfView = false;
                speed = 0;
            } else if (!outOfView && TV.transform.position.y >= 3) {
                outOfView = true;
                speed = 0;
            }
        } 
    }

    public void engage(bool inView) {
        if (inView) {
            if (!this.outOfView) return;
            this.speed = -velocity;
        }
        else {
            if (this.outOfView) return;
            this.speed = velocity;
        }
    }
}
