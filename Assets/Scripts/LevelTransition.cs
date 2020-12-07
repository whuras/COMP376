using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelTransition : MonoBehaviour
{
    public int SceneToLoad;

    private void OnTriggerEnter(Collider other)
    {
        SceneManager.LoadScene(SceneToLoad);
    }
}
