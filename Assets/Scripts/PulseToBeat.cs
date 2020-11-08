using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PulseToBeat : MonoBehaviour
{
    [Header("General")]
    [Tooltip("Amplitude of vertical position change")]
    public float BobAmplitude;
    [Tooltip("Amplitude of scale change")]
    public float ScaleAmplitude;

    Conductor mConductor;
    Vector3 mDefaultScale;
    Vector3 mDefaultPosition;

    /// <summary> Get default scale and position of object. </summary>
    void Start()
    {
        mConductor = FindObjectOfType<Conductor>();
        mDefaultScale = transform.localScale;
        mDefaultPosition = transform.localPosition;
    }

    /// <summary> Update position and scale according to time to beat. </summary>
    void Update()
    {
        float beatDistance = Mathf.Abs(2 * mConductor.GetTimeToBeat());
        
        Vector3 scale = mDefaultScale;
        scale.y *= (1+0.6f*ScaleAmplitude - ScaleAmplitude*beatDistance);
        scale.x *= (1-0.3f*ScaleAmplitude + ScaleAmplitude*beatDistance);
        scale.z *= (1+0.3f*ScaleAmplitude + ScaleAmplitude*beatDistance);
        transform.localScale = scale;

        Vector3 position = mDefaultPosition;
        position.y += BobAmplitude*(1f - beatDistance);
        transform.localPosition = position;
    }
}
