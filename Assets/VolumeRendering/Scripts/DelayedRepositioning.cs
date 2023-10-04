using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DelayedRepositioning : MonoBehaviour
{
    [SerializeField]
    Vector3 NextPos;

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
            this.transform.localPosition = NextPos;
            this.shouldRun = false;
        }
    }

    public void Reset()
    {
        this.FrameOffset = Time.frameCount;
        this.shouldRun = true;
    }
}
