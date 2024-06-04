using System.Collections.Generic;
using UnityEngine;
using static DicomGrid;

public class SegmentionFunctor : IProcess
{
    enum task
    {
        init = 0,
        DetermineTypes = 1,
        SecondPass = 2,
        ClassifySegments = 3,
        FinalizeFunciton
    }

    task CurrentTask = task.init;

    uint[] buffer;
    TypeOfVoxel[] definition;
    DicomGrid grid;
    uint tolerance;
    // used for later on parameters to work out differnt segments
    bool[] isInside;

    int index = 0;


    // count the amount of segments that are found
    int counter = 0;

    List<Vector3Int> border = new List<Vector3Int>();
    List<int[]> segments;

    public SegmentionFunctor(DicomGrid grid, uint tolerance)
    {
        this.grid = grid;
        buffer = grid.buffer;
        definition = new TypeOfVoxel[buffer.Length];
        this.tolerance = tolerance;
        isInside = new bool[buffer.Length];
    }

    public int processes()
    {
        if (this.buffer == null || this.buffer.Length < 1 || grid.Segmented)
            return -1;

        bool actionRequired = false;
        switch (CurrentTask)
        {
            case task.init:
                actionRequired = DetermineStartingLocation();
                if (actionRequired)
                {
                    CurrentTask = task.DetermineTypes;
                    Debug.Log("Chosen Starting Location");
                }
                break;
            case task.DetermineTypes:
                actionRequired = DetermineTypeOfVoxel();
                if (actionRequired)
                {
                    index--;
                    CurrentTask = task.SecondPass;
                    Debug.Log("Determine Types Passed");
                }
                break;
            case task.SecondPass:
                actionRequired = SecondPass();
                if (actionRequired)
                {
                    index = this.buffer.Length - 1;
                    grid.RelatedSegments = new int[this.buffer.Length][];
                    int increment = isInside.Length / 2;
                    List<int[]> segments = new List<int[]>();
                    CurrentTask = task.ClassifySegments;
                    Debug.Log("Second Pass Completed");
                }
                break;
            case task.ClassifySegments:
                actionRequired = ClassifySegments();
                if (actionRequired)
                {
                    CurrentTask = task.FinalizeFunciton;
                    Debug.Log("Classify Segments Complete");
                }
                break;
            case    task.FinalizeFunciton:
                actionRequired = FinalizeFunciton();
                if (actionRequired)
                {
                    Debug.Log("Done");
                    return 1;

                }
                break;
        }
        return 0;
    }

    private bool DetermineStartingLocation()
    {
        // find the starting point
        for (; index < this.buffer.Length; index++)
        {
            definition[index] = TypeOfVoxel.inside;
            if (index < tolerance)
            {
                definition[index] = TypeOfVoxel.outside;

                // get all of the surronding 
                Vector3 VIndex = grid.GetPosition(index);

                if (index > 0)
                    SetAllOutsideToBoundry(VIndex);

                // we need to increment because we wont for a while
                index++;

                // once we have a starting point then we want to 
                break;
            }
            else
            {
                definition[index] = TypeOfVoxel.inside;
            }
        }
        return true;
    }

    private bool DetermineTypeOfVoxel()
    {
        definition[index] = GetTypeSixDirection(grid.GetPositionAsInt(index), tolerance);

        index++;

        return !(index < this.buffer.Length);
    }

    private bool SecondPass()
    {
        // 2nd Pass over all the pixels to ensure they where allocated correctly

        // work out what type this object is
        definition[index] = this.GetTypeTweleveDirection(grid.GetPositionAsInt(index), tolerance);

        // depending on the result then we need to filter results by a way that makes sense
        switch (definition[index])
        {
            case TypeOfVoxel.border:
                // add the position to the border array to fit it in
                border.Add(grid.GetPositionAsInt(index));
                isInside[index] = false;
                break;
            case TypeOfVoxel.inside:
                isInside[index] = true;
                break;
            case TypeOfVoxel.outside:
            default:
                isInside[index] = false;
                break;
        }
        index--;
        return !(index >= 0);
    }

    private bool ClassifySegments()
    {
        int altIndex = (index + (this.buffer.Length / 2)) % this.buffer.Length;

        if (isInside[altIndex] && this.definition[altIndex] == TypeOfVoxel.inside)
        {
            // run a bfs to work detertine what pixels belong to this segmnet 
            int[] segment = DetermineInteriorVolumeIntTwelveDirection(grid.GetPositionAsInt(altIndex), counter, ref isInside);

            int sizeOfSegment = segment.Length;

            if (grid.sizeOfLargestSegment < sizeOfSegment)
            {
                grid.sizeOfLargestSegment = sizeOfSegment;
                grid.largestSegment = counter;
            }

            // set the next segment if it has more than just a small portion of data
            if (segment.Length > 8)
            {
                segments.Add(segment);
                counter++;
            }

        }

        index--;

        return !(index >= 0);

    }


