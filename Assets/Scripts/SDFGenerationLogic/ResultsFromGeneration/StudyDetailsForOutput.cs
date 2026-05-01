using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StudyDetailsForOutput
{
    public float NoiseMultiplier = 0.5f;

    public string ConditionName;

    public HashingMatrix hash;

    public AbstractGeometricShape[] shapeDetails;

    public AnswersFromGeneration answers;

    public void SetNoiseMultiplier(Material mat)
    {
        mat.SetFloat("_NoiseSuppression", NoiseMultiplier);
    }

    public string ToJSON()
    {
        return JsonUtility.ToJson(this, true);
    }
}
