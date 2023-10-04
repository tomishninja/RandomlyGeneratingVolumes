using UnityEngine;

public class ThreeDofPlaneBasedDicomGenerator : MonoBehaviour
{
    public enum TypeofVolume { PVM, DCM };
    public TypeofVolume typeofVolume;

    [SerializeField]
    private string file = "TestDicomData";

    [SerializeField]
    DicomGrid dataObject = new DicomGrid();

    [SerializeField]
    MeshRenderer renderer;

    Mesh mesh;

    Texture2D texture;

    [SerializeField]
    int width;

    [SerializeField]
    int height;

    [SerializeField]
    BoxCollider box;

    private void Awake()
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

    // Start is called before the first frame update
    void Start()
    {
        if (renderer == null)
        {
            this.renderer = this.GetComponent<MeshRenderer>();
        }

        if (mesh == null)
        {
            this.mesh = this.renderer.GetComponent<MeshFilter>().mesh;
            if (mesh.vertexCount != 4) mesh = null;
        }

        this.texture = new Texture2D(width, height);

        this.renderer.material.SetTexture("_MainTex", this.texture);
    }

    // Update is called once per frame
    void Update()
    {
        if (renderer != null)
        {
            uint maxValue, minValue;
            Vector3[] points = new Vector3[4];
            for (int index = 0; index < 4; index++)
            {
                points[index] = renderer.transform.TransformPoint(mesh.vertices[index]);
            }

            uint[] datapoints = this.dataObject.GetXYZSliceImage(box, points, width, height, out maxValue, out minValue);

            if (datapoints == null || maxValue < minValue)
            {
                Debug.LogError("Invalid Parameters According to Results");
            }

            // Create a new color arrray for the planes texture
            Color[] colors = new Color[datapoints.Length];
            float denominator = System.Convert.ToSingle(maxValue - minValue);
            if (denominator == 0)
            {
                denominator = 1;
            }

            float value;
            for (int index = 0; index < datapoints.Length; index++)
            {
                if (datapoints[index] == 0)
                    value = 0;
                else
                    value = System.Convert.ToSingle(datapoints[index] - minValue) / denominator;

                colors[index] = new Color(value, value, value, 1);
            }

            this.texture.SetPixels(colors);
            this.texture.Apply();
        }
    }
}
