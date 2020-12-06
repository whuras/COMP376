using UnityEngine;

public class MusicFrame
{
    public string Name { get; }
    public float IntroStartTime { get; }
    public float LoopStartTime { get; }
    public float LoopEndTime { get; }
    
    // Whether a queued music frame will transition immediately (on beat) instead of waiting until the end of the bar
    public bool TransitionImmediately { get; }

    // Start is called before the first frame update
    public MusicFrame(string name, float introStartTime, float loopStartTime, float loopEndTime, bool transitionImmediately)
    {
        Name = name;
        IntroStartTime = introStartTime;
        LoopStartTime = loopStartTime;
        LoopEndTime = loopEndTime;
        TransitionImmediately = transitionImmediately;
    }
    
}
