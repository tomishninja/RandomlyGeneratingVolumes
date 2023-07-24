using hLSL_Simulator;
using UnityEngine;

namespace GenoratingRandomSDF
{
    public class OuterSphereCheck : IVerification
    {
        float offset;
        int resolutionToCheckVolumeAt = 0;
        NoisyHierarchicalSpheres shaderSimulator;
        LayerManager layerManager;
        float amountToShrink;
        float radius;

        public OuterSphereCheck(NoisyHierarchicalSpheres shaderSimulator, float radius = 1f, float amountToShrink = 0.01f, float _offset = 0.5f)
        {
            this.offset = _offset;
            this.shaderSimulator = shaderSimulator;
            this.amountToShrink = amountToShrink;
            this.radius = radius;
        }

        public void SetLayerManager(LayerManager layerManager)
        {
            this.layerManager = layerManager;
        }

        public void Reset()
        {
            // do nothing this class does not have a state to reset
        }

        public void SetLayerManager(ref LayerManager layerManager)
        {
            this.layerManager = layerManager;
        }

        public int Verify(ref ShapeHandeler shapes)
        {
            // calculate the amount of points to check
            int samples = (resolutionToCheckVolumeAt ^ 2) * 2;

            // Cacluate the golden angle in randians
            float phi = System.Convert.ToSingle(System.Math.PI * (3.0 - System.Math.Sqrt(5.0)));

            for (int index = 0; index < samples; index++)
            {
                //float y = 1 - (index / (samples - 1f)) * 2; // -1 to 1
                float y = offset - (index / (samples - 1f)); // should be 0 to 1
                float _radius = Mathf.Sqrt(1 - y * y) * radius;

                // increment the golden angle
                float theta = phi * index;

                float x = Mathf.Cos(theta) * _radius;
                float z = Mathf.Sin(theta) * _radius;

                if (shaderSimulator.CheckSDFFirstLayer(new Vector3(x, y * radius, z)))
                {
                    // this failed so return 1 so we can do this again
                    shapes.CurrentShape.radius -= amountToShrink;
                    return 1;
                }
            }

            // this was succesfull so we move forward and return
            this.layerManager.SetToContainer();
            // don't increment from this one as we still need to check the next layer using this as the base
            return 0;
        }
    }
}
