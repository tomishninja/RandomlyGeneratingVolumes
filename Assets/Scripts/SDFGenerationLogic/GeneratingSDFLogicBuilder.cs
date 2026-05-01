using GeneratingRandomSDF;
using UnityEngine;

/// <summary>
/// Builder that constructs the full generation pipeline for one study session.
/// Instantiates and wires together the <see cref="StateControllerForAddingSDFs"/>,
/// <see cref="CheckingStateController"/>, and <see cref="LayerManager"/> objects
/// from Inspector-configured parameters.
/// </summary>
[System.Serializable]
public class GeneratingSDFLogicBuilder
{
    private enum TypeOfContatiner
    {
        Cube = 0,
        Sphere
    }

    private enum TypeOfInnerCheck
    {
        LiniarOptimised,
        ParrellUnoptimised,
        LiniarOctTree,
        LiniarOctTreeWithParrellUnoptomisedCheck
    }

    [SerializeField] GeneratingSDFLogicCheckerFactory factory;
    [SerializeField] private TypeOfContatiner collidertype = TypeOfContatiner.Cube;
    [SerializeField] private TypeOfInnerCheck innerCheckType = TypeOfInnerCheck.LiniarOptimised;

    public void Init(ref profiler.AbstractProfiler profiler, ref ShapeHandler shapes, ref HashingMatrix hashingMatrix, SphericalVolumeHierarchyLevelDetails conditionDetails)
    {
        factory.Init(ref profiler, ref shapes, ref hashingMatrix, conditionDetails);
    }

    public StateControllerForAddingSDFs BuildControllerForAddingSDFs()
    {
        return new StateControllerForAddingSDFs(
            factory.GetAddLargeSphereLogic(),
            factory.GetAddSmallSDFsToInnerLogic(),
            factory.GetAddMiddleLayerSDFLogic()
            );
    }

    public CheckingStateController BuildControllerForCheckingVolumes()
    {
        IVerification outer = null;
        switch (collidertype)
        {
            case TypeOfContatiner.Cube:
                outer = factory.GetOuterBoxCheck();
                break;
            case TypeOfContatiner.Sphere:
                outer = factory.GetOuterSphericalCheck();
                break;
        }

        IVerification inner = null;
        IVerification contained = null;
        switch (innerCheckType)
        {
            case TypeOfInnerCheck.LiniarOptimised:
                inner = factory.GetLinearInnerChecker();
                contained = inner;
                break;
            case TypeOfInnerCheck.ParrellUnoptimised:
                inner = factory.GetParallelInnerChecker();
                contained = inner;
                break;
            case TypeOfInnerCheck.LiniarOctTree:
            case TypeOfInnerCheck.LiniarOctTreeWithParrellUnoptomisedCheck:
                inner = factory.GetInnerOctTreeChecker();
                contained = inner;
                break;
        }

        return new CheckingStateController(ref outer, ref contained, ref inner);
    }

    public LayerManager CreateAndSetLayerMangerFor(ref CheckingStateController controllerA, ref StateControllerForAddingSDFs controllerB)
    {
        LayerManager layermanager = new LayerManager(ref controllerA, ref controllerB);

        controllerA.SetLayerManager(ref layermanager);

        return layermanager;
    }

    public IVerification CreateFinalCheckLogic()
    {
        switch (this.innerCheckType)
        {
            case TypeOfInnerCheck.LiniarOctTreeWithParrellUnoptomisedCheck:
                return factory.GetParrellelFinalChecker();
            default:
                return factory.GetNoChecker();
        }
    }
}
