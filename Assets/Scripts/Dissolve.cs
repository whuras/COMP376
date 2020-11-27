using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dissolve : MonoBehaviour
{
    [Tooltip("Renderer whose materials are to be dissolved")]
    public Renderer Renderer;
    [Tooltip("Material containing dissolve shader to apply")]
    public Material DissolveMaterial;

    MaterialPropertyBlock mMatBlock;
    Material[] mOriginalMaterials;
    Material[] mDissolveMaterials;
    
    void Start()
    {
        int numMaterials = Renderer.sharedMaterials.Length;
        mOriginalMaterials = Renderer.sharedMaterials;
        mDissolveMaterials = new Material[numMaterials];
        mMatBlock = new MaterialPropertyBlock();
        
        // Iterate over shared materials
        for (int i=0; i<numMaterials; i++)
        {
            mDissolveMaterials[i] = DissolveMaterial;
            mMatBlock.Clear();

            // Assign properties that need to be overwritten in dissolve shader
            mMatBlock.SetFloat("Vector1_3AA1D229", Renderer.sharedMaterials[i].GetFloat("_Glossiness"));
            mMatBlock.SetFloat("Vector1_844F0BC4", Renderer.sharedMaterials[i].GetFloat("_Metallic"));
            mMatBlock.SetColor("Color_A443451D", Renderer.sharedMaterials[i].GetColor("_Color"));
            Renderer.SetPropertyBlock(mMatBlock, i);
        }
    }
    
    public void SetDissolved(float progress)
    {        
        // Swap rendered materials for dissolve materials
        Renderer.sharedMaterials = mDissolveMaterials;
        
        // Iterate over property blocks and update dissolve progress
        int numMaterials = Renderer.sharedMaterials.Length;
        for (int i=0; i<numMaterials; i++)
        {
            Renderer.GetPropertyBlock(mMatBlock, i);
            mMatBlock.SetFloat("Vector1_65DF3157", progress);
            Renderer.SetPropertyBlock(mMatBlock, i);
        }
    }
}
