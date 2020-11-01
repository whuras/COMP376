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

    // Returns the normalized time to the nearest beat [-0.5,0.5]
    public float GetTimeToBeat()
    {
        double scaledBeatTime = AudioSettings.dspTime * mSourceBPS;
        int nearestBeat = (int) (scaledBeatTime + 0.5f);
        return (float) (scaledBeatTime - nearestBeat);
    }

    // Returns the time since the last beat [0,1[
    public float GetTimeSinceBeat()
    {
        double scaledBeatTime = AudioSettings.dspTime * mSourceBPS;
        return (float) (scaledBeatTime - (int) scaledBeatTime);
    }
}
