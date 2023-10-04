using UnityEngine;

[System.Serializable]
public struct VolumetricColorPicker
{
    public float density;
    public Color32 color;
}

[System.Serializable]
public struct RangeBasedVolumetricColorPicker
{
    [Range (0, 1)] public float min;
    [Range (0, 1)] public float max;
    public Color32 color;
}

[System.Serializable]
public struct VolumetricColorAndIntensityPicker
{
    public float density;
    public Color32 color;
    public float intensity;
}

[System.Serializable]
public struct StippllingColorPicker
{
    public enum Style
    {
        EdgeValueOnly,
        IntensityOnly,
        EdgePlusIntensity,
        EdegeMultipliedByIntensity
    };

    public float density;
    public Color32 color;
    public float intensity;
    public Style AlgorihtmToUse;
}
