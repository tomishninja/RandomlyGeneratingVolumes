using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestLerping : MonoBehaviour
{


    public Vector3 StartingVector = Vector3.up;

    // Start is called before the first frame update
    void Start()
    {

        Debug.Log("Test Data For Slerping");

        Debug.Log(Vector3.forward);
        Debug.Log(Slerp(Vector3.forward, Vector3.back, 0.5f));
        Debug.Log(Vector3.back);

        Vector3 nintyDegreesAway = Slerp(StartingVector, -StartingVector, 0.5f);
        Debug.Log(nintyDegreesAway);
        Debug.Log(Vector3.Slerp(StartingVector, -StartingVector, 0.5f));
        Vector3 CrossProduct = Vector3.Cross(nintyDegreesAway, StartingVector);
        Debug.Log(CrossProduct);
        Debug.Log(-CrossProduct);
        Debug.Log(-nintyDegreesAway);
        Debug.Log(Slerp(CrossProduct, nintyDegreesAway, 0.5f));
        Debug.Log(Slerp(-CrossProduct, nintyDegreesAway, 0.5f));
        Debug.Log(Slerp(-CrossProduct, -nintyDegreesAway, 0.5f));
        Debug.Log(Slerp(CrossProduct, -nintyDegreesAway, 0.5f));
        Debug.Log(Vector3.Slerp(CrossProduct, -nintyDegreesAway, 0.5f));
    }


    // Special Thanks to Johnathan, Shaun and Geof!
    private Vector3 Slerp(Vector3 start, Vector3 end, float percent)
    {

        // if the angle is 180 degrees away then you need to add some noise to the equasion
        float angleBetween = Vector3.Angle(start, end);
        if (angleBetween > 179.9 && angleBetween < 180.1)
        {
            end += new Vector3(0.01f, 0, 0.01f);
        }

        // Dot product - the cosine of the angle between 2 vectors.
        float dot = Vector3.Dot(start, end);
        // Clamp it to be in the range of Acos()
        // This may be unnecessary, but floating point
        // precision can be a fickle mistress.
        Mathf.Clamp(dot, -1.0f, 1.0f);
        // Acos(dot) returns the angle between start and end,
        // And multiplying that by percent returns the angle between
        // start and the final result.
        float theta = Mathf.Acos(dot) * percent;
        Vector3 RelativeVec = end - start * dot;
        RelativeVec.Normalize();
        // Orthonormal basis
        // The final result.
        return ((start * Mathf.Cos(theta)) + (RelativeVec * Mathf.Sin(theta)));
    }
}
