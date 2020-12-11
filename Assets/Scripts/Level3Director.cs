using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class Level3Director : MonoBehaviour
{
    public GameObject Boss;

    public GameObject MainPlayer;

    public GameObject Credits;

    private AudioSource AudioSource;
    public AudioClip TadaSound;

    // Need a camera when player is deactivated (ie. during credits)
    public GameObject Camera;

    // Start is called before the first frame update
    void Start()
    {
        AudioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ActivateCreditScreen()
    {
        AudioSource.PlayOneShot(TadaSound, 1f);
        Credits.SetActive(true);
        Camera.SetActive(true);
    }

    public void DeactiateCreditsScreen()
    {
        Credits.SetActive(false);
    }

    public void ActivatePlayer()
    {
        MainPlayer.SetActive(true);
    }

    public void DeactivePlayer()
    {
        MainPlayer.SetActive(false);
    }

    public void ToggleDeathAnimation()
    {
        Boss.GetComponent<Animator>().SetBool("isDead", true);
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
