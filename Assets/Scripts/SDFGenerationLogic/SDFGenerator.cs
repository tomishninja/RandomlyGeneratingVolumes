using profiler;
using System;
using System.IO;
using UnityEngine;

namespace GeneratingRandomSDF
{
    /// <summary>
    /// Indicates the current phase of a single volume generation cycle.
    /// </summary>
    public enum GenerationPhase
    {
        AddingShapes    =  0,
        VerifyingShapes =  1,
        FinalCheck      =  2,
        Complete        = -1
    }

    /// <summary>
    /// Return value of <see cref="SDFGenerator.Process"/>.
    /// </summary>
    public enum ProcessResult
    {
        InProgress         = 0,
        VisualizationReady = 1
    }

    /// <summary>
    /// Top-level orchestrator for pseudo-random SDF volume generation.
    /// Drives the three-phase pipeline (add shapes → verify shapes → optional final check)
    /// across all study conditions and participants, and writes the resulting JSON output files.
    /// </summary>
    [System.Serializable]
    public class SDFGenerator
    {
        // Logic Objects
        StudyGenerationPlanner conditionDetails;
        AbstractProfiler profiler;
        StudyDetailsForOutput[] StudyDesign;
        private RandomChildGeneratorFactory randomChildGeneratorFactory = new RandomChildGeneratorFactory();

        // Geometric Objects
        ShapeHandler shapes;
        HashingMatrix hashingMatrix;

        IWriteOutputFile outputFileWriter;
        StateControllerForAddingSDFs addingSDFController;
        CheckingStateController checkingTheVolumeController;

        // variables for outputting
        float NoiseMultiplier;

        // Output variables
        int StudyDesignOutputIndex = 0;
        int AmountOfFilesCreated = 0;

        // Counting variables
        public AmountTrackingParameters valuesToTrackVariousAmountsOfThings;

        // Behaviour Logic for the class
        private int currentlevel = 0;
        private GenerationPhase currentPhase = GenerationPhase.AddingShapes;

        ErrorHandlerFacade statsForErrorHandeling;
        IVerification optionalFinalCheck;

        VisualizationAdapter[] visualizations;

        /// <summary>Index of the volume currently being generated within the current participant's session.</summary>
        public int CurrentParticipantConditionIndex { get => StudyDesignOutputIndex; }
        public SphericalVolumeHierarchyLevelDetails CurrentLevel { get => conditionDetails.CurrentIteration; }

        public SDFGenerator(ref StudyGenerationPlanner conditionDetails, ref AbstractProfiler profiler,
            IWriteOutputFile outputFileWriter, ref CheckingStateController checkingTheVolumeController,
            ref StateControllerForAddingSDFs addingSDFController, ref HashingMatrix hashingMatrix,
            ref ErrorHandlerFacade statsForErrorHandeling, ShapeHandler shapes, float NoiseMultiplier,
            IVerification optionalFinalCheck = null, VisualizationAdapter[] visualizations = null)
        {
            this.conditionDetails = conditionDetails;
            this.profiler = profiler;
            this.outputFileWriter = outputFileWriter;
            this.checkingTheVolumeController = checkingTheVolumeController;
            this.addingSDFController = addingSDFController;
            this.statsForErrorHandeling = statsForErrorHandeling;
            this.shapes = shapes;
            this.visualizations = visualizations;

            this.NoiseMultiplier = NoiseMultiplier;

            if (hashingMatrix == null)
                this.hashingMatrix = HashingMatrix.InitalizeRandomHashingMatrix();
            else
                this.hashingMatrix = hashingMatrix;

            // Start up the study design array
            InitalizeStudyDesignArray();

            this.statsForErrorHandeling.Init(this, ref randomChildGeneratorFactory);

            this.optionalFinalCheck = optionalFinalCheck ?? new NoCheck();

            if (shapes == null)
                shapes = new ShapeHandler();

            shapes.Reset(conditionDetails.CurrentIteration.CountAll());
        }

