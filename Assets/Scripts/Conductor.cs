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

    double mCurrentTimeUnscaled = -1f;
    double mCurrentTimeScaled = 0f;
    float mSpeed = 1f;

    /// <summary> Get references to game objects, initialize settings, and start music. </summary>
    void Start()
    {
        mMusicSource = GetComponent<AudioSource>();
        mMusicSource.Play();

        mSourceBPS = SourceBPM / 60f;
        mCurrentTimeUnscaled = AudioSettings.dspTime;
    }

    /// <summary> We track the time of the song ourselves instead of using AudioSettings.dspTime directly because the latter is not updated consistently and is not affected by time scale. </summary>
    void Update()
    {
        double deltaTime = AudioSettings.dspTime - mCurrentTimeUnscaled;
        deltaTime *= mSpeed;
        mCurrentTimeScaled += deltaTime;
        mCurrentTimeUnscaled = AudioSettings.dspTime;
    }

    /// <summary> Returns time to/from nearest beat normalized from -0.5 to 0.5. </summary>
    /// <returns> Time to/from nearest beat normalized from -0.5 to 0.5 </returns>
    public float GetTimeToBeat()
    {
        double scaledBeatTime = mCurrentTimeScaled * mSourceBPS;
        int nearestBeat = (int) (scaledBeatTime + 0.5f);
        return (float) (scaledBeatTime - nearestBeat);
    }

    /// <summary> Returns time since last beat normalized from 0 to 1. </summary>
    /// <returns> Time since last beat normalized from 0 to 1 </returns>
    public float GetTimeSinceBeat()
    {
        double scaledBeatTime = mCurrentTimeScaled * mSourceBPS;
        return (float) (scaledBeatTime - (int) scaledBeatTime);
    }

    /// <summary> Returns index of current beat. </summary>
    /// <returns> Index of current beat </returns>
    public int GetBeat(int subdivision = 1)
    {
        double scaledBeatTime = mCurrentTimeScaled * mSourceBPS * subdivision;
        return (int) scaledBeatTime;
    }

    /// <summary> Change BPM of music. </summary>
    /// <param name="speed"> Scale by which music speed should be changed relative to default. </param>
    public void SetSpeed(float speed)
    {
        mSpeed = speed;
        mMusicSource.pitch = speed;
        Time.timeScale = speed;
    }
}