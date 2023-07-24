using UnityEngine;

namespace fileIO
{
    
    [System.Serializable]
    public class WriteStudyDesignToFile : IWriteOutputFile
    {
        public string defaultParticipantIDPrepend = "Pilot";
        public string defaultPathToOutputDirectory = "Assets/Output/";
        public const string FilePostFix = ".json";

        /// <summary>
        /// Writes the study design to a file.
        /// </summary>
        /// <param name="studyDesign"></param>
        /// <param name="particpantIndex"></param>
        /// <param name="customPrepend"></param>
        /// <param name="path"></param>
        public void WriteToFile(StudyDetailsForOutput[] studyDesign, int particpantIndex, string customPrepend = null, string path = null)
        {
            if (customPrepend == null)
            {
                customPrepend = defaultParticipantIDPrepend;
            }

            if (path == null)
            {
                path = defaultPathToOutputDirectory;
            }

            // initalize the object
            StudyDataSet output = new StudyDataSet();
            output.studyDesign = studyDesign;

            // Generate the file Name
            if (customPrepend != null)
                output.participantID = UserStudyHelperFuncitons.GenerateParticipantID(particpantIndex, customPrepend);
            else
                output.participantID = UserStudyHelperFuncitons.GenerateParticipantID(particpantIndex);

            // create a text file of the object as a object.
            string json = JsonUtility.ToJson(output, true);

            if (path == null)
                System.IO.File.WriteAllText(output.participantID + FilePostFix, json);
            else
                System.IO.File.WriteAllText(path + output.participantID + FilePostFix, json);
        }
    }
}