        /// <summary>
        /// Advances the generation pipeline by one step. Call once per frame.
        /// </summary>
        /// <returns>
        /// <see cref="ProcessResult.VisualizationReady"/> when a volume is complete and the
        /// visualization should update; <see cref="ProcessResult.InProgress"/> otherwise.
        /// </returns>
        public ProcessResult Process()
        {
            if (this.shapes.ShapesAreRemainingToVerify)
            {
                if (currentPhase == GenerationPhase.AddingShapes)
                {
                    try
                    {
                        currentPhase = (GenerationPhase)addingSDFController.Get().AddSDFs(ref shapes);
                    }
                    catch (RanForTooLongException)
                    {
                        currentPhase = (GenerationPhase)statsForErrorHandeling.AddSDFsFailed();
                    }
                }
                else if (currentPhase == GenerationPhase.VerifyingShapes)
                {
                    currentPhase = (GenerationPhase)checkingTheVolumeController.Get().Verify(ref shapes);
                }
            }
            else if (currentPhase == GenerationPhase.FinalCheck)
            {
                // A check of the whole volume is done this time
                currentPhase = (GenerationPhase)optionalFinalCheck.Verify(ref shapes);
            }
            else if (this.currentPhase == GenerationPhase.Complete)
            {
                // Create the final output
                StudyDetailsForOutput output = new StudyDetailsForOutput();

                if (visualizations != null)
                {
                    // Update the visualizations
                    for (int index = 0; index < visualizations.Length; index++)
                    {
                        visualizations[index].SetUpVisualization(shapes, this.conditionDetails.GetCurrentCondition());
                        hashingMatrix.SetInShader(visualizations[index].GetMaterial(), "_HashLineA", "_HashLineB", "_HashLineC");
                        output.SetNoiseMultiplier(visualizations[index].GetMaterial());
                    }
                }

                output.shapeDetails = this.shapes.GetShapesAsArray();

                output.answers = AnswersFromGeneration.GenerateAnswers(
                    this.shapes.GetShapesAsArray(),
                    this.conditionDetails.CurrentIteration,
                    this.addingSDFController.GetTheAmountOfinners(),
                    this.addingSDFController.GetTheAmountOfContained(),
                    this.conditionDetails.CurrentIteration.AmountOfContainers,
                    this.conditionDetails.CurrentIteration.AmountOfOuters,
                    this.conditionDetails.CurrentIteration.AmountOfCountables);

                output.hash = hashingMatrix.Clone();
                output.NoiseMultiplier = this.NoiseMultiplier;
                output.ConditionName = this.conditionDetails.GetCurrentCondition();

                // Save the logic profiler data
                try
                {
                    this.profiler.StopTimer();
                    this.profiler.WriteProiferDataAsCSV(System.IO.Path.Combine(Application.dataPath, DataGenerationDataProfiler.NAME_OF_LOGIC_OUTPUT_FILE));
                }
                catch (DirectoryNotFoundException ex)
                {
                    Debug.LogError(ex.Message + "\n" + ex.StackTrace);
                }

                // Depersist the random child count so the next iteration picks a fresh value
                RandomChildrenAllocationSystem allocationSystem = this.randomChildGeneratorFactory.GetChildGeneratorFor(
                    this.conditionDetails.CurrentIteration.ContainableAmountOfChildren.min,
                    this.conditionDetails.CurrentIteration.ContainableAmountOfChildren.max,
                    this.conditionDetails.CurrentIteration.AmountOfContainers);
                allocationSystem.DepersistRandomValue();

                this.statsForErrorHandeling.Reset();

                this.StudyDesign[StudyDesignOutputIndex] = output;
                this.StudyDesignOutputIndex++;

                this.profiler.Increment(DataGenerationDataProfiler.SUCCESS);

                if (this.StudyDesignOutputIndex >= this.StudyDesign.Length)
                {
                    try
                    {
                        this.profiler.WriteProiferDataAsCSV(System.IO.Path.Combine(Application.dataPath, DataGenerationDataProfiler.NAME_OF_LOGIC_OUTPUT_FILE));
                    }
                    catch (DirectoryNotFoundException ex)
                    {
                        Debug.LogError(ex.Message + "\n" + ex.StackTrace);
                    }

                    this.profiler.ClearAndInitProfiler();

                    // Write the completed participant file then reset the output array
                    this.WriteStudyDesignToFile();
                    this.InitalizeStudyDesignArray();

                    // Reset condition ordering for the next participant
                    this.conditionDetails.GetReadyForNextParticipant();
                }
                else
                {
                    // Advance to the next condition within this participant's session
                    conditionDetails.GetNext();
                    addingSDFController.SetCurrentShapeDetails(this.conditionDetails.CurrentIteration);
                }

                // Prepare state for the next generation cycle
                Reset();

                // Signal the caller that the visualization should be updated / screenshot taken
                return ProcessResult.VisualizationReady;
            }

            return ProcessResult.InProgress;
        }

        private int CountAmountOfRegions()
        {
            return this.conditionDetails.CurrentIteration.AmountOfOuters +
                this.conditionDetails.CurrentIteration.AmountOfContainers +
                this.conditionDetails.CurrentIteration.AmountOfCountables;
        }

        // Set up starting logic for output of the study design Details
        private void InitalizeStudyDesignArray()
        {
            this.StudyDesign = new StudyDetailsForOutput[this.conditionDetails.TotalCount()];
            this.StudyDesignOutputIndex = 0;
        }

        private void WriteStudyDesignToFile()
        {
            outputFileWriter.WriteToFile(StudyDesign, this.conditionDetails.ParticipantID);
            AmountOfFilesCreated++;
        }

        public void Reset()
        {
            // Revert all of the values back to their original ones
            currentlevel = 0;
            currentPhase = GenerationPhase.AddingShapes;

            this.shapes.Reset(this.CountAmountOfRegions());

            this.checkingTheVolumeController.Reset();
            this.addingSDFController.SetToOuter();

            this.statsForErrorHandeling.Reset();

            // Create a random hash to start with
            hashingMatrix = HashingMatrix.InitalizeRandomHashingMatrix();

            try
            {
                this.profiler.StopTimer();
                this.profiler.WriteProiferDataAsCSV(System.IO.Path.Combine(Application.dataPath, DataGenerationDataProfiler.NAME_OF_LOGIC_OUTPUT_FILE));
            }
            catch (DirectoryNotFoundException ex)
            {
                Debug.LogError(ex.Message + "\n" + ex.StackTrace);
            }
        }
    }
}
