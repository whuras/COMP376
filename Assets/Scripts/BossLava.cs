using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossLava : MonoBehaviour
{
    [Header("Lava Parameters")]
    public float lavaDamage = 5.0f;
    public float damageInterval = 1.0f; // time between damage ticks
    private float secondsSinceLastDamage = 0.0f;
    public float lavaTimer = 15f; // time between laval rises
    public float timer = 0.0f;
    public float movementTime = 5.0f; // time for the lava to rise

    public List<GameObject> sinkSections = new List<GameObject>(); // list of game objects to be sunk
    private List<Vector3> sinkSectionsDownPos = new List<Vector3>();
    private List<Vector3> sinkSectionsUpPos = new List<Vector3>();
    private int nextSink = 0;

    private bool isUp = false;
    private Vector3 downPos;
    private Vector3 upPos = new Vector3(0, 7.0f, 0);

    // Start is called before the first frame update
    void Start()
    {
        downPos = transform.position;

        for (int i = 0; i < sinkSections.Count; i++)
        {
            sinkSectionsUpPos.Add(sinkSections[i].transform.position);
            sinkSectionsDownPos.Add(new Vector3(sinkSectionsUpPos[i].x, sinkSectionsUpPos[i].y - 10, sinkSectionsUpPos[i].z));
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(nextSink < 4)
        {
            timer += Time.deltaTime;

            if (timer >= lavaTimer && !isUp)
            {
                StartCoroutine(MoveObject(gameObject, downPos, upPos));
                timer = 0.0f;
                isUp = !isUp;
            }
            else if (timer >= lavaTimer && isUp)
            {
                StartCoroutine(MoveObject(gameObject, upPos, downPos));
                StartCoroutine(MoveObject(sinkSections[nextSink], sinkSectionsUpPos[nextSink], sinkSectionsDownPos[nextSink]));
                nextSink += 1;
                timer = 0.0f;
                isUp = !isUp;
            }
        }
        
    }

    IEnumerator MoveObject(GameObject obj, Vector3 startPos, Vector3 endPos)
    {
        
        float elapsedTime = 0.0f;

        while (elapsedTime < movementTime)
        {
            obj.transform.position = Vector3.Lerp(startPos, endPos, (elapsedTime / movementTime));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        obj.transform.position = endPos;
        yield return null;
    }

    private void OnTriggerStay(Collider other)
    {
        secondsSinceLastDamage += Time.deltaTime;
        if (other.GetComponent<HealthController>())
        {
            ApplyDamage(other.GetComponent<HealthController>());
        }
    }

    private void ApplyDamage(HealthController target)
    {
        if (secondsSinceLastDamage > damageInterval)
        {
            Debug.Log("Taking lava dmg.");
            target.TakeDamage(lavaDamage);
            secondsSinceLastDamage = 0;
        }
    }
    
}
