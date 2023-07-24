using GenoratingRandomSDF;
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
    [SerializeField] ErrorHandelerFacade errorHandelerStats;
    [SerializeField] ShapeHandeler shapes;
    [SerializeField] HashingMatrix hashingMatrix;

    private SDFGenorator mainLogicOperator;

    [SerializeField] float NoiseMultiplier = 0.1f;

    AbstractProfiler profiler = new DataGenerationDataProfiler();

    int amountOfFilesCreated = 0;
    private int CurrentParticipant { get => startingParticipant + amountOfFilesCreated; }

    [SerializeField] DemoVisulizer[] DemoVisulizers;

    private void Awake()
    {
        screenShot.Init();
    }

    void Start()
    {
        profiler.ClearAndInitProfiler();
        shapes = new ShapeHandeler();
        hashingMatrix = HashingMatrix.InitalizeRandomHashingMatrix();

        // Set the condition details to the intial values
        conditionDetails.Init(CurrentParticipant);

        // Build the main objects
        builder.Init(ref profiler, ref shapes, ref hashingMatrix, conditionDetails.CurrentItteration);
        var controllerForAddingSDFs = builder.BuildControllerForAddingSDFs();
        var controllerForCheckingVolumes = builder.BuildControllerForCheckingVolumes();
        LayerManager layerManager = builder.CreateAndSetLayerMangerFor(ref controllerForCheckingVolumes, ref controllerForAddingSDFs);

        // Set the condition details to the intial values
        conditionDetails.Init(CurrentParticipant);

        mainLogicOperator = new SDFGenorator(
            ref conditionDetails, 
            ref profiler, 
            fileWriter, 
            ref controllerForCheckingVolumes,
            ref controllerForAddingSDFs,
            ref hashingMatrix,
            ref errorHandelerStats, 
            shapes, 
            NoiseMultiplier,
            builder.CreateFinalCheckLogic(),
            DemoVisulizers
            );
    }

    // Update is called once per frame
    void Update()
    {
        int outputFromMainProcess = -1;
        try
        {
            outputFromMainProcess = mainLogicOperator.Process();
        }
        catch(RanForTooLongException ex)
        {
            Debug.LogError(ex.Message);
        }
        /*catch(System.Exception ex)
        {
            Debug.LogError(ex.Message);
            Debug.LogError(ex.StackTrace);
            this.enabled = false;
        }*/

        // If the main process has finished then we can move onto the next participant
        if (outputFromMainProcess == 1)
        {
            screenShot.AttemptToTakeScreenShot(CurrentParticipant, mainLogicOperator.StudyDesignIndex);
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
