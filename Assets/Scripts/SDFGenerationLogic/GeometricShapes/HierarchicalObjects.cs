using UnityEngine;

[System.Serializable]
public abstract class HierarchicalObjects : AbstractGeometricShape
{
    // Amount of voxels found within
    [HideInInspector] public int AmountOfVoxelsWithin = 0;
    [HideInInspector] public Vector3 AveragePoint = Vector3.positiveInfinity;

    // Hierachical details
    [System.NonSerialized] public HierarchicalObjects Parent = null;
    [System.NonSerialized] public HierarchicalObjects[] Children = null;

    public abstract int AmountOfParents();

    public abstract bool HasAncestor(HierarchicalObjects possibleAncestor);
}
