using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StudyDetailsForOutput
{
    public float NoiseMulitiplier = 0.5f;

    public string ConditionName;

    public HashingMatrix hash;

    public AbstractGeometricShape[] shapeDetails;

    public AnswersFromGeneration answers;

    public void SetNoiseMultiper(Material mat)
    {
        mat.SetFloat("_NoiseSuppression", NoiseMulitiplier);
    }

    public string ToJSON()
    {
        return JsonUtility.ToJson(this, true);
    }
}
