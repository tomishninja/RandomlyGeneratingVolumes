using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GenoratingRandomSDF
{
    public class AmountTrackingParameters
    {
        // Counting variables
        public int AmountOfCountablesInsideAnother { get; set; }
        public int AmountOfCountablesOutside { get; set; }
        public int AmountOfContainers { get; set; }
        public int AmountOfOuters { get; set; }
        public int AmountOfCountables { get; set; }
        public int AmountOfNotCountables { get; set; }

        public void Reset()
        {
            AmountOfCountablesInsideAnother = 0;
            AmountOfCountablesOutside = 0;
            AmountOfContainers = 0;
            AmountOfOuters = 0;
            AmountOfCountables = 0;
            AmountOfNotCountables = 0;
        }
    }
}