    public bool FinalizeFunciton()
    {
        grid.amountOfSegments = counter;

        // check the flag so this isn't called twice
        grid.segmented = true;

        // set the border pixels to the array
        grid.borderPixels = border.ToArray();
        Queue<Vector3Int> largest = new Queue<Vector3Int>();

        for (index = 0; index < grid.borderPixels.Length; index++)
        {
            // find the AABB bounidng box for all of the border pixels
            if (grid.minAABB.x > grid.borderPixels[index].x)
                grid.minAABB.x = grid.borderPixels[index].x;
            if (grid.minAABB.y > grid.borderPixels[index].y)
                grid.minAABB.y = grid.borderPixels[index].y;
            if (grid.minAABB.z > grid.borderPixels[index].z)
                grid.minAABB.z = grid.borderPixels[index].z;
            if (grid.maxAABB.x < grid.borderPixels[index].x)
                grid.maxAABB.x = grid.borderPixels[index].x;
            if (grid.maxAABB.y < grid.borderPixels[index].y)
                grid.maxAABB.y = grid.borderPixels[index].y;
            if (grid.maxAABB.z < grid.borderPixels[index].z)
                grid.maxAABB.z = grid.borderPixels[index].z;

            int[] temp = grid.RelatedSegments[grid.GetIndex(grid.borderPixels[index])];
            if (temp != null && ArrayContains(grid.largestSegment, temp))
            {
                largest.Enqueue(grid.borderPixels[index]);
            }
        }

        grid.borderPixelsOfLargestSegment = largest.ToArray();

        // fix the x values I removed them becuse I was in a rush and I knew what I wanted to see from them
        // find the AABB bounidng box for all of the border pixels as percentages
        grid.minAABB.x = grid.minAABB.x / (float)grid.width;
        grid.minAABB.y = grid.minAABB.y / (float)grid.height;
        grid.minAABB.z = grid.minAABB.z / (float)grid.breath;
        grid.maxAABB.x = grid.maxAABB.x / (float)grid.width;
        grid.maxAABB.y = grid.maxAABB.y / (float)grid.height;
        grid.maxAABB.z = grid.maxAABB.z / (float)grid.breath;

        return true;
    }

    private void SetAllOutsideToBoundry(Vector3 current, int minIndex = int.MaxValue)
    {
        // add all of the naubors to the to see list
        Vector3 next;
        int nextIndex;
        if (current.x < grid.width)
        {
            next = current + new Vector3(1, 0, 0);
            nextIndex = grid.GetIndex(next);
            if (nextIndex < minIndex)
            {
                definition[nextIndex] = TypeOfVoxel.border;
            }
        }
        if (current.x > 0)
        {
            next = current - new Vector3(1, 0, 0);
            nextIndex = grid.GetIndex(next);
            if (nextIndex < minIndex)
            {
                definition[nextIndex] = TypeOfVoxel.border;
            }
        }

        if (current.y < grid.height)
        {
            next = current + new Vector3(0, 1, 0);
            nextIndex = grid.GetIndex(next);
            if (nextIndex < minIndex)
            {
                definition[nextIndex] = TypeOfVoxel.border;
            }
        }
        if (current.y > 0)
        {
            next = current - new Vector3(0, 1, 0);
            nextIndex = grid.GetIndex(next);
            if (nextIndex < minIndex)
            {
                definition[nextIndex] = TypeOfVoxel.border;
            }
        }
        if (current.z < grid.breath)
        {
            next = current + new Vector3(0, 0, 1);
            nextIndex = grid.GetIndex(next);
            if (nextIndex < minIndex)
            {
                definition[nextIndex] = TypeOfVoxel.border;
            }
        }
        if (current.z > 0)
        {
            next = current - new Vector3(0, 0, 1);
            nextIndex = grid.GetIndex(next);
            if (nextIndex < minIndex)
            {
                definition[nextIndex] = TypeOfVoxel.border;
            }
        }
    }

