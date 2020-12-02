using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary> Handles game music and keeps track of beats. </summary>
public class Conductor : MonoBehaviour
{
    [Tooltip("Beats per minute of music track")]
    public float SourceBPM;

    public int BarLength;

    public float TransitionBleed;
    
    AudioSource[] mMusicSources;
    float mSourceBPS;

    double mOffset;

    // Looping of the current MusicFrame will be disabled until AudioSettings.dspTime - mOffset >= mLoopDisabledUntil
    double mLoopDisabledUntil;

    private Queue<MusicFrame> MusicFrames;
    private MusicFrame mCurrentFrame;

    /// <summary> Get references to game objects, initialize settings, and start music. </summary>
    void Start()
    {    
        // Get the audio sources attached to the game object
        mMusicSources = GetComponents<AudioSource>();
        
        mSourceBPS = SourceBPM / 60f;

        // Setup music frame config
        MusicFrames = new Queue<MusicFrame>();
        
        //MusicFrames.Enqueue(new MusicFrame("Intro", 0,10F,15F));
        MusicFrames.Enqueue(new MusicFrame("Main Menu", 9.391F,9.391F,36.521F));
        MusicFrames.Enqueue(new MusicFrame("Level 1", 0.000F,82.434F,161.739F));
        MusicFrames.Enqueue(new MusicFrame("Level 2 Low Intensity", 136.695F,161.739F,270.260F));
        MusicFrames.Enqueue(new MusicFrame("Level 2 High Intensity", 270.260F,282.782F,351.652F));
        MusicFrames.Enqueue(new MusicFrame("Level 3 Low Intensity", 351.652F,355.826F,420.521F));
        MusicFrames.Enqueue(new MusicFrame("Level 3 High Intensity", 420.521F,441.391F,485.217F));
        MusicFrames.Enqueue(new MusicFrame("Credits", 485.217F,535.304F,566.000F));
        

        mCurrentFrame = MusicFrames.Dequeue();
        
        
        mOffset  = AudioSettings.dspTime + 0.5;
        
        // Setup the first music source to play
        mMusicSources[0].time = mCurrentFrame.IntroStartTime;
        mMusicSources[0].PlayScheduled(mOffset);

    }

    /// <summary> We track the time of the song ourselves instead of using AudioSettings.dspTime directly because the latter is not updated consistently and is not affected by time scale. </summary>
    void Update()
    {
        AudioSource currentlyPlaying = mMusicSources[0].isPlaying ? mMusicSources[0] : mMusicSources[1];

        HandleLoop();

        if (Input.GetKeyDown(KeyCode.T))
        {
            RequestTransition();
        }
        
        if (Input.GetKeyDown(KeyCode.F))
        {
            currentlyPlaying.time += 10;
            mOffset -= 10;
        }
        
        if (Input.GetKeyDown(KeyCode.R))
        {
            currentlyPlaying.time -= 10;
            mOffset += 10;
        }
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
    
    /// <summary> Returns index of the last played beat. </summary>
    /// <returns> Index of current beat </returns>
    public int GetBeat(int subdivision = 1)
    {
        double scaledBeatTime = (AudioSettings.dspTime - mOffset) * mSourceBPS * subdivision;
        return (int) scaledBeatTime;
    }

    public void RequestTransition()
    {
        HandleTransition();
    }

    private void HandleLoop()
    {
        if (AudioSettings.dspTime >= mLoopDisabledUntil)
        {
            AudioSource isPlaying = mMusicSources[0].isPlaying ? mMusicSources[0] : mMusicSources[1];
            AudioSource isStopped = mMusicSources[0].isPlaying ? mMusicSources[1] : mMusicSources[0];
            
            double loopAtTime = AudioSettings.dspTime + mCurrentFrame.LoopEndTime - isPlaying.time;
            
            isPlaying.SetScheduledEndTime(loopAtTime);
            isStopped.PlayScheduled(loopAtTime);
            isStopped.SetScheduledStartTime(loopAtTime);
            isStopped.time = mCurrentFrame.LoopStartTime;
            mLoopDisabledUntil = loopAtTime + 0.5;
            
            Debug.Log("loop setup!");
        }
    }
    
    private void HandleTransition()
    {
        AudioSource toPlay = mMusicSources[0].isPlaying ? mMusicSources[1] : mMusicSources[0];
        AudioSource toStop = mMusicSources[0].isPlaying ? mMusicSources[0] : mMusicSources[1];
        
        mCurrentFrame = MusicFrames.Dequeue();
        toPlay.time = mCurrentFrame.IntroStartTime;
        
        // Find the timestamp for the next bar end
        int beat = GetBeat() % BarLength;
        double timeBetweenBeats = 1 / mSourceBPS;
        double timeToNextBeat = timeBetweenBeats - (GetTimeSinceBeat() * timeBetweenBeats);

        double transitionTime = (BarLength - beat) * timeBetweenBeats + timeToNextBeat + AudioSettings.dspTime;
        
        Debug.Log("Switching to: " + mCurrentFrame.Name);
        Debug.Log("current beat: " + beat);
        Debug.Log("beat landed: " +( ((transitionTime - mOffset) * mSourceBPS) % 4));
        Debug.Log("current time: " + AudioSettings.dspTime);
        Debug.Log("transition time: " + transitionTime);

        mLoopDisabledUntil = transitionTime + 1F;
        
        toPlay.PlayScheduled(transitionTime);
        toPlay.SetScheduledStartTime(transitionTime);
        toStop.SetScheduledEndTime(transitionTime);
    }
}