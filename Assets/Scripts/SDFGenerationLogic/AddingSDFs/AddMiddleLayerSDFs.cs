using profiler;
using UnityEngine;

namespace GeneratingRandomSDF
{
    public class AddMiddleLayerSDFs : AbstractAddSDFLogic
    {
        public AddMiddleLayerSDFs(ref SphericalVolumeHierarchyLevelDetails conditionDetails, ref profiler.AbstractProfiler dataProfiler, ref ParametersForAddIngSDFs parameters, ref RandomChildGeneratorFactory randomChildGeneratorFactory) : base()
        {
            this.currentLevel = conditionDetails;
            this.dataProfiler = dataProfiler;
            this.parameters = parameters;
            this.randomChildGeneratorFactory = randomChildGeneratorFactory;
        }

        public override int AddSDFs(ref ShapeHandler shapes)
        {
            if (shapes.CurrentShape == null) throw new System.ArgumentException("No Parent Provided");

            // variables that control if placement doesn't work out and how long to try for
            int failCount = 0;
            bool failed = false;

            trackingVariables.AmountOfCountables = currentLevel.AmountOfCountables;
            int AmountOfCountablesLeftToCount = trackingVariables.AmountOfCountables;
            trackingVariables.AmountOfContainers = currentLevel.AmountOfContainers;
            trackingVariables.AmountOfCountablesInsideAnother = 0;
            int AmountOfNotCountables = currentLevel.AmountOfNotCountables;

            shapes.CurrentShape.Children = new HierarchicalObjects[trackingVariables.AmountOfContainers + AmountOfCountablesLeftToCount + AmountOfNotCountables];

            // Determine where to start in the array
            int index = 0;
            // Work out where we should be in the shape array
            if (shapes.CurrentShape.Children[0] == null || shapes.CurrentShape.Children[0].IsDefault())
            {
                for (; index < shapes.Length; index++)
                {
                    if (shapes.Get(index) == null || shapes.Get(index).IsDefault())
                        break;
                }
            }
            else
            {
                // if we are removing a existing child then we need to find the matching one in the other array
                for (; index < shapes.Length; index++)
                {
                    if (shapes.CurrentShape.Children[0].Equals(shapes.Get(index)))
                        break;
                }
            }

            int startingIndex = index;

            // Generate a random amount of children at the start, this may fail but it should prevent the system from picking easy answers. 
            RandomChildrenAllocationSystem randomChildrenAllocationSystem = randomChildGeneratorFactory.GetChildGeneratorFor(currentLevel.ContainableAmountOfChildren.min, currentLevel.ContainableAmountOfChildren.max, currentLevel.AmountOfContainers);

            // code that ensures the amount of countables is less than the total amount,
            // If this dosn't work it within a realistic time it will also change the seed as the system may be in a loop
            int CompleteAmountOfChildren;
            int failSafeIndex = 0, doubleFailSafe = 0;
            do
            {
                CompleteAmountOfChildren = randomChildrenAllocationSystem.GenerateARandomAmountOfChildren(true);
                failSafeIndex++;
                if (failSafeIndex > 1500)
                {
                    UnityEngine.Random.InitState((int)(Time.time * 7919));
                    failSafeIndex = 0;
                    doubleFailSafe++;

                    if (doubleFailSafe > 1500)
                    {
                        Debug.LogError("Parameter issue no child small enough can be found please stop program to fix Deactivating This script");
                        doubleFailSafe = 0;
                        return -1;
                    }
                }
            } while (CompleteAmountOfChildren >= currentLevel.AmountOfCountables);

            do
            {
                // don't keep looping though this unless it fails
                failed = false;
                int iteration = 0;

                // Generate a purmuation to use for this time
                int[] amountOfChildrenForEachContainer = randomChildrenAllocationSystem.GetRandomPermitationFor(CompleteAmountOfChildren);

                try
                {

                    for (; iteration < System.Math.Min(shapes.CurrentShape.Children.Length, currentLevel.AmountOfContainers); iteration++)
                    {
                        // create the shape we are trying to create
                        HierarchicalObjects shape = null;

                        // create the new child and take a guess of some maybe appropriate parameters
                        shape = shapes.CurrentShape.DraftNewChild(iteration, currentLevel.ContainableRadiusRange, 3, currentLevel.AmountOfContainers, SphereTolerance: parameters.sphereTolerance);

                        // fill in the information that was missed before
                        shape.importance = currentLevel.ContainableImportance;
                        shape.color = currentLevel.ContainableColor;
                        shape.Children = new HierarchicalObjects[amountOfChildrenForEachContainer[iteration]];

                        shapes.Set(index, shape);
                        index++;
                    }

                    // make sure the volumes for this set are ok or else repete the process
                    CheckToEnsureVolumeHasAppropriateVolume(ref shapes, ref failed, ref failCount);

                    // Newer version
                    AmountOfCountablesLeftToCount -= CompleteAmountOfChildren;
                    trackingVariables.AmountOfCountablesInsideAnother += CompleteAmountOfChildren;
                    // Log the amount of countables that still need to be itterated though
                    trackingVariables.AmountOfCountablesOutside = AmountOfCountablesLeftToCount;

                    // shorten the array now we know how many we should have. 
                    HierarchicalObjects[] tempArr = new HierarchicalObjects[trackingVariables.AmountOfContainers + AmountOfCountablesLeftToCount];
                    for (int i = 0; i < tempArr.Length; i++)
                    {
                        tempArr[i] = shapes.CurrentShape.Children[i];
                    }
                    shapes.CurrentShape.Children = tempArr;

                    int endOfNextLoop = iteration + AmountOfCountablesLeftToCount;
                    // add the required amount of the rest of them with coutable values
                    for (; iteration < endOfNextLoop; iteration++)
                    {
                        // create the shape we are trying to create
                        HierarchicalObjects shape = null;

                        // create the new child and take a guess of some maybe appropriate parameters
                        shape = shapes.CurrentShape.DraftNewChild(iteration, currentLevel.CountableRadiusRange, parameters.AmountOfRandomPointsToGenerate, -1, trackingVariables.AmountOfContainers - 1, SphereTolerance: parameters.sphereTolerance);

                        // fill in the information that was missed before
                        shape.importance = currentLevel.CountableImportance;
                        shape.color = currentLevel.CountableColor;

                        shape.Children = new HierarchicalObjects[0];

                        shapes.Set(index, shape);

                        index++;
                    }

                    CheckToEnsureVolumeHasAppropriateVolume(ref shapes, ref failed, ref failCount);


                    // fill the rest of them with noncoutable values
                    for (; iteration < shapes.CurrentShape.Children.Length; iteration++)
                    {
                        // create the shape we are trying to create
                        HierarchicalObjects shape = null;

                        // create the new child and take a guess of some maybe appropriate parameters
                        shape = shapes.CurrentShape.DraftNewChild(iteration, currentLevel.NotCountableRadiusRange, 5, -1, this.trackingVariables.AmountOfContainers - 1, SphereTolerance: parameters.sphereTolerance);

                        // fill in the information that was missed before
                        shape.importance = currentLevel.NotCountableImportance;
                        shape.color = currentLevel.NotCountableColor;

                        shape.Children = new HierarchicalObjects[0];

                        shapes.Set(index, shape);

                        index++;
                    }

                    CheckToEnsureVolumeHasAppropriateVolume(ref shapes, ref failed, ref failCount);
                }
                catch (RanForTooLongException)
                {
                    // the random generator is stuggling to fit the current item
                    // this likly means a earlier one is in its road so we need to start again
                    dataProfiler.Increment(DataGenerationDataProfiler.COULD_NOT_FIT_ALL_OBJECTS_IN_TIME);

                    // Remove all of the children that where created for this iteration
                    for (int ShapesIndex = startingIndex; ShapesIndex < shapes.Length; ShapesIndex++)
                    {
                        shapes.Set(ShapesIndex, null);
                    }

                    index = startingIndex;

                    AmountOfCountablesLeftToCount = currentLevel.AmountOfCountables;

                    shapes.CurrentShape.Children = new HierarchicalObjects[currentLevel.AmountOfContainers + AmountOfCountablesLeftToCount];


                    if (failCount > 100)
                    {
                        failCount = 0;

                        // Change the randomiser seed as it seems to be failing
                        UnityEngine.Random.InitState((int)(Time.time * 7919));

                        //Random.InitState(System.Convert.ToInt32(System.DateTime.Now.Ticks));
                        throw new RanForTooLongException("Cant fill this region up with current items");
                    }

                    // repeat the loop
                    failed = true;
                    failCount++;
                }
                catch (System.Exception ex)
                {
                    Debug.LogError(ex.Message + "\n" + ex.StackTrace);

                    dataProfiler.Increment(DataGenerationDataProfiler.UNEXPECTED_ERROR);

                    if (shapes.CurrentShape.Children != null)
                        shapes.CurrentShape.Children = new HierarchicalObjects[shapes.CurrentShape.Children.Length];
                    else
                        shapes.CurrentShape.Children = new HierarchicalObjects[0];

                    throw new System.Exception("Found a Unexpected erorr: " + ex.Message);

                    // This is normally caused by a unforseen error so it it happens I want to get out of this loop as fast as possible
                }
            } while (failed);

            return 1;
        }
    }
}

