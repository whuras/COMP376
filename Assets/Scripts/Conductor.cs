using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary> Handles game music and keeps track of beats. </summary>
public class Conductor : MonoBehaviour
{
    [Tooltip("Beats per minute of music track")]
    public float SourceBPM;

    AudioSource mMusicSource;
    float mSourceBPS;
    float mOffset;

    /// <summary> Get references to game objects, initialize settings, and start music. </summary>
    void Start()
    {
        mMusicSource = GetComponent<AudioSource>();
        mMusicSource.Play();

        mSourceBPS = SourceBPM / 60f;
        mOffset = (float) AudioSettings.dspTime;
    }

    /// <summary> Returns time to/from nearest beat normalized from -0.5 to 0.5. </summary>
    /// <returns> Time to/from nearest beat normalized from -0.5 to 0.5 </returns>
    public float GetTimeToBeat()
    {
        double scaledBeatTime = (AudioSettings.dspTime - mOffset) * mSourceBPS;
        int nearestBeat = (int) (scaledBeatTime + 0.5f);
        return (float) (scaledBeatTime - nearestBeat);
    }

    /// <summary> Returns time since last beat normalized from 0 to 1. </summary>
    /// <returns> Time since last beat normalized from 0 to 1 </returns>
    public float GetTimeSinceBeat()
    {
        double scaledBeatTime = (AudioSettings.dspTime - mOffset) * mSourceBPS;
        return (float) (scaledBeatTime - (int) scaledBeatTime);
    }
}