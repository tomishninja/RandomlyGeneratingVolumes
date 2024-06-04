﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.MixedReality.Toolkit.UI;
using TMPro;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.Examples.Demos
{

    [AddComponentMenu("Scripts/MRTK/Examples/ShowSliderValue")]
    public class ShowSliderValue : MonoBehaviour
    {
        [SerializeField]
        private TextMeshPro textMesh = null;

        [SerializeField] public bool isInt = false;

        [SerializeField] public float min = 0;
        [SerializeField] public float max = 1;

        public void OnSliderUpdated(SliderEventData eventData)
        {
            if (textMesh == null)
            {
                textMesh = GetComponent<TextMeshPro>();
            }

            if (textMesh != null)
            {
                if (isInt)
                    textMesh.text = $"{Mathf.RoundToInt(min + (eventData.NewValue * (max - min))):F0}";
                else
                    textMesh.text = $"{(min + (eventData.NewValue * (max - min))):F2}";
            }
        }
    }
}