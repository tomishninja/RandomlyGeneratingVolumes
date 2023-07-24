using UnityEngine;

namespace GenoratingRandomSDF
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

        public override int AddSDFs(ref ShapeHandeler shapes)
        {
            HierachicalObjects shape = new NoisySphereShapeGenerationHelper();

            shape.positon = Vector3.zero;
            shape.radius = currentLevel.OuterRadius;
            shape.importance = currentLevel.OuterImportance;
            shape.color = currentLevel.OuterColor;
            /*int amountOfChildren = randomChildGeneratorFactory.GetChildGenoratorFor(
                currentLevel..min,
                currentLevel.ContainableAmountOfChildren.max,
                1).GenerateARadomAmountOfChildren();
            Debug.Log("Amount of children: " + amountOfChildren);
            shape.Children = new HierachicalObjects[amountOfChildren];*/
            trackingVariables.AmountOfOuters = 1;

            shapes.Set(0, shape);

            // move on to the next step
            return 1;
        }
    }
}

