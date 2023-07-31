using profiler;
using System;
using System.IO;
using UnityEngine;

namespace GenoratingRandomSDF
{
    [System.Serializable]
    public class SDFGenorator
    {
        // Logic Objects
        StudyGenerationPlanner conditionDetails;
        AbstractProfiler profiler;
        StudyDetailsForOutput[] StudyDesign;
        private RandomChildGeneratorFactory randomChildGeneratorFactory = new RandomChildGeneratorFactory();

        // Geometric Objects
        ShapeHandeler shapes;
        HashingMatrix hashingMatrix;

        IWriteOutputFile outputFileWriter;
        StateControllerForAddingSDFs addingSDFController;
        CheckingStateContoller checkingTheVolumeController;

        // veribles for outputing
        float NoiseMultiplier;

        // Output variables
        int StudyDesignOutputIndex = 0;
        int AmountOfFilesCreated = 0;

        // Counting variables
        public AmountTrackingParameters valuesToTrackVariousAmountsOfThings;

        // Behaviour Logic for the class
        private int currentlevel = 0;
        private int currentSubProcess = 0;
        private bool CanUpdate = false;

        ErrorHandelerFacade statsForErrorHandeling;
        IVerification optionalFinalCheck;

        VisulizationAdapter[] visulizations;

        IProcess[] outputHandelers;
        int outputHandelersCount = 0;

        public int StudyDesignIndex { get => StudyDesignOutputIndex; }
        public SphericalVolumeHierarchyLevelDetails CurrentLevel { get => conditionDetails.CurrentItteration; }

        public SDFGenorator(ref StudyGenerationPlanner conditionDetails, ref AbstractProfiler profiler, IWriteOutputFile outputFileWriter, ref CheckingStateContoller checkingTheVolumeController, ref StateControllerForAddingSDFs addingSDFController, ref HashingMatrix hashingMatrix,
            ref ErrorHandelerFacade statsForErrorHandeling, ShapeHandeler shapes, float NoiseMultiplier, IVerification optionalFinalCheck = null, VisulizationAdapter[] visulizations = null, IProcess[] outputHandelers = null) : base()
        {
            this.conditionDetails = conditionDetails;
            this.profiler = profiler;
            this.outputFileWriter = outputFileWriter;
            this.checkingTheVolumeController = checkingTheVolumeController;
            this.addingSDFController = addingSDFController;
            this.statsForErrorHandeling = statsForErrorHandeling;
            this.shapes = shapes;
            this.visulizations = visulizations;
            this.outputHandelers = outputHandelers;

            this.NoiseMultiplier = NoiseMultiplier;

            if (hashingMatrix == null)
                this.hashingMatrix = HashingMatrix.InitalizeRandomHashingMatrix();
            else
                this.hashingMatrix = hashingMatrix;

            // Start up the study design array
            InitalizeStudyDesignArray();

            this.statsForErrorHandeling.Init(this, ref randomChildGeneratorFactory);

            if (optionalFinalCheck == null)
            {
                this.optionalFinalCheck = new NoCheck();
            }
            else
            {
                this.optionalFinalCheck = optionalFinalCheck;
            }

            if (shapes == null)
            {
                shapes = new ShapeHandeler();
            }

            shapes.Reset(conditionDetails.CurrentItteration.CountAll());
        }

