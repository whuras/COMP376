using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class TimelineManager : MonoBehaviour
{
    PlayableDirector playableDirector;

    // Start is called before the first frame update
    void Start()
    {
        playableDirector = GetComponent<PlayableDirector>();
    }

    public void StartTimeLine()
    {
        playableDirector.Play();
    }

    public void PauseTimeLine()
    {
        playableDirector.Pause();
    }

    public void StopTimeLine()
    {
        playableDirector.Stop();
    }
}
