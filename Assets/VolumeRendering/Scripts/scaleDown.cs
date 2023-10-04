using UnityEngine;


/// <summary>
/// Scales down a object shortly after the game starts
/// This lets things that ignore scale to be built and this to fix them
/// </summary>
public class scaleDown : MonoBehaviour
{
    [SerializeField]
    Vector3 nextScale;

    [SerializeField]
    int AmountOfFramesToWait = 1;

    /// <summary>
    /// Allows this script to be run more than once if needed
    /// </summary>
    private int FrameOffset = 0;

    /// <summary>
    /// A flag that tells the system if this should run
    /// </summary>
    private bool shouldRun = true;

    // Update is called once per frame
    void Update()
    {
        if (shouldRun && AmountOfFramesToWait > Time.frameCount - FrameOffset)
        {
            this.transform.localScale = nextScale;
            this.shouldRun = false;
        }
    }

    public void Reset()
    {
        this.FrameOffset = Time.frameCount;
        this.shouldRun = true;
    }
}
