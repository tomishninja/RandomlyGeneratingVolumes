using System.Diagnostics;
using System.Reflection;
using UnityEngine;

public class VaringIntensityColorVolumeRendering : MonoBehaviour
{
    const int MaxAmountOfColors = 4;

    enum TypeOfNormalsToRender
    {
        none = 0,
        depthBased = 1,
        convolutional = 2,
        convoluationalSobel = 3
    }

    #region volume

    [SerializeField] protected Shader[] shaders;
    protected Material[] materials;

    //[SerializeField] VolumetricColorPicker color;
    // this just exists for an easy way to navigate the editor 
    [SerializeField] VolumetricColorPicker startColor;
    [SerializeField] VolumetricColorAndIntensityPicker[] colors = new VolumetricColorAndIntensityPicker[4];
    [SerializeField] VolumetricColorPicker endColor;


    [Range(0f, 1f)] public float EdgeThreashold = 0.5f;
    [Range(0f, 1f)] public float threshold = 0.5f;
    [Range(0f, 1f)] public float sliceXMin = 0.0f, sliceXMax = 1.0f;
    [Range(0f, 1f)] public float sliceYMin = 0.0f, sliceYMax = 1.0f;
    [Range(0f, 1f)] public float sliceZMin = 0.0f, sliceZMax = 1.0f;
    public Quaternion axis = Quaternion.identity;

    public Texture volume;
    public Texture normalMap;

    private bool hasStarted = false;
    [SerializeField] bool createNormalTexture = false;

    public int high;
    public int low;

    private Color[] _colors = new Color[MaxAmountOfColors + 2];
    private float[] _density = new float[MaxAmountOfColors + 2];
    private float[] _intensity = new float[MaxAmountOfColors + 2];

    [SerializeField]
    DicomGrid dataObject = new DicomGrid();

    #endregion

    #region fileReading

    public enum TypeofVolume { PVM, DCM };
    public TypeofVolume typeofVolume;

    [SerializeField]
    private string file = "TestDicomData";

    #endregion

    public DicomGrid volumeInfo { get => this.dataObject; }

    public uint minSegmention = 10;
    //public bool RunDepthBasedCheck = false;

    [SerializeField] TypeOfNormalsToRender currentRun = TypeOfNormalsToRender.none;

    [Header("Texture Settings")]

    [SerializeField] FilterMode TextureFiltering = FilterMode.Point;

    [SerializeField] int RenderQueue = 3000;

    [Header("sdfInfo")]
    public CollectDataForSDFInfo sdfInfo;
    public bool renderClyinder = false;

    #region Smoothing Varibles

    enum TypeOfSmoothingToApply
    {
        Nothing,
        Average,
        Gaussian
    }

    [Header("Smoothing")]
    [SerializeField]
    TypeOfSmoothingToApply typeOfSmoothingToApply = TypeOfSmoothingToApply.Nothing;

    [SerializeField]
    int kernalSize = 3;

    [SerializeField]
    float sigma = 1;

    #endregion

    [Header("Multiple Material Behavious")]
    public float TimeBetweenIntervals = 20;
    private float LastTimeChanged = 0;
    private int materialIndex = 0;

    [Header("Optional Settings")]
    [SerializeField] phongLightingHelper _phonglightingHelper;
    [SerializeField] UserTrackingSystemForHatchingScript _hatchingTrackingScript;
    [SerializeField] SetUpStipplingMat StipplingHelper;

    // Start is called before the first frame update
    void Awake()
    {
        TextAsset mytxtData = (TextAsset)Resources.Load(file);
        
        if (mytxtData == null)
        {
            UnityEngine.Debug.Log("No Text Asset Found");
        }
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

        // make sure the kernal size is uneven
        if (this.kernalSize % 2 == 0) this.kernalSize++;
        switch (typeOfSmoothingToApply)
        {
            case TypeOfSmoothingToApply.Average:
                // smooth this object
                this.dataObject.AverageSmoothing(this.kernalSize);
                break;
            case TypeOfSmoothingToApply.Gaussian:
                // smooth object
                this.dataObject.GaussianSmoothing(this.kernalSize, sigma);
                break;
            default:
                break;
        }

        //if (this.RunDepthBasedCheck && !volumeInfo.Segmented)
        switch(currentRun)
        {
            case TypeOfNormalsToRender.depthBased:
                volumeInfo.SegmentDicom(minSegmention);
                break;
        }
    }

