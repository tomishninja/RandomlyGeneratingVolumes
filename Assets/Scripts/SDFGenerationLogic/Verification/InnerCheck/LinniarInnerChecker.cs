using hLSL_Simulator;
using profiler;
using System.Collections.Generic;
using UnityEngine;


namespace GenoratingRandomSDF
{
    public abstract class LinniarInnerChecker : InnerObjectCheckerParent, IVerification
    {
        private static float squareRootOfThreeHalved = 0.86602540378f;

        protected Vector3 offset;
        protected float SDFTollerance;
        protected float smallerSDFTollerance;
        protected int resolutionToCheckVolumeAt;

        public virtual void Init(ref NoisyHierarchicalSpheres shaderSimulator, ref AbstractProfiler profiler, int resolutionToCheckVolumeAt, float offset, int ResultionMulitiplierForSmallerTolerance = 4)
        {
            base.Init(ref shaderSimulator, ref profiler, resolutionToCheckVolumeAt);
            this.offset = new Vector3(offset, offset, offset);

            this.SDFTollerance = squareRootOfThreeHalved * (1f / resolutionToCheckVolumeAt);

            this.smallerSDFTollerance = squareRootOfThreeHalved * (1f / (resolutionToCheckVolumeAt * ResultionMulitiplierForSmallerTolerance));

            this.resolutionToCheckVolumeAt = resolutionToCheckVolumeAt;
        }

        protected abstract int RunValidation(ShapeHandeler shapes);


        public virtual int Verify(ref ShapeHandeler shapes)
        {
            int result = RunValidation(shapes);

            if (result == int.MinValue)
            {
                // The result was succesful
                return ItterationCheckWasSuccessful(ref shapes);
            }
            else
            {
                // The result was a failure reset the volume and start again
                return this.ItterationCheckFailed(result, ref shapes);
            }
        }

        public void Reset()
        {
            incrementor = 0;
            errorHandeler.Reset();
        }

        public void SetLayerManager(ref LayerManager layerManager)
        {
            this.layerManager = layerManager;
        }
    }
}