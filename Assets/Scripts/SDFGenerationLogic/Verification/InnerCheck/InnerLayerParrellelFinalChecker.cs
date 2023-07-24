using profiler;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace GenoratingRandomSDF
{
    public class InnerLayerParrellelFinalChecker : LinniarInnerChecker
    {

        public override int Verify(ref ShapeHandeler shapes)
        {
            int result = RunValidation(shapes);

            if (result == int.MinValue)
            {
                // The result was succesful
                return ItterationCheckWasSuccessful(ref shapes);
            }
            else
            {
                profiler.Increment(DataGenerationDataProfiler.TOO_FAILED_SECOND_CHECK);
                // The result was a failure reset the volume and start again
                return this.ItterationCheckFailed(result, ref shapes);
            }
        }

        protected override int RunValidation(ShapeHandeler shapes)
        {
            float yPercenatge = 0;
            float zPercenatge = ((float)incrementor / (float)resolutionToCheckVolumeAt);
            Vector3 currentPosition = Vector3.zero;

            int yMin = (int)System.Math.Floor(-offset.y * resolutionToCheckVolumeAt);
            int yMax = (int)System.Math.Ceiling(offset.y * resolutionToCheckVolumeAt);
            int xMin = (int)System.Math.Floor(-offset.x * resolutionToCheckVolumeAt);
            int xMax = (int)System.Math.Ceiling(offset.x * resolutionToCheckVolumeAt);

            // A value to tell the system where to start looking if it wants to view things deeper
            float percentageResultionOffSetOff = (1 / (float)resolutionToCheckVolumeAt) / 2;

            int outputValue = int.MinValue;

            for (int y = yMin; y < yMax; y++)
            {
                yPercenatge = ((float)y / (float)resolutionToCheckVolumeAt);

                Parallel.For(xMin, xMax, (x, state) =>
                {
                    float xPercenatge = ((float)x / (float)resolutionToCheckVolumeAt);

                    Vector3 pos = new Vector3(xPercenatge, yPercenatge, zPercenatge) - offset;


                    //if (Vector3.Distance(pos, Vector3.zero) > 0.5f) continue; // disabled because various artifacts where being found outside of the bounds with a center point within the radius

                    int index = -1;
                    float d = shaderSimulator.SDF(pos, out index);

                    if (d < this.SDFTollerance)
                    {
                        // Objects must be inside of boudning box
                        if (IsEdge(x, y, incrementor, resolutionToCheckVolumeAt))
                        {
                            Debug.Log("Not in boundingBox");
                            // hitting the egde of the box

                            profiler.Increment(DataGenerationDataProfiler.OUTSIDE_OF_MINIMAL_AABB);

                            Interlocked.Exchange(ref outputValue, index);
                            state.Break();
                        }

                        Stack<int> sdfs = shaderSimulator.GetAllValidSDFs(pos);
                        if (sdfs.Count > 0)
                        {
                            int[] allSDFs = sdfs.ToArray();

                            // if there are more than 3 objects at the given pixel that are valid then this is incorrect
                            if (allSDFs.Length > 3)
                            {
                                Debug.Log("In a sibling");
                                // overlapping children

                                profiler.Increment(DataGenerationDataProfiler.WITHIN_ANOTHER_SDF);

                                Interlocked.Exchange(ref outputValue, GetMax(allSDFs));
                                state.Break();
                            }

                            int outerLayers = 0;
                            int countainers = 0;
                            int amountOfChildren = 0;
                            int outerIndex = -1;
                            int countainerIndex = -1;
                            int childIndex = -1;

                            int childrenInVoxel = 0;
                            for (int i = 0; i < allSDFs.Length; i++)
                            {
                                switch (shapes.Get(allSDFs[i]).importance)
                                {
                                    case 0:
                                        outerLayers++;
                                        outerIndex = allSDFs[i];
                                        break;
                                    case 1:
                                        countainers++;
                                        countainerIndex = allSDFs[i];
                                        break;
                                    case 2:
                                        childrenInVoxel++;
                                        childIndex = allSDFs[i];
                                        break;
                                    default:
                                        break;
                                }
                            }

                            if (amountOfChildren > 1 || countainers > 1 || outerLayers > 1)
                            {
                                Debug.Log("In a sibling");
                                // overlapping children

                                profiler.Increment(DataGenerationDataProfiler.WITHIN_ANOTHER_SDF);

                                Interlocked.Exchange(ref outputValue, GetMax(allSDFs));
                                state.Break();
                            }
                            else if (outerLayers == 0 && (amountOfChildren > 1 || countainers > 1))
                            {
                                Debug.Log("Not In Parent");

                                profiler.Increment(DataGenerationDataProfiler.OUTSIDE_OF_PARENT);

                                // Child is outside of the bounds of the parent
                                Interlocked.Exchange(ref outputValue, GetMax(allSDFs));
                                state.Break();
                            }
                            else if (childIndex >= 0)
                            {
                                if (!(countainers > 1 && (shapes.Get(childIndex).HasAncestor(shapes.Get(countainerIndex))) || shapes.Get(childIndex).HasAncestor(shapes.Get(outerIndex))))
                                {
                                    Debug.Log("Not In Parent");

                                    profiler.Increment(DataGenerationDataProfiler.OUTSIDE_OF_PARENT);

                                    // Child is outside of the bounds of the parent
                                    Interlocked.Exchange(ref outputValue, GetMax(allSDFs));
                                    state.Break();
                                }
                            }
                            else if (countainerIndex >= 0)
                            {
                                if (!shapes.Get(countainerIndex).HasAncestor(shapes.Get(outerIndex)))
                                {
                                    Debug.Log("Not In Parent");

                                    profiler.Increment(DataGenerationDataProfiler.OUTSIDE_OF_PARENT);

                                    // Child is outside of the bounds of the parent
                                    Interlocked.Exchange(ref outputValue, GetMax(allSDFs));
                                    state.Break();
                                }
                            }
                        }
                    }
                });

                if (outputValue != int.MinValue) return outputValue;
            }

            profiler.Increment(DataGenerationDataProfiler.SUCCESS);

            return int.MinValue;
        }
    }
}