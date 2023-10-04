using UnityEngine;

// reads the data produced by Generate Study Design to the rest of the system. 
public class DataLoader : MonoBehaviour
{
    [SerializeField]
    string filePath;

    TargetPositionData database = new TargetPositionData();

    private void Start()
    {
        // read the database from a file
        database = JsonUtility.FromJson<TargetPositionData>(FileWriterManager.ReadString(filePath));
    }

    // return the target world position for a index
    public Vector3 GetTargetPosition(int index)
    {
        return this.transform.TransformPoint(database.Get(index).targetPosition);
    }

    // return the target world position for the optimal position.
    public Vector3 GetOptimalPosition(int index)
    {
        return this.transform.TransformPoint(database.Get(index).optimalPosition);
    }

    public int Count()
    {
        if (database == null) return 0;
        return database.Count();
    }
}