    /// <summary>
    /// Gets teh type of the current pixel, it is either 
    /// outside, inside or within
    /// </summary>
    /// <param name="current"></param>
    /// <param name="minIndex"></param>
    private TypeOfVoxel GetTypeTweleveDirection(Vector3Int current, uint tolerance = 0, int minIndex = int.MaxValue)
    {
        // make a educated guess about the pixel we are looking at by its tolerance
        TypeOfVoxel output = TypeOfVoxel.unknown;
        bool isGreaterThanTolerance = grid.Get(current) > tolerance;

        // add all of the naubors to the to see list
        Vector3 next;
        int nextIndex;
        if (current.x < grid.width - 1)
        {
            next = current + new Vector3Int(1, 0, 0);
            nextIndex = grid.GetIndex(next);
            if (nextIndex < definition.Length && definition[nextIndex] != TypeOfVoxel.unknown)
            {
                if (isGreaterThanTolerance)
                {
                    if (definition[nextIndex] == TypeOfVoxel.inside || definition[nextIndex] == TypeOfVoxel.border)
                    {
                        output = TypeOfVoxel.inside;
                    }
                    else if (definition[nextIndex] == TypeOfVoxel.outside)
                    {
                        return TypeOfVoxel.border;
                    }
                }
                else
                {
                    if (definition[nextIndex] == TypeOfVoxel.outside)
                    {
                        return TypeOfVoxel.outside;
                    }
                    else if (definition[nextIndex] == TypeOfVoxel.inside)
                    {
                        output = TypeOfVoxel.inside;
                    }
                }
            }
        }
        else if (isGreaterThanTolerance)
        {
            // if it is inside then it should be a border since it borders on the outside
            return TypeOfVoxel.border;
        }
        if (current.x > 0)
        {
            next = current - new Vector3Int(1, 0, 0);
            nextIndex = grid.GetIndex(next);
            if (nextIndex < definition.Length && definition[nextIndex] != TypeOfVoxel.unknown)
            {
                if (isGreaterThanTolerance)
                {
                    //if (definition[nextIndex] == TypeOfVoxel.outside) // don't do anything
                    if (definition[nextIndex] == TypeOfVoxel.inside || definition[nextIndex] == TypeOfVoxel.border)
                    {
                        output = TypeOfVoxel.inside;
                    }
                    else if (definition[nextIndex] == TypeOfVoxel.outside)
                    {
                        return TypeOfVoxel.border;
                    }
                }
                else
                {
                    if (definition[nextIndex] == TypeOfVoxel.outside)
                    {
                        return TypeOfVoxel.outside;
                    }
                    else if (definition[nextIndex] == TypeOfVoxel.inside)
                    {
                        output = TypeOfVoxel.inside;
                    }
                }
            }
        }
        else if (isGreaterThanTolerance)
        {
            // if it is inside then it should be a border since it borders on the outside
            return TypeOfVoxel.border;
        }

        if (current.y < grid.height - 1)
        {
            next = current + new Vector3Int(0, 1, 0);
            nextIndex = grid.GetIndex(next);
            if (nextIndex < definition.Length && definition[nextIndex] != TypeOfVoxel.unknown)
            {
                if (isGreaterThanTolerance)
                {
                    //if (definition[nextIndex] == TypeOfVoxel.outside) // don't do anything
                    if (definition[nextIndex] == TypeOfVoxel.inside || definition[nextIndex] == TypeOfVoxel.border)
                    {
                        output = TypeOfVoxel.inside;
                    }
                    else if (definition[nextIndex] == TypeOfVoxel.outside)
                    {
                        return TypeOfVoxel.border;
                    }
                }
                else
                {
                    if (definition[nextIndex] == TypeOfVoxel.outside)
                    {
                        return TypeOfVoxel.outside;
                    }
                    else if (definition[nextIndex] == TypeOfVoxel.inside)
                    {
                        output = TypeOfVoxel.inside;
                    }
                }
            }
        }
        else if (isGreaterThanTolerance)
        {
            // if it is inside then it should be a border since it borders on the outside
            return TypeOfVoxel.border;
        }

        if (current.y > 0)
        {
            next = current - new Vector3Int(0, 1, 0);
            nextIndex = grid.GetIndex(next);
            if (nextIndex < definition.Length && definition[nextIndex] != TypeOfVoxel.unknown)
            {
                if (isGreaterThanTolerance)
                {
                    //if (definition[nextIndex] == TypeOfVoxel.outside) // don't do anything
                    if (definition[nextIndex] == TypeOfVoxel.inside || definition[nextIndex] == TypeOfVoxel.border)
                    {
                        output = TypeOfVoxel.inside;
                    }
                    else if (definition[nextIndex] == TypeOfVoxel.outside)
                    {
                        return TypeOfVoxel.border;
                    }
                }
                else
                {
                    if (definition[nextIndex] == TypeOfVoxel.outside)
                    {
                        return TypeOfVoxel.outside;
                    }
                    else if (definition[nextIndex] == TypeOfVoxel.inside)
                    {
                        output = TypeOfVoxel.inside;
                    }
                }
            }
        }
        else if (isGreaterThanTolerance)
        {
            // if it is inside then it should be a border since it borders on the outside
            return TypeOfVoxel.border;
        }

        if (current.z < grid.breath - 1)
        {
            next = current + new Vector3Int(0, 0, 1);
            nextIndex = grid.GetIndex(next);
            if (nextIndex < definition.Length && definition[nextIndex] != TypeOfVoxel.unknown)
            {
                if (isGreaterThanTolerance)
                {
                    //if (definition[nextIndex] == TypeOfVoxel.outside) // don't do anything
                    if (definition[nextIndex] == TypeOfVoxel.inside || definition[nextIndex] == TypeOfVoxel.border)
                    {
                        output = TypeOfVoxel.inside;
                    }
                    else if (definition[nextIndex] == TypeOfVoxel.outside)
                    {
                        return TypeOfVoxel.border;
                    }
                }
                else
                {
                    if (definition[nextIndex] == TypeOfVoxel.outside)
                    {
                        return TypeOfVoxel.outside;
                    }
                    else if (definition[nextIndex] == TypeOfVoxel.inside)
                    {
                        output = TypeOfVoxel.inside;
                    }
                }
            }
        }
        else if (isGreaterThanTolerance)
        {
            // if it is inside then it should be a border since it borders on the outside
            return TypeOfVoxel.border;
        }

        if (current.z > 0)
        {
            next = current - new Vector3Int(0, 0, 1);
            nextIndex = grid.GetIndex(next);
            if (nextIndex < definition.Length && definition[nextIndex] != TypeOfVoxel.unknown)
            {
                if (isGreaterThanTolerance)
                {
                    //if (definition[nextIndex] == TypeOfVoxel.outside) // don't do anything
                    if (definition[nextIndex] == TypeOfVoxel.inside || definition[nextIndex] == TypeOfVoxel.border)
                    {
                        output = TypeOfVoxel.inside;
                    }
                    else if (definition[nextIndex] == TypeOfVoxel.outside)
                    {
                        return TypeOfVoxel.border;
                    }
                }
                else
                {
                    if (definition[nextIndex] == TypeOfVoxel.outside)
                    {
                        return TypeOfVoxel.outside;
                    }
                    else if (definition[nextIndex] == TypeOfVoxel.inside)
                    {
                        output = TypeOfVoxel.inside;
                    }
                }
            }
        }
        else if (isGreaterThanTolerance)
        {
            // if it is inside then it should be a border since it borders on the outside
            return TypeOfVoxel.border;
        }

        // diagonal Concerns
        if (current.x > 0 && current.y > 0)
        {
            next = current - new Vector3Int(1, 1, 0);
            nextIndex = grid.GetIndex(next);
            if (nextIndex < definition.Length && definition[nextIndex] != TypeOfVoxel.unknown)
            {
                if (isGreaterThanTolerance)
                {
                    //if (definition[nextIndex] == TypeOfVoxel.outside) // don't do anything
                    if (definition[nextIndex] == TypeOfVoxel.inside || definition[nextIndex] == TypeOfVoxel.border)
                    {
                        output = TypeOfVoxel.inside;
                    }
                    else if (definition[nextIndex] == TypeOfVoxel.outside)
                    {
                        return TypeOfVoxel.border;
                    }
                }
                else
                {
                    if (definition[nextIndex] == TypeOfVoxel.outside)
                    {
                        return TypeOfVoxel.outside;
                    }
                    else if (definition[nextIndex] == TypeOfVoxel.inside)
                    {
                        output = TypeOfVoxel.inside;
                    }
                }
            }
        }

        if (current.x > 0 && current.z > 0)
        {
            next = current - new Vector3Int(1, 0, 1);
            nextIndex = grid.GetIndex(next);
            if (nextIndex < definition.Length && definition[nextIndex] != TypeOfVoxel.unknown)
            {
                if (isGreaterThanTolerance)
                {
                    //if (definition[nextIndex] == TypeOfVoxel.outside) // don't do anything
                    if (definition[nextIndex] == TypeOfVoxel.inside || definition[nextIndex] == TypeOfVoxel.border)
                    {
                        output = TypeOfVoxel.inside;
                    }
                    else if (definition[nextIndex] == TypeOfVoxel.outside)
                    {
                        return TypeOfVoxel.border;
                    }
                }
                else
                {
                    if (definition[nextIndex] == TypeOfVoxel.outside)
                    {
                        return TypeOfVoxel.outside;
                    }
                    else if (definition[nextIndex] == TypeOfVoxel.inside)
                    {
                        output = TypeOfVoxel.inside;
                    }
                }
            }
        }

        if (current.y > 0 && current.z > 0)
        {
            next = current - new Vector3Int(0, 1, 1);
            nextIndex = grid.GetIndex(next);
            if (nextIndex < definition.Length && definition[nextIndex] != TypeOfVoxel.unknown)
            {
                if (isGreaterThanTolerance)
                {
                    //if (definition[nextIndex] == TypeOfVoxel.outside) // don't do anything
                    if (definition[nextIndex] == TypeOfVoxel.inside || definition[nextIndex] == TypeOfVoxel.border)
                    {
                        output = TypeOfVoxel.inside;
                    }
                    else if (definition[nextIndex] == TypeOfVoxel.outside)
                    {
                        return TypeOfVoxel.border;
                    }
                }
                else
                {
                    if (definition[nextIndex] == TypeOfVoxel.outside)
                    {
                        return TypeOfVoxel.outside;
                    }
                    else if (definition[nextIndex] == TypeOfVoxel.inside)
                    {
                        output = TypeOfVoxel.inside;
                    }
                }
            }
        }
        if (current.x < grid.width - 1 && current.y < grid.height - 1)
        {
            next = current + new Vector3Int(1, 1, 0);
            nextIndex = grid.GetIndex(next);
            if (nextIndex < definition.Length && definition[nextIndex] != TypeOfVoxel.unknown)
            {
                if (isGreaterThanTolerance)
                {
                    //if (definition[nextIndex] == TypeOfVoxel.outside) // don't do anything
                    if (definition[nextIndex] == TypeOfVoxel.inside || definition[nextIndex] == TypeOfVoxel.border)
                    {
                        output = TypeOfVoxel.inside;
                    }
                    else if (definition[nextIndex] == TypeOfVoxel.outside)
                    {
                        return TypeOfVoxel.border;
                    }
                }
                else
                {
                    if (definition[nextIndex] == TypeOfVoxel.outside)
                    {
                        return TypeOfVoxel.outside;
                    }
                    else if (definition[nextIndex] == TypeOfVoxel.inside)
                    {
                        output = TypeOfVoxel.inside;
                    }
                }
            }
        }
        if (current.x < grid.width - 1 && current.z < grid.breath - 1)
        {
            next = current + new Vector3Int(1, 0, 1);
            nextIndex = grid.GetIndex(next);
            if (nextIndex < definition.Length && definition[nextIndex] != TypeOfVoxel.unknown)
            {
                if (isGreaterThanTolerance)
                {
                    //if (definition[nextIndex] == TypeOfVoxel.outside) // don't do anything
                    if (definition[nextIndex] == TypeOfVoxel.inside || definition[nextIndex] == TypeOfVoxel.border)
                    {
                        output = TypeOfVoxel.inside;
                    }
                    else if (definition[nextIndex] == TypeOfVoxel.outside)
                    {
                        return TypeOfVoxel.border;
                    }
                }
                else
                {
                    if (definition[nextIndex] == TypeOfVoxel.outside)
                    {
                        return TypeOfVoxel.outside;
                    }
                    else if (definition[nextIndex] == TypeOfVoxel.inside)
                    {
                        output = TypeOfVoxel.inside;
                    }
                }
            }
        }
        if (current.x > 0 && current.y < grid.height - 1)
        {
            next = current + new Vector3Int(-1, 1, 0);
            nextIndex = grid.GetIndex(next);
            if (nextIndex < definition.Length && definition[nextIndex] != TypeOfVoxel.unknown)
            {
                if (isGreaterThanTolerance)
                {
                    //if (definition[nextIndex] == TypeOfVoxel.outside) // don't do anything
                    if (definition[nextIndex] == TypeOfVoxel.inside || definition[nextIndex] == TypeOfVoxel.border)
                    {
                        output = TypeOfVoxel.inside;
                    }
                    else if (definition[nextIndex] == TypeOfVoxel.outside)
                    {
                        return TypeOfVoxel.border;
                    }
                }
                else
                {
                    if (definition[nextIndex] == TypeOfVoxel.outside)
                    {
                        return TypeOfVoxel.outside;
                    }
                    else if (definition[nextIndex] == TypeOfVoxel.inside)
                    {
                        output = TypeOfVoxel.inside;
                    }
                }
            }
        }
        if (current.x > 0 && current.z < grid.breath - 1)
        {
            next = current + new Vector3Int(-1, 0, 1);
            nextIndex = grid.GetIndex(next);
            if (nextIndex < definition.Length && definition[nextIndex] != TypeOfVoxel.unknown)
            {
                if (isGreaterThanTolerance)
                {
                    //if (definition[nextIndex] == TypeOfVoxel.outside) // don't do anything
                    if (definition[nextIndex] == TypeOfVoxel.inside || definition[nextIndex] == TypeOfVoxel.border)
                    {
                        output = TypeOfVoxel.inside;
                    }
                    else if (definition[nextIndex] == TypeOfVoxel.outside)
                    {
                        return TypeOfVoxel.border;
                    }
                }
                else
                {
                    if (definition[nextIndex] == TypeOfVoxel.outside)
                    {
                        return TypeOfVoxel.outside;
                    }
                    else if (definition[nextIndex] == TypeOfVoxel.inside)
                    {
                        output = TypeOfVoxel.inside;
                    }
                }
            }
        }
        if (current.x < grid.width - 1 && current.z > 0)
        {
            next = current + new Vector3Int(1, 0, -1);
            nextIndex = grid.GetIndex(next);
            if (nextIndex < definition.Length && definition[nextIndex] != TypeOfVoxel.unknown)
            {
                if (isGreaterThanTolerance)
                {
                    //if (definition[nextIndex] == TypeOfVoxel.outside) // don't do anything
                    if (definition[nextIndex] == TypeOfVoxel.inside || definition[nextIndex] == TypeOfVoxel.border)
                    {
                        output = TypeOfVoxel.inside;
                    }
                    else if (definition[nextIndex] == TypeOfVoxel.outside)
                    {
                        return TypeOfVoxel.border;
                    }
                }
                else
                {
                    if (definition[nextIndex] == TypeOfVoxel.outside)
                    {
                        return TypeOfVoxel.outside;
                    }
                    else if (definition[nextIndex] == TypeOfVoxel.inside)
                    {
                        output = TypeOfVoxel.inside;
                    }
                }
            }
        }
        if (current.x < grid.width - 1 && current.y > 0)
        {
            next = current + new Vector3Int(1, -1, 0);
            nextIndex = grid.GetIndex(next);
            if (nextIndex < definition.Length && definition[nextIndex] != TypeOfVoxel.unknown)
            {
                if (isGreaterThanTolerance)
                {
                    //if (definition[nextIndex] == TypeOfVoxel.outside) // don't do anything
                    if (definition[nextIndex] == TypeOfVoxel.inside || definition[nextIndex] == TypeOfVoxel.border)
                    {
                        output = TypeOfVoxel.inside;
                    }
                    else if (definition[nextIndex] == TypeOfVoxel.outside)
                    {
                        return TypeOfVoxel.border;
                    }
                }
                else
                {
                    if (definition[nextIndex] == TypeOfVoxel.outside)
                    {
                        return TypeOfVoxel.outside;
                    }
                    else if (definition[nextIndex] == TypeOfVoxel.inside)
                    {
                        output = TypeOfVoxel.inside;
                    }
                }
            }
        }
        if (current.z > 0 - 1 && current.y < grid.height - 1)
        {
            next = current + new Vector3Int(0, -1, 1);
            nextIndex = grid.GetIndex(next);
            if (nextIndex < definition.Length && definition[nextIndex] != TypeOfVoxel.unknown)
            {
                if (isGreaterThanTolerance)
                {
                    //if (definition[nextIndex] == TypeOfVoxel.outside) // don't do anything
                    if (definition[nextIndex] == TypeOfVoxel.inside || definition[nextIndex] == TypeOfVoxel.border)
                    {
                        output = TypeOfVoxel.inside;
                    }
                    else if (definition[nextIndex] == TypeOfVoxel.outside)
                    {
                        return TypeOfVoxel.border;
                    }
                }
                else
                {
                    if (definition[nextIndex] == TypeOfVoxel.outside)
                    {
                        return TypeOfVoxel.outside;
                    }
                    else if (definition[nextIndex] == TypeOfVoxel.inside)
                    {
                        output = TypeOfVoxel.inside;
                    }
                }
            }
        }
        if (current.z < grid.breath - 1 && current.y > 0)
        {
            next = current + new Vector3Int(0, -1, 1);
            nextIndex = grid.GetIndex(next);
            if (nextIndex < definition.Length && definition[nextIndex] != TypeOfVoxel.unknown)
            {
                if (isGreaterThanTolerance)
                {
                    //if (definition[nextIndex] == TypeOfVoxel.outside) // don't do anything
                    if (definition[nextIndex] == TypeOfVoxel.inside || definition[nextIndex] == TypeOfVoxel.border)
                    {
                        output = TypeOfVoxel.inside;
                    }
                    else if (definition[nextIndex] == TypeOfVoxel.outside)
                    {
                        return TypeOfVoxel.border;
                    }
                }
                else
                {
                    if (definition[nextIndex] == TypeOfVoxel.outside)
                    {
                        return TypeOfVoxel.outside;
                    }
                    else if (definition[nextIndex] == TypeOfVoxel.inside)
                    {
                        output = TypeOfVoxel.inside;
                    }
                }
            }
        }

        return output;
    }

