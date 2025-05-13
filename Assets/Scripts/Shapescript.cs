using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Shapescript : MonoBehaviour
{
    public GameObject experiment;
    public bool highlighted = false;
    public Vector3 probeScaleNormal;
    public Vector3 probeScaleLarge;
    public bool exploded = false;
    public Vector3 defaultToNormal;
    public Vector3 normalToLarge;
    public bool scaled = false;
    public Vector3 spawnPosition;
    public int spawnPositionCode;
    public int shapeCode;
    public int colorCode;
    public int probePositionCode;
    public float rotation;

    // Color constants: red green blue gray cyan magenta yellow

    // Start is called before the first frame update
    void Start()
    {
        //spawnPosition = new Vector3(0.0f,0.0f,0.0f);
        //spawnPositionCode = 0;
        //shapeCode = 0;
        //colorCode = 0;
        //probePositionCode = 0;
        experiment = GameObject.FindGameObjectsWithTag("experimentobj")[0];
        //GetComponent<Renderer>().enabled = true;

        //rotation
        switch (gameObject.tag)
        {

            case "cube":
                transform.rotation = experiment.GetComponent<Experimentscript>().cube.transform.rotation;
                break;

            case "sphere":
                transform.rotation = experiment.GetComponent<Experimentscript>().sphere.transform.rotation;
                break;
            /*
            case "cylinder":
                transform.rotation = experiment.GetComponent<Experimentscript>().cylinder.transform.rotation;
                break;
            
            case "triangle":
                transform.rotation = experiment.GetComponent<Experimentscript>().triangle.transform.rotation;
                break;
            */

            case "diamond":
                transform.rotation = experiment.GetComponent<Experimentscript>().diamond.transform.rotation;
                break;

            case "star":
                transform.rotation = experiment.GetComponent<Experimentscript>().star.transform.rotation;
                break;
            /*
            case "bar":
                //transform.rotation = experiment.GetComponent<Experimentscript>().bar.transform.rotation;
                break;
            */
            default:
                //probeScaleNormal = new Vector3(1f,1f,1f);
                break;
        }

        //size
        switch (gameObject.tag)
        {

            case "cube":
                probeScaleNormal = new Vector3(0.05f, 0.05f, 0.05f);
                break;
            /*
            case "sphere":
                probeScaleNormal = new Vector3(0.05f,0.05f,0.05f);
                break;
            
            case "cylinder":
                probeScaleNormal = new Vector3(0.05f,0.025f,0.05f);
                break;
            */
            case "triangle":
                probeScaleNormal = new Vector3(0.06f, 0.06f, 0.07f);
                break;
            /*
            case "diamond":
                probeScaleNormal = new Vector3(0.05f,0.1f,0.05f);
                break;
            */
            case "star":
                probeScaleNormal = new Vector3(0.05f, 0.05f, 0.05f);
                break;

            case "bar":
                probeScaleNormal = new Vector3(0.12f, 0.05f, 0.12f);
                break;

            default:
                //probeScaleNormal = new Vector3(1f,1f,1f);
                break;
        }
        probeScaleLarge = probeScaleNormal;
        probeScaleLarge *= 1.15f;

        defaultToNormal = probeScaleNormal - transform.localScale;
        normalToLarge = probeScaleLarge - probeScaleNormal;
        //transform.localScale += defaultToNormal; moved to switch statement.

        switch (experiment.GetComponent<Experimentscript>().stage)
        {
            case "presentation":
                transform.localScale *= 0.35f; //0.13f; //0.15f; //0.35f; //0.7f;
                break;
            case "answer":
                //transform.localScale += defaultToNormal;
                transform.localScale *= 0.175f; //0.13f;
                break;
            case "none":
                //transform.localScale *= 4f;
                //transform.localScale += defaultToNormal;
                transform.localScale += defaultToNormal;
                break;
            case "turn":
                transform.eulerAngles = transform.eulerAngles + new Vector3(0f, 90f, 0f);
                transform.localScale *= 0.2f; //0.13f;
                break;
            default:
                transform.localScale *= 0.2f; //0.13f;
                break;
        }

        if (!GetComponent<Renderer>().enabled)
        {
            GetComponent<Renderer>().enabled = true;
        }


        //experiment.GetComponent<Experimentscript>().tm.text = probeScaleLarge.ToString();

    }

    // Update is called once per frame
    void Update()
    {
        /*
        if (highlighted)
        {
            if (gameObject.transform.localScale.Equals(probeScaleNormal))
            {
                scaleUp();
            }
        } else {
            if (gameObject.transform.localScale.Equals(probeScaleLarge))
            {
                scaleDown();
            }
        }
        */
    }

    public void setColour(Color newColor)
    {
        //if (gameObject.tag.Equals("cone"))
        //{
        //    GetComponentInChildren<MeshRenderer>().material.color = newColor;
        //} else {
        GetComponent<MeshRenderer>().material.color = newColor;
        //}
    }

    public Color getColour()
    {
        //if (gameObject.tag.Equals("cone"))
        //{
        //    return GetComponentInChildren<MeshRenderer>().material.color;
        //} else {
        return GetComponent<MeshRenderer>().material.color;
        //}
    }

    public void setShape(Mesh newMesh)
    {
        //if (gameObject.tag.Equals("cone"))
        //{
        //    GetComponentInChildren<MeshFilter>().mesh = newMesh;
        //} else {
        GetComponent<MeshFilter>().mesh = newMesh;
        //}
    }

    public void setScale(float sx, float sy, float sz)
    {
        transform.localScale = new Vector3(sx, sy, sz);
    }

    public void setPosition(float px, float py, float pz)
    {
        transform.position = new Vector3(px, py, pz);
    }

    public void addPosition(float px, float py, float pz)
    {
        transform.position += new Vector3(px, py, pz);
    }

    public void setVisible(bool vis)
    {
        //if (gameObject.tag.Equals("cone"))
        //{
        //    GetComponentInChildren<Renderer>().enabled = vis;
        //} else {
        GetComponent<Renderer>().enabled = vis;
        //}
    }

    public void scaleDown()
    {
        if (scaled)
        {
            transform.localScale -= normalToLarge;
            scaled = false;
        }

    }

    public void scaleUp()
    {
        if (!scaled)
        {
            transform.localScale += normalToLarge;
            scaled = true;
        }

    }

    public void hoverEntered()
    {
        if ((!experiment.GetComponent<Experimentscript>().stage.Equals("answer")) || GetComponent<Renderer>().enabled == false)
        {
            return;
        }
        highlighted = true;
        experiment.GetComponent<Experimentscript>().highlightedObject = gameObject;
        experiment.GetComponent<Experimentscript>().primaryButtonDown();
        //experiment.GetComponent<Experimentscript>().tm.text = "shape.hoverEntered";
    }
    public void hoverExited()
    {
        highlighted = false;
        //experiment.GetComponent<Experimentscript>().highlightedObject = null;
        //experiment.GetComponent<Experimentscript>().objectHoverExited(gameObject);
    }

    public void explode()
    {
        /*
        if (tag.Equals("star")) {
            transform.GetChild(0).gameObject.AddComponent<TriangleExplosion>();
            transform.GetChild(1).gameObject.AddComponent<TriangleExplosion>();
            StartCoroutine(transform.GetChild(0).gameObject.GetComponent<TriangleExplosion>().SplitMesh(false));
            StartCoroutine(transform.GetChild(1).gameObject.GetComponent<TriangleExplosion>().SplitMesh(false));
        } else {
        */
        gameObject.AddComponent<TriangleExplosion>();
        StartCoroutine(gameObject.GetComponent<TriangleExplosion>().SplitMesh(false));
        //}
    }

    public void initProbe()
    {
        transform.localScale += defaultToNormal;
    }

    public float getRotation()
    {
        return transform.eulerAngles.z;
    }

    public void setRotation(float newRot)
    {
        transform.eulerAngles = new Vector3(
            0f,
            0f,
            newRot
        );
        rotation = newRot;
    }

    public void SetCollidersStatus(bool active)
    {
        foreach (Collider c in GetComponentsInChildren<Collider>())
        {
            c.enabled = active;
        }
    }

}
