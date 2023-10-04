using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class TimerDatabase : MonoBehaviour
{
    /// <summary>
    /// Where to output the file
    /// </summary>
    public string OutputDir;

    /// <summary>
    /// The name object the place in the file
    /// </summary>
    public string FileName;

    /// <summary>
    /// the array of items that this object will gather from other scripts
    /// </summary>
    public DataItem[] GenericDataItems;

    /// <summary>
    /// a array of keys
    /// </summary>
    [SerializeField]
    string[] dbKeys;

    /// <summary>
    /// A dictionary that holds the running data
    /// </summary>
    private Dictionary<string, TimerDataRows> database = new Dictionary<string, TimerDataRows>();

    // a queue that holds the used data
    private Queue<TimerDataRows> dataRows = new Queue<TimerDataRows>();

    private StringBuilder previousData;

    // Start is called before the first frame update
    void Start()
    {
        // add all of the new keys to the database so they can be added later
        for (int index = 0; index < dbKeys.Length; index++)
        {
            database.Add(dbKeys[index], null);
        }
    }

    public void StartTimer(string key)
    {
        // increment the int by one if it is accepted
        if (database.TryGetValue(key, out TimerDataRows value))
        {
            // only start the timer if it hasn't already started
            if (value == null)
                database[key] = new TimerDataRows(key);
        }
    }

    public void StopTimer(string key)
    {
        // increment the int by one if it is accepted
        if (database.TryGetValue(key, out TimerDataRows value))
        {
            // only start the timer if it hasn't already started
            if (value != null)
            {
                // write the final file
                database[key].End();

                // send it to the stack
                dataRows.Enqueue(database[key]);

                // remove the current entry from the timer set
                database[key] = null;
            }
        }
    }

    public void SaveAndClear()
    {
        // Make sure the previous string builder exists
        if(this.previousData == null)
        {
            this.previousData = new StringBuilder();
            this.previousData.AppendLine(TimerDataRows.GetCSVHeader());
        }

        
        // time this data
        TimerDataRows obj = this.dataRows.Dequeue();
        while (obj != null)
        {
            // add the data sourced from a outside file
            for (int index = 0; index < this.GenericDataItems.Length; index++)
            {
                // get the property out of the object
                object value = this.GenericDataItems[index].GetPropValue();

                // Get the data to the output
                previousData.Append(value);
                previousData.Append(",");
            }


            previousData.AppendLine(obj.AsCSVRow());
            obj = this.dataRows.Dequeue();
        }

        // the clearing part

    }

    /// <summary>
    /// Write the data to a file and clean the database
    /// </summary>
    public void Flush()
    {
        // create the output string
        StringBuilder sb;
        if (previousData == null)
        {
            sb = new StringBuilder();
            sb.AppendLine(TimerDataRows.GetCSVHeader());
        }
        else
        {
            sb = this.previousData;
        }

        // time this data
        TimerDataRows obj = this.dataRows.Dequeue();
        while (obj != null)
        {
            // add the data sourced from a outside file
            for (int index = 0; index < this.GenericDataItems.Length; index++)
            {
                // get the property out of the object
                object value = this.GenericDataItems[index].GetPropValue();

                // Get the data to the output
                sb.Append(value);
                sb.Append(",");
            }

            sb.AppendLine(obj.AsCSVRow());
            obj = this.dataRows.Dequeue();
        }

        // write the data out to the data base
        FileWriterManager.WriteString(sb.ToString(), this.FileName, this.OutputDir);

        // clean out the string buffer
        sb.Clear();
    }
}


public class TimerDataRows
{
    string ValueName;
    float StartTime;
    float EndTime;
    float TotalTime;
    int StartFrame;
    int EndFrame;

    public TimerDataRows(string name)
    {
        this.ValueName = name;
        this.StartTime = Time.time;
        this.StartFrame = Time.frameCount;
    }

    public void End()
    {
        this.EndTime = Time.time;
        this.EndFrame = Time.frameCount;
        this.TotalTime = this.EndTime - this.StartTime;
    }

    /// <summary>
    /// Gets all the feilds in this class and returns them as a csv file syle row
    /// </summary>
    /// <returns> A Row formated in the style of a csv file</returns>
    public string AsCSVRow()
    {
        System.Type t = typeof(TimerDataRows);
        System.Reflection.FieldInfo[] fields = t.GetFields();
        StringBuilder csvdata = new StringBuilder();
        foreach (var f in fields)
        {
            if (csvdata.Length > 0)
                csvdata.Append(",");

            var x = f.GetValue(this);

            if (x != null)
            {
                if (x.GetType() != typeof(Vector3))
                {
                    csvdata.Append(x.ToString());
                }
                else
                {
                    Vector3 v = (Vector3)x;
                    csvdata.Append(v.x);
                    csvdata.Append(",");
                    csvdata.Append(v.y);
                    csvdata.Append(",");
                    csvdata.Append(v.z);
                }
            }
        }

        // return the csv data
        return csvdata.ToString();
    }

    public static string GetCSVHeader()
    {
        System.Type t = typeof(TimerDataRows);
        System.Reflection.FieldInfo[] fields = t.GetFields();
        StringBuilder csvdata = new StringBuilder();
        foreach (var f in fields)
        {
            if (csvdata.Length > 0)
                csvdata.Append(",");

            csvdata.Append(f.Name);
        }

        // return the csv data
        return csvdata.ToString();
    }
}