    protected virtual void Start()
    {
        volume = GetAsTexture3DOneByte(low, high);
        switch (currentRun)
        {
            case TypeOfNormalsToRender.convolutional:
                normalMap = CreateConvolutional3DTexture();
                break;
            case TypeOfNormalsToRender.convoluationalSobel:
                normalMap = CreateNormalTexture(low, high);
                break;
        }

        // set the colors in the array
        SetColorArray();

        // if there is a volume then start to render the object
        if (volume != null)
        {
            this.StartMethod();

            for(int index = 0; index < materials.Length; index++)
            {
                if (_phonglightingHelper != null)
                {
                    _phonglightingHelper.AddMaterial(materials[index]);
                }

                if (_hatchingTrackingScript != null)
                {
                    _hatchingTrackingScript.AddMat(materials[index]);
                }

                if (StipplingHelper != null)
                {
                    StipplingHelper.SetUpMat(materials[index]);
                }
            }
        }
    }

    private void Update()
    {
        if (Time.time - this.LastTimeChanged > this.TimeBetweenIntervals)
        {
            this.LastTimeChanged = Time.time;

            this.materialIndex = (this.materialIndex + 1) % this.materials.Length;

            if (materials.Length > 0)
            {
                GetComponent<MeshRenderer>().sharedMaterial = materials[this.materialIndex];
            }
        }
    }