    /// <summary>
    /// Gets teh type of the current pixel, it is either 
    /// outside, inside or within
    /// </summary>
    /// <param name="current"></param>
    /// <param name="minIndex"></param>
    private TypeOfVoxel GetTypeSixDirection(Vector3Int current, uint tolerance = 0, int minIndex = int.MaxValue)
    {
        // make a educated guess about the pixel we are looking at by its tolerance
        TypeOfVoxel output = TypeOfVoxel.unknown;
        bool isGreaterThanTolerance = grid.Get(current) > tolerance;

        // add all of the naubors to the to see list
        Vector3 next;
        int nextIndex;
        if (current.x < grid.width - 1)
        {
            next = current + new Vector3Int(1, 0, 0);
            nextIndex = grid.GetIndex(next);
            if (nextIndex < definition.Length && definition[nextIndex] != TypeOfVoxel.unknown)
            {
                if (isGreaterThanTolerance)
                {
                    if (definition[nextIndex] == TypeOfVoxel.inside || definition[nextIndex] == TypeOfVoxel.border)
                    {
                        output = TypeOfVoxel.inside;
                    }
                    else if (definition[nextIndex] == TypeOfVoxel.outside)
                    {
                        return TypeOfVoxel.border;
                    }
                }
                else
                {
                    if (definition[nextIndex] == TypeOfVoxel.outside)
                    {
                        return TypeOfVoxel.outside;
                    }
                    else if (definition[nextIndex] == TypeOfVoxel.inside)
                    {
                        output = TypeOfVoxel.inside;
                    }
                }
            }
        }
        if (current.x > 0)
        {
            next = current - new Vector3Int(1, 0, 0);
            nextIndex = grid.GetIndex(next);
            if (nextIndex < definition.Length && definition[nextIndex] != TypeOfVoxel.unknown)
            {
                if (isGreaterThanTolerance)
                {
                    //if (definition[nextIndex] == TypeOfVoxel.outside) // don't do anything
                    if (definition[nextIndex] == TypeOfVoxel.inside || definition[nextIndex] == TypeOfVoxel.border)
                    {
                        output = TypeOfVoxel.inside;
                    }
                    else if (definition[nextIndex] == TypeOfVoxel.outside)
                    {
                        return TypeOfVoxel.border;
                    }
                }
                else
                {
                    if (definition[nextIndex] == TypeOfVoxel.outside)
                    {
                        return TypeOfVoxel.outside;
                    }
                    else if (definition[nextIndex] == TypeOfVoxel.inside)
                    {
                        output = TypeOfVoxel.inside;
                    }
                }
            }
        }

        if (current.y < grid.height - 1)
        {
            next = current + new Vector3Int(0, 1, 0);
            nextIndex = grid.GetIndex(next);
            if (nextIndex < definition.Length && definition[nextIndex] != TypeOfVoxel.unknown)
            {
                if (isGreaterThanTolerance)
                {
                    //if (definition[nextIndex] == TypeOfVoxel.outside) // don't do anything
                    if (definition[nextIndex] == TypeOfVoxel.inside || definition[nextIndex] == TypeOfVoxel.border)
                    {
                        output = TypeOfVoxel.inside;
                    }
                    else if (definition[nextIndex] == TypeOfVoxel.outside)
                    {
                        return TypeOfVoxel.border;
                    }
                }
                else
                {
                    if (definition[nextIndex] == TypeOfVoxel.outside)
                    {
                        return TypeOfVoxel.outside;
                    }
                    else if (definition[nextIndex] == TypeOfVoxel.inside)
                    {
                        output = TypeOfVoxel.inside;
                    }
                }
            }
        }
        if (current.y > 0)
        {
            next = current - new Vector3Int(0, 1, 0);
            nextIndex = grid.GetIndex(next);
            if (nextIndex < definition.Length && definition[nextIndex] != TypeOfVoxel.unknown)
            {
                if (isGreaterThanTolerance)
                {
                    //if (definition[nextIndex] == TypeOfVoxel.outside) // don't do anything
                    if (definition[nextIndex] == TypeOfVoxel.inside || definition[nextIndex] == TypeOfVoxel.border)
                    {
                        output = TypeOfVoxel.inside;
                    }
                    else if (definition[nextIndex] == TypeOfVoxel.outside)
                    {
                        return TypeOfVoxel.border;
                    }
                }
                else
                {
                    if (definition[nextIndex] == TypeOfVoxel.outside)
                    {
                        return TypeOfVoxel.outside;
                    }
                    else if (definition[nextIndex] == TypeOfVoxel.inside)
                    {
                        output = TypeOfVoxel.inside;
                    }
                }
            }
        }
        if (current.z < grid.breath - 1)
        {
            next = current + new Vector3Int(0, 0, 1);
            nextIndex = grid.GetIndex(next);
            if (nextIndex < definition.Length && definition[nextIndex] != TypeOfVoxel.unknown)
            {
                if (isGreaterThanTolerance)
                {
                    //if (definition[nextIndex] == TypeOfVoxel.outside) // don't do anything
                    if (definition[nextIndex] == TypeOfVoxel.inside || definition[nextIndex] == TypeOfVoxel.border)
                    {
                        output = TypeOfVoxel.inside;
                    }
                    else if (definition[nextIndex] == TypeOfVoxel.outside)
                    {
                        return TypeOfVoxel.border;
                    }
                }
                else
                {
                    if (definition[nextIndex] == TypeOfVoxel.outside)
                    {
                        return TypeOfVoxel.outside;
                    }
                    else if (definition[nextIndex] == TypeOfVoxel.inside)
                    {
                        output = TypeOfVoxel.inside;
                    }
                }
            }
        }
        if (current.z > 0)
        {
            next = current - new Vector3Int(0, 0, 1);
            nextIndex = grid.GetIndex(next);
            if (nextIndex < definition.Length && definition[nextIndex] != TypeOfVoxel.unknown)
            {
                if (isGreaterThanTolerance)
                {
                    //if (definition[nextIndex] == TypeOfVoxel.outside) // don't do anything
                    if (definition[nextIndex] == TypeOfVoxel.inside || definition[nextIndex] == TypeOfVoxel.border)
                    {
                        output = TypeOfVoxel.inside;
                    }
                    else if (definition[nextIndex] == TypeOfVoxel.outside)
                    {
                        return TypeOfVoxel.border;
                    }
                }
                else
                {
                    if (definition[nextIndex] == TypeOfVoxel.outside)
                    {
                        return TypeOfVoxel.outside;
                    }
                    else if (definition[nextIndex] == TypeOfVoxel.inside)
                    {
                        output = TypeOfVoxel.inside;
                    }
                }
            }
        }

        return output;
    }

