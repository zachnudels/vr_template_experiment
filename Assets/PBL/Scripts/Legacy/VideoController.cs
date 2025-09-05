using System.Collections;
using System.Collections.Generic;
using UnityEngine.Video;

using UnityEngine;

public class VideoController : MonoBehaviour
{

    public VideoClip[] videoClips;

    private VideoPlayer videoPlayer;
    private MeshRenderer mesh;

    public Material videoMaterial;
    public Material blankMaterial;

    private RectTransform textTransform;

    private bool hasStarted;


    // Use this for initialization
    void Start() {
    }

    // Update is called once per frame
    void Update() {
        //if (videoPlayer != null) {
            //if (hasStarted && CalculatePlayedFraction() > 0.99) {
                //Debug.Log("Blank material");
                //mesh.sharedMaterial = blankMaterial;
                //hasStarted = false;
            //}
        //}
    }

    public void Initialize() {
        mesh = GetComponentsInChildren<MeshRenderer>()[1];
        videoPlayer = GetComponentInChildren<VideoPlayer>();
        textTransform = GameObject.Find("ScreenText").GetComponent<RectTransform>();
        if ( textTransform == null) {
            Debug.Log("Text transfform is null");
        } else {
            Debug.Log(textTransform.position);
            Debug.Log(textTransform.sizeDelta);
        }
        videoPlayer.targetTexture.Release();
        mesh.sharedMaterial = blankMaterial;
    }

    public float PlayClip(int index) {
        textTransform.localPosition = new Vector3(0.44f, textTransform.localPosition.y, textTransform.localPosition.z);
        textTransform.sizeDelta = new Vector2(0.07f, 1f);

        videoPlayer.clip = videoClips[index];
        //hasStarted = true;
        mesh.sharedMaterial = videoMaterial;
        videoPlayer.Play();
        return (float)(1.0f * videoPlayer.clip.frameCount * (1.0f / videoPlayer.clip.frameRate));
    }

    public void StopClip() {
        textTransform.localPosition = new Vector3(-0.045f, textTransform.localPosition.y, textTransform.localPosition.z);
        textTransform.sizeDelta = new Vector2(0.57f, 1f);
        mesh.sharedMaterial = blankMaterial;
        videoPlayer.Stop();
    }


    double CalculatePlayedFraction() {
        double fraction = (double)videoPlayer.frame / (double)videoPlayer.clip.frameCount;
        return fraction;
    }


}
