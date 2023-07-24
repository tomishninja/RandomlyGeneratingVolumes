using System;
using UnityEngine;

[System.Serializable]
public class NoisySphereShapeGenerationHelper : HierachicalObjects
{
    /// <summary>
    /// The children of this shape
    /// </summary>
    /// <param name="childIndex"></param>
    /// <returns></returns>
    /// <exception cref="RanForTooLongException"></exception>
    public override HierachicalObjects MoveChild(int childIndex)
    {
        Vector3 pos = Vector3.positiveInfinity;
        //float radius = -1f;
        int counter = 0;
        do
        {
            // Get a value form the allowed range for the radius
            //radius = UnityEngine.Random.Range(minAndMaxRadiusMultipler.min, minAndMaxRadiusMultipler.max);

            if (counter > 50)
            {
                // Something when really wrong
                throw new RanForTooLongException("This excpetion cant find a way to add a child here");
            }

            // get a value from the allowed range from the radius
            // The bounding box is shrunk to remove values that wont fit the basic object
            pos = new Vector3(
                UnityEngine.Random.Range(AABB_Min.x + Children[childIndex].radius, AABB_Max.x - Children[childIndex].radius),
                UnityEngine.Random.Range(AABB_Min.y + Children[childIndex].radius, AABB_Max.y - Children[childIndex].radius),
                UnityEngine.Random.Range(AABB_Min.z + Children[childIndex].radius, AABB_Max.z - Children[childIndex].radius)
                );

            counter++;
        } while (IsNotInBounds(pos, radius, childIndex));

        // create the shape we are trying to create
        Children[childIndex].positon = pos;
        //shape.radius = radius;
        return Children[childIndex];
    }

    public override HierachicalObjects DraftNewChild(int childIndex, MinAndMaxFloat minAndMaxRadiusMultipler)
    {
        Vector3 pos = Vector3.positiveInfinity;
        float radius = -1f;
        int counter = 0;
        do
        {
            // Get a value form the allowed range for the radius
            radius = UnityEngine.Random.Range(minAndMaxRadiusMultipler.min, minAndMaxRadiusMultipler.max);

            if (counter > 50)
            {
                // Something when really wrong
                throw new RanForTooLongException("This excpetion cant find a way to add a child here");
            }

            // get a value from the allowed range from the radius
            // The bounding box is shrunk to remove values that wont fit the basic object
            pos = new Vector3(
                UnityEngine.Random.Range(AABB_Min.x + radius, AABB_Max.x - radius),
                UnityEngine.Random.Range(AABB_Min.y + radius, AABB_Max.y - radius),
                UnityEngine.Random.Range(AABB_Min.z + radius, AABB_Max.z - radius)
                );

            counter++;
        } while (IsNotInBounds(pos, radius, childIndex));

        // create the shape we are trying to create
        NoisySphereShapeGenerationHelper shape = new NoisySphereShapeGenerationHelper();
        shape.positon = pos;
        shape.radius = radius;
        shape.Parent = this;
        Children[childIndex] = shape;
        return Children[childIndex];
    }

    public override HierachicalObjects DraftNewChild(int childIndex, MinAndMaxFloat containableRadiusRange, 
        int AmountOfTimesToLookForABetterObject = 50, int StartingIndexToLookForBestRandomlyGeneratedPoint = 1, 
        int StartingIndexToLookCheckForBestPositionAgainst = 0, float SphereTolerance = 0)
    {
        Vector3 pos = Vector3.positiveInfinity;
        float radius = -1f;

        int amountOfItterations = childIndex < StartingIndexToLookForBestRandomlyGeneratedPoint ? 1 : AmountOfTimesToLookForABetterObject;

        Vector3 bestPos = Vector3.positiveInfinity;
        float bestRadius = -1f;

        for (int index = 0; index < amountOfItterations; index++)
        {
            int counter = 0;
            do
            {
                if (counter > 500)
                {
                    // if this has succeeded once keep going we just couldn't find one this time
                    if (bestRadius > 0 && bestPos != Vector3.positiveInfinity)
                    {
                        break;
                    }
                    else
                    {
                        // else if this wasn't the case then we need to perform some course correction
                        throw new RanForTooLongException("This excpetion cant find a way to add a child here");
                    }
                }

                // Get a value form the allowed range for the radius
                radius = UnityEngine.Random.Range(containableRadiusRange.min, containableRadiusRange.max);

                // get a value from the allowed range from the radius
                // The bounding box is shrunk to remove values that wont fit the basic object
                pos = new Vector3(
                    UnityEngine.Random.Range(AABB_Min.x + radius, AABB_Max.x - radius),
                    UnityEngine.Random.Range(AABB_Min.y + radius, AABB_Max.y - radius),
                    UnityEngine.Random.Range(AABB_Min.z + radius, AABB_Max.z - radius)
                    );

                // Get a value form the allowed range for the radius
                //radius = UnityEngine.Random.Range(containableRadiusRange.min, containableRadiusRange.max);

                counter++;
            } while (IsNotInBounds(pos, radius + SphereTolerance, childIndex));

            if (index < 1 || IsBetterThanBest(pos, radius + SphereTolerance, bestPos, bestRadius + SphereTolerance, childIndex, StartingIndexToLookCheckForBestPositionAgainst))
            {
                bestPos = pos;
                bestRadius = radius;
            }
        }

        // create the shape we are trying to create
        NoisySphereShapeGenerationHelper shape = new NoisySphereShapeGenerationHelper();
        shape.positon = bestPos;
        shape.radius = bestRadius;
        shape.Parent = this;
        Children[childIndex] = shape;
        return Children[childIndex];
    }

