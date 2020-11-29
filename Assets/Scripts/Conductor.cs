using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary> Handles game music and keeps track of beats. </summary>
public class Conductor : MonoBehaviour
{
    [Tooltip("Beats per minute of music track")]
    public float SourceBPM;

    public int BarLength;
    
    AudioSource mMusicSource;
    float mSourceBPS;

    double mOffset;
    double mCurrentTimeUnscaled = -1f;
    double mCurrentTimeScaled = 0f;
    float mSpeed = 1f;
    
    bool mTransitionRequested;
    private Queue<MusicFrame> MusicFrames;
    private MusicFrame mCurrentFrame;

    /// <summary> Get references to game objects, initialize settings, and start music. </summary>
    void Start()
    {
        mTransitionRequested = false;
        
        mMusicSource = GetComponent<AudioSource>();
        mMusicSource.Play();

        mSourceBPS = SourceBPM / 60f;
        mCurrentTimeUnscaled = AudioSettings.dspTime;
        mOffset = 0;
        
        MusicFrames = new Queue<MusicFrame>();
        
        //MusicFrames.Enqueue(new MusicFrame("Intro", 0,10F,15F));
        MusicFrames.Enqueue(new MusicFrame("Intro", 0,82.434F,161.739F));
        MusicFrames.Enqueue(new MusicFrame("Level 2 Low Intensity", 136.695F,161.739F,270.260F));
        MusicFrames.Enqueue(new MusicFrame("Level 2 High Intensity", 270.260F,270.260F,405.913F));
        MusicFrames.Enqueue(new MusicFrame("Level 3", 405.913F,426.782F,587.478F));

        mCurrentFrame = MusicFrames.Dequeue();

        mMusicSource.time = mCurrentFrame.IntroStartTime;
    }

    /// <summary> We track the time of the song ourselves instead of using AudioSettings.dspTime directly because the latter is not updated consistently and is not affected by time scale. </summary>
    void Update()
    {
        double deltaTime = AudioSettings.dspTime - mCurrentTimeUnscaled;
        deltaTime *= mSpeed;
        mCurrentTimeScaled += deltaTime;
        mCurrentTimeUnscaled = AudioSettings.dspTime;

        Debug.Log(mMusicSource.time);
        HandleLoop();

        if (Input.GetKeyDown(KeyCode.T))
        {
            RequestTransition();
        }
        
        if (mTransitionRequested)
        {
            HandleTransition();
        }
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

    public void RequestTransition()
    {
        mTransitionRequested = true;
    }

    private void HandleLoop()
    {
        if (mCurrentTimeScaled - mOffset >= mCurrentFrame.LoopEndTime - mCurrentFrame.LoopStartTime)
        {
            mMusicSource.time = mCurrentFrame.LoopStartTime;
            mOffset = mCurrentTimeScaled;
        }
    }
    
    private void HandleTransition()
    {
        // We want to transition at the end of a bar, but before the first beat of the next bar
        // If the next beat is the beginning of a new bar
        if (GetBeat() % BarLength == 0 && GetTimeSinceBeat() >= 0.95)
        {
            mCurrentFrame = MusicFrames.Dequeue();
            mMusicSource.time = mCurrentFrame.IntroStartTime;
            mOffset = mCurrentTimeScaled;
            mTransitionRequested = false;
        }
    }
}