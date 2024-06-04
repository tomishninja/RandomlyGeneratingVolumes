using hLSL_Simulator;
using octTree;
using profiler;
using UnityEngine;


namespace GenoratingRandomSDF
{
    [System.Serializable]
    public class InnerOctTreeCheck : InnerObjectCheckerParent, IVerification
    {
        enum OctTreeResolution : int
        {
            _1x1x1 = 0,
            _8x8x8 = 1,
            _64x64x64 = 2,
            _512x512x512 = 3,
            _4096x4096x4096 = 4,
            _32768x32768x32768 = 5,
            _262144x262144x262144 = 6,
            _2097152x2097152x2097152 = 7,
        }

        [SerializeField] OctTreeResolution OctTreeMaxDepth = OctTreeResolution._512x512x512;
        [SerializeField] OctTreeResolution OctTreeStartingDepth = OctTreeResolution._8x8x8;
        private OctreeNode[] StartingNodes = null;
        OctTree octTree = null;
        OctTreeVerificationMethods OctTreeMethodBehaviour = null;

        // Testing how the contrustors work on serilized objects
        public InnerOctTreeCheck() : base(0) { }

        Vector3 offset;

        public void Init(ref NoisyHierarchicalSpheres shaderSimulator, ref AbstractProfiler profiler, int endOfCheck, float _offset)
        {
            base.Init(ref shaderSimulator, ref profiler, endOfCheck);

            octTree = new OctTree(-offset, offset, (int)OctTreeMaxDepth);
            this.offset = new Vector3(_offset, _offset, _offset);

            OctTreeMethodBehaviour = new OctTreeVerificationMethods(ref shaderSimulator, ref profiler, this);

            // If the array of starting nodes dosn't exist create it. note long function
            if (StartingNodes == null)
            {
                StartingNodes = octTree.GetNodesAtDepth((int)OctTreeStartingDepth).ToArray();
            }
        }

        public void SetLayerManager(ref LayerManager layerManager)
        {
            this.layerManager = layerManager;
        }

        public void Reset()
        {
            incrementor = 0;
            errorHandeler.Reset();
        }

        public int Verify(ref ShapeHandeler shapes)
        {
            int result;
            if (incrementor < StartingNodes.Length)
                result = StartingNodes[incrementor].SearchBranch(OctTreeMethodBehaviour.LinearVerifyInnerAtPositionForOctTree, shapes);
            else
                return this.ItterationCheckWasSuccessful(ref shapes);

            if (result == int.MinValue)
            {
                // the result was successful
                return this.ItterationCheckWasSuccessful(ref shapes);
            }
            else
            {
                return this.ItterationCheckFailed(result, ref shapes);
            }
        }
    }
}