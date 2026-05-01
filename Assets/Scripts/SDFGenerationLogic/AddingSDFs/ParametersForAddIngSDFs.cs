using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GeneratingRandomSDF
{
    [System.Serializable]
    public class ParametersForAddIngSDFs
    {
        // Objects created via the input
        [SerializeField] internal float sphereTolerance;
        [SerializeField] internal bool showOutputsOfVolumes;

        [SerializeField] internal int amountOfTimesAddingTryingToFitSDFDataBeforeHigherException = 100;
        [SerializeField] internal float maxiumSizeThatObjectsCanAccululateWithinTheVolume = 0.3f;
        [SerializeField] internal int AmountOfRandomPointsToGenerate = 1;

    }
}

