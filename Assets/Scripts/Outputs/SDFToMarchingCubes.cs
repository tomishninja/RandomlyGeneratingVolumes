using GenoratingRandomSDF;
using System;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

namespace GenoratingRandomSDF
{
    [System.Serializable]
    public class SDFToMarchingCubes
    {
        private static Vector3 offset = new Vector3(0.5f, 0.5f, 0.5f);

        MeshToUnityObject[] ObjectsToHouseMeshes;

        int resolution = 256;

        float[,,] buffer;

        List<Vector3> verts = new List<Vector3>();
        List<int> indices = new List<int>();
        List<Vector3> normals = new List<Vector3>();

        hLSL_Simulator.NoisyHierarchicalSpheres ShaderVerification;

        Mesh mesh = null;

        public SDFToMarchingCubes(ref hLSL_Simulator.NoisyHierarchicalSpheres ShaderVerification, MeshToUnityObject[] objectsToHouseMeshes, int resolution)
        {
            this.ShaderVerification = ShaderVerification;
            this.resolution = resolution;
            buffer = new float[resolution, resolution, resolution];
            this.ObjectsToHouseMeshes = objectsToHouseMeshes;
        }

        public void Reset()
        {
            buffer = new float[resolution, resolution, resolution];
            mesh = null;
            verts = new List<Vector3>();
            indices = new List<int>();
            normals = new List<Vector3>();
        }

        public float CreateBufferForMarchingCubes(int zIndex, ShapeHandeler shapes, int targetLayer)
        {
            float xPercenatge;
            float yPercenatge;
            float zPercenatge = ((float)zIndex / (float)resolution);

            for (int y = 0; y < resolution; y++)
            {
                yPercenatge = ((float)y / (float)resolution);

                for (int x = 0; x < resolution; x++)
                {

                    buffer[x, y, zIndex] = 0;
                    xPercenatge = ((float)x / (float)resolution);

                    Vector3 pos = new Vector3(xPercenatge, yPercenatge, zPercenatge) - offset;

                    int index = -1;
                    float d = ShaderVerification.SDF(pos, out index);

                    if (d < 0)
                    {
                        Stack<int> sdfs = ShaderVerification.GetAllValidSDFs(pos);

                        while (sdfs.Count > 0)
                        {
                            int sdfIndex = sdfs.Pop();
                            float importance = shapes.Get(sdfIndex).importance;

                            if (importance >= (targetLayer-0.01) && importance <= (targetLayer + 0.01))
                            {
                                buffer[x, y, zIndex] = 1;
                                break;
                            }
                        }

                        /*float importance = shapes.Get(index).importance;

                        if (importance >= targetLayer)
                        {
                            buffer[x, y, zIndex] = 1;
                        }*/
                    }
                }
            }

            return targetLayer;
        }

        /*public float CreateBufferForMarchingCubes(int zIndex, ShapeHandeler shapes)
         {
             float xPercenatge = 0;
             float yPercenatge = 0;
             float zPercenatge = ((float)zIndex / (float)resolution);
             Vector3 currentPosition = Vector3.zero;

             float highestImportance = -1;

             for (int y = 0; y < resolution; y++)
             {
                 yPercenatge = ((float)y / (float)resolution);

                 for (int x = 0; x < resolution; x++)
                 {
                     xPercenatge = ((float)x / (float)resolution);

                     Vector3 pos = new Vector3(xPercenatge, yPercenatge, zPercenatge) - offset;

                     int index = -1;
                     float d = ShaderVerification.SDF(pos, out index);

                     if (d < 0)
                     {
                         Stack<int> sdfs = ShaderVerification.GetAllValidSDFs(pos);
                         float importance = float.MinValue;

                         while (sdfs.Count > 0)
                         {
                             int sdfIndex = sdfs.Pop();

                             if (shapes.Get(index).importance > importance)
                             {
                                 importance = shapes.Get(index).importance;

                                 if (importance > highestImportance)
                                 {
                                     highestImportance = importance;
                                 }
                             }
                         }

                         buffer[x, y, zIndex] = importance + 1;
                     }
                     else
                     {
                         buffer[x, y, zIndex] = 0;
                     }
                 }
             }

             return highestImportance;
         }*/

        public bool CreateMarchingCubesMesh(float surfaceMin = 0)
        {
            Marching marching = new MarchingCubes(surfaceMin);
            marching.Generate(buffer, verts, indices);
            return true;
        }

