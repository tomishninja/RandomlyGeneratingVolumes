using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetermineMeshIntersection : MonoBehaviour
{
    public Vector2Int testPixel;

    public GameObject testObject;

    [SerializeField]
    private Vector2Int resolution;

    [SerializeField]
    private MeshCollider quadCollider;

    [SerializeField]
    private MeshCollider otherCollider;

    [SerializeField]
    private Material quadMataterial;

    [SerializeField]
    private Texture2D quadTexture;

    private DynamicTexture colorBuffer;


    // Start is called before the first frame update
    void Start()
    {
        // make the texture
        quadTexture = new Texture2D(resolution.x, resolution.y, TextureFormat.ARGB32, false)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Point,
            anisoLevel = 0
        };

        colorBuffer = new DynamicTexture(Color.white, resolution.x, resolution.y);
        colorBuffer.Set(Color.black, testPixel.x, testPixel.y);
        colorBuffer.ApplyToTexture(quadTexture);

        quadMataterial.SetTexture("_MainTex", quadTexture);

        Debug.Log(quadCollider.sharedMesh.vertexCount);
        //testObject.transform.position = this.transform.TransformPoint(quadCollider.sharedMesh.vertices[1]);
    }

    // Update is called once per frame
    void LateUpdate()
    {
        //colorBuffer.Set(Color.black, testPixel.x, testPixel.y);
        //colorBuffer.SetAll(Color.black);
        //colorBuffer = new DynamicTexture(Color.green, resolution.x, resolution.y);

        for (int y = 0; y < resolution.y; y++)
            for (int x = 0; x < resolution.x; x++)
                if (IsInsideOfObjectCheck.IsInCollider(otherCollider, FindWorldPosForPixel(x, y)))
                    colorBuffer.Set(Color.black, x, y);
                else
                    colorBuffer.Set(Color.white, x, y);

        colorBuffer.ApplyToTexture(quadTexture);

        //Vector3 start = FindWorldPosForPixel(testPixel.x, testPixel.y);
        //testObject.transform.position = start;
        // determine what the normal of the plane is
        //Vector3 pNorm = (Vector3.Cross(quadCollider.sharedMesh.vertices[1] - quadCollider.sharedMesh.vertices[0], quadCollider.sharedMesh.vertices[2] - quadCollider.sharedMesh.vertices[0])).normalized;
        
        //Debug.Log("is Inside: " + IsInsideOfObjectCheck.IsInCollider(otherCollider, start));
    }

    Vector3 FindWorldPosForPixel(int x, int y)
    {
        float h = ((float)x / (float)resolution.x) + ((0.5f / (float)resolution.x));
        float v = ((float)y / (float)resolution.y) + ((0.5f / (float)resolution.y));

        // Bilinar Lerp
        Vector3 cTop = Vector3.Lerp(quadCollider.sharedMesh.vertices[2], quadCollider.sharedMesh.vertices[3], h);
        Vector3 cBot = Vector3.Lerp(quadCollider.sharedMesh.vertices[0], quadCollider.sharedMesh.vertices[1], h);
        Vector3 final = Vector3.Lerp(cBot, cTop, v);

        return this.transform.TransformPoint(final);
    }
}

class DynamicTexture
{
    Color[] buffer;
    int width, height;

    public DynamicTexture(int xRes = 256, int yRes = 256)
    {
        buffer = new Color[xRes * yRes];
        width = xRes;
        height = yRes;
        SetAll(Color.clear);
    }

    public DynamicTexture(Color defaultColor, int xRes = 256, int yRes = 256)
    {
        buffer = new Color[xRes * yRes];
        width = xRes;
        height = yRes;
        SetAll(defaultColor);
    }

    public void SetAll(Color color)
    {
        for(int i = 0; i < buffer.Length; i++)
        {
            buffer[i] = color;
        }
    }

    public void Set(Color color, int x, int y)
    {
        this.buffer[(y * width) + x] = color;
    }

    public Color Get(int x, int y)
    {
        return this.buffer[(y * width) + x];
    }

    public void ApplyToTexture(Texture2D texture)
    {
        texture.SetPixels(buffer);
        texture.Apply();
    }
}
