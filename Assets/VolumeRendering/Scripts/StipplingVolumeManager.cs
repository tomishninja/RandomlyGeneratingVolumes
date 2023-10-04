﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StipplingVolumeManager : MonoBehaviour
{
    #region volume

    [SerializeField] protected Shader shader;
    protected Material material;

    public enum TypeOfStipple
    {
        Original,
        Test
    }

    [SerializeField] TypeOfStipple Algorithm;

    [Range(0f, 1f)] public float threshold = 0.5f;
    [Range(0.5f, 5f)] public float intensity = 1.5f;
    [Range(0f, 1f)] public float sliceXMin = 0.0f, sliceXMax = 1.0f;
    [Range(0f, 1f)] public float sliceYMin = 0.0f, sliceYMax = 1.0f;
    [Range(0f, 1f)] public float sliceZMin = 0.0f, sliceZMax = 1.0f;
    public Quaternion axis = Quaternion.identity;

    public Texture volume = null;

    private bool hasStarted = false;

    public uint min, max;

    [Range(0f, 1f)] public float edgeTolerance = 0.5f;
    [Range(0f, 1f)] public float edgeStippleRate = 0.5f;

    [SerializeField]
    DicomGrid dataObject = new DicomGrid();

    #endregion

    #region fileReading

    public enum TypeofVolume { PVM, DCM, SDF_Sphere };
    public TypeofVolume typeofVolume;

    [SerializeField]
    private string file = "TestDicomData";

    #endregion

    public DicomGrid volumeInfo { get => this.dataObject; }

    [SerializeField]
    StippllingColorPicker[] Colors;

    // Start is called before the first frame update
    void Awake()
    {
        TextAsset mytxtData = (TextAsset)Resources.Load(file);
        if (typeofVolume == TypeofVolume.DCM)
        {
            // logic for a dicom
            dataObject = JsonUtility.FromJson<DicomGrid>(mytxtData.text);
        }
        else if (typeofVolume == TypeofVolume.PVM)
        {
            PVMData temp = JsonUtility.FromJson<PVMData>(mytxtData.text);

            dataObject.buffer = temp.data;
            dataObject.width = temp.width;
            dataObject.height = temp.height;
            dataObject.breath = temp.breath;
        }
    }

    protected virtual void Start()
    {
        if (volume == null)
        {
            switch (Algorithm)
            {
                case TypeOfStipple.Original:
                    this.volume = this.CreateOriginalStippleTexture(min, max, 255, 0.1f);
                    break;
                case TypeOfStipple.Test:
                    this.volume = this.CreateStippleTexture(min, max, 255, 0.1f);
                    break;
                default:
                    this.volume = this.CreateOriginalStippleTexture(min, max, 255, 0.1f);
                    Debug.Log("Picked a option that didn't exist original used");
                    break;
            }
        }

        // if there is a volume then start to render the object
        if (volume != null)
        {
            this.StartMethod();
        }
        else
        {
            Debug.Log("Volume == null");
        }
    }

    private void StartMethod()
    {
        material = new Material(shader);
        material.renderQueue = 3000;
        GetComponent<MeshFilter>().sharedMesh = Build();
        GetComponent<MeshRenderer>().sharedMaterial = material;
        hasStarted = true;
    }

    protected void LateUpdate()
    {
        // if there is a volume then start to render the object
        if (volume != null && !hasStarted)
            this.StartMethod();

        // if the volume didn't create then don't do the rest
        if (!hasStarted) return;

        // get going on with the normal stuff for each frame
        material.SetTexture("_Volume", volume);
        material.SetFloat("_Threshold", threshold);
        material.SetFloat("_Intensity", intensity);
        material.SetVector("_SliceMin", new Vector3(sliceXMin, sliceYMin, sliceZMin));
        material.SetVector("_SliceMax", new Vector3(sliceXMax, sliceYMax, sliceZMax));
        material.SetMatrix("_AxisRotationMatrix", Matrix4x4.Rotate(axis));
    }

    Mesh Build()
    {
        // the verties ofor the final cube
        var vertices = new Vector3[] {
                new Vector3 (-0.5f, -0.5f, -0.5f),
                new Vector3 ( 0.5f, -0.5f, -0.5f),
                new Vector3 ( 0.5f,  0.5f, -0.5f),
                new Vector3 (-0.5f,  0.5f, -0.5f),
                new Vector3 (-0.5f,  0.5f,  0.5f),
                new Vector3 ( 0.5f,  0.5f,  0.5f),
                new Vector3 ( 0.5f, -0.5f,  0.5f),
                new Vector3 (-0.5f, -0.5f,  0.5f),
            };

        // The mesh that contains the rendering.
        var triangles = new int[] {
                0, 2, 1,
                0, 3, 2,
                2, 3, 4,
                2, 4, 5,
                1, 2, 5,
                1, 5, 6,
                0, 7, 4,
                0, 4, 3,
                5, 4, 7,
                5, 7, 6,
                0, 6, 7,
                0, 1, 6
            };

        // constuct the cube the volume will be stored in
        var mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.hideFlags = HideFlags.HideAndDontSave;
        return mesh;
    }

    public Texture3D CreateStippleTexture(uint minThreashold, uint maxThreashold, int resulition, float OddsOfStipple = 1f)
    {
        if (OddsOfStipple > 1) OddsOfStipple = 1;
        else if (OddsOfStipple < 0) OddsOfStipple = 0;

        Texture3D texture = new Texture3D(resulition, resulition, resulition, TextureFormat.ARGB32, false)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Point,
            anisoLevel = 0
        };

        // smooth the volume so no werid artifacts appear. 
        volumeInfo.GaussianSmoothing(3, 1.4f);

        int length = resulition * resulition * resulition;
        Color32[] colors = new Color32[length];

        for (int index = 0; index < length; index++)
        {
            // Get the position
            //int pos = new Vector3Int(index % height, (index / width) % height, index / (width * height));
            Vector3Int pos = new Vector3Int(index % resulition, (index / resulition) % resulition, index / (resulition * resulition));

            // get the pos as a percentage
            Vector3 percentage = new Vector3((float)pos.x / (float)resulition, (float)pos.y / (float)resulition, (float)pos.z / (float)resulition);
            pos = volumeInfo.GetFromPercentage(percentage);

            // get the point of the data
            uint dataAt = volumeInfo.Get(pos);

            int i = 0;
            if (Colors[i].density > dataAt)
            {
                colors[index] = Color.clear;
                continue;
            }

            // work out when 
            while (i < Colors.Length - 1 &&
                !(Colors[i].density < dataAt && Colors[i + 1].density > dataAt)
                )
            {
                i++;
            }

            // TODO create a 3D Discrete Laplacian operator  // edge detetion

            // choose a color if it gets picked
            //float chance = ((float)((int)Colors[i].intensity - (int)minThreashold) / (float)(maxThreashold - minThreashold)) * OddsOfStipple;
            //float chance = Colors[i].intensity;
            //if (Random.Range(0.0f, 1f) < chance)
            if (Random.Range(0.0f, 1f) < Colors[i].intensity)
            {
                colors[index] = Colors[i].color;
            }
            else
            {
                colors[index] = Color.clear;
            }
        }

        // set the information we created before
        texture.SetPixels32(colors, 0);
        texture.Apply();

        return texture;
    }

    public Texture3D CreateOriginalStippleTexture(uint minThreashold, uint maxThreashold, int resulition, float OddsOfStipple = 1f)
    {
        if (OddsOfStipple > 1) OddsOfStipple = 1;
        else if (OddsOfStipple < 0) OddsOfStipple = 0;

        Texture3D texture = new Texture3D(resulition, resulition, resulition, TextureFormat.ARGB32, false)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Point,
            anisoLevel = 0
        };

        // smooth the volume so no werid artifacts appear. 
        //volumeInfo.GaussianSmoothing(3, 1.4f);

        // get the sobel information
        float[] edgeBuffer = volumeInfo.GetSobelBuffer();
        //Debug.Log(edgeBuffer.Length + " : " + volumeInfo.Count());

        int length = resulition * resulition * resulition;
        Color32[] colors = new Color32[length];
        
        for (int index = 0; index < length; index++)
        {
            // Get the position
            Vector3Int pos = new Vector3Int(index % resulition, (index / resulition) % resulition, index / (resulition * resulition));

            // get the pos as a percentage
            Vector3 percentage = new Vector3(
                Mathf.Clamp((float)pos.x / (float)resulition, 0f, 1f),
                Mathf.Clamp((float)pos.y / (float)resulition, 0f, 1f),
                Mathf.Clamp((float)pos.z / (float)resulition, 0f, 1f));

            pos = volumeInfo.GetFromPercentage(percentage);

            float edge = edgeBuffer[volumeInfo.GetIndexSafe(pos)];
            if (float.IsNaN(edge))
                edge = 0;
            if (edge > edgeTolerance)
                edge = this.edgeStippleRate;
            else
                edge = 0;


            // get the point of the data
            uint dataAt = volumeInfo.Get(pos);

            int i = 0;
            // if it is less the the minium write that
            if (Colors[i].density > dataAt)
            {
                colors[index] = Color.clear;
                continue;
            }

            // work out when 
            while (i < Colors.Length -1 && !(Colors[i].density < dataAt && Colors[i + 1].density > dataAt))
            {
                i++;
            }

            // choose a color if it gets picked
            //float chance = ((float)((int)Colors[i].intensity - (int)minThreashold) / (float)(maxThreashold - minThreashold)) * OddsOfStipple;
            //float chance = Colors[i].intensity;
            //if (Random.Range(0.0f, 1f) < chance)

            float value;
            switch (Colors[i].AlgorihtmToUse)
            {
                case StippllingColorPicker.Style.IntensityOnly:
                    value = Colors[i].intensity;
                    break;
                case StippllingColorPicker.Style.EdgePlusIntensity:
                    value = Colors[i].intensity + edge;
                    break;
                case StippllingColorPicker.Style.EdgeValueOnly:
                    value = edge;
                    break;
                case StippllingColorPicker.Style.EdegeMultipliedByIntensity:
                    value = Colors[i].intensity * edge;
                    break;
                default:
                    value = Colors[i].intensity;
                    break;
            }

            if (Random.Range(0.0f, 1f) < value)
            {
                colors[index] = Colors[i].color;
            }
            else
            {
                colors[index] = Color.clear;
            }
        }

        // set the information we created before
        texture.SetPixels32(colors, 0);
        texture.Apply();

        return texture;
    }

    class AABBInt
    {
        public Vector3Int min;
        public Vector3Int max;

        AABBInt()
        {
            this.min = Vector3Int.zero;
            this.max = Vector3Int.zero;
        }

        AABBInt(Vector3Int max)
        {
            this.min = Vector3Int.zero;
            this.max = max;
        }

        AABBInt(Vector3Int min, Vector3Int max)
        {
            this.min = min;
            this.max = max;
        }
    }
}
