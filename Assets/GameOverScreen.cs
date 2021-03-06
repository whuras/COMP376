﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverScreen : MonoBehaviour
{
    public float TimeBeforeReset;
    public float FadeInTime;
    
    public Image Background;
    public Text[] TextObjects;

    private float mStartTime;
    private Conductor mConductor;

    // Start is called before the first frame update
    void Start()
    {
        mStartTime = Time.time;
        mConductor = Conductor.GetActiveConductor();
        mConductor.RequestDeathJingle();
        Destroy(Conductor.GetActiveConductorGameObject(), TimeBeforeReset);
        //disable player hud
    }

    // Update is called once per frame
    void Update()
    {
        float currentTime = Time.time;
        if (currentTime < mStartTime + FadeInTime)
        {
            ChangeOpacity((currentTime - mStartTime) / FadeInTime);
        }

        if (currentTime > mStartTime + TimeBeforeReset)
        {
            SceneManager.LoadScene("_Start");
        }
    }

    private void ChangeOpacity(float opacity)
    {
        Background.color = new Color(Background.color.r, Background.color.g, Background.color.b, opacity);
        foreach (Text t in TextObjects)
        {
            t.color = new Color(t.color.r, t.color.g, t.color.b, opacity);
        }
    }
}
