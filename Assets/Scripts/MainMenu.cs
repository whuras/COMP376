using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public GameObject MainMenuButtons;
    public GameObject OptionsScreen;
    
    private Conductor mConductor;
    private AudioSource mSelectionSFX;

    private Slider mVolumeSliderComponent;
    private void Start()
    {
        mSelectionSFX = GetComponent<AudioSource>();
        OptionsScreen.SetActive(false);
        mVolumeSliderComponent = OptionsScreen.GetComponentInChildren<Slider>();
        mVolumeSliderComponent.onValueChanged.AddListener(delegate { OnVolumeSliderChangedEvent(); });
        mConductor = Conductor.GetActiveConductor();
        Cursor.lockState = CursorLockMode.None;
    }

    public void PlayGame()
    {
        mSelectionSFX.Play();
        mConductor.RequestTransition();
        SceneManager.LoadScene(1);
        PlayerController.ResetScore();
    }

    public void OpenOptions()
    {
        mSelectionSFX.Play();
        MainMenuButtons.SetActive(false);
        OptionsScreen.SetActive(true);
    }

    public void CloseOptions()
    {
        mSelectionSFX.Play();
        MainMenuButtons.SetActive(true);
        OptionsScreen.SetActive(false);
    }

    public void QuitGame()
    {
        mSelectionSFX.Play();
        Application.Quit();
    }

    private void OnVolumeSliderChangedEvent()
    {
        mConductor.SetVolume(mVolumeSliderComponent.value);
    }
}
