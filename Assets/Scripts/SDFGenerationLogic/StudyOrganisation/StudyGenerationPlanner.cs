using UnityEngine;

[System.Serializable]
public class StudyGenerationPlanner
{
    enum Filebehaviour
    {
        None = 0,
        Read,
        Write
    }

    [SerializeField] string[] conditions;
    [SerializeField] PermutationType typeOfPermutationForConditions;
    [SerializeField] PermutationType typeOfPermutationForDifferentVarients;
    string[] currentConditions;
    [SerializeField] int repeats = 1;

    /// Repeting Index
    int repetingIndex = 0;
    /// index of the item to return
    int levelDetailIndex = 0;
    /// Condition index
    int conditionIndex = 0;
    int previousConditionIndex = 0;
    int permutationrepeatIndex;
    int currentIndex = 0;
    [SerializeField] int participantID = 0;

    public int ParticipantID { get => participantID; set => participantID = value; }

    // Helper Classes
    ItterationOrderHandeler<string> ConditionPermutationsHandeler = null;
    ItterationOrderHandeler<SphericalVolumeHierarchyLevelDetails> LevelPermutationsHandeler = null;

    // Obsolute code that needs to be removed
    [SerializeField] SphericalVolumeHierarchyLevelDetails[] levelDetails;
    SphericalVolumeHierarchyLevelDetails[] PermutatedlevelDetails;
    public SphericalVolumeHierarchyLevelDetails[] LevelDetails { get => levelDetails; }

    public SphericalVolumeHierarchyLevelDetails CurrentItteration { get => PermutatedlevelDetails[levelDetailIndex]; }

    [Header("File IO For Saving Generation Settings")]
    [SerializeField] Filebehaviour filebehaviour = Filebehaviour.None;
    [SerializeField] JSONFileReaderWriter<StudyGenerationPlanner> fileReaderWriter;

    /// <summary>
    /// Inorder returns the various level details 
    /// </summary>
    /// <returns>the next level if it exists and null when all condtions are exaushed</returns>
    public SphericalVolumeHierarchyLevelDetails GetNext()
    {
        if (conditionIndex < conditions.Length)
        {
            // get the return value
            //VolumeHierarachyLevelDetailsForCountingStudy output = levelDetails[levelDetailIndex];

            // increment the indexs
            levelDetailIndex++;
            previousConditionIndex = conditionIndex;
            if (levelDetailIndex >= levelDetails.Length)
            {
                // reset the level index so we can look at a new level
                levelDetailIndex = 0;
                repetingIndex++;

                // Set permuations to match the next veriation
                permutationrepeatIndex++;
                PermutatedlevelDetails = LevelPermutationsHandeler.Get(permutationrepeatIndex);

                if (repetingIndex >= repeats)
                {
                    repetingIndex = 0;
                    conditionIndex++;
                }
            }

            currentIndex++;

            Debug.LogWarning("Logic Itteration: " + levelDetailIndex);

            return levelDetails[levelDetailIndex];
        }
        else
        {
            // return null if nothing is left to send
            return null;
        }
    }

    public int TotalCount()
    {
        return this.conditions.Length * this.repeats * this.levelDetails.Length;
    }

    public int CurrentIndex()
    {
        return currentIndex;
    }


    public bool IsAtEnd()
    {
        return conditionIndex >= conditions.Length - 1;
    }


    public string GetCurrentCondition()
    {
        if (conditionIndex < conditions.Length)
            return currentConditions[conditionIndex];
        else
            return null;
    }

    public string GetFinalCondition()
    {
        return currentConditions[currentConditions.Length - 1];
    }

    public void GetReadyForNextParticipant()
    {
        this.participantID++;
        this.Reset(this.participantID);
    }

    public void Reset(int participantID)
    {
        levelDetailIndex = 0;
        repetingIndex = 0;
        conditionIndex = 0;
        currentIndex = 0;
        this.participantID = participantID;

        SetPerumationDetails(this.participantID);
    }


    public void Init(int participantID)
    {
        // Set the peruation logic for the various classes
        ConditionPermutationsHandeler = new PermutationFactory<string>().BuildPermuationCreator(typeOfPermutationForConditions, this.conditions);

        LevelPermutationsHandeler = new PermutationFactory<SphericalVolumeHierarchyLevelDetails>().BuildPermuationCreator(typeOfPermutationForDifferentVarients, this.levelDetails);

        // Set the current condition order
        currentConditions = ConditionPermutationsHandeler.Get(participantID);

        SetPerumationDetails(participantID);

        // File Writing behviour
        switch (this.filebehaviour)
        {
            case Filebehaviour.Read:
                StudyGenerationPlanner other = fileReaderWriter.ReadFile();
                this.repeats = other.repeats;
                this.conditions = other.conditions;
                this.levelDetails = other.levelDetails;

                break;
            case Filebehaviour.Write:
                fileReaderWriter.WriteFile(this);
                break;
            default:
                // the default is just to read the input from the unity editor
                break;
        }
    }

    public void SetPerumationDetails(int participantID)
    {
        // calcuate the starting index for the perutations
        permutationrepeatIndex = conditions.Length * repeats * participantID;

        // Get the first set of permutated level details
        PermutatedlevelDetails = LevelPermutationsHandeler.Get(permutationrepeatIndex);
    }
}
