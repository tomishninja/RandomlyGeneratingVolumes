using UnityEngine;

public class TestingGreatSphereicalDistance : MonoBehaviour
{
    public Transform UserObject;
    public Transform CenterObject;
    public Transform TargetObject;

    public Transform UserPositionChecker;
    public Transform IdealPositionObject;

    public float radius = 10;

    // Update is called once per frame
    void Update()
    {
        // workout the distance for the raduis between the two positons
        Vector3 idealPosition = CenterObject.position - (TargetObject.position - CenterObject.position).normalized * radius;

        Vector3 userPosition = CenterObject.position + (UserObject.position - CenterObject.position).normalized * radius;


        IdealPositionObject.transform.position = idealPosition;
        UserPositionChecker.transform.position = userPosition;

        // once both are calculated then set the positions
        Debug.Log("Spherical Distance: " + SphericalDistance(idealPosition, userPosition));
        Debug.Log("Distance To User: " + Vector3.Distance(CenterObject.position, userPosition));
        Debug.Log("Distance To Ideal: " + Vector3.Distance(CenterObject.position, idealPosition));
    }

    float SphericalDistance(Vector3 position1, Vector3 position2)
    {
        return Mathf.Acos(Vector3.Dot(position1.normalized, position2.normalized));
    }
}
