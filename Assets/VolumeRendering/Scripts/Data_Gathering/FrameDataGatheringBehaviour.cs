using System.Linq;
using System.Text;
using UnityEngine;

[System.Serializable]
public class FrameDataGatheringBehaviour : MonoBehaviour
{
    // basic behaviour modifier telling the code when to track inputs
    public enum Frequency
    {
        EveryFrame,
        Never
    }

    /// <summary>
    /// Where to output the file
    /// </summary>
    public string OutputDir;

    /// <summary>
    /// The name object the place in the file
    /// </summary>
    public string FileName;

    /// <summary>
    /// The frequency that this instance of the script will run at.
    /// </summary>
    public Frequency frequency = Frequency.EveryFrame;

    /// <summary>
    /// the array of items that this object will gather from other scripts
    /// </summary>
    public DataItem[] GenericDataItems;

    /// <summary>
    /// Behavioural object manipulaiton from wihtin unity
    /// </summary>
    public UnityData unityDataGatherer;

    /// <summary>
    /// The output builder as a csv.
    /// </summary>
    private System.Text.StringBuilder output;

    
    private void Start()
    {
        // get the string builder ready
        output = new System.Text.StringBuilder();
        output.AppendLine(this.GetCSVHeader());
    }

    // Update is called once per frame
    void Update()
    {
        // Work out if you should update the frame
        switch (frequency)
        {
            case Frequency.EveryFrame:
                WriteFrame();
                break;
            default:
                // Do nothing
                break;
        }
    }

    /// <summary>
    /// Writes a line of data to the output
    /// </summary>
    public void WriteFrame()
    {
        // get the unity data and write it to the frame
        string unity = this.unityDataGatherer.getUnityData();
        if (unity.Length > 0)
        {
            output.Append(unity);
        }
        

        for (int index = 0; index < this.GenericDataItems.Length; index++)
        {
            // get the property out of the object
            object value = this.GenericDataItems[index].GetPropValue();

            // Get the data to the output
            WriteValueToOutput(value);
        }

        // this data has been recorded now move on to the next line
        output.AppendLine();
    }

    /// <summary>
    /// writes the value of a object to the string buffer
    /// </summary>
    /// <param name="value">
    /// the base value that was found. Must be a primitive, string, vector or quaternion
    /// </param>
    public void WriteValueToOutput(object value)
    {
        if (value.GetType() == typeof(Vector3))
        {
            Debug.Log("Vector3");
             //TODO
        }
        else if(value.GetType() == typeof(Vector2))
        {
            //TODO
        }
        else if (value.GetType() == typeof(Vector4))
        {
            //TODO
        }
        else if (value.GetType() == typeof(Quaternion))
        {
            //TODO
        }
        else
        {
            // default behaviour (int, float, string )
            output.Append(value.ToString());
            Debug.Log(value.ToString());
        }
        output.Append(",");
    }

    /// <summary>
    /// Write the data to a file and move on
    /// </summary>
    public void Flush()
    {
        //TODO write string buffer to file
        FileWriterManager.WriteString(output.ToString(), this.FileName, this.OutputDir);

        // clean out the string buffer
        output.Clear();
        output.AppendLine(this.GetCSVHeader());
    }

    public string GetCSVHeader()
    {
        // initalizes with either the output with a empty string or the unity data
        string output = this.unityDataGatherer.GetUnityDataCSVHeader();

        for (int index = 0; index < this.GenericDataItems.Length; index++)
        {
            output += this.GenericDataItems[index].GetName() + ",";
        }

        return output;
    }

    /// <summary>
    /// Stops the system from recording data
    /// </summary>
    public void Pause()
    {
        this.frequency = Frequency.Never;
    }

    /// <summary>
    /// Change the rate you collect data from the system
    /// </summary>
    /// <param name="frequency">
    /// The speed you want to get data from the system
    /// </param>
    public void changeFrequency(Frequency frequency)
    {
        this.frequency = frequency;
    }

}

/// <summary>
/// This class will get the main system to write the data 
/// </summary>
[System.Serializable]
public class UnityData
{
    public bool deltaTime = false;
    public bool systemTime = false;
    public bool frameCount = false;

    public string getUnityData()
    {
        string output = "";

        if (deltaTime)
        {
            output += Time.deltaTime.ToString() + ",";
        }

        if (systemTime)
        {
            output += Time.time.ToString() + ",";
        }

        if (frameCount)
        {
            output += Time.frameCount.ToString() + ",";
        }

        return output;
    }

    public string GetUnityDataCSVHeader()
    {
        string output = "";

        if (deltaTime)
        {
            output += "DeltaTime,";
        }

        if (systemTime)
        {
            output += "SystemTime,";
        }

        if (frameCount)
        {
            output += "Frame,";
        }

        return output;
    }
}


