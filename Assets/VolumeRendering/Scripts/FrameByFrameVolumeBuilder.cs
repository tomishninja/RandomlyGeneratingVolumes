using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrameByFrameVolumeBuilder : MonoBehaviour
{
    [SerializeField]
    private BasicVolumeManager volumeMangager;

    [SerializeField]
    private string outputFileName;

    [SerializeField]
    int index = 0;

    [SerializeField]
    int percentage = -1;

    Color32[] colors = null;

    octTree.OctTreeMedicalData octTree = null;

    [SerializeField] // for Testing Remove;
    Vector3 bfsCorrection;

    private void Awake()
    {
    }

    private void Start()
    {
        volumeMangager.enabled = false;
        colors = new Color32[volumeMangager.volumeInfo.buffer.Length];
        octTree = volumeMangager.volumeInfo.GetAsOctTree();
        bfsCorrection = BuildSquareVersionOfData(volumeMangager.volumeInfo);
    }

    // Update is called once per frame
    void Update()
    {
        if (index < colors.Length)
        {
            colors[index] = volumeMangager.GetVolumeData(index, bfsCorrection, 1, 256, octTree);
            index++;

            if (percentage != Mathf.FloorToInt((float)index / (float)colors.Length * 100))
            {
                percentage = Mathf.FloorToInt((float)index / (float)colors.Length * 100);
                Debug.Log(percentage);
            }
        }
        else
        {
            volumeMangager.SetVolume(colors, FilterMode.Bilinear);
            FileWriterManager.WriteString(BuildColorArrayOutput(), "outputFileName", FileWriterManager.DefaultFilePath);
            this.enabled = false;
        }
    }

    private Vector3 BuildSquareVersionOfData(DicomGrid data)
    {
        // check all of the data point to calcualte the data
        float maxDimention = System.Convert.ToSingle(System.Math.Max(System.Math.Max(data.height, data.breath), data.width));

        // determine the correction needed for each value
        // x == width
        return new Vector3(
            (1f / (float)data.width) * maxDimention,
            (1f / (float)data.height) * maxDimention,
            (1f / (float)data.breath) * maxDimention
            );
    }

    string BuildColorArrayOutput()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        for (int index = 0; index < colors.Length; index++)
        {
            sb.AppendLine(colors[index].ToString());
        }

        return sb.ToString();
    }
}