    public override float TotalPercentOfThisVolumeThatIsFree(int CurrentChildIndex = -1)
    {
        if (CurrentChildIndex < 0 || CurrentChildIndex > Children.Length)
        {
            CurrentChildIndex = Children.Length;
        }

        // work out the total volume of all the children nodes
        float totalVolumeOfChildren = 0;
        if (Children != null)
        {
            for (int index = 0; index < CurrentChildIndex; index++)
            {
                if (Children[index] != null)
                {
                    totalVolumeOfChildren += Children[index].GetVolume();
                }
            }
        }

        // return the total children volume
        return totalVolumeOfChildren / GetVolume();
    }

    public override float GetVolume()
    {
        return CacluateVolume(this.radius);
    }

    private bool IsNotInBounds(Vector3 position, float radius, int CurrentChildIndex)
    {
        // TODO Not sure if this logic works
        // See if this object is within the radius of this the parent as well as the AABB
        float dist = Vector3.Distance(this.positon, position); // up here for deugging
        if (dist > Mathf.Abs(this.radius - radius)) return true;

        // Look at all the valid children and if they are valid check if they are too close to add
        if (Children != null)
        {
            for (int index = 0; index < CurrentChildIndex; index++)
            {
                if (Children[index] != null)
                {
                    float distanceAway = Vector3.Distance(Children[index].positon, position);
                    if (distanceAway < (radius + Children[index].radius)) return true;
                }
            }
        }
        return false;
    }


    private bool IsBetterThanBest(Vector3 position, float radius, Vector3 bestPos, float BestRadius, int CurrentChildIndex, int startingIndex = 0)
    {
        float minDistanceToAnObject = this.radius - Vector3.Distance(position, this.positon);
        float minDistanceToBestObject = this.radius - Vector3.Distance(bestPos, this.positon);
        float totalDistanceOfCurrent = minDistanceToAnObject;
        float totalDistanceOfBest = minDistanceToBestObject;

        // Look at all the valid children and if they are valid check if they are too close to add
        if (Children != null)
        {
            for (int index = startingIndex; index < CurrentChildIndex; index++)
            {
                if (Children[index] != null)
                {
                    float distanceAwayCurrent = Vector3.Distance(Children[index].positon, position);
                    float distanceAwayBest = Vector3.Distance(Children[index].positon, bestPos);
                    totalDistanceOfCurrent += distanceAwayCurrent;// - (radius + Children[index].radius);
                    totalDistanceOfBest += distanceAwayBest;// - (BestRadius + Children[index].radius);

                    minDistanceToAnObject = Mathf.Min(minDistanceToAnObject, distanceAwayCurrent);
                    minDistanceToBestObject = Mathf.Min(minDistanceToBestObject, distanceAwayBest);
                }
            }
        }

        int devisor = CurrentChildIndex - startingIndex;
        //return minDistanceToAnObject > minDistanceToBestObject
        return (totalDistanceOfCurrent / devisor) > (totalDistanceOfBest / devisor);
    }

    public override int AmountOfParents()
    {
        if (this.Parent == null) return 0;

        int index = 0;
        HierachicalObjects p = this.Parent;
        while (p != null)
        {
            index++;
            p = p.Parent;
        }
        return index;
    }

    public override bool HasAncestor(HierachicalObjects possibleAncestor)
    {
        if (possibleAncestor == null || this.Parent == null)
        {
            return false;
        }
        else if (this.Parent.Equals(possibleAncestor))
        {
            return true;
        }
        else
        {
            return this.Parent.HasAncestor(possibleAncestor);
        }
    }

    public override void AddDataToAverageVoxelPostion(Vector3 newPosition)
    {
        if (AveragePoint.Equals(Vector3.positiveInfinity))
        {
            AveragePoint = newPosition;
        }
        else
        {
            AveragePoint = (AveragePoint + newPosition) / 2;
        }
    }

    public override bool IsDefault()
    {
        return this.Equals(new NoisySphereShapeGenerationHelper());
    }

    public static float CacluateVolume(float radius)
    {
        return System.Convert.ToSingle(4.0 / 3 * Math.PI * System.Math.Pow(radius, 3));
    }
}
