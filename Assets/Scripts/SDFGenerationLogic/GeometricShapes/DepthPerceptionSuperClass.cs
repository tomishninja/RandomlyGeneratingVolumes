using UnityEngine;

public abstract class DepthPerceptionSuperClass : AbstractGeometricShape
{
    public Vector4 getPosAndSizeVetor4ForGroundTruth()
    {
        return new Vector4(this.positon.x, this.positon.y, 0, this.radius);
    }
}
