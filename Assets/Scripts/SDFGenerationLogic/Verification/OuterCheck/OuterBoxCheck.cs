using hLSL_Simulator;
using UnityEngine;

namespace GenoratingRandomSDF
{
    public class OuterBoxCheck : IVerification
    {
        Vector3 offset;
        int resolutionToCheckVolumeAt = 0;
        NoisyHierarchicalSpheres shaderSimulator;
        float amountToShrink;
        LayerManager layerManager;

        public OuterBoxCheck(NoisyHierarchicalSpheres shaderSimulator, float amountToShrink = 0.01f, float _offset = 0.5f)
        {
            offset = new Vector3(_offset, _offset, _offset);
            this.shaderSimulator = shaderSimulator;
            this.amountToShrink = amountToShrink;
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
            float xPercenatge;
            float yPercenatge;
            float zPercenatge = 0;

            // Make sure the 
            for (int z = 0; z < 2; z++)
            {
                for (int y = 0; y < resolutionToCheckVolumeAt; y++)
                {
                    yPercenatge = y / (float)resolutionToCheckVolumeAt;

                    for (int x = 0; x < resolutionToCheckVolumeAt; x++)
                    {
                        xPercenatge = x / (float)resolutionToCheckVolumeAt;

                        if (shaderSimulator.CheckSDFFirstLayer(new Vector3(xPercenatge, yPercenatge, zPercenatge) - offset)
                            ||
                            shaderSimulator.CheckSDFFirstLayer(new Vector3(yPercenatge, xPercenatge, zPercenatge) - offset)
                            ||
                            shaderSimulator.CheckSDFFirstLayer(new Vector3(yPercenatge, zPercenatge, xPercenatge) - offset))
                        {
                            //this failed so return 1 so we can do this again
                            shapes.CurrentShape.radius -= amountToShrink;
                            return 1;
                        }
                    }
                }
            }

            // this worked so return 
            layerManager.SetToContainer();
            return 0;
        }
    }
}


