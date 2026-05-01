using UnityEngine;

public abstract class DepthPerceptionSuperClass : AbstractGeometricShape
{
    public Vector4 GetPositionAndSizeVector4ForGroundTruth()
    {
        return new Vector4(this.position.x, this.position.y, 0, this.radius);
    }
}