        public Mesh CreateMesh()
        {
            Mesh _mesh = new Mesh();
            _mesh.indexFormat = IndexFormat.UInt32;
            _mesh.SetVertices(verts);
            _mesh.SetTriangles(indices, 0);

            if (normals.Count > 0)
                _mesh.SetNormals(normals);
            else
                _mesh.RecalculateNormals();

            _mesh.RecalculateBounds();

            for (int index = 0; index < ObjectsToHouseMeshes.Length; index++)
            {
                ObjectsToHouseMeshes[index].SetUp(_mesh);
            }

            this.mesh = _mesh;
            return _mesh;
        }

        public Mesh SmoothMesh(int iterations, float strength)
        {
            if (mesh == null)
            {
                Debug.LogError("Mesh is null.");
                return null;
            }

            Vector3[] vertices = mesh.vertices;
            Vector3[] smoothedVertices = new Vector3[vertices.Length];

            for (int i = 0; i < iterations; i++)
            {
                // Apply Laplacian smoothing to each vertex
                for (int j = 0; j < vertices.Length; j++)
                {
                    Vector3 smoothedVertex = vertices[j];

                    // Get the neighboring vertices
                    int[] neighborIndices = GetNeighborIndices(j);

                    // Calculate the average position of the neighboring vertices
                    for (int k = 0; k < neighborIndices.Length; k++)
                    {
                        smoothedVertex += vertices[neighborIndices[k]];
                    }

                    smoothedVertex /= (neighborIndices.Length + 1);

                    // Smooth the vertex position based on the strength parameter
                    smoothedVertex = Vector3.Lerp(vertices[j], smoothedVertex, strength);

                    smoothedVertices[j] = smoothedVertex;
                }

                // Update the vertices with the smoothed positions
                mesh.vertices = smoothedVertices;
                mesh.RecalculateNormals();
            }

            mesh.RecalculateBounds();
            return mesh;
        }

        private int[] GetNeighborIndices(int vertexIndex)
        {
            int submeshCount = mesh.subMeshCount;
            for (int submeshIndex = 0; submeshIndex < submeshCount; submeshIndex++)
            {
                int[] indices = mesh.GetIndices(submeshIndex);
                for (int i = 0; i < indices.Length; i += 3)
                {
                    if (indices[i] == vertexIndex)
                    {
                        int[] neighborIndices = new int[2];
                        neighborIndices[0] = indices[i + 1];
                        neighborIndices[1] = indices[i + 2];
                        return neighborIndices;
                    }
                    else if (indices[i + 1] == vertexIndex)
                    {
                        int[] neighborIndices = new int[2];
                        neighborIndices[0] = indices[i];
                        neighborIndices[1] = indices[i + 2];
                        return neighborIndices;
                    }
                    else if (indices[i + 2] == vertexIndex)
                    {
                        int[] neighborIndices = new int[2];
                        neighborIndices[0] = indices[i];
                        neighborIndices[1] = indices[i + 1];
                        return neighborIndices;
                    }
                }
            }

            return new int[0];
        }

        public bool ConvertToOBJ(string savePath)
        {
            if (mesh == null)
            {
                Debug.LogError("Mesh is null.");
                return false;
            }

            // Get the vertices, triangles, and normals from the mesh
            Vector3[] vertices = mesh.vertices;
            int[] triangles = mesh.triangles;
            Vector3[] normals = mesh.normals;

            // Create a new StreamWriter to write to the OBJ file
            StreamWriter writer;
            if (savePath == null || savePath.Length == 0)
            {
                writer = new StreamWriter(Application.dataPath + "MeshData" + Time.frameCount + ".obj");
                Debug.Log("Saved Instead to: " + Application.dataPath);
            }
            else
            {
                writer = new StreamWriter(savePath);
            }

            // Write the vertex data to the OBJ file
            foreach (Vector3 vertex in vertices)
            {
                string line = string.Format("v {0} {1} {2}", vertex.x, vertex.y, vertex.z);
                writer.WriteLine(line);
            }

            // Write the normal data to the OBJ file
            foreach (Vector3 normal in normals)
            {
                string line = string.Format("vn {0} {1} {2}", normal.x, normal.y, normal.z);
                writer.WriteLine(line);
            }

            // Write the triangle data to the OBJ file
            for (int i = 0; i < triangles.Length; i += 3)
            {
                int index1 = triangles[i] + 1;
                int index2 = triangles[i + 1] + 1;
                int index3 = triangles[i + 2] + 1;
                string line = string.Format("f {0}//{0} {1}//{1} {2}//{2}", index1, index2, index3);
                writer.WriteLine(line);
            }

            // Close the StreamWriter
            writer.Close();

            Debug.Log("Mesh converted to OBJ and saved to: " + savePath);

            return true;
        }
    }
}