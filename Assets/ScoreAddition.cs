using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class ScoreAddition : MonoBehaviour
{
    public Vector3 initialVelocityMin;
    public Vector3 initialVelocityMax;

    // Total time the text is displayed on screen
    public float Lifetime;
    
    // how long it takes to fade out
    public float FadeOutDuration;
    
    
    private Text mText;
    private Rigidbody mRigidbody;

    private float lifetimeStartTime;
    private float fadeoutStartTime;
    
    // Start is called before the first frame update
    void Start()
    {
        mText = GetComponent<Text>();
        mRigidbody = GetComponent<Rigidbody>();

        mRigidbody.velocity = new Vector3(Random.Range(initialVelocityMin.x, initialVelocityMax.x), Random.Range(initialVelocityMin.y, initialVelocityMax.y), Random.Range(initialVelocityMin.z, initialVelocityMax.z));
        lifetimeStartTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        // We must start fading out here
        if (Time.time >= lifetimeStartTime + Lifetime - FadeOutDuration)
        {
            Color c = mText.color;
            c.a = 1 - (Time.time - lifetimeStartTime - (Lifetime - FadeOutDuration)) / FadeOutDuration;
            Debug.Log(c.a);
            mText.color = c;
        }

        if (Time.time >= lifetimeStartTime + Lifetime)
        {
            Destroy(gameObject);
        }
    }

    public void SetScore(int score)
    {
        mText = GetComponent<Text>();
        mText.text = $"+{score}";
    }
    private void FixedUpdate()
    {
        mRigidbody.AddForce(Physics.gravity * mRigidbody.mass * 100);
    }
}
