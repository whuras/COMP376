using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class Level2Director : MonoBehaviour
{
    // Scripts
    public GameObject Conductor;
    public GameObject MainRoadConductor;
    public GameObject FadeManager;
    
    // Components
    public GameObject Director;

    // Animations
    public GameObject Bartender;

    // UI
    public GameObject MainPlayer;

    // Triggers
    public GameObject CutsceneTwoTrigger;

    // Positions
    Vector3 BartenderPos1 = new Vector3(-11.75f, 27.22f, 13.07f);
    Vector3 BartenderPos2;

    void Start()
    {
        // Bartender Is Always Running In These Cutscenes
        Bartender.GetComponent<Animator>().SetBool("isRunning", true);
        
        // Disable Player HUD for Cutscenes
        MainPlayer.SetActive(false);
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
        MainRoadConductor.GetComponent<MainroadConductor>().StartConductor((Conductor.GetComponent<Conductor>().GetBeat() / 4 + 1) * 4);
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
