using UnityEngine;

[System.Serializable]
public class TargetPositionData
{
    [SerializeField]
    public TargetPosition[] data;

    // Add a new entry to the data base
    public void Add(TargetPosition info)
    {
        TargetPosition[] ndata;

        if (data != null)
        {
            ndata = new TargetPosition[data.Length + 1];

            for (int index = 0; index < data.Length; index++)
            {
                // if we already have it then don't add more
                if (data[index] == info)
                    return;

                ndata[index] = data[index];
            }
        }
        else
        {
            ndata = new TargetPosition[1];
        }

        ndata[ndata.Length - 1] = info;

        data = ndata;
    }

    public TargetPosition Get(int index)
    {
        // validate the input
        if (data == null || index < 0 || index >= data.Length)
            return null;

        // return data at the point the user is asking for
        return data[index];
    }

    // Count the amount of entries in the database
    public int Count()
    {
        if (this.data == null) return 0;
        return this.data.Length;
    }

    public string ToJSON()
    {
        return JsonUtility.ToJson(this);
    }
}

[System.Serializable]
public class TargetPosition
{
    [SerializeField]
    public Vector3 targetPosition;

    [SerializeField]
    public Vector3 optimalPosition;

    public TargetPosition(Vector3 targetPosition, Vector3 optimalPosition)
    {
        this.targetPosition = targetPosition;
        this.optimalPosition = optimalPosition;
    }
}
