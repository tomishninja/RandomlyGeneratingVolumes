using profiler;
using UnityEngine;

namespace GenoratingRandomSDF
{
    [System.Serializable]
    public class AddSDFsToContainingItem : AbstractAddSDFLogic
    {
        public AddSDFsToContainingItem(ref SphericalVolumeHierarchyLevelDetails levelDetails, ref profiler.AbstractProfiler dataProfiler, ref ParametersForAddIngSDFs parameters,
            ref RandomChildGeneratorFactory randomChildGeneratorFactory, bool showOutputs = false) : base()
        {
            this.currentLevel = levelDetails;
            this.dataProfiler = dataProfiler;
            this.parameters = parameters;
            this.randomChildGeneratorFactory = randomChildGeneratorFactory;
            this.showOutputsOfVolumes = showOutputs;
        }

        public override int AddSDFs(ref ShapeHandeler shapes)
        {
            // Veribles that control if veriables don't work out and how long to try for
            int amountOfFails = 0;
            bool failed = false;

            do
            {
                // don't keep looping though this unless it fails
                failed = false;

                try
                {
                    int itteration = 0;

                    for (; itteration < shapes.CurrentShape.Children.Length; itteration++)
                    {
                        int index = 0;

                        // Work out where we should be in the shape array
                        if (shapes.CurrentShape.Children[itteration] == null || shapes.CurrentShape.Children[itteration].IsDefault())
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
                                if (shapes.CurrentShape.Children[itteration].Equals(shapes.Get(index)))
                                    break;
                            }
                        }

                        // create the shape we are trying to create
                        HierachicalObjects shape = null;

                        // create the new child and take a guess of some maybe appropriate parameters
                        shape = shapes.CurrentShape.DraftNewChild(itteration, currentLevel.CountableRadiusRange, parameters.AmountOfRandomPointsToGenerate, shapes.CurrentShape.Children.Length, 0, SphereTolerance: parameters.sphereTollerance);

                        // fill in the information that was missed before
                        shape.importance = currentLevel.CountableImportance;
                        shape.color = currentLevel.CountableColor;

                        shape.Children = new HierachicalObjects[0];

                        shapes.Set(index, shape);
                    }

                    // make sure the volumes for this set are ok or else repete the process
                    CheckToEnsureVolumeHasAppropriateVolume(ref shapes, ref failed, ref amountOfFails);

                }
                catch (RanForTooLongException)
                {
                    dataProfiler.Increment(DataGenerationDataProfiler.COULD_NOT_FIT_ALL_OBJECTS_IN_TIME);

                    //
                    shapes.Empty();

                    // the random generator is stuggling to fit the current item
                    // this likly means a earlier one is in its road so we need to start again
                    if (amountOfFails > parameters.amountOfTimesAddingTryingToFitSDFDataBeforeHigherException)
                    {
                        amountOfFails = 0;

                        UnityEngine.Random.InitState((int)(Time.time * 7919));

                        throw new RanForTooLongException("Cant fill this region up with current items");
                    }

                    // repeat the loop
                    failed = true;
                    amountOfFails++;
                }
                catch (System.Exception ex)
                {
                    Debug.LogError(ex.Message + "\n" + ex.StackTrace);

                    dataProfiler.Increment(DataGenerationDataProfiler.UNEXPECTED_ERROR);

                    if (shapes.CurrentShape.Children != null)
                        shapes.CurrentShape.Children = new HierachicalObjects[shapes.CurrentShape.Children.Length];
                    else
                        shapes.CurrentShape.Children = new HierachicalObjects[0];

                    // This is normally caused by a unforseen error so it it happens I want to get out of this loop as fast as possible
                }
            } while (failed);

            return 1;
        }
    }
}

