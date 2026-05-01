using hLSL_Simulator;
using profiler;
using System.Collections.Generic;
using UnityEngine;


namespace GeneratingRandomSDF
{
    public abstract class LinearInnerChecker : InnerObjectCheckerParent, IVerification
    {
        private static float squareRootOfThreeHalved = 0.86602540378f;

        protected Vector3 offset;
        protected float SDFTolerance;
        protected float smallerSDFTolerance;
        protected int resolutionToCheckVolumeAt;

        public virtual void Init(ref NoisyHierarchicalSpheres shaderSimulator, ref AbstractProfiler profiler, int resolutionToCheckVolumeAt, float offset, int ResolutionMultiplierForSmallerTolerance = 4)
        {
            base.Init(ref shaderSimulator, ref profiler, resolutionToCheckVolumeAt);
            this.offset = new Vector3(offset, offset, offset);

            this.SDFTolerance = squareRootOfThreeHalved * (1f / resolutionToCheckVolumeAt);

            this.smallerSDFTolerance = squareRootOfThreeHalved * (1f / (resolutionToCheckVolumeAt * ResolutionMultiplierForSmallerTolerance));

            this.resolutionToCheckVolumeAt = resolutionToCheckVolumeAt;
        }

        protected abstract int RunValidation(ShapeHandler shapes);


        public virtual int Verify(ref ShapeHandler shapes)
        {
            int result = RunValidation(shapes);

            if (result == int.MinValue)
            {
                // The result was succesful
                return IterationCheckWasSuccessful(ref shapes);
            }
            else
            {
                // The result was a failure reset the volume and start again
                return this.IterationCheckFailed(result, ref shapes);
            }
        }

        public void Reset()
        {
            incrementor = 0;
            errorHandler.Reset();
        }

        public void SetLayerManager(ref LayerManager layerManager)
        {
            this.layerManager = layerManager;
        }
    }
}