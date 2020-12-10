using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossFlames : MonoBehaviour
{
    public Vector3 upPos = new Vector3(0,3,0);
    public bool raiseFlames = false;
    public float movementTime = 2.0f;

    // Update is called once per frame
    void Update()
    {
        if(raiseFlames)
        {
            raiseFlames = false;
            StartCoroutine(MoveObject(gameObject, transform.position, upPos));
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
}
