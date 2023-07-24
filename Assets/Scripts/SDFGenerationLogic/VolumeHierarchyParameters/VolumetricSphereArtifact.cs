using UnityEngine;

[System.Serializable]
public class VolumetricSphereArtifact : VolumetricArtifact
{
    public MinAndMaxFloat minAndMaxRadiusMultipers = new MinAndMaxFloat(1f, 1f);
    public float Radius { get => (minAndMaxRadiusMultipers.min + minAndMaxRadiusMultipers.max) / 2; }
}