        public int Process()
        {
            if (this.shapes.ShapesAreRemainingToVerify)
            {
                if (currentSubProcess == 0)
                {
                    try
                    {
                        currentSubProcess = addingSDFController.Get().AddSDFs(ref shapes);
                        Debug.Log(1);
                    }
                    catch (RanForTooLongException)
                    {
                        currentSubProcess = statsForErrorHandeling.AddSDFsFailed();
                    }
                    catch (Exception)
                    {
                        // Throws a exception to the next object on the stack
                        statsForErrorHandeling.AddSDFTriggeredAnUnknownError();
                    }
                }
                else if (currentSubProcess == 1)
                {
                    currentSubProcess = checkingTheVolumeController.Get().Verify(ref shapes);
                    //Debug.Log(2);
                    //Debug.Log("Current Sub Process : " + currentSubProcess);
                    //Debug.Log("Amount Of Shapes Remaining : " + shapes.AmountOfShapesRemaing);
                }
            }
            else if (currentSubProcess == 2)
            {
                // A check of the whole volume is done this time
                currentSubProcess = optionalFinalCheck.Verify(ref shapes);
                Debug.Log(3);
            }
            else if (this.currentSubProcess < 0)
            {
                if (outputHandelers != null && outputHandelersCount < outputHandelers.Length)
                {
                    outputHandelers[outputHandelersCount].processes();
                }
                else
                {
                    // create the final output
                    StudyDetailsForOutput output = new StudyDetailsForOutput();
                    Debug.Log(4);

                    if (visulizations != null)
                    {
                        // update the visulizations
                        for (int index = 0; index < visulizations.Length; index++)
                        {
                            // Set up the visulization
                            visulizations[index].SetUpVisulization(shapes, this.conditionDetails.GetCurrentCondition());

                            // Set up the hashing matrix
                            hashingMatrix.SetInShader(visulizations[index].GetMaterial(), "_HashLineA", "_HashLineB", "_HashLineC");

                            // Set up the noise for the visulization
                            output.SetNoiseMultiper(visulizations[index].GetMaterial());
                        }
                    }

                    output.shapeDetails = this.shapes.GetShapesAsArray();

                    output.answers = AnswersFromGeneration.GenerateAnswers(this.shapes.GetShapesAsArray(), this.conditionDetails.CurrentItteration, this.addingSDFController.GetTheAmountOfinners(), this.addingSDFController.GetTheAmountOfContained(), this.conditionDetails.CurrentItteration.AmountOfContainers, this.conditionDetails.CurrentItteration.AmountOfOuters, this.conditionDetails.CurrentItteration.AmountOfCountables);

                    output.hash = hashingMatrix.Clone();
                    output.NoiseMulitiplier = this.NoiseMultiplier;
                    output.ConditionName = this.conditionDetails.GetCurrentCondition();

                    // Printout the answers
                    Debug.Log(output.ToJSON());



                    // save the logic files
                    try
                    {
                        // Print out these answers this can be handy for allowing the system to shut down
                        this.profiler.StopTimer();
                        this.profiler.WriteProiferDataAsCSV(System.IO.Path.Combine(Application.dataPath, DataGenerationDataProfiler.NAME_OF_LOGIC_OUTPUT_FILE));
                    }
                    catch (DirectoryNotFoundException ex)
                    {
                        Debug.LogError(ex.Message + "\n" + ex.StackTrace);
                    }


                    // Allow for a new amount of children to end once this is successful
                    RandomChildrenAllocationSystem allocationSystem = this.randomChildGeneratorFactory.GetChildGenoratorFor(this.conditionDetails.CurrentItteration.ContainableAmountOfChildren.min, this.conditionDetails.CurrentItteration.ContainableAmountOfChildren.max, this.conditionDetails.CurrentItteration.AmountOfContainers);
                    allocationSystem.DepersistsRandomValue();

                    this.statsForErrorHandeling.Reset();

                    // 
                    this.StudyDesign[StudyDesignOutputIndex] = output;
                    this.StudyDesignOutputIndex++;

                    this.profiler.Increment(DataGenerationDataProfiler.SUCCESS);

                    // Set stuff up for the next to time the next itteration
                    //participantTimeData = this.AddEndTimesTimeDatabase(participantTimeData);
                    //WriteDictionaryToCSV<string, int>(System.IO.Path.Combine(Application.persistentDataPath, "Participant" + (startingPartcipant + AmountOfFilesCreated) + "IntputTimeData.csv"), this.logicData);
                    //participantTimeData = null;

                    // back up the overall time data incase of a accedent
                    //overallTimeData = this.AddEndTimesTimeDatabase(overallTimeData);
                    //WriteDictionaryToCSV<string, float>(System.IO.Path.Combine(Application.persistentDataPath, "OverallTimesTakenToProduceInputData.csv"), overallTimeData);

                    // Study design
                    if (this.StudyDesignOutputIndex >= this.StudyDesign.Length)
                    {

                        try
                        {
                            // Print out these answers
                            this.profiler.WriteProiferDataAsCSV(System.IO.Path.Combine(Application.dataPath, DataGenerationDataProfiler.NAME_OF_LOGIC_OUTPUT_FILE));
                        }
                        catch (DirectoryNotFoundException ex)
                        {
                            Debug.LogError(ex.Message + "\n" + ex.StackTrace);
                        }

                        this.profiler.ClearAndInitProfiler();

                        // write the old file to a file
                        this.WriteStudyDesignToFile();
                        // then resart the array that was writen
                        this.InitalizeStudyDesignArray();

                        // reset the indexs for the conditions
                        this.conditionDetails.GetReadyForNextParticipant();

                        this.StudyDesignOutputIndex = 0;
                    }
                    else
                    {
                        // If we are not reseting the contion details we then want to get the next value
                        conditionDetails.GetNext();
                        addingSDFController.SetCurrentShapeDetails(this.conditionDetails.CurrentItteration);
                    }

                    // get everything ready to do it all again
                    Reset();
                    Debug.Log("Reset Activated");
                    // tells the system next frame to take a image.

                    // Return one to tell the parent system that the visulization will change and to prerform post processing steps, Like phtograhy
                    return 1;
                }
                
            }

            return 0;
        }

        private int CountAmountOfRegions()
        {
            return this.conditionDetails.CurrentItteration.AmountOfOuters +
                this.conditionDetails.CurrentItteration.AmountOfContainers +
                this.conditionDetails.CurrentItteration.AmountOfCountables;
        }

        // Set up starting logic for output of the study design Details
        private void InitalizeStudyDesignArray()
        {
            this.StudyDesign = new StudyDetailsForOutput[this.conditionDetails.TotalCount()];
            this.StudyDesignOutputIndex = 0;
        }

        private void WriteStudyDesignToFile()
        {
           // Write json to file
           outputFileWriter.WriteToFile(StudyDesign, this.conditionDetails.ParticipantID);

            // increment the amount of files created
            AmountOfFilesCreated++;
        }


        public void Reset()
        {
            // revert all of the values back to thier orignal ones
            currentlevel = 0;
            currentSubProcess = 0;
            outputHandelersCount = 0;

            this.shapes.Reset(this.CountAmountOfRegions());

            this.checkingTheVolumeController.Reset();
            this.addingSDFController.SetToOuter();
            this.addingSDFController.SetCurrentShapeDetails(CurrentLevel);

            this.statsForErrorHandeling.Reset();

            // create a random hash to start with
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

