using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserTrackingSystemForHatchingScript : MonoBehaviour
{
    [SerializeField] string name;
    [SerializeField] Material[] mat;
    [SerializeField] Vector3[] angleOffSet;

    private void Start()
    {
        if (mat != null)
        {
            for (int index = 0; index < mat.Length; index++)
            {
                if (mat[index] != null)
                {
                    this.SetHashingParameters(mat[index]);
                }
            }
        }
    }

    void Update()
    {
        // look at the main camera
        this.transform.LookAt(Camera.main.transform.position);

        Matrix4x4[] array = new Matrix4x4[angleOffSet.Length];
        // Calcuate the new postion and set the matrix
        for (int index = 0; index < angleOffSet.Length; index++)
        {
            array[index] = Matrix4x4.TRS(Vector3.zero, transform.localRotation * Quaternion.Euler(angleOffSet[index]), Vector3.one).inverse;
            Debug.Log("NewTransform" + array[index]);
        }

        for (int index = 0; index < mat.Length; index++)
        {
            mat[index].SetMatrixArray(name, array);
        }
    }

    public void AddMat(Material newMat)
    {
        Material[] newArray = new Material[this.mat.Length + 1];

        for(int index = 0; index < this.mat.Length; index++)
        {
            newArray[index] = this.mat[index];
        }

        SetHashingParameters(newMat);
        newArray[this.mat.Length] = newMat;

        this.mat = newArray;
    }

    public void SetHashingParameters(Material newMat)
    {
        newMat.SetVector("_GridSpaceCenter", Vector4.zero);
        newMat.SetVector("_GridSpaceDimentions", new Vector3(0.009f, 0.015f, 0f));
        newMat.SetVector("_GridDimentions", new Vector3(0.019f, 0.038f, 0f));
        newMat.SetFloat("_Range", 0.05f);
        newMat.SetFloat("_StepsToEffect", 1.1f);
    }
}
