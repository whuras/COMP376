using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelTransition : MonoBehaviour
{
    public int SceneToLoad;
    public bool TransitionMusic;
    private void OnTriggerEnter(Collider other)
    {
        if (TransitionMusic)
        {
            Conductor.GetActiveConductor().RequestTransition();
        }
        SceneManager.LoadScene(SceneToLoad);
    }
}
