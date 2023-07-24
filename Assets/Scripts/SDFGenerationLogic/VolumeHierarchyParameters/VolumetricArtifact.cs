using UnityEngine;

[System.Serializable]
public class VolumetricArtifact
{
    public Color color;
    public MinAndMaxInt amountOfChildrenRange = new MinAndMaxInt(0, 0);
    public int importance;
}
