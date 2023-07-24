using hLSL_Simulator;
using profiler;
using System.Collections.Generic;
using UnityEngine;

namespace GenoratingRandomSDF
{
    public class InnerObjectCheckerParent
    {
        protected NoisyHierarchicalSpheres shaderSimulator;
        protected LayerManager layerManager;
        protected profiler.AbstractProfiler profiler;
        protected ErrorHandelerFacade errorHandeler;
        int endOfCheck;

        protected int incrementor = 0;
        public int parentsNewAmountOfVoxelsWithin;

        readonly protected int nextSubprocessAfterAllChecks;

        public InnerObjectCheckerParent()
        {
            this.nextSubprocessAfterAllChecks = 0;
        }

        public InnerObjectCheckerParent(int nextSubprocessAfterAllChecks)
        {
            this.nextSubprocessAfterAllChecks = nextSubprocessAfterAllChecks;
        }

        public virtual void Init(ref NoisyHierarchicalSpheres shaderSimulator, ref AbstractProfiler profiler, int endOfCheck)
        {
            this.shaderSimulator = shaderSimulator;
            this.profiler = profiler;
            this.endOfCheck = endOfCheck;
            this.errorHandeler = new ErrorHandelerFacade();
        }

        protected virtual int ItterationCheckFailed(int innerLayerResult, ref ShapeHandeler shapes, bool doingDoubleCheck = false, bool willMove = true)
        {
            // Change the pixels and set the z index to zero so this can start again
            //currentSubProcess = 0;
            incrementor = 0;
            errorHandeler.IncrementFailsInARow();

            if (errorHandeler.ShouldChangeRandomSeed)
            {
                UnityEngine.Random.InitState((int)(Time.time * 7919));

                if (errorHandeler.ShouldReset)
                {
                    // Empty the shape array where needed
                    shapes.Empty();
                    profiler.Increment(DataGenerationDataProfiler.REPLACED_ALL_CHILDREN);

                    errorHandeler.ItterationCheckFailedResetTasks(errorHandeler.amountOfResets);
                }
                return 0;
            }
            else if (willMove && innerLayerResult > 0)
            {
                try
                {
                    shapes.MoveSDF(shapes.CurrentShape, innerLayerResult);
                    profiler.Increment(DataGenerationDataProfiler.MOVED_A_REGION);

                    // after moving this region then check it again
                    return 1;
                }
                catch (RanForTooLongException)
                {
                    // if the above dosn't work then we want to rearrange the whole volume
                    shapes.Empty();
                    profiler.Increment(DataGenerationDataProfiler.REPLACED_ALL_CHILDREN);

                    Debug.Log("Couldn\'t completly fix volume stucture, making a new one instead");

                    //currentSubProcess = 0;
                    return 0;
                }

            }
            else
            {
                // this is bad and means somehting odd happened best to just rebuild everything
                return 0;
            }
        }

        protected virtual int ItterationCheckWasSuccessful(ref ShapeHandeler shapes, bool doingDoubleCheck = false)
        {
            // increment z for the next frame version
            incrementor++;

            if (!doingDoubleCheck)
                profiler.Increment(DataGenerationDataProfiler.AMOUNT_OF_SUCCESSFUL_LAYERS);

            // this would mean that the image is done and every thing was successful.
            if (incrementor >= endOfCheck)
            {

                // Set everthing up for the next itteration
                incrementor = 0;
                errorHandeler.Reset();

                // increment to the next shape
                if (!shapes.IncrementCurrentShapeToNextShape())
                {
                    if (shapes.CurrentShape.AmountOfParents() == 0)
                    {
                        // this happens with the outer trigger as it has no children yet
                        this.layerManager.SetToContainer();
                        return 0;
                    }
                    else
                    {
                        Debug.Log("Incorrect Number of elements found please fix");
                        // reset the value something is wrong

                        // Empty the shape array where needed
                        shapes.Empty();
                        profiler.Increment(DataGenerationDataProfiler.REPLACED_ALL_CHILDREN);

                        errorHandeler.ItterationCheckFailedResetTasks(errorHandeler.amountOfResets);
                        return 0;
                    }
                }
                else if (!shapes.ShapesAreRemainingToVerify && (!doingDoubleCheck))
                {
                    // the volume is complete
                    return -1;
                }
                else if (shapes.CurrentShape.AmountOfParents() > 0)
                {
                    this.layerManager.SetToInner();

                    // return the new subprocess
                    return nextSubprocessAfterAllChecks;
                }
                else
                {
                    this.layerManager.SetToContainer();
                    return nextSubprocessAfterAllChecks;
                }
            }
            else
            {
                // return the same subprocess before moving on to the next stage
                return 1;
            }
        }

        public int GetMax(int[] input)
        {
            if (input == null || input.Length < 1) throw new System.ArgumentException("Input array may not be null or empty for GetMax function to work");

            int max = input[0];

            for (int index = 1; index < input.Length; index++)
            {
                max = System.Math.Max(max, input[index]);
            }

            return max;
        }

        public bool IsChild(HierachicalObjects parent, int indexOfShape, ref ShapeHandeler shapes)
        {
            if (parent.Children == null) return false;
            for (int i = 0; i < parent.Children.Length; i++)
            {
                if (shapes.Get(indexOfShape).Equals(parent.Children[i]))
                    return true;
            }
            return false;
        }

        public bool IsEdge(int x, int y, int z, int AmountOfChecks)
        {
            return x == 0 || y == 0 || y == 0 || x == AmountOfChecks - 1 || x == AmountOfChecks - 1 || x == AmountOfChecks - 1;
        }

        public int CountChildrenInStack(HierachicalObjects parent, Stack<int> indexs, ref ShapeHandeler shapes)
        {
            int childrenInVoxel = 0;
            while (indexs.Count > 0)
            {
                if (IsChild(parent, indexs.Pop(), ref shapes))
                {
                    childrenInVoxel++;
                }
            }
            return childrenInVoxel;
        }

        public int ReturnFirstChild(HierachicalObjects parent, int[] indexs, ref HierachicalObjects child, ref ShapeHandeler shapes)
        {
            for (int index = 0; index < indexs.Length; index++)
            {
                if (IsChild(parent, indexs[index], ref shapes))
                {
                    child = shapes.Get(indexs[index]);
                    return indexs[index];
                }
            }
            return -1;
        }
    }
}
