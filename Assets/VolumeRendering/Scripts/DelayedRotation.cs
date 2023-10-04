using UnityEngine;

/// <summary>
/// Sets a objects position a couple of frames after it is told to
/// </summary>
public class DelayedRotation : MonoBehaviour
{
    [SerializeField]
    Vector3 nextRotation;

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
            this.transform.localEulerAngles = nextRotation;
            this.shouldRun = false;
        }
    }

    public void Reset()
    {
        this.FrameOffset = Time.frameCount;
        this.shouldRun = true;
    }
}
