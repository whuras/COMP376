using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutSceneTrigger : MonoBehaviour
{
    public Level2Director director;

    bool mCutSceneHasTriggered;

    void OnStart()
    {
        mCutSceneHasTriggered = false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!mCutSceneHasTriggered)
        {
            mCutSceneHasTriggered = true;
            director.StartTimeLine();
        }
    }


}
