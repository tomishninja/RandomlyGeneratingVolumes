using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class PlaneInformation : MonoBehaviour
{
    public bool ShowDebugInfo = true;

    Mesh mesh;

    public readonly Plane plane;
    private void Start()
    {
        mesh = this.GetComponent<MeshFilter>().mesh;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 normal = Vector3.up;
        if (mesh && mesh.normals.Length > 0)
        {
            normal = this.transform.TransformDirection(mesh.normals[0]);
        }

        plane.SetNormalAndPosition(normal, transform.position);

        Debug.Log("plane Normal: " + plane.normal);
        Debug.Log("normal normal: " + normal);
        Debug.Log(transform.localPosition);
    }
}
