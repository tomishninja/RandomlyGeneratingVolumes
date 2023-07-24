using GenoratingRandomSDF;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace octTree
{
    public class OctreeNode
    {
        private static float squareRootOfThreeHalved = 0.86602540378f;

        private Vector3 min;
        private Vector3 max;
        private OctreeNode[] children;
        private int depth;

        public Vector3 Center { get => (max + min) / 2f; }

        public OctreeNode(Vector3 min, Vector3 max, int depth)
        {
            this.min = min;
            this.max = max;
            this.children = new OctreeNode[8];
            this.depth = depth;
        }

        public Vector3 Min
        {
            get { return min; }
        }

        public Vector3 Max
        {
            get { return max; }
        }

        public Vector3 Size
        {
            get { return max - min; }
        }

        public bool IsLeaf
        {
            get { return children == null; }
        }

        public OctreeNode[] Children
        {
            get { return children; }
        }

        public int AmountOfLeaves_AssumesCube
        {
            get { return (int)Math.Pow(8, depth); }
        }

        public float SphericalRadius()
        {
            Vector3 size = Size;
            float HighestSize = Mathf.Max(size.x, Mathf.Max(size.y, size.z));
            return squareRootOfThreeHalved * HighestSize;
        }


        public void Subdivide(int depth)
        {
            if (depth <= 0)
            {
                this.children = null;
                return;
            }

            Vector3 size = Size / 2;
            Vector3 center = min + size;

            children[0] = new OctreeNode(min, center, depth);
            children[1] = new OctreeNode(new Vector3(center.x, min.y, min.z), new Vector3(max.x, center.y, center.z), depth);
            children[2] = new OctreeNode(new Vector3(center.x, min.y, center.z), new Vector3(max.x, center.y, max.z), depth);
            children[3] = new OctreeNode(new Vector3(min.x, min.y, center.z), new Vector3(center.x, center.y, max.z), depth);
            children[4] = new OctreeNode(new Vector3(min.x, center.y, min.z), new Vector3(center.x, max.y, center.z), depth);
            children[5] = new OctreeNode(new Vector3(center.x, center.y, min.z), new Vector3(max.x, max.y, center.z), depth);
            children[6] = new OctreeNode(center, max, depth);
            children[7] = new OctreeNode(new Vector3(min.x, center.y, center.z), new Vector3(center.x, max.y, max.z), depth);
            foreach (OctreeNode child in children)
            {
                child.Subdivide(depth - 1);
            }
        }

        public OctreeNode GetNodeContainingPoint(Vector3 point)
        {
            if (!Contains(point))
            {
                return null;
            }

            if (IsLeaf)
            {
                return this;
            }

            foreach (OctreeNode child in children)
            {
                OctreeNode node = child.GetNodeContainingPoint(point);
                if (node != null)
                {
                    return node;
                }
            }

            return null;
        }

        public int SearchBranch(Func<OctreeNode, ShapeHandeler, int, int> spatialCheck, ShapeHandeler shapes, int expectedAnswer = int.MinValue)
        {
            int result = spatialCheck(this, shapes, expectedAnswer);
            // if the result is the expected answer than this branch is ok and we can prune it
            if (result == expectedAnswer)
            {
                return result;
            }

            // if it is not then we need to check the rest of the branches 
            if (children != null)
            {
                for (int i = 0; i < 8; i++)
                {
                    if (children[i] != null)
                    {
                        result = children[i].SearchBranch(spatialCheck, shapes, expectedAnswer);
                        if (result != expectedAnswer)
                        {
                            return result;
                        }
                    }
                }
            }
            
            return expectedAnswer;
        }

        public bool Contains(Vector3 point)
        {
            return point.x >= min.x && point.x <= max.x
                && point.y >= min.y && point.y <= max.y
                && point.z >= min.z && point.z <= max.z;
        }

        public List<OctreeNode> GetNodesAtDepth(int depth)
        {
            List<OctreeNode> nodes = new List<OctreeNode>();

            if (depth == 0)
            {
                nodes.Add(this);
                return nodes;
            }

            if (!IsLeaf)
            {
                foreach (OctreeNode child in children)
                {
                    nodes.AddRange(child.GetNodesAtDepth(depth - 1));
                }
            }

            return nodes;
        }

        internal int SearchBranch(object linearVerifyInnerAtPositionForOctTree, ShapeHandeler shapes)
        {
            throw new NotImplementedException();
        }
    }
}

