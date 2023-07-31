using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstrainPosition : MonoBehaviour
{
    [SerializeField] Vector3 minPosition, maxPosition;

    // Update is called once per frame
    void Update()
    {
        Vector3 position = this.transform.localPosition;
        position = Vector3.Max(position, minPosition);
        position = Vector3.Min(position, maxPosition);

        this.transform.localPosition = position;
    }
}
