using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace profiler
{

    public abstract class AbstractProfiler
    {
        
        protected Dictionary<string, int> logicData = null;

        /// <summary>
        /// Writes a dictionary to a CSV file
        /// </summary>
        /// <typeparam name="T">The type of the keys</typeparam>
        /// <typeparam name="V">The types of the values</typeparam>
        /// <param name="path">The path were the file will need to presented</param>
        /// <param name="dictionary"></param>
        /// <param name="delimiter"></param>
        public static void WriteDictionaryToCSV<T, V>(string path, Dictionary<T, V> dictionary, string delimiter = ",")
        {
            Debug.Log("Printing data to " + path);
            System.IO.File.WriteAllLines(path, dictionary.Select(x => x.Key + delimiter + x.Value + delimiter + System.Environment.NewLine));
        }

        public abstract void StartTimer();

        public abstract void StopTimer();

        public abstract void Increment(string value, int amount = 1);

        public abstract void ClearAndInitProfiler();

        public abstract void ApplicationStopped();

        public abstract void WriteProiferDataAsCSV(string path);
    }
}

