using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Playables;

public class TutorialManager : MonoBehaviour
{
    // UI
    public GameObject tipsText;
    public GameObject goalHUD;
    public Text goalText;
    public Text tipText;

    // Timeline
    public TimelineManager timelineManager;
    
    // Manipulated GameObjects
    public GameObject largeBottle;
    public GameObject bartender;
    public GameObject door;
    public GameObject levelTransition;

    // Objective Tracking
    [SerializeField]
    int bottleCount = 0;
    [SerializeField]
    int shotCount = 0;
    [SerializeField]
    int bottleGoal = 10;
    [SerializeField]
    int shotGoal = 5;

    // Tutorial Phase Falgs
    bool tutorialPhaseTwoStarted;
    bool tutorialPhaseThreeStarted;

    private void Start()
    {
        timelineManager = timelineManager.GetComponent<TimelineManager>();
        goalText.text = bottleCount.ToString() + "/" + bottleGoal.ToString() + " Whiskey Bottles";
        tipText.text = "Tip: Shoot to the beat wth the Left Mouse Button. Press \"R\" to Reload!";
        tutorialPhaseTwoStarted = false;
        largeBottle.SetActive(false);
        levelTransition.SetActive(false);
        bartender.GetComponent<Animator>().SetBool("isRunning", false);
    }

    void Update()
    {
        // Start the second cutscene
        if(bottleCount >= bottleGoal && !tutorialPhaseTwoStarted)
        {
            tutorialPhaseTwoStarted = true;
            timelineManager.StartTimeLine();
        }

        // Start the third cutscene
        if(shotCount >= shotGoal && !tutorialPhaseThreeStarted)
        {
            tutorialPhaseThreeStarted = true;
            timelineManager.StartTimeLine();
        }
    }

    // Setup the second phase of the tutorial
    public void StartPhaseTwo()
    {
        goalText.text = shotCount.ToString() + "/" + shotGoal.ToString() + " Consecutive Shots";
        tipText.text = "Tip: Shoot the large bottle to the beat consecutively to build a combo \n and increase your damage!";
        largeBottle.SetActive(true);
    }

    // Setup the third phase of the tutorial
    public void StartPhaseThree()
    {
        goalText.text = "Chase The Bartender!";
        tipText.text = "Tip: Double tap a movement key to dash!";
    }

    // Used by Bottle script
    public void UpdateShotCount(int amount)
    {
        if (shotCount + amount >= 0)
        {
            shotCount += amount;
            goalText.text = shotCount.ToString() + "/" + shotGoal.ToString() + " Consecutive Shots";
        }
    }

    // Used by Bottle script
    public void UpdateBottleCount(int amount)
    {
        bottleCount += amount;
        goalText.text = bottleCount.ToString() + "/" + bottleGoal.ToString() + " Whiskey Bottles";
    }

    // Used by timeline signals
    public void ToggleTutorialDialog()
    {
        tipsText.SetActive(true);
        goalHUD.SetActive(true);
    }

    // Used by timeline signals
    public void ToggleBartender()
    {
        bartender.SetActive(false);
    }

    // Used by timeline signals
    public void StartTimeline()
    {
        timelineManager.StartTimeLine();
    }

    // Used by timeline signals
    public void PauseTimeLine()
    {
        timelineManager.PauseTimeLine();
    }

    // Used by timeline signals
    public void OpenDoor()
    {
        door.transform.rotation = Quaternion.Euler(0.0f, 320f, 0.0f);
    }

    // Used by timeline signals
    public void SpawnLevelTransition()
    {
        levelTransition.SetActive(true);
    }

    // Used by Bottle script
    public bool GetTutorialPhaseTwoStarted()
    {
        return tutorialPhaseTwoStarted;
    }

    public void TransitionMusic()
    {
        Conductor c = Conductor.GetActiveConductor();
        c.RequestTransition();
    }
}
