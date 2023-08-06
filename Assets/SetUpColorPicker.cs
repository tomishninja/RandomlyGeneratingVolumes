using UnityEngine;

public class SetUpColorPicker : MonoBehaviour
{
    [SerializeField] Material VolumeMaterial;
    [SerializeField] string NameOfVolumeField;
    [SerializeField] string NameOfRangeFild;
    [SerializeField] Material SphereGuideMaterial;

    [SerializeField] int resolution = 256;

    [SerializeField] Vector3 pointInTheVolume = new Vector3(1, 1, 1);
    [SerializeField] Transform ObjectToUseAsPoint;

    [SerializeField] bool AllowColorToChange = false;

    // Start is called before the first frame update
    void Start()
    {
        VolumeMaterial.SetTexture(NameOfVolumeField, GenerateRGBColorSpectrumTexture(resolution));
    }

    public void SetColor(Vector3 color)
    {
        pointInTheVolume = color;
        ObjectToUseAsPoint.localPosition = new Vector3(color.x - 1, color.y - 1, color.z - 1);
    }

    public void SetColor(Color color)
    {
        pointInTheVolume = new Vector3(color.r, color.g, color.b);
        ObjectToUseAsPoint.localPosition = new Vector3(color.r - 1, color.g - 1, color.b - 1);
    }

    // Update is called once per frame
    void Update()
    {
        SphereGuideMaterial.SetColor("_Color", GetColorAtPoint(pointInTheVolume));
        VolumeMaterial.SetVector(NameOfRangeFild, pointInTheVolume);
        
        if (AllowColorToChange)
        {
            pointInTheVolume = new Vector3(1+ObjectToUseAsPoint.localPosition.x, 1+ObjectToUseAsPoint.localPosition.y, 1+ObjectToUseAsPoint.localPosition.z);
        }
    }

    public Color CurrentColor { get => GetColorAtPoint(pointInTheVolume); }

    public static Color GetColorAtPoint(Vector3 point)
    {
        return new Color (point.x, point.y, point.z, 1f);
    }

    public static Texture3D GenerateRGBColorSpectrumTexture(int resolution)
    {
        Texture3D texture = new Texture3D(resolution, resolution, resolution, TextureFormat.RGB24, false);
        Color[] colors = new Color[resolution * resolution * resolution];

        for (int z = 0; z < resolution; z++)
        {
            for (int y = 0; y < resolution; y++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    // Calculate the normalized R, G, and B values
                    float r = x / (float)(resolution - 1);
                    float g = y / (float)(resolution - 1);
                    float b = z / (float)(resolution - 1);

                    // Assign the color to the texture
                    colors[z * resolution * resolution + y * resolution + x] = new Color(r, g, b, 1f);
                }
            }
        }

        texture.SetPixels(colors);
        texture.Apply();

        return texture;
    }

    public void UserIsChangingColor()
    {
        AllowColorToChange = true;
    }

    public void UserHasFinishedChangingColor()
    {
        AllowColorToChange = false;
    }
}
