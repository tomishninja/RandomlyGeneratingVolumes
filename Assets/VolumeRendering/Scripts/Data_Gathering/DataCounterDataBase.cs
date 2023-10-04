using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class DataCounterDataBase : MonoBehaviour
{
    public string OutputDir;

    public string FileName;

    /// <summary>
    /// the array of items that this object will gather from other scripts
    /// </summary>
    public DataItem[] GenericDataItems;

    /// <summary>
    /// 
    /// </summary>
    [SerializeField]
    string[] dbKeys;

    /// <summary>
    /// 
    /// </summary>
    private Dictionary<string, int> database = new Dictionary<string, int>();

    private void Start()
    {
        // add all of the new keys to the database so they can be added later
        for (int index = 0; index < dbKeys.Length; index++)
        {
            database.Add(dbKeys[index], 0);
        }
    }

    /// <summary>
    /// Increments the key value by one when selected
    /// </summary>
    /// <param name="key">
    /// The database term you have selected
    /// </param>
    public void Trigger(string key)
    {
        // increment the int by one if it is accepted
        if (database.TryGetValue(key, out int value))
        {
            database[key]++;
        }
    }

    public void Flush()
    {
        // create the output string
        StringBuilder sb = new StringBuilder();
        sb.AppendLine(this.GetCSVHeader());

        // add the data sourced from a outside file
        for (int index = 0; index < this.GenericDataItems.Length; index++)
        {
            // get the property out of the object
            object value = this.GenericDataItems[index].GetPropValue();

            // Get the data to the output
            sb.Append(value);
            sb.Append(",");
        }

        // the above will be writen now to each line as a preamble so save it and begin to use the sb else where
        string preamble = sb.ToString();
        sb.Clear();

        // add in all of the rows
        for (int index = 0; index < dbKeys.Length; index++)
        {
            sb.Append(preamble + dbKeys[index] + "," + database[dbKeys[index]]);
        }

        // write the data out to the data base
        FileWriterManager.WriteString(sb.ToString(), this.FileName, this.OutputDir);

        // clean out the string buffer
        sb.Clear();
    }

    private string GetCSVHeader()
    {
        string output = "";

        // get all of the names of the varibles we need to add
        for (int index = 0; index < GenericDataItems.Length; index++)
        {
            output += this.GenericDataItems[index].GetName() + ",";
        }

        output += "Key,Count";

        return output;
    }
}
