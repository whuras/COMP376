using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeLevel2Conductor : MonoBehaviour
{
    public Conductor MainConductor;
    public GameObject NewConductor;
    public GameObject OldConductor;

    private void OnTriggerExit(Collider other)
    {
        OldConductor.SetActive(false);
        NewConductor.SetActive(true);
        NewConductor.GetComponent<MarketplaceConductor>().StartConductor((MainConductor.GetComponent<Conductor>().GetBeat()));
        Destroy(this.gameObject);
    }
}
