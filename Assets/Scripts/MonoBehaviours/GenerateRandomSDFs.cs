using GeneratingRandomSDF;
using profiler;
using UnityEngine;

public class GenerateRandomSDFs : MonoBehaviour
{
    [SerializeField] int startingParticipant = 0;

    [Header("Main Logic Settings")]
    [SerializeField] ParametersForAddIngSDFs AddingShapeparameters;
    [SerializeField] StudyGenerationPlanner conditionDetails;
    [SerializeField] ScreenShot screenShot;

    [SerializeField] GeneratingSDFLogicBuilder builder;

    [SerializeField] fileIO.WriteStudyDesignToFile fileWriter;
    [SerializeField] ErrorHandlerFacade errorHandlerStats;
    [SerializeField] ShapeHandler shapes;
    [SerializeField] HashingMatrix hashingMatrix;

    private SDFGenerator mainLogicOperator;

    [SerializeField] float NoiseMultiplier = 0.1f;

    AbstractProfiler profiler = new DataGenerationDataProfiler();

    int amountOfFilesCreated = 0;
    private int CurrentParticipant { get => startingParticipant + amountOfFilesCreated; }

    [SerializeField] DemoVisualizer[] DemoVisualizers;

    private void Awake()
    {
        screenShot.Init();
    }

    void Start()
    {
        profiler.ClearAndInitProfiler();
        shapes = new ShapeHandler();
        hashingMatrix = HashingMatrix.InitalizeRandomHashingMatrix();

        // Build the main objects
        builder.Init(ref profiler, ref shapes, ref hashingMatrix, conditionDetails.CurrentIteration);
        var controllerForAddingSDFs = builder.BuildControllerForAddingSDFs();
        var controllerForCheckingVolumes = builder.BuildControllerForCheckingVolumes();
        LayerManager layerManager = builder.CreateAndSetLayerMangerFor(ref controllerForCheckingVolumes, ref controllerForAddingSDFs);

        // Initialise condition details once, after the builder has been configured
        conditionDetails.Init(CurrentParticipant);

        mainLogicOperator = new SDFGenerator(
            ref conditionDetails,
            ref profiler,
            fileWriter,
            ref controllerForCheckingVolumes,
            ref controllerForAddingSDFs,
            ref hashingMatrix,
            ref errorHandlerStats,
            shapes,
            NoiseMultiplier,
            builder.CreateFinalCheckLogic(),
            DemoVisualizers
            );
    }

    // Update is called once per frame
    void Update()
    {
        ProcessResult outputFromMainProcess = ProcessResult.InProgress;
        try
        {
            outputFromMainProcess = mainLogicOperator.Process();
        }
        catch (RanForTooLongException ex)
        {
            Debug.LogError(ex.Message);
        }

        // If the main process has finished then take a screenshot for post-processing
        if (outputFromMainProcess == ProcessResult.VisualizationReady)
        {
            screenShot.AttemptToTakeScreenShot(CurrentParticipant, mainLogicOperator.CurrentParticipantConditionIndex);
        }
    }

    private void OnDestroy()
    {
        profiler.ApplicationStopped();
    }

    private void OnApplicationQuit()
    {
        profiler.ApplicationStopped();
    }

    private void OnDisable()
    {
        profiler.ApplicationStopped();
    }
}
