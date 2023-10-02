using UnityEngine;

namespace GenoratingRandomSDF
{
    [System.Serializable]
    public class MeshOutputManager : IProcess
    {
        SDFToMarchingCubes marchingCubes;

        int zIndex = 0;

        int resolution;

        string fileDir = string.Empty;

        float highestImportance = 3;

        ShapeHandeler shapes;

        int layerCounter = 0;

        enum Process
        {
            NotStarted,
            CreatingBuffer,
            CreatingMesh,
            ConvertToUnityMesh,
            WriteToFile
        }

        Process currentProcess = Process.NotStarted;

        public MeshOutputManager(ref hLSL_Simulator.NoisyHierarchicalSpheres ShaderVerification, string fileDir, ref ShapeHandeler shapes, MeshToUnityObject[] objectsToHouseMeshes, int resolution = 256, float highestImportance = 3f)
        {
            marchingCubes = new SDFToMarchingCubes(ref ShaderVerification, objectsToHouseMeshes, resolution);
            this.resolution = resolution;
            zIndex = 0;
            this.fileDir = fileDir;
            this.shapes = shapes;
            this.highestImportance = highestImportance;
        }

        public int CreateMesh()
        {
            switch (currentProcess)
            {
                case Process.NotStarted:
                case Process.CreatingBuffer:
                    float importance = marchingCubes.CreateBufferForMarchingCubes(zIndex, shapes, layerCounter);

                    /*if (importance > highestImportance)
                    {
                        highestImportance = importance;
                    }*/

                    // write out the current volume data
                    marchingCubes.WriteVolumetricDataAsJSONFile(fileDir, "f" + layerCounter + "h" + marchingCubes.GetHashAsString());

                    zIndex++;
                    currentProcess = Process.CreatingBuffer;

                    if (zIndex >= resolution)
                    {
                        currentProcess = Process.CreatingMesh;
                    }
                    return 0;

                case Process.CreatingMesh:
                    marchingCubes.CreateMarchingCubesMesh(0);
                    currentProcess = Process.ConvertToUnityMesh;
                    return 0;

                case Process.ConvertToUnityMesh:
                    marchingCubes.CreateMesh();
                    currentProcess = Process.WriteToFile;
                    return 0;

                case Process.WriteToFile:
                    if (marchingCubes.ConvertToOBJ(fileDir, "f" + layerCounter + "h" + marchingCubes.GetHashAsString()))
                    {
                        layerCounter++;
                        if (layerCounter > highestImportance)
                        {
                            Reset();
                            return 1;
                        }
                        else
                        {
                            currentProcess = Process.CreatingBuffer;
                            this.zIndex = 0;
                            marchingCubes.Reset();
                            return 0;
                        }
                    }
                    else
                    {
                        // something went wrong so reset the objects
                        Reset();
                        return 0;
                    }
                default: 
                    return 1;
            }
        }

        public void Reset()
        {
            currentProcess = Process.NotStarted;
            this.zIndex = 0;
            marchingCubes.Reset();
            highestImportance = -1;
            layerCounter = 0;
        }

        public int processes()
        {
            return CreateMesh();
        }
    }
}