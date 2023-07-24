using profiler;
using System.Collections.Generic;
using UnityEngine;

namespace profiler{
    public class DataGenerationDataProfiler : AbstractProfiler
    {
        // strings for dictonary data
        public const string NAME_OF_LOGIC_OUTPUT_FILE = "logicData.csv";
        public const string SUCCESS = "Successful Attempts";
        public const string COULD_NOT_FIT_ALL_OBJECTS_IN_TIME = "Could not randomly place all of the objects";
        public const string WITHIN_ANOTHER_SDF = "Within Another SDF";
        public const string OUTSIDE_OF_MINIMAL_AABB = "outside of Bounding Box";
        public const string OUTSIDE_OF_PARENT = "Outside of containing object";
        public const string UNEXPECTED_ERROR = "Unexpected Error";
        public const string MOVED_A_REGION = "Was able to move a region";
        public const string REPLACED_ALL_CHILDREN = "replaced all the child nodes";
        public const string AMOUNT_OF_SUCCESSFUL_LAYERS = "No Issues With A Layer";
        public const string TOO_MANY_LARGE_ARTIFICATS = "Too Many Large Artifacts";
        public const string TOO_FAILED_SECOND_CHECK = "Failed Second Check";

        public const string ITTERATION = "Itteration";
        public const string STARTTIME = "Start Time";
        public const string ENDTIME = "End Time";
        public const string TIMETAKEN = "End Time";

        private bool TimerRunning = false;
        [SerializeField] private string path = "Assets/Scripts/SDFGenerationLogic/Profiler/Output/";

        public override void Increment(string value, int amount = 1)
        {
            this.logicData[value] += amount;
        }

        public override void ClearAndInitProfiler()
        {
            this.logicData = new Dictionary<string, int>();
            logicData.Add(SUCCESS, 0);
            logicData.Add(COULD_NOT_FIT_ALL_OBJECTS_IN_TIME, 0);
            logicData.Add(WITHIN_ANOTHER_SDF, 0);
            logicData.Add(OUTSIDE_OF_MINIMAL_AABB, 0);
            logicData.Add(OUTSIDE_OF_PARENT, 0);
            logicData.Add(UNEXPECTED_ERROR, 0);
            logicData.Add(MOVED_A_REGION, 0);
            logicData.Add(REPLACED_ALL_CHILDREN, 0);
            logicData.Add(AMOUNT_OF_SUCCESSFUL_LAYERS, 0);
            logicData.Add(TOO_MANY_LARGE_ARTIFICATS, 0);
            logicData.Add(TOO_FAILED_SECOND_CHECK, 0);
        }

        public override void StartTimer()
        {
            logicData[STARTTIME + "seconds"] = (int)System.Math.Floor(Time.time);
            logicData[STARTTIME + "milliseconds"] = (int)((System.Math.Floor(Time.time) - Time.time) * 1000);
            TimerRunning = true;
        }

        public override void StopTimer()
        {
            if (TimerRunning)
            {
                logicData[ENDTIME + "seconds"] = (int)System.Math.Floor(Time.time);
                logicData[ENDTIME + "milliseconds"] = (int)((System.Math.Floor(Time.time) - Time.time) * 1000);
                logicData[TIMETAKEN + "seconds"] = logicData[ENDTIME + "seconds"] - logicData[STARTTIME + "seconds"];
                logicData[TIMETAKEN + "milliseconds"] = logicData[ENDTIME + "milliseconds"] - logicData[STARTTIME + "milliseconds"];
                TimerRunning = false;
            }
        }

        public override void WriteProiferDataAsCSV(string path)
        {
            WriteDictionaryToCSV<string, int>(System.IO.Path.Combine(this.path, NAME_OF_LOGIC_OUTPUT_FILE), logicData);
        }

        public override void ApplicationStopped()
        {
            if (TimerRunning)
            {
                this.StopTimer();
            }
            WriteDictionaryToCSV<string, int>(NAME_OF_LOGIC_OUTPUT_FILE, logicData);
        }
    }
}


