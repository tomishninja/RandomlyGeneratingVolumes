using GenoratingRandomSDF;
using hLSL_Simulator;
using profiler;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace GenoratingRandomSDF
{
    public class ParrellInnerChecker : LinniarInnerChecker
    {
        protected override int RunValidation(ShapeHandeler shapes)
        {
            float yPercenatge = 0;
            float zPercenatge = ((float)incrementor / (float)resolutionToCheckVolumeAt);
            Vector3 currentPosition = Vector3.zero;

            int yMin = (int)System.Math.Floor((shapes.CurrentShape.AABB_Min.y + offset.y) * resolutionToCheckVolumeAt);
            int yMax = (int)System.Math.Ceiling((shapes.CurrentShape.AABB_Max.y + offset.y) * resolutionToCheckVolumeAt);
            int xMin = (int)System.Math.Floor((shapes.CurrentShape.AABB_Min.x + offset.x) * resolutionToCheckVolumeAt);
            int xMax = (int)System.Math.Ceiling((shapes.CurrentShape.AABB_Max.x + offset.x) * resolutionToCheckVolumeAt);

            // A value to tell the system where to start looking if it wants to view things deeper
            float percentageResultionOffSetOff = (1 / (float)resolutionToCheckVolumeAt) / 2;
            //float smallerMovementIncrementer = (1 / (float)resolutionToCheckVolumeAt) / resultionMulitiplierForSmallerTolerance;//TODO update this with a better version

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
                            if (IsChild(shapes.CurrentShape, index, ref shapes))
                            {
                                Debug.Log("Not in boundingBox");
                                // hitting the egde of the box

                                profiler.Increment(DataGenerationDataProfiler.OUTSIDE_OF_MINIMAL_AABB);

                                Interlocked.Exchange(ref outputValue, index);
                                state.Break();
                            }
                        }

                        Stack<int> sdfs = shaderSimulator.GetAllValidSDFs(pos);
                        int[] allSDFs = sdfs.ToArray();

                        int amountOfChildren = CountChildrenInStack(shapes.CurrentShape, sdfs, ref shapes);
                        if (amountOfChildren > 1)
                        {
                            Debug.Log("In a sibling");
                            // overlapping children

                            profiler.Increment(DataGenerationDataProfiler.WITHIN_ANOTHER_SDF);

                            Interlocked.Exchange(ref outputValue, GetMax(allSDFs));
                            state.Break();
                        }

                        // Check the parent is in this sdf
                        bool parentExists = false;
                        for (int i = 0; i < allSDFs.Length; i++)
                        {
                            if (shapes.CurrentShape.Equals(shapes.Get(allSDFs[i])))
                            {
                                parentExists = true;
                                break;
                            }
                        }

                        HierachicalObjects child = null;
                        int childIndex = ReturnFirstChild(shapes.CurrentShape, allSDFs, ref child, ref shapes);
                        if (child != null && !parentExists)
                        {
                            Debug.Log("Not In Parent");

                            profiler.Increment(DataGenerationDataProfiler.OUTSIDE_OF_PARENT);

                            // Child is outside of the bounds of the parent
                            Interlocked.Exchange(ref outputValue, GetMax(allSDFs));
                            state.Break();
                        }

                        //
                        // Cacluate components like area and such
                        //
                        if (amountOfChildren == 0 && parentExists)
                        {
                            // Child is outside of the bounds of the parent
                            parentsNewAmountOfVoxelsWithin++;
                        }
                        else if (child != null)
                        {
                            // in here update the child checks
                            child.AmountOfVoxelsWithin++;
                            child.AABB_Min = Vector3.Min(child.AABB_Min, pos);
                            child.AABB_Max = Vector3.Max(child.AABB_Max, pos);
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