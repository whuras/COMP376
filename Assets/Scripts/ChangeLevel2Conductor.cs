using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeLevel2Conductor : MonoBehaviour
{
    public Conductor MainConductor;
    public GameObject NewConductor;

    void Start()
    {
        MainConductor = Conductor.GetActiveConductor();
    }
    
    private void OnTriggerExit(Collider other)
    {
        NewConductor.SetActive(true);
        if (NewConductor.tag == "Marketplace")
        {
            NewConductor.GetComponent<MarketplaceConductor>().StartConductor((MainConductor.GetBeat() / 4 + 1) * 4);
        }
        else if (NewConductor.tag == "BehindMarketplace" || NewConductor.tag == "Canyon")
        {
            NewConductor.GetComponent<MainroadConductor>().StartConductor((MainConductor.GetBeat() + 1));
        }
        Destroy(gameObject);
    }
}
