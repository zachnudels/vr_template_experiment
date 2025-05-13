using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class shapescript : MonoBehaviour
{

    // Color constants: red green blue gray cyan magenta yellow

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void setColour(Color newColor)
    {
        GetComponent<MeshRenderer>().material.color = newColor;
    }

    public void setShape(Mesh newMesh)
    {
        GetComponent<MeshFilter>().mesh = newMesh;

        //Vector3 scale = transform.localScale;
        //scale.Set(scl.x, scl.y, scl.z);
        GetComponent<Transform>().localScale = new Vector3(0.5f, 0.5f, 0.5f);

        //scale.Set(scl.x, scl.y, scl.z);
    }
}
