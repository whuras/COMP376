using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class Level2Director : MonoBehaviour
{
    // Scripts
    public GameObject MainRoadConductor;
    public GameObject FadeManager;

    // Animations
    public GameObject Bartender;

    // UI
    public GameObject MainPlayer;

    // Triggers
    public GameObject CutsceneTwoTrigger;

    private Conductor mConductor;
    
    void Start()
    {
        // Bartender Is Always Running In These Cutscenes
        Bartender.GetComponent<Animator>().SetBool("isRunning", true);
        
        // Disable Player HUD for Cutscenes
        MainPlayer.SetActive(false);
        
        mConductor = Conductor.GetActiveConductor();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ActivatePlayer()
    {
        MainPlayer.SetActive(true);
    }

    public void DeactivePlayer()
    {
        MainPlayer.SetActive(false);
    }

    public void ToggleCameraFade()
    {
        FadeManager.GetComponent<CameraFade>().FadeCamera();
    }
    
    public void ActivateBartender()
    {
        Bartender.SetActive(true);
    }

    public void DeactivateBartender()
    {
        Bartender.SetActive(false);
    }

    public void ToggleMainRoadConductorStart()
    {
        MainRoadConductor.SetActive(true);
        mConductor = Conductor.GetActiveConductor();
        MainRoadConductor.GetComponent<MainroadConductor>().StartConductor(((mConductor.GetBeat() / 4 + 1) * 4) + 13);
    }

    public void PauseTimeLine()
    {
        GetComponent<PlayableDirector>().Pause();
    }

    public void StartTimeLine()
    {
        GetComponent<PlayableDirector>().Play();
    }

    public void StopTimeLine()
    {
        GetComponent<PlayableDirector>().Stop();
    }
}
