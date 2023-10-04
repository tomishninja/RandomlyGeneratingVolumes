using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class phongLightingHelper : MonoBehaviour
{
    [SerializeField] Material[] materialsToAffect;

    [SerializeField] Light sceneLighting;
    [SerializeField][Range(0, 1)] float shininess;
    [SerializeField][Range(0, 1)] float diffuse;
    [SerializeField][Range(0, 1)] float specular;

    [SerializeField] Camera camera;

    public void AddMaterial(Material mat)
    {
        if (mat != null)
        {
            Material[] newMaterials = new Material[materialsToAffect.Length + 1];
            for (int index = 0; index < materialsToAffect.Length; index++)
            {
                newMaterials[index] = materialsToAffect[index];
            }
            newMaterials[materialsToAffect.Length] = mat;
            materialsToAffect = newMaterials;
        }
    }

    void Start()
    {
        if (camera == null)
        {
            camera = Camera.main;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        for (int index = 0; index < materialsToAffect.Length; index++)
        {
            materialsToAffect[index].SetVector("_ViewDirection", -camera.transform.forward);
            materialsToAffect[index].SetVector("_LightDirection", -sceneLighting.transform.forward);
            materialsToAffect[index].SetVector("_LightColor", sceneLighting.color);
            materialsToAffect[index].SetFloat("_Shininess", shininess);
            materialsToAffect[index].SetFloat("_DiffuseColor", diffuse);
            materialsToAffect[index].SetFloat("_SpecularColor", specular);
        }
    }
}
