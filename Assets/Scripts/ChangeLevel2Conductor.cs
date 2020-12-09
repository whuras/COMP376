using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeLevel2Conductor : MonoBehaviour
{
    public Conductor MainConductor;
    public GameObject NewConductor;

    private void OnTriggerExit(Collider other)
    {
        NewConductor.SetActive(true);
        if (NewConductor.tag == "Marketplace")
        {
            NewConductor.GetComponent<MarketplaceConductor>().StartConductor((MainConductor.GetComponent<Conductor>().GetBeat() / 4 + 1) * 4);
        }
        else if (NewConductor.tag == "BehindMarketplace" || NewConductor.tag == "Canyon")
        {
            NewConductor.GetComponent<MainroadConductor>().StartConductor((MainConductor.GetComponent<Conductor>().GetBeat() / 4 + 1) * 4);
        }
        Destroy(gameObject);
    }
}
