using UnityEngine;

public class MeshToUnityObject : MonoBehaviour
{
    [SerializeField] Material material;
    [SerializeField] Vector3 position;

    public void SetUp(Mesh mesh)
    {
        GameObject go = new GameObject("Mesh");
        go.transform.parent = transform;
        go.AddComponent<MeshFilter>();
        go.AddComponent<MeshRenderer>();
        go.GetComponent<Renderer>().material = material;
        go.GetComponent<MeshFilter>().mesh = mesh;
        go.transform.localPosition = position;
    }
}
