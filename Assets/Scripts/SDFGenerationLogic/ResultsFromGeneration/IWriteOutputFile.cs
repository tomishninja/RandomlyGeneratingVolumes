using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IWriteOutputFile
{
    public abstract void WriteToFile(StudyDetailsForOutput[] studyDesign, int particpantIndex, string customPrepend = "", string path = null);
}
