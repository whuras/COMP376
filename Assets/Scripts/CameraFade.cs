using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFade : MonoBehaviour
{
    [Range(0, 5)]
    public int pauseBeforeFade;
    
    [Range(0, 10)]
    public float fadeTime;
    
    [HideInInspector]
    public Texture whtTexture;
    public Color fadeColor;

    float currentTime;
    Color colorLerp;
    bool canStartFade;

    // Start is called before the first frame update
    void Start()
    {
        colorLerp = fadeColor;
        StartCoroutine("StartCameraFade");
    }

    // Update is called once per frame
    void Update()
    {
        if (canStartFade)
        {
            currentTime += Time.deltaTime;
            colorLerp = Color.Lerp(fadeColor, Color.clear, currentTime / fadeTime);
        
            if (currentTime >= fadeTime)
            {
                FadeComplete();
            }
        }
    }

    public void FadeCamera()
    {
        print("in here!");
        colorLerp = fadeColor;
        gameObject.SetActive(true);
        enabled = true;
        canStartFade = false;
        StartCoroutine("StartCameraFade");
    }

    public void OnGUI()
    {
        GUI.color = colorLerp;
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), whtTexture);
    }

    public void FadeComplete()
    {
        enabled = false;
        gameObject.SetActive(false);
    }

    IEnumerator StartCameraFade()
    {
        yield return new WaitForSeconds(pauseBeforeFade);

        canStartFade = true;

        yield return null;
    }
}
