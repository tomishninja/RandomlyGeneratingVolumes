using GeneratingRandomSDF;
using UnityEngine;

public interface VisualizationAdapter
{
    public abstract void SetUpVisualization(ShapeHandler shapes, string CurrentCondition = null);

    public abstract Material GetMaterial();
}
