using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    // UI
    public GameObject tutorialDialogue;
    public GameObject tipsText;
    public GameObject goalHUD;
    public Text goalText;

    // Bottles
    int bottleCount = 0;
    [SerializeField]
    int bottleGoal = 10;

    private void Start()
    {
        goalText.text = bottleCount.ToString() + "/" + bottleGoal.ToString() + " Whiskey Bottles";
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            tutorialDialogue.SetActive(false);
            tipsText.SetActive(true);
            goalHUD.SetActive(true);
        }

        if(bottleCount >= bottleGoal)
        {
            Debug.Log("Tutorial over."); // TODO
        }
    }

    public void UpdateBottleCount(int amount)
    {
        bottleCount += amount;
        goalText.text = bottleCount.ToString() + "/" + bottleGoal.ToString() + " Whiskey Bottles";
    }
}
