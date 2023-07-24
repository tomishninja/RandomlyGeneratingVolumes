using GenoratingRandomSDF;
using UnityEngine;

[System.Serializable]
public class GeneratingSDFLogicCheckerFactory
{
    SphericalVolumeHierarchyLevelDetails conditionDetails;
    [SerializeField] profiler.AbstractProfiler dataProfiler;
    [SerializeField] RandomChildGeneratorFactory randomChildGeneratorFactory;

    [Header("Details For Adding new SDFs to the volume")]
    [SerializeField] ParametersForAddIngSDFs parametersForAddingSDFs;

    [Header("General Verification Checkers")]
    [SerializeField] float SDFTolerance = 0.01f;
    [SerializeField] float NoiseMultiplier = 0.001f;
    [SerializeField] int randomCords = 8;
    hLSL_Simulator.NoisyHierarchicalSpheres shaderSimulator;
    LayerManager layerManager = null;
    [SerializeField] float cordOffset = 0.5f;

    [Header("Outer layer parameters")]
    [SerializeField] float radiusOfOuterSphere = 1f;
    [SerializeField] float amountToShrink = 0.01f;

    [Header("Inner layer parameters")]
    [SerializeField] int Resolution = 128;
    [SerializeField] int ResultionMulitiplierForSmallerTolerance = 4;

    [Header("Oct Tree Parameters")]
    [SerializeField] InnerOctTreeCheck octTreeObject;

    [Header("Debug Options")]
    bool showOutputsOfVolumes = false;

    public void Init(ref profiler.AbstractProfiler dataProfiler, ref ShapeHandeler shapes, ref HashingMatrix hasingMaxtrix, SphericalVolumeHierarchyLevelDetails conditionDetails)
    {
        this.shaderSimulator = new hLSL_Simulator.NoisyHierarchicalSpheres(ref shapes, ref hasingMaxtrix, SDFTolerance, randomCords, NoiseMultiplier);
        this.dataProfiler = dataProfiler;
        randomChildGeneratorFactory = new RandomChildGeneratorFactory();
        this.conditionDetails = conditionDetails;
    }

    public virtual AddLargeSphereToOuter GetAddLargeSphereLogic()
    {
        return new GenoratingRandomSDF.AddLargeSphereToOuter(ref conditionDetails, ref randomChildGeneratorFactory);
    }

    public virtual AddMiddleLayerSDFs GetAddMiddleLayerSDFLogic()
    {
        return new GenoratingRandomSDF.AddMiddleLayerSDFs(ref conditionDetails, ref dataProfiler, ref parametersForAddingSDFs,  ref randomChildGeneratorFactory);
    }

    public virtual AddSDFsToContainingItem GetAddSmallSDFsToInnerLogic()
    {
        return new GenoratingRandomSDF.AddSDFsToContainingItem(ref conditionDetails, ref dataProfiler, ref parametersForAddingSDFs, ref randomChildGeneratorFactory);
    }

    public virtual GenoratingRandomSDF.OuterSphereCheck GetOuterSphericalCheck()
    {
        return new GenoratingRandomSDF.OuterSphereCheck(shaderSimulator, radiusOfOuterSphere, amountToShrink, cordOffset);
    }

    public virtual GenoratingRandomSDF.OuterBoxCheck GetOuterBoxCheck()
    {
        return new GenoratingRandomSDF.OuterBoxCheck(shaderSimulator, cordOffset, amountToShrink);
    }

    public virtual GenoratingRandomSDF.LinniarInnerChecker GetLinniarInnerChecker()
    {
        InnerLayerParrellelFinalChecker output = new InnerLayerParrellelFinalChecker();

        output.Init(ref shaderSimulator, ref dataProfiler, Resolution, cordOffset, ResultionMulitiplierForSmallerTolerance);

        return output;
    }

    public virtual GenoratingRandomSDF.UnoptimiseLinearSearch GetUnoptimiseLinearSearch()
    {
        UnoptimiseLinearSearch output = new UnoptimiseLinearSearch();

        output.Init(ref shaderSimulator, ref dataProfiler, Resolution, cordOffset, ResultionMulitiplierForSmallerTolerance);

        return output;
    }
    public virtual GenoratingRandomSDF.InnerLayerParrellelFinalChecker GetParrellelFinalChecker()
    {
        InnerLayerParrellelFinalChecker output = new InnerLayerParrellelFinalChecker();

        output.Init(ref shaderSimulator, ref dataProfiler, Resolution, cordOffset, ResultionMulitiplierForSmallerTolerance);

        return output;
    }

    public virtual ParrellInnerChecker GetParrellInnerChecker()
    {
        ParrellInnerChecker output = new ParrellInnerChecker();

        output.Init(ref shaderSimulator, ref dataProfiler, Resolution, cordOffset, ResultionMulitiplierForSmallerTolerance);

        return output;
    }

    public virtual InnerOctTreeCheck GetInnerOctTreeChecker()
    {
        this.octTreeObject.Init(ref shaderSimulator, ref dataProfiler, 8, cordOffset);

        return this.octTreeObject;
    }

    public virtual NoCheck GetNoChecker()
    {
        return new NoCheck();
    }
}
