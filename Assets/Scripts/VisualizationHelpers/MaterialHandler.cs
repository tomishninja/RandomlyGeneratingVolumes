using System;
using UnityEngine;

[System.Serializable]
public class MaterialHandler : IComparable
{
    [SerializeField] public Material mat;
    [SerializeField] public int AmountOfRegions;

    public int CompareTo(object obj)
    {
        if (obj is MaterialHandler)
            return this.AmountOfRegions.CompareTo((obj as MaterialHandler).AmountOfRegions);

        throw new ArgumentException("Object is not comparable");
    }
}
