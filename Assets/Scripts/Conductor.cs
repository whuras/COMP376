using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Conductor : MonoBehaviour
{
    public float SourceBPM;

    AudioSource mMusicSource;
    float mSourceBPS;
    double mSourceStartTime;

    void Start()
    {
        mMusicSource = GetComponent<AudioSource>();
        mMusicSource.Play();
        mSourceStartTime = AudioSettings.dspTime;
        mSourceBPS = SourceBPM / 60f;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log(GetTimeSinceBeat());
        }
    }
    
    public float GetTimeToBeat()
    {
        double scaledBeatTime = AudioSettings.dspTime * mSourceBPS;
        int nearestBeat = (int) (scaledBeatTime + 0.5f);
        return (float) (scaledBeatTime - nearestBeat);
    }
    
    public float GetTimeSinceBeat()
    {
        double scaledBeatTime = AudioSettings.dspTime * mSourceBPS;
        return (float) (scaledBeatTime - (int) scaledBeatTime);
    }
}
