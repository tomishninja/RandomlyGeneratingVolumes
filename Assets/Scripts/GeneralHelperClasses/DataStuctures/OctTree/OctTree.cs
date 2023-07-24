using System;
using System.Collections.Generic;
using UnityEngine;

namespace octTree
{
    public class OctTree
    {
        private OctreeNode root;
        private int maxDepth;

        public OctTree(Vector3 min, Vector3 max, int maxDepth)
        {
            this.root = new OctreeNode(min, max, maxDepth);
            this.maxDepth = maxDepth;
            this.root.Subdivide(maxDepth - 1);
        }

        public OctTree(OctreeNode root, int maxDepth)
        {
            this.root = root;
            this.maxDepth = maxDepth;
        }

        public OctreeNode Root
        {
            get { return root; }
        }

        public OctreeNode GetNodeContainingPoint(Vector3 point)
        {
            if (!root.Contains(point))
            {
                return null;
            }

            OctreeNode currentNode = root;
            while (!currentNode.IsLeaf)
            {
                bool foundChild = false;
                foreach (OctreeNode child in currentNode.Children)
                {
                    if (child.Contains(point))
                    {
                        currentNode = child;
                        foundChild = true;
                        break;
                    }
                }

                if (!foundChild)
                {
                    // The point is on a boundary between OctreeNodes
                    // Return the current node as the one containing the point
                    return currentNode;
                }
            }
            return currentNode;
        }

        public void TraverseTree(Vector3 point, Func<OctreeNode, bool> condition)
        {
            OctreeNode currentNode = GetNodeContainingPoint(point);
            TraverseTreeRecursive(currentNode, point, condition);
        }

        private void TraverseTreeRecursive(OctreeNode node, Vector3 point, Func<OctreeNode, bool> condition)
        {
            // Check if the condition is met for this node
            if (condition(node))
            {
                // Do something with the node, or return it
            }

            // If this is a leaf node, we're done
            if (node.IsLeaf)
            {
                return;
            }

            // Recurse into the appropriate child nodes
            foreach (OctreeNode child in node.Children)
            {
                if (child.Contains(point))
                {
                    TraverseTreeRecursive(child, point, condition);
                }
            }
        }

        public List<OctreeNode> GetNodesAtDepth(int depth)
        {
            return this.root.GetNodesAtDepth(depth);
        }
    }
}
