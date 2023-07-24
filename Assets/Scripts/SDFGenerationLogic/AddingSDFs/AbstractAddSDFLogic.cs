using profiler;
using UnityEngine;

namespace GenoratingRandomSDF
{
    public abstract class AbstractAddSDFLogic
    {
        protected RandomChildGeneratorFactory randomChildGeneratorFactory;

        protected SphericalVolumeHierarchyLevelDetails currentLevel;
        protected profiler.AbstractProfiler dataProfiler;
        protected ParametersForAddIngSDFs parameters;
        protected bool showOutputsOfVolumes = false;
        protected AmountTrackingParameters trackingVariables;

        public AbstractAddSDFLogic()
        {
            this.trackingVariables = new AmountTrackingParameters();
        }

        public void SetCurrentShapeDetails(SphericalVolumeHierarchyLevelDetails currentLevel)
        {
            this.currentLevel = currentLevel;
        }

        public int AmountOfSDFsAdded { get; protected set; }

        public abstract int AddSDFs(ref ShapeHandeler shapes);

        public void CheckToEnsureVolumeHasAppropriateVolume(ref ShapeHandeler shapes, ref bool failed, ref int amountOfFails)
        {
            // make sure the volumes for this set are ok or else repete the process
            if (shapes.CurrentShape.TotalPercentOfThisVolumeThatIsFree() > parameters.maxiumSizeThatObjectsCanAccululateWithinTheVolume)
            {
                failed = true;
                dataProfiler.Increment(DataGenerationDataProfiler.TOO_MANY_LARGE_ARTIFICATS);
                Debug.Log("Too full: " + shapes.CurrentShape.TotalPercentOfThisVolumeThatIsFree() + "% full");
                shapes.Empty();
                amountOfFails++;

                // the random generator is stuggling to fit the current item
                // this likly means a earlier one is in its road so we need to start again
                if (amountOfFails > parameters.amountOfTimesAddingTryingToFitSDFDataBeforeHigherException)
                {
                    amountOfFails = 0;

                    UnityEngine.Random.InitState((int)(Time.time * 7919));

                    throw new RanForTooLongException("Cant fill this region up with current items");
                }
            }
            else if (showOutputsOfVolumes)
            {
                Debug.Log("Within Range : " + shapes.CurrentShape.TotalPercentOfThisVolumeThatIsFree() + "% full");
            }
        }
    }
}

