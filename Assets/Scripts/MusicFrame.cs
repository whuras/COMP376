using UnityEngine;

public class MusicFrame
{
    public string Name { get; }
    public float IntroStartTime { get; }
    public float LoopStartTime { get; }
    public float LoopEndTime { get; }

    // Start is called before the first frame update
    public MusicFrame(string name, float introStartTime, float loopStartTime, float loopEndTime)
    {
        Name = name;
        IntroStartTime = introStartTime;
        LoopStartTime = loopStartTime;
        LoopEndTime = loopEndTime;
    }
    
}
