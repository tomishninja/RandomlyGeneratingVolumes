using profiler;
using UnityEngine;

namespace GeneratingRandomSDF
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

        public abstract int AddSDFs(ref ShapeHandler shapes);

        public void CheckToEnsureVolumeHasAppropriateVolume(ref ShapeHandler shapes, ref bool failed, ref int failCount)
        {
            // Make sure the volumes for this set are within the allowed fill percentage
            if (shapes.CurrentShape.TotalPercentOfThisVolumeThatIsFree() > parameters.maxiumSizeThatObjectsCanAccululateWithinTheVolume)
            {
                failed = true;
                dataProfiler.Increment(DataGenerationDataProfiler.TOO_MANY_LARGE_ARTIFICATS);
                shapes.Empty();
                failCount++;

                // The random generator is struggling to fit the current item;
                // an earlier one is likely blocking so we need to start again
                if (failCount > parameters.amountOfTimesAddingTryingToFitSDFDataBeforeHigherException)
                {
                    failCount = 0;

                    UnityEngine.Random.InitState((int)(Time.time * 7919));

                    throw new RanForTooLongException("Cant fill this region up with current items");
                }
            }
        }
    }
}

