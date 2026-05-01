using UnityEngine;

namespace GeneratingRandomSDF
{
    public class AddLargeSphereToOuter : AbstractAddSDFLogic
    {
        public AddLargeSphereToOuter(ref SphericalVolumeHierarchyLevelDetails conditionDetails, ref
            RandomChildGeneratorFactory randomChildGeneratorFactory) : base()
        {
            this.currentLevel = conditionDetails;
            this.randomChildGeneratorFactory = randomChildGeneratorFactory;
            this.showOutputsOfVolumes = false;
            this.dataProfiler = null;
            this.parameters = null;
        }

        public override int AddSDFs(ref ShapeHandler shapes)
        {
            HierarchicalObjects shape = new NoisySphereShapeGenerationHelper();

            shape.position = Vector3.zero;
            shape.radius = currentLevel.OuterRadius;
            shape.importance = currentLevel.OuterImportance;
            shape.color = currentLevel.OuterColor;
            trackingVariables.AmountOfOuters = 1;

            shapes.Set(0, shape);

            // move on to the next step
            return 1;
        }
    }
}

