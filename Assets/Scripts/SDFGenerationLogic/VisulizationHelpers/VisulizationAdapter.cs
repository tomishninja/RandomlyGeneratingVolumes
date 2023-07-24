using GenoratingRandomSDF;
using UnityEngine;

public interface VisulizationAdapter
{
    public abstract void SetUpVisulization(ShapeHandeler shapes, string CurrentCondition = null);

    public abstract Material GetMaterial();
}
