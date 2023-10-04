using UnityEngine;

public class GenerateStudyDesign : MonoBehaviour
{
    int index = 0;

    public bool useBVHTrees = false;

    [SerializeField]
    int maxItterations = 10;

    [SerializeField]
    private MeshFilter SkinCollider;

    [SerializeField]
    GenerateARandomPointInsideOfThisObject TargetPointPlacer;

    [SerializeField]
    int framesToWait = 10;

    [Header("File Ouput Details")]
    [SerializeField]
    string filePath;

    [SerializeField]
    string fileName;
    
    GameObject Target;

    ClosestPointOnMesh closestPointOnMeshObj = new ClosestPointOnMesh();
    FindClosestPointOnMesh BVHsearch= null;

    TargetPositionData database = new TargetPositionData();

    bool wroteToFile = false;

    enum Task
    {
        FindNewPosition,
        FindClosestPosition,
    }

    // Start is called before the first frame update
    void Start()
    {
        if (useBVHTrees)
        {
            BVHsearch = new FindClosestPointOnMesh(SkinCollider.mesh);
        }

        Target = TargetPointPlacer.Target;

        if (framesToWait % 2 == 1)
        {
            framesToWait++;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // give the system some time to load the details of the sytem while you are processing stuff
        if (index < framesToWait)
        {
            index++;
            return;
        }

        if (database.Count() < maxItterations)
        {
            if (index %2 == 0)
            {
                // step 1: find a new random position and set it to a game Object
                
                // Place the target
                TargetPointPlacer.CalculateNewRandomPosition();
            }
            else
            {
                // step 2: find the closest point on a mesh to it.
                Vector3 optimal = GetOptimalPosition();

                // step 3: save to a Data Stucture
                database.Add(new TargetPosition(
                    this.transform.InverseTransformPoint(Target.transform.position),
                    this.transform.InverseTransformPoint(optimal)
                    )
                );
            }

            // increment the index
            index++;

            // once we have enough data write to a file
        }
        else if (!wroteToFile)
        {
            // write the file as a text file
            FileWriterManager.WriteString(database.ToJSON(), fileName, filePath);
            Debug.Log(database.ToJSON());

            // write dont do this again
            wroteToFile = true;
        }

        
    }

    public Vector3 GetOptimalPosition()
    {
        if (BVHsearch != null)
            return SkinCollider.transform.TransformPoint(BVHsearch.GetClosestPoint(
                SkinCollider.gameObject.transform.InverseTransformPoint(Target.transform.position))
                );
        else
            return SkinCollider.transform.TransformPoint(
                closestPointOnMeshObj.FindClosestPointOnMesh(SkinCollider.mesh, SkinCollider.gameObject.transform.InverseTransformPoint(Target.transform.position)
                )
        );
    }
}