    public Texture3D CreateConvolutional3DTexture()
    {
        int index = 0;
        Color32[] normalInformation = new Color32[dataObject.width * dataObject.height * dataObject.breath];

        for (int z = 0; z < dataObject.breath; z++)
        {
            for (int y = 0; y < dataObject.height; y++)
            {
                for (int x = 0; x < dataObject.width; x++)
                {
                    Vector3 normalizedGradient = ComputeGradient(x, y, z).normalized;

                    normalInformation[index] = new Color32(System.Convert.ToByte(normalizedGradient.x * 255), System.Convert.ToByte(normalizedGradient.y * 255), System.Convert.ToByte(normalizedGradient.z * 255), 0);
                    index++;
                }
            }
        }

        for (int i = 0; i < 100; i++)
        {
            // find a random object to slelect
            int x = Random.Range(0, dataObject.width);
            int y = Random.Range(0, dataObject.height);
            int z = Random.Range(0, dataObject.breath);
        }

        Texture3D output = new Texture3D(dataObject.width, dataObject.height, dataObject.breath, TextureFormat.RGBA32, false)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Point,
            anisoLevel = 0
        };
        output.SetPixels32(normalInformation);
        output.Apply();
        return output;
    }

    public Texture3D CreateConolutional3DTextureWithNormalizedMagntiude()
    {
        int index = 0;
        float minMagnatude = float.MaxValue;
        float maxMagnatude = float.MinValue;
        Color[] normalInformation = new Color[dataObject.width * dataObject.height * dataObject.breath];

        for (int z = 0; z < dataObject.breath; z++)
        {
            for (int y = 0; y < dataObject.height; y++)
            {
                for (int x = 0; x < dataObject.width; x++)
                {
                    Vector3 currentGradient = ComputeGradient(x, y, z);

                    minMagnatude = Mathf.Min(minMagnatude, currentGradient.magnitude);
                    maxMagnatude = Mathf.Max(maxMagnatude, currentGradient.magnitude);
                    index++;
                }
            }
        }

        index = 0;
        for (int z = 0; z < dataObject.breath; z++)
        {
            for (int y = 0; y < dataObject.height; y++)
            {
                for (int x = 0; x < dataObject.width; x++)
                {
                    Vector3 currentGradient = ComputeGradient(x, y, z);
                    Vector3 normalizedGradient = currentGradient.normalized;

                    minMagnatude = Mathf.Min(minMagnatude, currentGradient.magnitude);
                    maxMagnatude = Mathf.Max(maxMagnatude, currentGradient.magnitude);

                    normalInformation[index] = new Color(normalizedGradient.x, normalizedGradient.y, normalizedGradient.z);
                    index++;
                }
            }
        }

        Texture3D output = new Texture3D(dataObject.width, dataObject.height, dataObject.breath, TextureFormat.RGBAFloat, false);
        output.SetPixels(normalInformation);
        output.Apply();
        return output;
    }

    private Vector3 ComputeGradient(int x, int y, int z)
    {
        // If the voxel coordinates are out of bounds, assume a value of 0
        if (x < 0 || x >= dataObject.width || y < 0 || y >= dataObject.height || z < 0 || z >= dataObject.breath)
        {
            return Vector3.zero;
        }

        // Compute gradients using central differences
        float dx = 0.5f * (dataObject.Get(x + 1, y, z) - dataObject.Get(x - 1, y, z)); //(volume[z, y, x + 1] - volume[z, y, x - 1]);
        float dy = 0.5f * (dataObject.Get(x, y + 1, z) - dataObject.Get(x, y - 1, z)); //(volume[z, y + 1, x] - volume[z, y - 1, x]);
        float dz = 0.5f * (dataObject.Get(x, y, z + 1) - dataObject.Get(x, y, z - 1)); //(volume[z + 1, y, x] - volume[z - 1, y, x]);

        return new Vector3(dx, dy, dz);
    }

    static public int Partition(VolumetricColorAndIntensityPicker[] arr, int left, int right)
    {
        float pivot;
        pivot = arr[left].density;
        int count = 0;
        while (count < arr.Length * arr.Length) // it needs the stop eventually
        {
            while (arr[left].density < pivot)
            {
                left++;
            }
            while (arr[right].density > pivot)
            {
                right--;
            }

            // we found the spot add it
            if (left < right)
            {
                VolumetricColorAndIntensityPicker temp = arr[right];
                arr[right] = arr[left];
                arr[left] = temp;
            }
            else
            {
                return right;
            }
            count++;
        }
        return 0;
    }

    private void QuickSort(VolumetricColorAndIntensityPicker[] arr, int left, int right)
    {
        int pivot;
        if (left < right)
        {
            pivot = Partition(arr, left, right);
            if (pivot > 1)
            {
                QuickSort(arr, left, pivot - 1);
            }
            
            if (pivot + 1 < right)
            {
                QuickSort(arr, pivot + 1, right);
            }
        }
    }

    private void StartMethod()
    {
        if (shaders != null)
        {
            materials = new Material[shaders.Length];

            for (int index = 0; index < shaders.Length; index++)
            {
                materials[index] = new Material(shaders[index]);
                materials[index].renderQueue = RenderQueue;
                hasStarted = true;
            }

            if (materials.Length > 0)
            {
                GetComponent<MeshFilter>().sharedMesh = Build();
                GetComponent<MeshRenderer>().sharedMaterial = materials[this.materialIndex];
            }
        }
    }

    private void SetColorArray()
    {
        QuickSort(colors, 0, colors.Length - 1);

        int loopend = colors.Length;
        if (loopend > MaxAmountOfColors)
        {
            loopend = MaxAmountOfColors;
        }

        _colors[0] = startColor.color;
        _density[0] = startColor.density;
        _intensity[0] = 0;

        // add all of the color values in to the list
        for (int index = 0; index < loopend; index++)
        {
            _colors[index + 1] = colors[index].color;
            _density[index + 1] = colors[index].density;
            _intensity[index + 1] = colors[index].intensity;
        }

        // if the array isn't 1 then just add some default values on to the end
        for (int index = colors.Length + 1; index < _colors.Length; index++)
        {
            _colors[index] = endColor.color;
            _density[index] = endColor.density;
            _intensity[index] = 1;
        }

    }

    protected void LateUpdate()
    {
        // if there is a volume then start to render the object
        if (volume != null && !hasStarted)
            this.StartMethod();

        // if the volume didn't create then don't do the rest
        if (!hasStarted) return;


        // set the colors in the array
        SetColorArray();

        for(int index = 0; index < materials.Length; index++)
        {
            // get going on with the normal stuff for each frame
            materials[index].SetTexture("_Volume", volume);
            materials[index].SetTexture("_NormalMap", normalMap);
            materials[index].SetColorArray("_Colors", _colors);
            materials[index].SetFloatArray("_Density", _density);
            materials[index].SetFloatArray("_Intensity", _intensity);
            materials[index].SetFloat("_Threshold", threshold);
            materials[index].SetVector("_SliceMin", new Vector3(sliceXMin, sliceYMin, sliceZMin));
            materials[index].SetVector("_SliceMax", new Vector3(sliceXMax, sliceYMax, sliceZMax));
            materials[index].SetMatrix("_AxisRotationMatrix", Matrix4x4.Rotate(axis));
            materials[index].SetVector("aabbMin", this.volumeInfo.minAABB);
            materials[index].SetVector("aabbMax", this.volumeInfo.maxAABB);
            materials[index].SetFloat("_MaxDimention", this.volumeInfo.width * 2); // TODO change this
            materials[index].SetFloat("_EdgeThreshold", this.EdgeThreashold);
        }
        

        // program in the SDF's
        SDFEclipeData[] sdfData;
        if (sdfInfo != null)
        {
            sdfData = sdfInfo.GetAsArray();

            Vector4[] positions = new Vector4[sdfData.Length];
            Vector4[] radius = new Vector4[sdfData.Length];
            Color[] colors = new Color[sdfData.Length];

            for (int index = 0; index < sdfData.Length; index++)
            {
                positions[index] = sdfData[index].position;
                radius[index] = sdfData[index].Radius;
                colors[index] = sdfData[index].Color;
            }

            for (int index = 0; index < materials.Length; index++)
            {
                materials[index].SetVectorArray("_SpherePos", positions);
                materials[index].SetColorArray("_SphereColors", colors);
                materials[index].SetVectorArray("_SphereRadius", radius);

                materials[index].SetColor("_CylinderColor", sdfInfo.CylinderColor);
                materials[index].SetFloat("_CylinderRadius", sdfInfo.cylinderRadius);
            }
        }
        else
        {
            Vector4 pos = new Vector4(-100, -100, -100, -100);
            Vector4 r = new Vector4(0.0001f, 0.0001f, 0.0001f, 0.0001f);


            Vector4[] positions = new Vector4[3];
            Vector4[] radius = new Vector4[3];
            Color[] colors = new Color[3];

            for (int index = 0; index < 3; index++)
            {
                positions[index] = pos;
                radius[index] = r;
                colors[index] = Color.clear;
            }
            for (int index = 0; index < materials.Length; index++)
            {
                materials[index].SetVectorArray("_SpherePos", positions);
                materials[index].SetColorArray("_SphereColors", colors);
                materials[index].SetVectorArray("_SphereRadius", radius);

                materials[index].SetColor("_CylinderColor", Color.clear);
                materials[index].SetFloat("_CylinderRadius", 0);
            }
        }



        if (renderClyinder)
        {
            for (int index = 0; index < materials.Length; index++)
            {
                materials[index].SetInt("_StartIndexOfCylinder", 1);
                materials[index].SetInt("_EndIndexOfCylinder", 0); // not working
            }
        }
        else
        {
            for (int index = 0; index < materials.Length; index++)
            {
                materials[index].SetInt("_StartIndexOfCylinder", -1);
                materials[index].SetInt("_EndIndexOfCylinder", -1);
            }
        }

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

    void OnValidate()
    {
        Constrain(ref sliceXMin, ref sliceXMax);
        Constrain(ref sliceYMin, ref sliceYMax);
        Constrain(ref sliceZMin, ref sliceZMax);
    }

    void Constrain(ref float min, ref float max)
    {
        const float threshold = 0.025f;
        if (min > max - threshold)
        {
            min = max - threshold;
        }
        else if (max < min + threshold)
        {
            max = min + threshold;
        }
    }


    Texture3D CreateNormalTexture(int lowThreashold = 0, int highThreashhold = 1500)
    {
        // Generate the texture
        Texture3D texture = new Texture3D(dataObject.width, dataObject.height, dataObject.breath, TextureFormat.ARGB32, false)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Point,
            anisoLevel = 0
        };

        // Create a color array
        Color[] colors = new Color[dataObject.buffer.Length];

        // loop though all the pixels to create the texture
        for (int index = 0; index < dataObject.buffer.Length; index++)
        {
            try
            {
                Vector4 normal = dataObject.CalculateNormalConvolution(index);

                float magnatude = (float)(normal.w - lowThreashold) / (highThreashhold - lowThreashold) * (float)byte.MaxValue;

                /*
                if (index % 10000 == 0)
                {
                    UnityEngine.Debug.Log("Raw Magnatude is " + normal.w);
                }
                */

                if (magnatude < 0)
                    magnatude = 0;
                else if (magnatude > 1)
                    magnatude = 1;

                colors[index] = new Color(normal.x, normal.y, normal.z, magnatude);
            }
            catch (System.ArgumentOutOfRangeException)
            {
                colors[index] = new Color(0, 0, 0, 0);
                break;
            }
        }

        // set the information we created before
        texture.SetPixels(colors, 0);
        texture.Apply();

        return texture;
    }

    public Texture3D GetAsTexture3DOneByte(int lowThreashold = 0, int highThreashhold = 1500)
    {
        Texture3D texture = new Texture3D(dataObject.width, dataObject.height, dataObject.breath, TextureFormat.ARGB32, false)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = TextureFiltering,
            anisoLevel = 0
        };

        Color32[] colors = new Color32[dataObject.buffer.Length];

        for (int index = 0; index < dataObject.buffer.Length; index++)
        {
            float threasholdValue = (float)(dataObject.buffer[index] - lowThreashold) / (highThreashhold - lowThreashold) * (float)byte.MaxValue;

            if (threasholdValue < byte.MinValue)
                threasholdValue = byte.MinValue;
            else if (threasholdValue >= byte.MaxValue)
                threasholdValue = byte.MaxValue;

            if (currentRun == TypeOfNormalsToRender.depthBased)
            {
                float distanceToSkip = 0;
                if (dataObject.definition[index] == DicomGrid.TypeOfVoxel.outside)
                {
                    distanceToSkip = dataObject.GetDistanceToNearestBoarder(index);
                }

                // Set the current values to the output texture for the min based check. 
                try
                {
                    colors[index] = new Color32(System.Convert.ToByte(distanceToSkip * 256f), 
                        0, 
                        0, 
                        System.Convert.ToByte(threasholdValue));
                }
                catch (System.OverflowException ex)
                {
                    UnityEngine.Debug.Log(ex.Message);
                    UnityEngine.Debug.Log(threasholdValue);
                    UnityEngine.Debug.Log(distanceToSkip);
                    colors[index] = new Color32(0, 0, 0, byte.MaxValue);
                    break;
                }
            }
            else
            {
                try
                {
                    colors[index] = new Color32(0, 0, 0, System.Convert.ToByte(threasholdValue));
                }
                catch (System.OverflowException ex)
                {
                    UnityEngine.Debug.Log(threasholdValue);
                    colors[index] = new Color32(0, 0, 0, byte.MaxValue);
                    break;
                }
            }
        }

        // set the information we created before
        texture.SetPixels32(colors, 0);
        texture.Apply();

        return texture;
    }

    void OnDestroy()
    {
        for (int index = 0; index < materials.Length; index++)
        {
            Destroy(materials[index]);
        }
    }
}
