using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutputVisibleFemaleData : MonoBehaviour
{
    public enum TypeofVolume { PVM, DCM, SDF_Sphere };
    public TypeofVolume typeofVolume;

    [SerializeField]
    private string file = "TestDicomData";
    
    DicomGrid dataObject = new DicomGrid();

    // Start is called before the first frame update
    void Awake()
    {
        TextAsset mytxtData = (TextAsset)Resources.Load(file);
        if (typeofVolume == TypeofVolume.DCM)
        {
            // logic for a dicom
            dataObject = JsonUtility.FromJson<DicomGrid>(mytxtData.text);
        }
        else if (typeofVolume == TypeofVolume.PVM)
        {
            PVMData temp = JsonUtility.FromJson<PVMData>(mytxtData.text);

            //dataObject.buffer = temp.data;
            dataObject.width = temp.width;
            dataObject.height = temp.height;
            dataObject.breath = temp.breath;
        }
    }

    private void Start()
    {
        Debug.Log("Count: " + dataObject.Count());
        if (dataObject.Count() > 0)
        {
            Debug.Log("Min: " + dataObject.Min());
            Debug.Log("Avg: " + dataObject.Avg());
            Debug.Log("Max: " + dataObject.Max());
        }
        
    }

    private void Update()
    {
        Debug.Log("Count: " + dataObject.Count());
        if (dataObject.Count() > 0)
        {
            Debug.Log("Min: " + dataObject.Min());
            Debug.Log("Avg: " + dataObject.Avg());
            Debug.Log("Max: " + dataObject.Max());
        }
    }
}
