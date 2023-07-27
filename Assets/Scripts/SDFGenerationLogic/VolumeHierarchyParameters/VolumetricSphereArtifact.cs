using UnityEngine;

[System.Serializable]
public class VolumetricSphereArtifact : VolumetricArtifact
{
    public MinAndMaxFloat minAndMaxRadiusMultipers = new MinAndMaxFloat(1f, 1f);

    public float Radius { get => (minAndMaxRadiusMultipers.min + minAndMaxRadiusMultipers.max) / 2; }

    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
            return false;

        VolumetricSphereArtifact other = (VolumetricSphereArtifact)obj;

        // Compare minAndMaxRadiusMultipers
        if (!minAndMaxRadiusMultipers.Equals(other.minAndMaxRadiusMultipers))
            return false;

        return true;
    }

    public override int GetHashCode()
    {
        int hash = 17;
        hash = hash * 23 + minAndMaxRadiusMultipers.GetHashCode();
        return hash;
    }
}
