using GeneratingRandomSDF;
using UnityEngine;

/// <summary>
/// Factory that creates the concrete <see cref="IVerification"/> strategy instances
/// (outer check, container check, inner check) used by <see cref="CheckingStateController"/>.
/// All configuration is driven by Unity's Inspector.
/// </summary>
[System.Serializable]
public class GeneratingSDFLogicCheckerFactory
{
    SphericalVolumeHierarchyLevelDetails conditionDetails;
    [SerializeField] profiler.AbstractProfiler dataProfiler;
    [SerializeField] RandomChildGeneratorFactory randomChildGeneratorFactory;

    [Header("Details For Adding new SDFs to the volume")]
    [SerializeField] ParametersForAddIngSDFs parametersForAddingSDFs;

    [Header("General Verification Checkers")]
    hLSL_Simulator.NoisyHierarchicalSpheres shaderSimulator;
    LayerManager layerManager = null;
    [SerializeField] float cordOffset = 0.5f;

    [Header("Outer layer parameters")]
    [SerializeField] float radiusOfOuterSphere = 1f;
    [SerializeField] float amountToShrink = 0.01f;

    [Header("Inner layer parameters")]
    [SerializeField] int Resolution = 128;
    [SerializeField] int ResolutionMultiplierForSmallerTolerance = 4;

    [Header("Oct Tree Parameters")]
    [SerializeField] InnerOctTreeCheck octTreeObject;

    [Header("Debug Options")]
    bool showOutputsOfVolumes = false;

    public void Init(ref profiler.AbstractProfiler dataProfiler, ref ShapeHandler shapes, ref HashingMatrix hasingMaxtrix, SphericalVolumeHierarchyLevelDetails conditionDetails)
    {
        this.shaderSimulator = new hLSL_Simulator.NoisyHierarchicalSpheres(ref shapes, ref hasingMaxtrix);
        this.dataProfiler = dataProfiler;
        randomChildGeneratorFactory = new RandomChildGeneratorFactory();
        this.conditionDetails = conditionDetails;
    }

    public virtual AddLargeSphereToOuter GetAddLargeSphereLogic()
    {
        return new GeneratingRandomSDF.AddLargeSphereToOuter(ref conditionDetails, ref randomChildGeneratorFactory);
    }

    public virtual AddMiddleLayerSDFs GetAddMiddleLayerSDFLogic()
    {
        return new GeneratingRandomSDF.AddMiddleLayerSDFs(ref conditionDetails, ref dataProfiler, ref parametersForAddingSDFs,  ref randomChildGeneratorFactory);
    }

    public virtual AddSDFsToContainingItem GetAddSmallSDFsToInnerLogic()
    {
        return new GeneratingRandomSDF.AddSDFsToContainingItem(ref conditionDetails, ref dataProfiler, ref parametersForAddingSDFs, ref randomChildGeneratorFactory);
    }

    public virtual GeneratingRandomSDF.OuterSphereCheck GetOuterSphericalCheck()
    {
        return new GeneratingRandomSDF.OuterSphereCheck(shaderSimulator, radiusOfOuterSphere, amountToShrink, cordOffset);
    }

    public virtual GeneratingRandomSDF.OuterBoxCheck GetOuterBoxCheck()
    {
        return new GeneratingRandomSDF.OuterBoxCheck(shaderSimulator, cordOffset, amountToShrink);
    }

    public virtual GeneratingRandomSDF.LinearInnerChecker GetLinearInnerChecker()
    {
        InnerLayerParallelFinalChecker output = new InnerLayerParallelFinalChecker();

        output.Init(ref shaderSimulator, ref dataProfiler, Resolution, cordOffset, ResolutionMultiplierForSmallerTolerance);

        return output;
    }

    public virtual GeneratingRandomSDF.UnoptimiseLinearSearch GetUnoptimiseLinearSearch()
    {
        UnoptimiseLinearSearch output = new UnoptimiseLinearSearch();

        output.Init(ref shaderSimulator, ref dataProfiler, Resolution, cordOffset, ResolutionMultiplierForSmallerTolerance);

        return output;
    }
    public virtual GeneratingRandomSDF.InnerLayerParallelFinalChecker GetParrellelFinalChecker()
    {
        InnerLayerParallelFinalChecker output = new InnerLayerParallelFinalChecker();

        output.Init(ref shaderSimulator, ref dataProfiler, Resolution, cordOffset, ResolutionMultiplierForSmallerTolerance);

        return output;
    }

    public virtual ParallelInnerChecker GetParallelInnerChecker()
    {
        ParallelInnerChecker output = new ParallelInnerChecker();

        output.Init(ref shaderSimulator, ref dataProfiler, Resolution, cordOffset, ResolutionMultiplierForSmallerTolerance);

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
