using System.Collections.Generic;
using UnityEngine;
using profiler;

namespace GenoratingRandomSDF
{
    public class UnoptimiseLinearSearch : LinniarInnerChecker
    {
        protected override int RunValidation(ShapeHandeler shapes)
        {
            float xPercenatge = 0;
            float yPercenatge = 0;
            float zPercenatge = ((float)incrementor / (float)resolutionToCheckVolumeAt);
            Vector3 currentPosition = Vector3.zero;

            int yMin = (int)System.Math.Floor((shapes.CurrentShape.AABB_Min.y + offset.y) * resolutionToCheckVolumeAt);
            int yMax = (int)System.Math.Ceiling((shapes.CurrentShape.AABB_Max.y + offset.y) * resolutionToCheckVolumeAt);
            int xMin = (int)System.Math.Floor((shapes.CurrentShape.AABB_Min.x + offset.x) * resolutionToCheckVolumeAt);
            int xMax = (int)System.Math.Ceiling((shapes.CurrentShape.AABB_Max.x + offset.x) * resolutionToCheckVolumeAt);

            // A value to tell the system where to start looking if it wants to view things deeper
            float percentageResultionOffSetOff = (1 / (float)resolutionToCheckVolumeAt) / 2;
            float smallerMovementIncrementer = (1 / (float)resolutionToCheckVolumeAt) / 4;//TODO update this with a better version

            for (int y = yMin; y < yMax; y++)
            {
                yPercenatge = ((float)y / (float)resolutionToCheckVolumeAt);

                for (int x = xMin; x < xMax; x++)
                {
                    xPercenatge = ((float)x / (float)resolutionToCheckVolumeAt);

                    Vector3 pos = new Vector3(xPercenatge, yPercenatge, zPercenatge) - offset;//parent.AABB_Min;


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

                                return index;
                            }
                        }


                        Stack<int> sdfs = shaderSimulator.GetAllValidSDFs(pos);
                        int[] allSDFs = sdfs.ToArray();

                        bool parentExists = false;

                        int amountOfChildren = CountChildrenInStack(shapes.CurrentShape, sdfs, ref shapes);

                        if (amountOfChildren > 1)
                        {

                            for (float smallerZ = zPercenatge - percentageResultionOffSetOff; smallerZ < zPercenatge + percentageResultionOffSetOff; smallerZ += smallerMovementIncrementer)
                            {

                                for (float smallerY = yPercenatge - percentageResultionOffSetOff; smallerY < yPercenatge + percentageResultionOffSetOff; smallerY += smallerMovementIncrementer)
                                {
                                    for (float smallerX = xPercenatge - percentageResultionOffSetOff; smallerX < xPercenatge + percentageResultionOffSetOff; smallerX += smallerMovementIncrementer)
                                    {
                                        Stack<int> smallerSDFsFoundAtVoxel = shaderSimulator.GetAllValidSDFs(pos, smallerSDFTollerance);
                                        int[] smallerSDFArray = smallerSDFsFoundAtVoxel.ToArray();

                                        amountOfChildren = CountChildrenInStack(shapes.CurrentShape, smallerSDFsFoundAtVoxel, ref shapes);
                                        if (amountOfChildren > 1)
                                        {
                                            // if there are two children in the smaller stack move on
                                            Debug.Log("In a sibling");
                                            // overlapping children

                                            profiler.Increment(DataGenerationDataProfiler.WITHIN_ANOTHER_SDF);

                                            return GetMax(smallerSDFArray);
                                        }
                                        else
                                        {
                                            // Check the parent is in this sdf
                                            for (int i = 0; i < smallerSDFArray.Length; i++)
                                            {
                                                if (shapes.CurrentShape.Equals(shapes.Get(smallerSDFArray[i])))
                                                {
                                                    parentExists = true;
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            // Check the parent is in this sdf
                            for (int i = 0; i < allSDFs.Length; i++)
                            {
                                if (shapes.CurrentShape.Equals(shapes.Get(allSDFs[i])))
                                {
                                    parentExists = true;
                                    break;
                                }
                            }
                        }

                        HierachicalObjects child = null;
                        int childIndex = ReturnFirstChild(shapes.CurrentShape, allSDFs, ref child, ref shapes);
                        if (child != null && !parentExists)
                        {
                            Debug.Log("Not In Parent");

                            profiler.Increment(DataGenerationDataProfiler.OUTSIDE_OF_PARENT);

                            // Child is outside of the bounds of the parent
                            return GetMax(allSDFs);
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

                            //if (currentShapeIndex != 0)
                            //{
                            //    Debug.Log("Has Children");
                            //}

                            //this.Shapes[childIndex].AmountOfVoxelsWithin++;
                            //this.Shapes[childIndex].AABB_Min = Vector3.Max(child.AABB_Min, pos);
                            //this.Shapes[childIndex].AABB_Max = Vector3.Min(child.AABB_Max, pos);
                        }
                    }
                }
            }

            //for (int index = 0; index < parent.Children.Length; index++)
            //{
            //    Debug.Log(parent.Children[index].AABB_Max + "And" + parent.Children[index].AABB_Min);
            //}

            profiler.Increment(DataGenerationDataProfiler.SUCCESS);

            return int.MinValue;
        }
    }
}

