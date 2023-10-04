using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarchingRenderer : MonoBehaviour
{
    public VaringIntensityColorVolumeRendering dataGrabberA;
    public DicomData dataGrabberB;
    private DicomGrid volumeInfo;

    public enum MARCHING_MODE { CUBES, TETRAHEDRON };

    public Material m_material;

    public MARCHING_MODE mode = MARCHING_MODE.CUBES;

    List<GameObject> meshes = new List<GameObject>();

    public int toleranceMin;
    public int toleranceMax;

    public float shrinkBy = 5.12f;

    // Start is called before the first frame update
    void Start()
    {
        if (dataGrabberA != null)
        {
            volumeInfo = dataGrabberA.volumeInfo;
        }
        else if (dataGrabberB != null)
        {
            volumeInfo = dataGrabberB.GetDicomData();
        }
        else
        {
            Debug.LogError("No data being loaded for Marching Renderer Script");
            return;
        }

        MarchingCubesProject.Marching marching = null;
        if (mode == MARCHING_MODE.TETRAHEDRON)
            marching = new MarchingCubesProject.MarchingTertrahedron(toleranceMin, toleranceMax);
        else
            marching = new MarchingCubesProject.MarchingCubes(toleranceMin, toleranceMax);

        List<Vector3> verts = new List<Vector3>();
        List<int> indices = new List<int>();

        //The mesh produced is not optimal. There is one vert for each index.
        //Would need to weld vertices for better quality mesh.
        marching.Generate(volumeInfo.buffer, volumeInfo.width, volumeInfo.height, volumeInfo.breath, ref verts, ref indices, 4);

        //Vector3 min = Vector3.positiveInfinity;
        //Vector3 max = Vector3.negativeInfinity;
        //for(int index = 0; index < verts.Count; index++)
        //{
        //    min = Vector3.Min(min, verts[index]);
        //    max = Vector3.Max(max, verts[index]);
        //}

        //Debug.Log("Min : " + min);
        //Debug.Log("Max : " + max);

        for(int index = 0; index < verts.Count; index++)
        {
            verts[index] = new Vector3(0.5f, 0.5f, 0.5f)-(verts[index] / shrinkBy);
        }

        //A mesh in unity can only be made up of 65000 verts.
        //Need to split the verts between multiple meshes.

        int maxVertsPerMesh = 30000; //must be divisible by 3, ie 3 verts == 1 triangle
        int numMeshes = verts.Count / maxVertsPerMesh + 1;

        List<Mesh> toConvertToFile = new List<Mesh>();

        for (int i = 0; i < numMeshes; i++)
        {

            List<Vector3> splitVerts = new List<Vector3>();
            List<int> splitIndices = new List<int>();

            for (int j = 0; j < maxVertsPerMesh; j++)
            {
                int idx = i * maxVertsPerMesh + j;

                if (idx < verts.Count)
                {
                    splitVerts.Add(verts[idx]);
                    splitIndices.Add(j);
                }
            }

            if (splitVerts.Count == 0) continue;

            Mesh mesh = new Mesh();
            mesh.SetVertices(splitVerts);
            mesh.SetTriangles(splitIndices, 0);
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();

            GameObject go = new GameObject("Mesh");
            go.transform.parent = transform;
            go.AddComponent<MeshFilter>();
            go.AddComponent<MeshRenderer>();
            go.GetComponent<Renderer>().material = m_material;
            go.GetComponent<MeshFilter>().mesh = mesh;
            go.transform.localPosition = new Vector3(0, 0, 0);

            meshes.Add(go);
            toConvertToFile.Add(mesh);
        }
        ExportToObj(toConvertToFile, Application.dataPath + "VideoDemoObject");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static void ExportToObj(List<Mesh> meshes, string filePath)
    {
        using (System.IO.StreamWriter writer = new System.IO.StreamWriter(filePath))
        {
            int vertexOffset = 0;

            foreach (Mesh mesh in meshes)
            {
                Vector3[] vertices = mesh.vertices;
                Vector3[] normals = mesh.normals;
                Vector2[] uv = mesh.uv;
                int[] triangles = mesh.triangles;

                for (int i = 0; i < vertices.Length; i++)
                {
                    writer.WriteLine("v {0} {1} {2}", vertices[i].x, vertices[i].y, vertices[i].z);
                }

                for (int i = 0; i < normals.Length; i++)
                {
                    writer.WriteLine("vn {0} {1} {2}", normals[i].x, normals[i].y, normals[i].z);
                }

                for (int i = 0; i < uv.Length; i++)
                {
                    writer.WriteLine("vt {0} {1}", uv[i].x, uv[i].y);
                }

                for (int i = 0; i < triangles.Length; i += 3)
                {
                    int triangleIndex1 = triangles[i] + 1 + vertexOffset;
                    int triangleIndex2 = triangles[i + 1] + 1 + vertexOffset;
                    int triangleIndex3 = triangles[i + 2] + 1 + vertexOffset;

                    writer.WriteLine("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}",
                        triangleIndex1, triangleIndex2, triangleIndex3);
                }

                vertexOffset += vertices.Length;
            }
        }

        Debug.Log("Meshes exported to OBJ: " + filePath);
    }
}
