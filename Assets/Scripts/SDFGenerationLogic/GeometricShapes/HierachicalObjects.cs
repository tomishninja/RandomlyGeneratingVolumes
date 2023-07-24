using UnityEngine;

[System.Serializable]
public abstract class HierachicalObjects : AbstractGeometricShape
{
    // Amount of voxels found within
    [HideInInspector] public int AmountOfVoxelsWithin = 0;
    [HideInInspector] public Vector3 AveragePoint = Vector3.positiveInfinity;

    // Hierachical details
    [System.NonSerialized] public HierachicalObjects Parent = null;
    [System.NonSerialized] public HierachicalObjects[] Children = null;

    public abstract int AmountOfParents();

    public abstract bool HasAncestor(HierachicalObjects possibleAncestor);
}
