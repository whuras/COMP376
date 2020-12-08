using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;

[System.Serializable]
public struct PostProcessingStates
{
    public Color mVignetteColor;
    public float mVignetteIntensity;
    public float mVignetteSmoothness;
    public float mColorContrast;
    public float mColorSaturation; 
}

public class PostProcessingController : MonoBehaviour
{
    [FormerlySerializedAs("mDamagedIntensity")] public PostProcessingStates[] DamagedIntensity;
    public float HurtFadeAwayTime;
    public float HurtDuration;

    private Volume mPostProcessing;
    private Vignette mVignette;
    private ColorAdjustments mColorAdjustments;
    
    private PostProcessingStates mDefaultProcessingState;
    private PostProcessingStates mCurrentProcessingState;
    private float mHurtEffectStart;

    private HealthController mHealthController;
    
    private int mHurtLevel;
    // Start is called before the first frame update
    void Start()
    {
        mPostProcessing = GetComponentInChildren<Volume>();

        mPostProcessing.profile.TryGet(out mVignette);
        mPostProcessing.profile.TryGet(out mColorAdjustments);

        mDefaultProcessingState = new PostProcessingStates
        {
            mVignetteColor = mVignette.color.value,
            mVignetteIntensity = mVignette.intensity.value,
            mVignetteSmoothness = mVignette.smoothness.value,
            mColorContrast = mColorAdjustments.contrast.value,
            mColorSaturation = mColorAdjustments.saturation.value
        };

        mCurrentProcessingState = mDefaultProcessingState;
        
        mHealthController = gameObject.GetComponentInParent<HealthController>();
        mHealthController.OnDamaged += IncreaseHurtIntensity;

        mHurtEffectStart = 3.40282347E+38F;
    }

    // Update is called once per frame
    void Update()
    {
        float currentTime = Time.time;
        if (currentTime > mHurtEffectStart + HurtDuration &&  currentTime <= mHurtEffectStart + HurtDuration + HurtFadeAwayTime)
        {
            float progress = (currentTime - mHurtEffectStart - HurtDuration) / HurtFadeAwayTime;
            Debug.Log(progress);
            SetProcessingState(LerpProcessingState(mCurrentProcessingState, mDefaultProcessingState, progress));
        }

        if (currentTime > mHurtEffectStart + HurtDuration + HurtFadeAwayTime)
        {
            mHurtLevel = 0;
            mHurtEffectStart = 3.40282347E+38F;
            mCurrentProcessingState = mDefaultProcessingState;
        }
    }

    public void IncreaseHurtIntensity()
    {
        mHurtEffectStart = Time.time;
        mCurrentProcessingState = DamagedIntensity[mHurtLevel];

        if (mHurtLevel < DamagedIntensity.Length - 1)
        {
            mHurtLevel++;
        }

        SetProcessingState(mCurrentProcessingState);
    }

    private PostProcessingStates LerpProcessingState(PostProcessingStates a, PostProcessingStates b, float progress)
    {
        ColorParameter vColor = new ColorParameter(a.mVignetteColor);
        vColor.Interp(a.mVignetteColor, b.mVignetteColor, progress);
        
        PostProcessingStates toReturn = new PostProcessingStates
        {
            mColorContrast = Mathf.Lerp(a.mColorContrast, b.mColorContrast, progress),
            mColorSaturation = Mathf.Lerp(a.mColorSaturation, b.mColorSaturation, progress),
            mVignetteColor = vColor.value,
            mVignetteIntensity = Mathf.Lerp(a.mVignetteIntensity, b.mVignetteIntensity, progress),
            mVignetteSmoothness = Mathf.Lerp(a.mVignetteSmoothness, b.mVignetteSmoothness, progress)
        };

        return toReturn;
    }
    
    private void SetProcessingState(PostProcessingStates s)
    {
        mVignette.color.Override(s.mVignetteColor);
        mVignette.intensity.Override(s.mVignetteIntensity);
        mVignette.smoothness.Override(s.mVignetteSmoothness);
        mColorAdjustments.contrast.Override(s.mColorContrast);
        mColorAdjustments.saturation.Override(s.mColorSaturation);
    }
}
