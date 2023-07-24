using octTree;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System;
using UnityEngine;
using hLSL_Simulator;
using profiler;

namespace GenoratingRandomSDF
{

    /// <summary>
    /// The verification methods for an octree.
    ///    
    /// </summary>
    public class OctTreeVerificationMethods
    {
        /// <summary>
        /// The controller object for the SDF generator.
        /// </summary>
        hLSL_Simulator.NoisyHierarchicalSpheres shaderSimulator;

        ShapeHandeler shapes;

        profiler.AbstractProfiler dataProfiler;

        InnerObjectCheckerParent verificationMethods;

        /// <summary>
        /// Initializes a new instance of the <see cref="OctTreeVerificationMethods"/> class.
        /// </summary>
        /// <param name="shaderSimulator">The controller object for the SDF generator.</param>
        public OctTreeVerificationMethods(ref NoisyHierarchicalSpheres shaderSimulator, ref profiler.AbstractProfiler dataProfiler, InnerObjectCheckerParent verificationMethods)
        {
            this.shaderSimulator = shaderSimulator;
            this.dataProfiler = dataProfiler;
            this.verificationMethods = verificationMethods;
        }

        /// <summary>
        /// Gets the number of leaf nodes at a specified depth.
        /// </summary>
        /// <param name="depth">The depth to get the number of leaf nodes for.</param>
        /// <returns>The number of leaf nodes at the specified depth.</returns>
        public static int GetNumberOfLeafNodes(int depth)
        {
            return (int)Math.Pow(8, depth);
        }

        /// <summary>
        /// Verifies the inner nodes of an octree in a linear fashion.
        /// </summary>
        /// <param name="node">The node to verify.</param>
        /// <param name="outputValue">The output value to return if verification fails.</param>
        /// <returns>The output value or the maximum SDF ID found.</returns>
        public int LinearVerifyInnerAtPositionForOctTree(OctreeNode node, ShapeHandeler shapes, int outputValue = int.MinValue)
        {
            Vector3 pos = node.Center;

            Stack<int> sdfs = shaderSimulator.GetAllValidSDFs(pos, node.SphericalRadius());

            if (sdfs.Count == 0)
            {
                return outputValue;
            }

            int[] allSDFs = sdfs.ToArray();

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
            int childIndex = this.verificationMethods.ReturnFirstChild(shapes.CurrentShape, allSDFs, ref child, ref shapes);
            if (child != null && !parentExists)
            {
                Debug.Log("Not In Parent");

                dataProfiler.Increment(DataGenerationDataProfiler.OUTSIDE_OF_PARENT);

                // Child is outside of the bounds of the parent
                return this.verificationMethods.GetMax(allSDFs);
            }

            if (!parentExists)
            {
                return outputValue;
            }

            int amountOfChildren = this.verificationMethods.CountChildrenInStack(shapes.CurrentShape, sdfs, ref shapes);
            if (amountOfChildren > 1)
            {
                Debug.Log("In a sibling");
                // overlapping children

                dataProfiler.Increment(DataGenerationDataProfiler.WITHIN_ANOTHER_SDF);

                return this.verificationMethods.GetMax(allSDFs);
            }

            //
            // Cacluate components like area and such
            //
            // Amuount of voxels this takes up
            int amouontOfVoxelsThisIs = node.AmountOfLeaves_AssumesCube;
            if (amountOfChildren == 0 && parentExists)
            {
                // Child is outside of the bounds of the parent
                this.verificationMethods.parentsNewAmountOfVoxelsWithin += amouontOfVoxelsThisIs;
            }
            else if (child != null)
            {
                // in here update the child checks
                child.AmountOfVoxelsWithin += amouontOfVoxelsThisIs;
                child.AABB_Min = Vector3.Min(child.AABB_Min, pos);
                child.AABB_Max = Vector3.Max(child.AABB_Max, pos);
            }

            return outputValue;
        }

        /// <summary>
        /// Verifies the inner nodes of an octree in a parell fashion.
        /// </summary>
        /// <param name="node">The node to verify.</param>
        /// <param name="outputValue">The output value to return if verification fails.</param>
        /// <returns>The output value or the maximum SDF ID found.</returns>
        public int ParrellVerifyInnerAtPositionForOctTree(OctreeNode node, ParallelLoopState state, int outputValue = int.MinValue)
        {
            Vector3 pos = node.Center;


            Stack<int> sdfs = shaderSimulator.GetAllValidSDFs(pos, node.SphericalRadius());
            int[] allSDFs = sdfs.ToArray();


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
            int childIndex = this.verificationMethods.ReturnFirstChild(shapes.CurrentShape, allSDFs, ref child, ref shapes);
            if (child != null && !parentExists)
            {
                Debug.Log("Not In Parent");

                dataProfiler.Increment(DataGenerationDataProfiler.OUTSIDE_OF_PARENT);

                // Child is outside of the bounds of the parent
                Interlocked.Exchange(ref outputValue, this.verificationMethods.GetMax(allSDFs));
                state.Break();
            }

            if (!parentExists)
            {
                return outputValue;
            }

            int amountOfChildren = this.verificationMethods.CountChildrenInStack(shapes.CurrentShape, sdfs, ref shapes);
            if (amountOfChildren > 1)
            {
                Debug.Log("In a sibling");
                // overlapping children

                dataProfiler.Increment(DataGenerationDataProfiler.WITHIN_ANOTHER_SDF);

                Interlocked.Exchange(ref outputValue, this.verificationMethods.GetMax(allSDFs));
                state.Break();
            }

            //
            // Cacluate components like area and such
            //
            int amouontOfVoxelsThisIs = node.AmountOfLeaves_AssumesCube;
            if (amountOfChildren == 0 && parentExists)
            {
                // Child is outside of the bounds of the parent
                this.verificationMethods.parentsNewAmountOfVoxelsWithin += amouontOfVoxelsThisIs;
            }
            else if (child != null)
            {
                // in here update the child checks
                child.AmountOfVoxelsWithin += amouontOfVoxelsThisIs;
                child.AABB_Min = Vector3.Min(child.AABB_Min, pos);
                child.AABB_Max = Vector3.Max(child.AABB_Max, pos);
            }

            return outputValue;
        }
    }
}
