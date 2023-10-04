using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TomsClippingPlaneHandeler : MonoBehaviour
{
    [SerializeField] Material[] materials;

    [SerializeField] Transform plane;

    [SerializeField] bool RunEachFrame = true;

    // Start is called before the first frame update
    void Start()
    {
        if (plane == null)
        {
            plane = this.transform;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (RunEachFrame)
        {
            this.SetClippingPlaneOnAllMaterials();
        }
    }

    public void SetClippingPlaneOnAllMaterials(string NameInShader = "_ClippingPlane")
    {
        Vector3 up = transform.up;
        Vector4 clipPlane = new Vector4(up.x, up.y, up.z, Vector3.Dot(up, plane.position));

        for (int i = 0; i < materials.Length; i++)
        {
            materials[i].SetVector(NameInShader, clipPlane);
        }
    }
}
