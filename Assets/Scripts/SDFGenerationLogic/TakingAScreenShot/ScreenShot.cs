using UnityEngine;

[System.Serializable]
public class ScreenShot
{
    private const string PREPEND_FOR_FILE_NAME_OF_PHOTOS_FOLDER = "PhotosOf";

    [SerializeField] bool takeImages = false;
    public bool takeImageThisFrame { get; set; }
    [SerializeField] string pathForImages;
    [SerializeField] string prependForImages;
    [SerializeField] bool createANewFolderForEachParticipant;
    [SerializeField] int ScreenShotUpresScaling = 4;
    [SerializeField] bool writeMessageWhenSuccsessful = true;

    [Header("Output File Details")]
    [SerializeField] string prependForOutputFile;
    [SerializeField] string pathForFiles;

    public void Init()
    {
        takeImageThisFrame = false;
    }

    public void AttemptToTakeScreenShot(int participantID, int currentItteration)
    {
        if (takeImages)
        {
            TakeScreenShot(participantID, currentItteration);
            takeImageThisFrame = false;
        }
    }

    private void TakeScreenShot(int currentParticipant, int currentItteration)
    {
        Debug.Log("ScreenShot");

        string fileNameForImage = pathForImages;

        // Add the folder for the participant if required
        if (createANewFolderForEachParticipant)
        {
            fileNameForImage += "/" + PREPEND_FOR_FILE_NAME_OF_PHOTOS_FOLDER + UserStudyHelperFuncitons.GenerateParticipantID(currentParticipant, prependForOutputFile) + "/";
        }

        try
        {
            // Add the directory if it does not exist
            if (!System.IO.Directory.Exists(fileNameForImage))
            {
                System.IO.Directory.CreateDirectory(fileNameForImage);
            }

            // finishup the image name
            fileNameForImage += prependForImages;
            fileNameForImage += "Participant";
            fileNameForImage += currentParticipant;
            fileNameForImage += "_Itteration";
            fileNameForImage += currentItteration;
            fileNameForImage += ".png";

            if (writeMessageWhenSuccsessful)
                Debug.Log("ScreenShot Taken: " + currentItteration);

            // take the image
            ScreenCapture.CaptureScreenshot(fileNameForImage, ScreenShotUpresScaling);
            takeImageThisFrame = false;

            Debug.Log(fileNameForImage);
        }
        catch (System.IO.DirectoryNotFoundException)
        {
            Debug.LogError("Repository for photos could not be found, No photo was taken");
            takeImages = false;
        }
    }
}
