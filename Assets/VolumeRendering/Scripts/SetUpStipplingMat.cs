using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetUpStipplingMat : MonoBehaviour
{
    [SerializeField] float _StippleThreashold = 1.89f;
    [SerializeField] float _Resolution = 128;
    [SerializeField] float _StippleSize = 0.122f;
    [SerializeField] float _CurvatureRange = 1f;
    [SerializeField] float _StepsToEffect = 1.1f;
    [SerializeField] float _Range = 0.1f;

    public void SetUpMat(Material mat)
    {
        mat.SetFloat("_StippleThreashold", _StippleThreashold);
        mat.SetFloat("_Resolution", _Resolution);
        mat.SetFloat("_StippleSize", _StippleSize);
        mat.SetFloat("_CurvatureRange", _CurvatureRange);
        mat.SetFloat("_StepsToEffect", _StepsToEffect);
        mat.SetFloat("_Range", _Range);
    }
}
