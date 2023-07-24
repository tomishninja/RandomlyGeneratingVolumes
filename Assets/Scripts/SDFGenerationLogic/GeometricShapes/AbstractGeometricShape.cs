using UnityEngine;

public abstract class AbstractGeometricShape
{
    [SerializeField] public float importance;
    [SerializeField] public Vector3 positon;
    [SerializeField] public float radius;
    [SerializeField] public Color color;

    // AABB surronding bounding box
    [System.NonSerialized] public Vector3 AABB_Min = new Vector3(-0.5f, -0.5f, -0.5f);
    [System.NonSerialized] public Vector3 AABB_Max = new Vector3(0.5f, 0.5f, 0.5f);

    public abstract HierachicalObjects MoveChild(int childIndex);

    public abstract HierachicalObjects DraftNewChild(int childIndex, MinAndMaxFloat minAndMaxRadiusMultipler);

    public abstract HierachicalObjects DraftNewChild(int childIndex, MinAndMaxFloat containableRadiusRange,
        int AmountOfTimesToLookForABetterObject = 50, int StartingIndexToLookForBestRandomlyGeneratedPoint = 1,
        int StartingIndexToLookCheckForBestPositionAgainst = 0, float SphereTolerance = 0);

    public abstract float TotalPercentOfThisVolumeThatIsFree(int CurrentChildIndex = -1);

    public abstract float GetVolume();

    public abstract bool IsDefault();

    public abstract void AddDataToAverageVoxelPostion(Vector3 newPosition);

    public Vector4 getPosAndSizeVetor4()
    {
        return new Vector4(this.positon.x, this.positon.y, this.positon.z, this.radius);
    }

    public string GetJSON()
    {
        return UnityEngine.JsonUtility.ToJson(this);
    }

    public override bool Equals(object obj)
    {
        if (obj == null) return false;
        if (!(obj.GetType() == typeof(AbstractGeometricShape))) return false;
        AbstractGeometricShape other = (AbstractGeometricShape)obj;
        if (other.positon != this.positon) return false;
        if ((int)(other.radius * 1000.0) != (int)(this.radius * 1000.0)) return false;
        return true;
    }
}