    /// <summary>
    /// Used to work out what volumes are attached to this volume
    /// </summary>
    /// <param name="start">the start of the search</param>
    /// <param name="segmentVolumeId">an Id that gets placed on these</param>
    /// <param name="seen">A list of still valid pixels</param>
    /// <returns></returns>
    private int[] DetermineInteriorVolumeIntTwelveDirection(Vector3Int start, int segmentVolumeId, ref bool[] seen)
    {
        List<int> output = new List<int>();
        Queue<Vector3Int> toSee = new Queue<Vector3Int>();
        int x = start.x;
        int y = start.y;
        int z = start.z;

        toSee.Enqueue(new Vector3Int(x, y, z));

        do
        {
            // Get and move the current point of memory
            Vector3Int current = toSee.Dequeue();
            output.Add(grid.GetIndex(current));

            // add all of the naubors to the to see list
            Vector3Int next;
            if (current.x < grid.width - 1)
            {
                next = current + new Vector3Int(1, 0, 0);
                DetermineSegmentInt(next, segmentVolumeId, ref seen, ref toSee);
            }
            if (current.x > 0)
            {
                next = current - new Vector3Int(1, 0, 0);
                DetermineSegmentInt(next, segmentVolumeId, ref seen, ref toSee);
            }
            if (current.y < grid.height - 1)
            {
                next = current + new Vector3Int(0, 1, 0);
                DetermineSegmentInt(next, segmentVolumeId, ref seen, ref toSee);
            }
            if (current.y > 0)
            {
                next = current - new Vector3Int(0, 1, 0);
                DetermineSegmentInt(next, segmentVolumeId, ref seen, ref toSee);
            }
            if (current.z < grid.breath - 1)
            {
                next = current + new Vector3Int(0, 0, 1);
                DetermineSegmentInt(next, segmentVolumeId, ref seen, ref toSee);
            }
            if (current.z > 0)
            {
                next = current - new Vector3Int(0, 0, 1);
                DetermineSegmentInt(next, segmentVolumeId, ref seen, ref toSee);
            }
            if (current.x < grid.width - 1 && current.y > 0)
            {
                next = current + new Vector3Int(1, -1, 0);
                DetermineSegmentInt(next, segmentVolumeId, ref seen, ref toSee);
            }
            if (current.x < grid.width - 1 && current.z > 0)
            {
                next = current + new Vector3Int(1, 0, -1);
                DetermineSegmentInt(next, segmentVolumeId, ref seen, ref toSee);
            }
            if (current.y < grid.height - 1 && current.x > 0)
            {
                next = current + new Vector3Int(-1, 1, 0);
                DetermineSegmentInt(next, segmentVolumeId, ref seen, ref toSee);
            }
            if (current.y < grid.height - 1 && current.z > 0)
            {
                next = current + new Vector3Int(0, 1, -1);
                DetermineSegmentInt(next, segmentVolumeId, ref seen, ref toSee);
            }
            if (current.z < grid.breath - 1 && current.x > 0)
            {
                next = current + new Vector3Int(-1, 0, 1);
                DetermineSegmentInt(next, segmentVolumeId, ref seen, ref toSee);
            }
            if (current.z < grid.breath - 1 && current.y > 0)
            {
                next = current + new Vector3Int(0, -1, 1);
                DetermineSegmentInt(next, segmentVolumeId, ref seen, ref toSee);
            }

        } while (toSee.Count > 0);

        return output.ToArray();
    }

    private void DetermineSegmentInt(Vector3Int next, int segmentVolume, ref bool[] seen, ref Queue<Vector3Int> toSee)
    {
        int i = grid.GetIndex(next.x, next.y, next.z);

        // if we haven't seen it then append it
        if (seen[i])
        {
            seen[i] = false;
            toSee.Enqueue(next);

            grid.RelatedSegments[i] = ArrayAppend(segmentVolume, grid.RelatedSegments[i]);
        }
        else if (definition[i] == TypeOfVoxel.border)
        {
            grid.RelatedSegments[i] = ArrayAppend(segmentVolume, grid.RelatedSegments[i]);
        }
    }
}
