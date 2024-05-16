using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class DicomGrid
{
    #region varibles
    /// <summary>
    /// The phyical location of the slices in the image.
    /// This can be used to tell the distance that exists between slices
    /// and how the slices should be ordered
    /// </summary>
    public float[] sliceLocations;

    /// <summary>
    /// The width of the grid in voxels
    /// </summary>
    public int width;

    /// <summary>
    /// the height of the grid in voxels
    /// </summary>
    public int height;

    /// <summary>
    /// the breath of the grid in voxels
    /// </summary>
    public int breath;

    /// <summary>
    /// This tells the system how much space is inbetween the slices by millimeters
    /// </summary>
    public float spacingBetweenSlices = 1;

    /// <summary>
    /// The thinkness of the slice in Millimeters
    /// </summary>
    public float sliceThickness = 1;

    /// <summary>
    /// The size of each voxel 
    /// </summary>
    public float pixelSpacingX = 1;

    /// <summary>
    /// The size of each voxel
    /// </summary>
    public float pixelSpacingY = 1;

    /// <summary>
    /// A generic identifier that all the dicoms should have in common
    /// </summary>
    public string frameOfReferenceId;

    /// <summary>
    /// this is the buffer object it holds all of the voxel data that is stored within
    /// the image
    /// </summary>
    public uint[] buffer;

    #endregion

    #region bufferBasedCommands

    // tells the system if this object was created successfully
    public bool Exists()
    {
        return this.buffer == null;
    }

    /// <summary>
    /// The amount of pixels there are in the buffer
    /// </summary>
    /// <returns>0</returns>
    public int Count()
    {
        if (this.buffer != null)
            return this.buffer.Length;
        else
            return 0;
    }

    /// <summary>
    /// finds the smallest value in the dicom grid buffer object
    /// </summary>
    /// <returns>a unit that represents the minium value</returns>
    public uint Min()
    {
        uint min = uint.MaxValue;

        for (int index = 0; index < buffer.Length; index++)
        {
            if (buffer[index] < min)
            {
                min = buffer[index];
            }
        }

        return min;
    }

    /// <summary>
    /// finds the smallest value in the dicom grid buffer object
    /// </summary>
    /// <returns>a unit that represents the minium value</returns>
    public uint Max()
    {
        uint max = uint.MinValue;

        for (int index = 0; index < buffer.Length; index++)
        {
            if (buffer[index] > max)
            {
                max = buffer[index];
            }
        }

        return max;
    }



    /// <summary>
    /// finds the smallest value in the dicom grid buffer object
    /// </summary>
    /// <returns>a unit that represents the minium value</returns>
    public uint Avg()
    {
        uint avg = 0;

        for (int index = 0; index < buffer.Length; index++)
        {
            if (buffer[index] > avg)
            {
                avg = (avg + buffer[index]) / 2;
            }
        }

        return avg;
    }

    #endregion

    #region olderDemoCode

    public int GetThicknessValue()
    {
        return (int)Math.Round(sliceThickness);
    }

    /// <summary>
    /// This function returns an array with the size in mm of the 
    /// resultion frame of the image
    /// </summary>
    /// <returns>
    /// A float array with a size of two. 
    /// The first value (0) with be the x value 
    /// and the second value(1) will contrain the y value
    /// </returns>
    public float[] getXYResultion()
    {
        return new float[] {
            width,
            height
        };
    }

    /// <summary>
    /// returns the amount of distance the object will need to travel in mm
    /// </summary>
    /// <returns>
    /// A float represent real world distance of the Z axis
    /// s</returns>
    public float PhysicalDistanceAlongZ()
    {
        return breath * GetThicknessValue();
    }

    public int[] GetXYSliceImage(int zIndex, int highValue, int lowValue)
    {
        // if invalid parmaters are addded return null
        if (zIndex < 0 || zIndex >= breath) return null;

        // get the out put array intialized
        int[] output = new int[height * width];

        // set the starting point for the output array
        int indexOuter = zIndex * output.Length;

        // just set up the value verible that will dictate the value of the element
        byte value = 0;

        // loop though the arrays and set the values as needed
        for (int index = 0; index < output.Length; index++, indexOuter++)
        {
            if (buffer[indexOuter] < lowValue)
                value = 0;
            else if (buffer[indexOuter] > highValue)
                value = 255;
            else
                value = (byte)Math.Round((buffer[indexOuter] - lowValue) / (double)highValue * 255);

            output[index] = ((255 - value) << 24) + (value << 16) + (value << 8) + value;
        }

        return output;
    }

    public Color32[] GetXYSliceColorArray(int zIndex, int highValue, int lowValue)
    {
        // if invalid parmaters are addded return null
        if (zIndex < 0 || zIndex >= breath) return null;

        // get the out put array intialized
        Color32[] output = new Color32[height * width];

        // set the starting point for the output array
        int indexOuter = zIndex * output.Length;

        // just set up the value verible that will dictate the value of the element
        byte value = 0;

        // loop though the arrays and set the values as needed
        for (int index = 0; index < output.Length; index++, indexOuter++)
        {
            if (buffer[indexOuter] < lowValue)
                value = 0;
            else if (buffer[indexOuter] > highValue)
                value = 255;
            else
                value = (byte)(Math.Round(((buffer[indexOuter] - lowValue) / (double)highValue) * 255));

            output[index] = new Color32(value, value, value, (byte)(255 - value));
        }

        return output;
    }

    public int[] GetZYSliceImage(int xIndex, int highValue, int lowValue)
    {
        // if invalid parmaters are addded return null
        if (xIndex < 0 || xIndex > width) return null;

        // this has the amount of pixels each pixel is worth from this persepective
        int thinknessValue = GetThicknessValue();

        // get the out put array intialized
        int[] output = new int[height * breath * thinknessValue];

        // the amount to grow the array by each round
        int incrementalAmount = width * height;

        int zIndex = xIndex;
        int index = 0;
        byte value = 0;

        while (index < output.Length)
        {
            if (buffer[zIndex] < lowValue)
                value = 0;
            else if (buffer[zIndex] > highValue)
                value = 255;
            else
                value = (byte)(Math.Round(((buffer[zIndex] - highValue) / (double)lowValue) * 255));

            int colorVal = ((255 - value) << 24) + (value << 16) + (value << 8) + value;

            for (int i = 0; i < thinknessValue; i++)
                output[index++] = colorVal;

            zIndex += incrementalAmount;
            if (zIndex >= buffer.Length)
            {
                xIndex += width;
                zIndex = xIndex;
            }
        }

        return output;
    }

    /// <summary>
    /// This function returns an array with the size in mm of the 
    /// resultion frame of the image
    /// </summary>
    /// <returns>
    /// A float array with a size of two. 
    /// The first value (0) with be the x value 
    /// and the second value(1) will contrain the y value
    /// </returns>
    public float[] getZYResultion()
    {
        return new float[] {
            breath * GetThicknessValue(),
            height
        };
    }

    /// <summary>
    /// returns the amount of distance the object will need to travel in mm
    /// </summary>
    /// <returns>
    /// A float represent real world distance of the X axis
    /// s</returns>
    public float PhysicalDistanceAlongX()
    {
        return width;
    }

    public Color32[] GetZYSliceImageColorArray(int xIndex, int highValue, int lowValue)
    {
        // if invalid parmaters are addded return null
        if (xIndex < 0 || xIndex > width) return null;

        // this has the amount of pixels each pixel is worth from this persepective
        int thinknessValue = GetThicknessValue();

        // get the out put array intialized
        Color32[] output = new Color32[height * breath * thinknessValue];

        // the amount to grow the array by each round
        int incrementalAmount = width * height;

        int zIndex = xIndex;
        int index = 0;
        byte value = 0;

        while (index < output.Length)
        {
            if (buffer[zIndex] < lowValue)
                value = 0;
            else if (buffer[zIndex] > highValue)
                value = 255;
            else
                value = (byte)(Math.Round(((buffer[zIndex] - highValue) / (double)lowValue) * 255));

            Color32 colorVal = new Color32((byte)(255 - value), value, value, value);

            for (int i = 0; i < thinknessValue; i++)
                output[index++] = colorVal;

            zIndex += incrementalAmount;
            if (zIndex >= buffer.Length)
            {
                xIndex += width;
                zIndex = xIndex;
            }
        }

        return output;
    }

    public int[] GetXZSliceImage(int yIndex, int highValue, int lowValue)
    {
        // if invalid parmaters are addded return null
        if (yIndex < 0 || yIndex > height) return null;

        // this has the amount of pixels each pixel is worth from this persepective
        int thinknessValue = GetThicknessValue();

        // get the output array intialized
        int[] output = new int[width * breath * thinknessValue];

        // get the note the starting location of the first index
        int xIndex = yIndex * width;

        // the amount to grow the array by each round
        int carriageReturn = xIndex;

        int incrementalValue = width * height;

        // veribles that will be itterated on thoughout the loop
        int index = 0;
        byte value = 0;

        while (index < output.Length)
        {
            //Console.WriteLine(buffer[yIndex]);
            // get the color out of the array
            if (buffer[yIndex] < lowValue)
                value = 0;
            else if (buffer[yIndex] > highValue)
                value = 255;
            else
                value = (byte)(Math.Round(((buffer[yIndex] - highValue) / (double)lowValue) * 255));

            int colorVal = ((255 - value) << 24) + (value << 16) + (value << 8) + value;

            for (int i = 0; i < thinknessValue; i++)
                output[index++] = colorVal;

            // increment the rest of the indexes as required
            yIndex += incrementalValue;
            if (yIndex >= buffer.Length)
            {
                xIndex++;
                yIndex = xIndex;
            }
        }

        return output;
    }

    /// <summary>
    /// This function returns an array with the size in mm of the 
    /// resultion frame of the image
    /// </summary>
    /// <returns>
    /// A float array with a size of two. 
    /// The first value (0) with be the x value 
    /// and the second value(1) will contrain the y value
    /// </returns>
    public float[] getXZResultion()
    {
        return new float[] {
            width,
            breath * GetThicknessValue()
        };
    }

    /// <summary>
    /// returns the amount of distance the object will need to travel in mm
    /// </summary>
    /// <returns>a float represent real world distance of the Y axis</returns>
    public float PhysicalDistanceAlongY()
    {
        return height;
    }

    public Color32[] GetXZSliceImageColorArray(int yIndex, int highValue, int lowValue)
    {
        // if invalid parmaters are addded return null
        if (yIndex < 0 || yIndex > height) return null;

        // this has the amount of pixels each pixel is worth from this persepective
        int thinknessValue = GetThicknessValue();

        // get the output array intialized
        Color32[] output = new Color32[width * breath * thinknessValue];

        // get the note the starting location of the first index
        int xIndex = yIndex * width;

        // the amount to grow the array by each round
        int carriageReturn = xIndex;

        int incrementalValue = width * height;

        // veribles that will be itterated on thoughout the loop
        int index = 0;
        byte value = 0;

        while (index < output.Length)
        {
            //Console.WriteLine(buffer[yIndex]);
            // get the color out of the array
            if (buffer[yIndex] < lowValue)
                value = 0;
            else if (buffer[yIndex] > highValue)
                value = 255;
            else
                value = (byte)(Math.Round((buffer[yIndex] - highValue) / (double)lowValue) * 255);

            Color32 colorVal = new Color32((byte)(255 - value), value, value, value);

            for (int i = 0; i < thinknessValue; i++)
                output[index++] = colorVal;

            // increment the rest of the indexes as required
            yIndex += incrementalValue;
            if (yIndex >= buffer.Length)
            {
                xIndex++;
                yIndex = xIndex;
            }
        }

        return output;
    }

    /// <summary>
    /// Used for rendering pannels on planes
    /// </summary>
    /// <param name="box">
    /// The bounding box you wish to map to
    /// </param>
    /// <param name="points">
    /// The points that make up the corners of the plane to use in world space
    /// </param>
    /// <param name="width">
    /// the amouunt of pixels you want to render wide
    /// </param>
    /// <param name="height">
    /// the amouunt of pixels you want to render high
    /// </param>
    /// <returns></returns>
    public uint[] GetXYZSliceImage(BoxCollider box, Vector3[] points, int width, int height, out uint maxValue, out uint minValue)
    {
        maxValue = uint.MinValue;
        minValue = uint.MaxValue;

        // we want to find can only look between 4 points
        if (points.Length != 4)
        {
            return null;
        }

        // We will treat the box as a AABB to simplify some code going futher
        Vector3 center = box.gameObject.transform.localPosition + box.center;
        Vector3 size = Multi(box.gameObject.transform.localScale, box.size);
        Vector3 offset = size / 2f;

        Vector3 min = center - offset;
        Vector3 max = center + offset;

        float offsetWidth = this.width / 2f;
        float offsetHeight = this.height / 2f;
        uint[] output = new uint[width * height];

        // transform the position of the points to the bounding boxes local space. 
        for (int i = 0; i < points.Length; i++)
        {
            points[i] = box.transform.InverseTransformPoint(points[i]);
        }

        // this values increments with the inner loop but never goes away.
        int index = 0;
        // loop over the X and Y coords
        for (int y = 0; y < height; y++)
        {
            float yPercenatge = y / (float)height;

            for (int x = 0; x < width; x++, index++)
            {
                float xPercentage = x / (float)width;

                // work out where this spot on the plane is in the volume
                Vector3 currentPosition = BiliniarInterLerpolation(points, yPercenatge, xPercentage);

                // finds the percentage the values are in the AABB
                Vector3 percentage = Div(currentPosition - min, max - min);

                // get the value based on the position calcuated before
                uint value = GetFromPercentageAABB(percentage);

                output[index] = value;

                if (value > maxValue) maxValue = value;
                if (value < minValue) minValue = value;
            }
        }

        return output;
    }

    /// <summary>
    /// used for GetXYZSliceImage function gets the value that is the percentage
    /// specificed with in the volume.
    /// </summary>
    /// <param name="range">
    /// the percentage of the item you want to get
    /// </param>
    /// <returns>
    /// zero if the object is out of bounds or the value at the given percetage
    /// as a uint
    /// </returns>
    private uint GetFromPercentageAABB(Vector3 range)
    {
        // if it is out of range return zero
        if (range.x < 0 || range.y < 0 || range.z < 0 || range.x > 1 || range.y > 1 || range.z > 1) return 0;

        // all values are in range so format the value into a pixel
        return this.Get(
            Mathf.RoundToInt(range.x * this.width),
            Mathf.RoundToInt(range.y * this.height),
            Mathf.RoundToInt(range.z * this.breath)
            );
    }

    private Vector3 BiliniarInterLerpolation(Vector3[] points, float xpercentage, float ypercentage)
    {
        if (points.Length != 4) return Vector3.negativeInfinity;

        // Bilinar Lerp
        Vector3 cTop = Vector3.Lerp(points[2], points[3], ypercentage);
        Vector3 cBot = Vector3.Lerp(points[0], points[1], ypercentage);
        Vector3 final = Vector3.Lerp(cBot, cTop, xpercentage);

        return final;
    }

    #endregion

    public string GetJSON()
    {
        return UnityEngine.JsonUtility.ToJson(this);
    }

    #region 3DImageBufferCode

    public Vector3 GetPosition(int index)
    {
        return new Vector3(index % height, (index / width) % height, index / (width * height));
    }

    public Vector3Int GetPositionAsInt(int index)
    {
        return new Vector3Int(index % height, (index / width) % height, index / (width * height));
    }

    public uint Get(int x, int y, int z)
    {
        if (x < 0 || y < 0 || z < 0 || x >= width || y >= height || z >= breath)
            return 0;

        try
        {
            return this.buffer[(z * (height * width)) + (y * width) + x];
        }
        catch (Exception)
        {
            Debug.LogWarning(new Vector3(x, y, z));
        }
        return 0;

    }

    public int GetIndex(Vector3 position)
    {
        return Mathf.RoundToInt((position.z * (height * width)) + (position.y * width) + position.x);
    }

    public int GetIndex(Vector3Int position)
    {
        return Mathf.RoundToInt((position.z * (height * width)) + (position.y * width) + position.x);
    }

    /// will return a index that is within the bounds of the equasion every time but runs slower
    public int GetIndexSafe(Vector3Int position)
    {
        position = Vector3Int.Min(position, new Vector3Int(width - 1, height - 1, breath - 1));
        position = Vector3Int.Max(position, new Vector3Int(0, 0, 0));
        return Mathf.RoundToInt((position.z * (height * width)) + (position.y * width) + position.x);
    }

    public int GetIndex(int x, int y, int z)
    {
        return (z * (height * width)) + (y * width) + x;
    }

    public uint Get(Vector3 position)
    {
        return this.Get(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y), Mathf.RoundToInt(position.z));
    }

    /// <summary>
    /// checks if the position is in the grid
    /// </summary>
    /// <param name="position">
    /// The positon to check as a vector 3
    /// </param>
    /// <returns>
    /// return true if there is a value for this position or else return true if it exists
    /// </returns>
    public bool Exists(Vector3 position)
    {
        if (position.x < 0 || position.y < 0 || position.z < 0)
            return false;
        if (position.x >= width)
            return false;
        if (position.y >= height)
            return false;
        if (position.z >= breath)
            return false;
        return true;
    }

    #endregion

    #region BreathFirstSearch

    /// <summary>
    /// Brute force search for the closest point
    /// </summary>
    /// <param name="position"></param>
    /// <param name="tolerance"></param>
    /// <returns></returns>
    public float MiniumDistanceToTheExit(Vector3 position, uint tolerance)
    {
        position = new Vector3(position.x * width, position.y * height, position.z * breath);

        float smallestDistance = 999999f;
        float temp;

        for (int z = 0; z < this.breath; z++)
            for (int y = 0; y < this.height; y++)
                for (int x = 0; x < this.width; x++)
                {
                    if (this.Get(x, y, z) < tolerance)
                    {
                        temp = Vector3.Distance(new Vector3(x, y, z), position);

                        if (smallestDistance > temp)
                        {
                            smallestDistance = temp;
                        }
                    }
                }

        if (smallestDistance < 0.001)
            smallestDistance = 0.0001f;

        return smallestDistance;
    }

    public KeyValuePair<float, Vector3> MiniumDistanceAndPosToTheExit(Vector3 position, uint tolerance)
    {
        position = new Vector3(position.x * width, position.y * height, position.z * breath);

        KeyValuePair<float, Vector3> result = new KeyValuePair<float, Vector3>();
        float smallestDistance = 999999f;
        float temp;

        for (int z = 0; z < this.breath; z++)
            for (int y = 0; y < this.height; y++)
                for (int x = 0; x < this.width; x++)
                {
                    if (this.Get(x, y, z) < tolerance)
                    {
                        temp = Vector3.Distance(new Vector3(x, y, z), position);
                        if (temp < smallestDistance)
                        {
                            smallestDistance = temp;
                            result = new KeyValuePair<float, Vector3>(smallestDistance, new Vector3((float)x / (float)this.width, (float)y / (float)this.height, (float)z / (float)this.breath));
                        }
                    }
                }

        if (smallestDistance < 0.001)
            smallestDistance = 0.0001f;

        return result;
    }


    // Writes the closest point to a database
    public ClosetPointOnVolumeData[] BuildMinimalDistanceDataBase(Vector3 position, uint tolerance)
    {
        // work out the orgin info
        Vector3 RPos = position;
        position = new Vector3(position.x * width, position.y * height, position.z * breath);
        Vector3Int vPos = new Vector3Int((int)Math.Round(position.x), (int)Math.Round(position.y), (int)Math.Round(position.z));

        // create a data stucture where you can hold that data
        List<ClosetPointOnVolumeData> result = new List<ClosetPointOnVolumeData>();

        for (int z = 0; z < this.breath; z++)
            for (int y = 0; y < this.height; y++)
                for (int x = 0; x < this.width; x++)
                {
                    if (this.Get(x, y, z) < tolerance)
                    {
                        result.Add(new ClosetPointOnVolumeData(
                        new Vector3Int(x, y, z),
                        new Vector3((float)x / (float)this.width, (float)y / (float)this.height, (float)z / (float)this.breath),
                        Vector3.Distance(new Vector3(x, y, z), position),
                        vPos,
                        RPos
                        ));
                    }
                }

        // format the array to  be inserted
        return result.ToArray();
    }

    public Vector3? GetClosestPointWithAThreashold(Vector3 start, uint greaterThan, uint lessThan = uint.MaxValue)
    {
        return GetClosestPointWithAThreashold(start, new Vector3(1, 1, 1), greaterThan, lessThan);
    }

    public static Vector3 Multi(Vector3 lhs, Vector3 rhs)
    {
        return new Vector3(
            lhs.x * rhs.x,
            lhs.y * rhs.y,
            lhs.z * rhs.z
            );
    }

    public static Vector3 Div(Vector3 lhs, Vector3 rhs)
    {
        return new Vector3(
            lhs.x / rhs.x,
            lhs.y / rhs.y,
            lhs.z / rhs.z
            );
    }

    public Vector3? GetClosestPointWithAThreashold(Vector3 start, Vector3 correction, uint greaterThan, uint lessThan = uint.MaxValue)
    {
        Queue<Vector3> toSee = new Queue<Vector3>();
        bool[] seen = new bool[this.width * this.breath * this.height];
        float ClosestPointDistance = float.MaxValue;
        Vector3? ClosetPoint = null;

        toSee.Enqueue(start);

        do
        {
            // Get and move the current point of memory
            Vector3 current = toSee.Dequeue();
            float dist = Vector3.Distance(Multi(start, correction), Multi(current, correction));

            if (dist < ClosestPointDistance && dist > 0.5)
            {
                // add all of the naubors to the to see list
                // or else it will see if they are closer than the currrent closest
                Vector3 next;
                if (current.x < this.width)
                {
                    next = current + new Vector3(1, 0, 0);
                    if (this.definition[this.GetIndex(next)] == TypeOfVoxel.inside)
                    {
                        uint v = this.Get(next);
                        if (v > greaterThan && v < lessThan)
                        {
                            seen[this.GetIndex(next)] = true;
                            toSee.Enqueue(next);
                        }
                        else if (!seen[this.GetIndex(next)])
                        {
                            dist = Vector3.Distance(Multi(start, correction), Multi(current, correction));
                            if (dist > 0.5f && ClosestPointDistance > dist && ClosestPointDistance > dist)
                            {
                                ClosestPointDistance = dist;
                                ClosetPoint = current;
                                seen[this.GetIndex(next)] = true;
                            }
                        }
                    }
                    else if (!seen[this.GetIndex(next)])
                    {
                        dist = Vector3.Distance(Multi(start, correction), Multi(current, correction));
                        if (dist > 0.5f && ClosestPointDistance > dist)
                        {
                            ClosestPointDistance = dist;
                            ClosetPoint = current;
                            seen[this.GetIndex(next)] = true;
                        }
                    }
                }
                else
                {
                    dist = Vector3.Distance(Multi(start, correction), Multi(current, correction));
                    if (dist > 0.5f && ClosestPointDistance > dist)
                    {
                        ClosestPointDistance = dist;
                        ClosetPoint = current;
                    }
                }


                if (current.x > 0)
                {
                    next = current - new Vector3(1, 0, 0);
                    if (this.definition[this.GetIndex(next)] == TypeOfVoxel.inside)
                    {
                        uint v = this.Get(next);
                        if (v > greaterThan && v < lessThan)
                        {
                            seen[this.GetIndex(next)] = true;
                            toSee.Enqueue(next);
                        }
                        else if (!seen[this.GetIndex(next)])
                        {
                            dist = Vector3.Distance(Multi(start, correction), Multi(current, correction));
                            if (dist > 0.5f && ClosestPointDistance > dist)
                            {
                                ClosestPointDistance = dist;
                                ClosetPoint = current;
                                seen[this.GetIndex(next)] = true;
                            }
                        }
                    }
                    else if (!seen[this.GetIndex(next)])
                    {
                        dist = Vector3.Distance(Multi(start, correction), Multi(current, correction));
                        if (dist > 0.5f && ClosestPointDistance > dist)
                        {
                            ClosestPointDistance = dist;
                            ClosetPoint = current;
                            seen[this.GetIndex(next)] = true;
                        }
                    }
                }
                else
                {
                    dist = Vector3.Distance(Multi(start, correction), Multi(current, correction));
                    if (dist > 0.5f && ClosestPointDistance > dist)
                    {
                        ClosestPointDistance = dist;
                        ClosetPoint = current;
                    }
                }

                if (current.y < this.height)
                {
                    next = current + new Vector3(0, 1, 0);
                    if (this.definition[this.GetIndex(next)] == TypeOfVoxel.inside)
                    {
                        uint v = this.Get(next);
                        if (v > greaterThan && v < lessThan)
                        {
                            seen[this.GetIndex(next)] = true;
                            toSee.Enqueue(next);
                            seen[this.GetIndex(next)] = true;
                        }
                        else if (!seen[this.GetIndex(next)])
                        {
                            dist = Vector3.Distance(Multi(start, correction), Multi(current, correction));
                            if (dist > 0.5f && ClosestPointDistance > dist)
                            {
                                ClosestPointDistance = dist;
                                ClosetPoint = current;
                                seen[this.GetIndex(next)] = true;
                            }
                        }
                    }
                    else if (!seen[this.GetIndex(next)])
                    {
                        dist = Vector3.Distance(Multi(start, correction), Multi(current, correction));
                        if (dist > 0.5f && ClosestPointDistance > dist)
                        {
                            ClosestPointDistance = dist;
                            ClosetPoint = current;
                            seen[this.GetIndex(next)] = true;
                        }
                    }
                }
                else
                {
                    dist = Vector3.Distance(Multi(start, correction), Multi(current, correction));
                    if (dist > 0.5f && ClosestPointDistance > dist)
                    {
                        ClosestPointDistance = dist;
                        ClosetPoint = current;
                    }
                }


                if (current.y > 0)
                {
                    next = current - new Vector3(0, 1, 0);
                    if (this.definition[this.GetIndex(next)] == TypeOfVoxel.inside)
                    {
                        uint v = this.Get(next);
                        if (v > greaterThan && v < lessThan)
                        {
                            seen[this.GetIndex(next)] = true;
                            toSee.Enqueue(next);
                        }
                        else if (!seen[this.GetIndex(next)])
                        {
                            dist = Vector3.Distance(Multi(start, correction), Multi(current, correction));
                            if (dist > 0.5f && ClosestPointDistance > dist)
                            {
                                ClosestPointDistance = dist;
                                ClosetPoint = current;
                                seen[this.GetIndex(next)] = true;
                            }
                        }
                    }
                    else if (!seen[this.GetIndex(next)])
                    {
                        dist = Vector3.Distance(Multi(start, correction), Multi(current, correction));
                        if (dist > 0.5f && ClosestPointDistance > dist)
                        {
                            ClosestPointDistance = dist;
                            ClosetPoint = current;
                            seen[this.GetIndex(next)] = true;
                        }
                    }
                }
                else
                {
                    dist = Vector3.Distance(Multi(start, correction), Multi(current, correction));
                    if (dist > 0.5f && ClosestPointDistance > dist)
                    {
                        ClosestPointDistance = dist;
                        ClosetPoint = current;
                    }
                }


                if (current.z < this.breath)
                {
                    next = current + new Vector3(0, 0, 1);
                    if (this.definition[this.GetIndex(next)] == TypeOfVoxel.inside)
                    {
                        uint v = this.Get(next);
                        if (v > greaterThan && v < lessThan)
                        {
                            seen[this.GetIndex(next)] = true;
                            toSee.Enqueue(next);
                        }
                        else if (!seen[this.GetIndex(next)])
                        {
                            dist = Vector3.Distance(Multi(start, correction), Multi(current, correction));
                            if (dist > 0.5f && ClosestPointDistance > dist)
                            {
                                ClosestPointDistance = dist;
                                ClosetPoint = current;
                                seen[this.GetIndex(next)] = true;
                            }
                        }
                    }
                    else if (!seen[this.GetIndex(next)])
                    {
                        dist = Vector3.Distance(Multi(start, correction), Multi(current, correction));
                        if (dist > 0.5f && ClosestPointDistance > dist)
                        {
                            ClosestPointDistance = dist;
                            ClosetPoint = current;
                            seen[this.GetIndex(next)] = true;
                        }
                    }
                }
                else
                {
                    dist = Vector3.Distance(Multi(start, correction), Multi(current, correction));
                    if (dist > 0.5f && ClosestPointDistance > dist)
                    {
                        ClosestPointDistance = dist;
                        ClosetPoint = current;
                    }
                }


                if (current.z > 0)
                {
                    next = current - new Vector3(0, 0, 1);
                    if (this.definition[this.GetIndex(next)] == TypeOfVoxel.inside)
                    {
                        uint v = this.Get(next);
                        if (v > greaterThan && v < lessThan)
                        {
                            seen[this.GetIndex(next)] = true;
                            toSee.Enqueue(next);
                        }
                        else if (!seen[this.GetIndex(next)])
                        {
                            dist = Vector3.Distance(Multi(start, correction), Multi(current, correction));
                            if (dist > 0.5f && ClosestPointDistance > dist)
                            {
                                ClosestPointDistance = dist;
                                ClosetPoint = current;
                                seen[this.GetIndex(next)] = true;
                            }
                        }
                    }
                    else if (!seen[this.GetIndex(next)])
                    {
                        dist = Vector3.Distance(Multi(start, correction), Multi(current, correction));
                        if (dist > 0.5f && ClosestPointDistance > dist)
                        {
                            ClosestPointDistance = dist;
                            ClosetPoint = current;
                            seen[this.GetIndex(next)] = true;
                        }
                    }
                }
                else
                {
                    dist = Vector3.Distance(Multi(start, correction), Multi(current, correction));
                    if (dist > 0.5f && ClosestPointDistance > dist)
                    {
                        ClosestPointDistance = dist;
                        ClosetPoint = current;
                    }
                }
            }

        } while (toSee.Count > 0);

        return ClosetPoint;
    }

    /// <summary>
    /// Used to work out what volumes are attached to this volume
    /// </summary>
    /// <param name="start">the start of the search</param>
    /// <param name="segmentVolumeId">an Id that gets placed on these</param>
    /// <param name="seen">A list of still valid pixels</param>
    /// <returns></returns>
    private int[] DetermineInteriorVolume(Vector3 start, int segmentVolumeId, ref bool[] seen)
    {
        List<int> output = new List<int>();
        Queue<Vector3> toSee = new Queue<Vector3>();

        toSee.Enqueue(start);

        do
        {
            // Get and move the current point of memory
            Vector3 current = toSee.Dequeue();
            output.Add(this.GetIndex(current));

            // add all of the naubors to the to see list
            Vector3 next;
            if (current.x < this.width - 1)
            {
                next = current + new Vector3(1, 0, 0);
                DetermineSegment(next, segmentVolumeId, ref seen, ref toSee);
            }
            if (current.x > 0)
            {
                next = current - new Vector3(1, 0, 0);
                DetermineSegment(next, segmentVolumeId, ref seen, ref toSee);
            }
            if (current.y < this.height - 1)
            {
                next = current + new Vector3(0, 1, 0);
                DetermineSegment(next, segmentVolumeId, ref seen, ref toSee);
            }
            if (current.y > 0)
            {
                next = current - new Vector3(0, 1, 0);
                DetermineSegment(next, segmentVolumeId, ref seen, ref toSee);
            }
            if (current.z < this.breath - 1)
            {
                next = current + new Vector3(0, 0, 1);
                DetermineSegment(next, segmentVolumeId, ref seen, ref toSee);
            }
            if (current.z > 0)
            {
                next = current - new Vector3(0, 0, 1);
                DetermineSegment(next, segmentVolumeId, ref seen, ref toSee);
            }

        } while (toSee.Count > 0);

        return output.ToArray();
    }

    private void DetermineSegment(Vector3 next, int segmentVolume, ref bool[] seen, ref Queue<Vector3> toSee)
    {
        int i = this.GetIndex(next);

        // if we haven't seen it then append it
        if (seen[i])
        {
            seen[i] = false;
            toSee.Enqueue(next);

            RelatedSegments[i] = ArrayAppend(segmentVolume, RelatedSegments[i]);
        }
        else if (definition[i] == TypeOfVoxel.border)
        {
            RelatedSegments[i] = ArrayAppend(segmentVolume, RelatedSegments[i]);
        }
    }



    private void DetermineSegmentInt(Vector3Int next, int segmentVolume, ref bool[] seen, ref Queue<Vector3Int> toSee)
    {
        int i = this.GetIndex(next.x, next.y, next.z);

        // if we haven't seen it then append it
        if (seen[i])
        {
            seen[i] = false;
            toSee.Enqueue(next);

            RelatedSegments[i] = ArrayAppend(segmentVolume, RelatedSegments[i]);
        }
        else if (definition[i] == TypeOfVoxel.border)
        {
            RelatedSegments[i] = ArrayAppend(segmentVolume, RelatedSegments[i]);
        }
    }

    /// <summary>
    /// Used to work out what volumes are attached to this volume
    /// </summary>
    /// <param name="start">the start of the search</param>
    /// <param name="segmentVolumeId">an Id that gets placed on these</param>
    /// <param name="seen">A list of still valid pixels</param>
    /// <returns></returns>
    private int[] DetermineInteriorVolumeInt(Vector3 start, int segmentVolumeId, ref bool[] seen)
    {
        List<int> output = new List<int>();
        Queue<Vector3Int> toSee = new Queue<Vector3Int>();
        int x = Mathf.RoundToInt(start.x);
        int y = Mathf.RoundToInt(start.y);
        int z = Mathf.RoundToInt(start.z);

        toSee.Enqueue(new Vector3Int(x, y, z));

        do
        {
            // Get and move the current point of memory
            Vector3Int current = toSee.Dequeue();
            output.Add(this.GetIndex(current));

            // add all of the naubors to the to see list
            Vector3Int next;
            if (current.x < this.width - 1)
            {
                next = current + new Vector3Int(1, 0, 0);
                DetermineSegmentInt(next, segmentVolumeId, ref seen, ref toSee);
            }
            if (current.x > 0)
            {
                next = current - new Vector3Int(1, 0, 0);
                DetermineSegmentInt(next, segmentVolumeId, ref seen, ref toSee);
            }
            if (current.y < this.height - 1)
            {
                next = current + new Vector3Int(0, 1, 0);
                DetermineSegmentInt(next, segmentVolumeId, ref seen, ref toSee);
            }
            if (current.y > 0)
            {
                next = current - new Vector3Int(0, 1, 0);
                DetermineSegmentInt(next, segmentVolumeId, ref seen, ref toSee);
            }
            if (current.z < this.breath - 1)
            {
                next = current + new Vector3Int(0, 0, 1);
                DetermineSegmentInt(next, segmentVolumeId, ref seen, ref toSee);
            }
            if (current.z > 0)
            {
                next = current - new Vector3Int(0, 0, 1);
                DetermineSegmentInt(next, segmentVolumeId, ref seen, ref toSee);
            }

        } while (toSee.Count > 0);

        return output.ToArray();
    }

    /// <summary>
    /// Used to work out what volumes are attached to this volume
    /// </summary>
    /// <param name="start">the start of the search</param>
    /// <param name="segmentVolumeId">an Id that gets placed on these</param>
    /// <param name="seen">A list of still valid pixels</param>
    /// <returns></returns>
    private int[] DetermineInteriorVolumeInt(Vector3Int start, int segmentVolumeId, ref bool[] seen)
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
            output.Add(this.GetIndex(current));

            // add all of the naubors to the to see list
            Vector3Int next;
            if (current.x < this.width - 1)
            {
                next = current + new Vector3Int(1, 0, 0);
                DetermineSegmentInt(next, segmentVolumeId, ref seen, ref toSee);
            }
            if (current.x > 0)
            {
                next = current - new Vector3Int(1, 0, 0);
                DetermineSegmentInt(next, segmentVolumeId, ref seen, ref toSee);
            }
            if (current.y < this.height - 1)
            {
                next = current + new Vector3Int(0, 1, 0);
                DetermineSegmentInt(next, segmentVolumeId, ref seen, ref toSee);
            }
            if (current.y > 0)
            {
                next = current - new Vector3Int(0, 1, 0);
                DetermineSegmentInt(next, segmentVolumeId, ref seen, ref toSee);
            }
            if (current.z < this.breath - 1)
            {
                next = current + new Vector3Int(0, 0, 1);
                DetermineSegmentInt(next, segmentVolumeId, ref seen, ref toSee);
            }
            if (current.z > 0)
            {
                next = current - new Vector3Int(0, 0, 1);
                DetermineSegmentInt(next, segmentVolumeId, ref seen, ref toSee);
            }

        } while (toSee.Count > 0);

        return output.ToArray();
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
            output.Add(this.GetIndex(current));

            // add all of the naubors to the to see list
            Vector3Int next;
            if (current.x < this.width - 1)
            {
                next = current + new Vector3Int(1, 0, 0);
                DetermineSegmentInt(next, segmentVolumeId, ref seen, ref toSee);
            }
            if (current.x > 0)
            {
                next = current - new Vector3Int(1, 0, 0);
                DetermineSegmentInt(next, segmentVolumeId, ref seen, ref toSee);
            }
            if (current.y < this.height - 1)
            {
                next = current + new Vector3Int(0, 1, 0);
                DetermineSegmentInt(next, segmentVolumeId, ref seen, ref toSee);
            }
            if (current.y > 0)
            {
                next = current - new Vector3Int(0, 1, 0);
                DetermineSegmentInt(next, segmentVolumeId, ref seen, ref toSee);
            }
            if (current.z < this.breath - 1)
            {
                next = current + new Vector3Int(0, 0, 1);
                DetermineSegmentInt(next, segmentVolumeId, ref seen, ref toSee);
            }
            if (current.z > 0)
            {
                next = current - new Vector3Int(0, 0, 1);
                DetermineSegmentInt(next, segmentVolumeId, ref seen, ref toSee);
            }
            if (current.x < this.width - 1 && current.y > 0)
            {
                next = current + new Vector3Int(1, -1, 0);
                DetermineSegmentInt(next, segmentVolumeId, ref seen, ref toSee);
            }
            if (current.x < this.width - 1 && current.z > 0)
            {
                next = current + new Vector3Int(1, 0, -1);
                DetermineSegmentInt(next, segmentVolumeId, ref seen, ref toSee);
            }
            if (current.y < this.height - 1 && current.x > 0)
            {
                next = current + new Vector3Int(-1, 1, 0);
                DetermineSegmentInt(next, segmentVolumeId, ref seen, ref toSee);
            }
            if (current.y < this.height - 1 && current.z > 0)
            {
                next = current + new Vector3Int(0, 1, -1);
                DetermineSegmentInt(next, segmentVolumeId, ref seen, ref toSee);
            }
            if (current.z < this.breath - 1 && current.x > 0)
            {
                next = current + new Vector3Int(-1, 0, 1);
                DetermineSegmentInt(next, segmentVolumeId, ref seen, ref toSee);
            }
            if (current.z < this.breath - 1 && current.y > 0)
            {
                next = current + new Vector3Int(0, -1, 1);
                DetermineSegmentInt(next, segmentVolumeId, ref seen, ref toSee);
            }

        } while (toSee.Count > 0);

        return output.ToArray();
    }

    #endregion

    #region segmentionParameters

    public enum TypeOfVoxel
    {
        outside = 1,
        inside = 2,
        border = 3,
        unknown = 0
    };

    public TypeOfVoxel[] definition;

    public Vector3Int[] borderPixels;

    public Vector3Int[] borderPixelsOfLargestSegment;

    public int[][] RelatedSegments;

    public int largestSegment;

    public int amountOfSegments;

    public int sizeOfLargestSegment;

    private bool segmented = false;
    public bool Segmented { get => segmented; }

    // used to create a mininmum bounding volume hyeriacty
    public Vector3 minAABB = Vector3.positiveInfinity, maxAABB = Vector3.negativeInfinity;

    #endregion

    #region segmentionBasedCode

    /// <summary>
    /// Performs a 2 pass Segmention over the dicom to
    /// segment various sets are not 
    /// </summary>
    /// <param name="tolerance"></param>
    public void SegmentDicom(uint tolerance)
    {
        // make sure this can run
        if (this.buffer == null || this.buffer.Length < 1 || segmented)
            return;


        definition = new TypeOfVoxel[this.buffer.Length];

        int index = 0;
        List<Vector3Int> border = new List<Vector3Int>();

        // find the starting point
        for (; index < this.buffer.Length; index++)
        {
            definition[index] = TypeOfVoxel.inside;
            if (index < tolerance)
            {
                definition[index] = TypeOfVoxel.outside;

                // get all of the surronding 
                Vector3 VIndex = this.GetPosition(index);

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

        // Work out what type the rest of the pixels are now we have one outside now
        for (; index < this.buffer.Length; index++)
        {
            // work out what type this object is
            definition[index] = GetTypeSixDirection(this.GetPositionAsInt(index), tolerance);
        }

        // used for later on parameters to work out differnt segments
        bool[] isInside = new bool[buffer.Length];

        index--;

        // 2nd Pass over all the pixels to ensure they where allocated correctly
        for (; index >= 0; index--)
        {
            // work out what type this object is
            definition[index] = this.GetTypeTweleveDirection(this.GetPositionAsInt(index), tolerance);

            // depending on the result then we need to filter results by a way that makes sense
            switch (definition[index])
            {
                case TypeOfVoxel.border:
                    // add the position to the border array to fit it in
                    border.Add(this.GetPositionAsInt(index));
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
        }

        // set the border pixels to the array
        //borderPixels = border.ToArray();

        // count the amount of segments that are found
        int counter = 0;

        this.RelatedSegments = new int[this.buffer.Length][];

        int increment = isInside.Length / 2;
        List<int[]> segments = new List<int[]>();
        // work out what segments belong to what segmnets
        for (index = this.buffer.Length - 1; index >= 0; index--)
        {
            int altIndex = (index + (this.buffer.Length / 2)) % this.buffer.Length;

            if (isInside[altIndex] && this.definition[altIndex] == TypeOfVoxel.inside)
            {
                // run a bfs to work detertine what pixels belong to this segmnet 
                int[] segment = DetermineInteriorVolumeIntTwelveDirection(this.GetPositionAsInt(altIndex), counter, ref isInside);

                int sizeOfSegment = segment.Length;

                if (sizeOfLargestSegment < sizeOfSegment)
                {
                    this.sizeOfLargestSegment = sizeOfSegment;
                    this.largestSegment = counter;
                }

                // set the next segment if it has more than just a small portion of data
                if (segment.Length > 8)
                {
                    segments.Add(segment);
                    counter++;
                }
            }
        }
        this.amountOfSegments = counter;

        // check the flag so this isn't called twice
        segmented = true;

        // set the border pixels to the array
        borderPixels = border.ToArray();
        Queue<Vector3Int> largest = new Queue<Vector3Int>();

        for (index = 0; index < this.borderPixels.Length; index++)
        {
            // find the AABB bounidng box for all of the border pixels
            if (minAABB.x > this.borderPixels[index].x)
                minAABB.x = this.borderPixels[index].x;
            if (minAABB.y > this.borderPixels[index].y)
                minAABB.y = this.borderPixels[index].y;
            if (minAABB.z > this.borderPixels[index].z)
                minAABB.z = this.borderPixels[index].z;
            if (maxAABB.x < this.borderPixels[index].x)
                maxAABB.x = this.borderPixels[index].x;
            if (maxAABB.y < this.borderPixels[index].y)
                maxAABB.y = this.borderPixels[index].y;
            if (maxAABB.z < this.borderPixels[index].z)
                maxAABB.z = this.borderPixels[index].z;

            int[] temp = this.RelatedSegments[this.GetIndex(this.borderPixels[index])];
            if (temp != null && ArrayContains(this.largestSegment, temp))
            {
                largest.Enqueue(this.borderPixels[index]);
            }
        }

        this.borderPixelsOfLargestSegment = largest.ToArray();

        // fix the x values I removed them becuse I was in a rush and I knew what I wanted to see from them
        // find the AABB bounidng box for all of the border pixels as percentages
        minAABB.x = minAABB.x / (float)this.width;
        minAABB.y = minAABB.y / (float)this.height;
        minAABB.z = minAABB.z / (float)this.breath;
        maxAABB.x = maxAABB.x / (float)this.width;
        maxAABB.y = maxAABB.y / (float)this.height;
        maxAABB.z = maxAABB.z / (float)this.breath;
    }

    /// <summary>
    /// Not used Currently...
    /// This was designed to work out if any segments potentially naugbored each other
    /// </summary>
    /// <param name="index">the current point in the buffer that needs to be checked</param>
    /// <returns>a list of segments that need to be merged</returns>
    private List<int> MergeNauborSegments(int index)
    {
        List<int> output = new List<int>();
        Vector3 current = this.GetPosition(index);
        Vector3 next;
        int segment = this.RelatedSegments[index][0];

        int nextIndex;
        if (current.x < this.width)
        {
            next = current + new Vector3(1, 0, 0);
            nextIndex = this.GetIndex(next);
            if (definition[nextIndex] == TypeOfVoxel.inside && segment != this.RelatedSegments[nextIndex][0])
            {
                output.AddRange(this.RelatedSegments[nextIndex]);
                ChangeValue(this.RelatedSegments[nextIndex][0], segment, this.RelatedSegments[nextIndex]);
            }
        }
        if (current.x > 0)
        {
            next = current - new Vector3(1, 0, 0);
            nextIndex = this.GetIndex(next);
            if (definition[nextIndex] == TypeOfVoxel.inside && segment != this.RelatedSegments[nextIndex][0])
            {
                output.AddRange(this.RelatedSegments[nextIndex]);
                ChangeValue(this.RelatedSegments[nextIndex][0], segment, this.RelatedSegments[nextIndex]);
            }
        }

        if (current.y < this.height - 1)
        {
            next = current + new Vector3(0, 1, 0);
            nextIndex = this.GetIndex(next);
            if (definition[nextIndex] == TypeOfVoxel.inside && segment != this.RelatedSegments[nextIndex][0])
            {
                output.AddRange(this.RelatedSegments[nextIndex]);
                ChangeValue(this.RelatedSegments[nextIndex][0], segment, this.RelatedSegments[nextIndex]);
            }
        }
        if (current.y > 0)
        {
            next = current - new Vector3(0, 1, 0);
            nextIndex = this.GetIndex(next);
            if (definition[nextIndex] == TypeOfVoxel.inside && segment != this.RelatedSegments[nextIndex][0])
            {
                output.AddRange(this.RelatedSegments[nextIndex]);
                ChangeValue(this.RelatedSegments[nextIndex][0], segment, this.RelatedSegments[nextIndex]);
            }
        }
        if (current.z < this.breath - 1)
        {
            next = current + new Vector3(0, 0, 1);
            nextIndex = this.GetIndex(next);
            if (definition[nextIndex] == TypeOfVoxel.inside && segment != this.RelatedSegments[nextIndex][0])
            {
                output.AddRange(this.RelatedSegments[nextIndex]);
                ChangeValue(this.RelatedSegments[nextIndex][0], segment, this.RelatedSegments[nextIndex]);
            }
        }
        if (current.z > 0)
        {
            next = current - new Vector3(0, 0, 1);
            nextIndex = this.GetIndex(next);
            if (definition[nextIndex] == TypeOfVoxel.inside && segment != this.RelatedSegments[nextIndex][0])
            {
                output.AddRange(this.RelatedSegments[nextIndex]);
                ChangeValue(this.RelatedSegments[nextIndex][0], segment, this.RelatedSegments[nextIndex]);
            }
        }

        return output;
    }

    private void SetAllOutsideToBoundry(Vector3 current, int minIndex = int.MaxValue)
    {
        // add all of the naubors to the to see list
        Vector3 next;
        int nextIndex;
        if (current.x < this.width)
        {
            next = current + new Vector3(1, 0, 0);
            nextIndex = this.GetIndex(next);
            if (nextIndex < minIndex)
            {
                definition[nextIndex] = TypeOfVoxel.border;
            }
        }
        if (current.x > 0)
        {
            next = current - new Vector3(1, 0, 0);
            nextIndex = this.GetIndex(next);
            if (nextIndex < minIndex)
            {
                definition[nextIndex] = TypeOfVoxel.border;
            }
        }

        if (current.y < this.height)
        {
            next = current + new Vector3(0, 1, 0);
            nextIndex = this.GetIndex(next);
            if (nextIndex < minIndex)
            {
                definition[nextIndex] = TypeOfVoxel.border;
            }
        }
        if (current.y > 0)
        {
            next = current - new Vector3(0, 1, 0);
            nextIndex = this.GetIndex(next);
            if (nextIndex < minIndex)
            {
                definition[nextIndex] = TypeOfVoxel.border;
            }
        }
        if (current.z < this.breath)
        {
            next = current + new Vector3(0, 0, 1);
            nextIndex = this.GetIndex(next);
            if (nextIndex < minIndex)
            {
                definition[nextIndex] = TypeOfVoxel.border;
            }
        }
        if (current.z > 0)
        {
            next = current - new Vector3(0, 0, 1);
            nextIndex = this.GetIndex(next);
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
    private TypeOfVoxel GetTypeSixDirection(Vector3Int current, uint tolerance = 0, int minIndex = int.MaxValue)
    {
        // make a educated guess about the pixel we are looking at by its tolerance
        TypeOfVoxel output = TypeOfVoxel.unknown;
        bool isGreaterThanTolerance = this.Get(current) > tolerance;

        // add all of the naubors to the to see list
        Vector3 next;
        int nextIndex;
        if (current.x < this.width - 1)
        {
            next = current + new Vector3Int(1, 0, 0);
            nextIndex = this.GetIndex(next);
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
            nextIndex = this.GetIndex(next);
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

        if (current.y < this.height - 1)
        {
            next = current + new Vector3Int(0, 1, 0);
            nextIndex = this.GetIndex(next);
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
            nextIndex = this.GetIndex(next);
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
        if (current.z < this.breath - 1)
        {
            next = current + new Vector3Int(0, 0, 1);
            nextIndex = this.GetIndex(next);
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
            nextIndex = this.GetIndex(next);
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
    private TypeOfVoxel GetTypeTweleveDirection(Vector3Int current, uint tolerance = 0, int minIndex = int.MaxValue)
    {
        // make a educated guess about the pixel we are looking at by its tolerance
        TypeOfVoxel output = TypeOfVoxel.unknown;
        bool isGreaterThanTolerance = this.Get(current) > tolerance;

        // add all of the naubors to the to see list
        Vector3 next;
        int nextIndex;
        if (current.x < this.width - 1)
        {
            next = current + new Vector3Int(1, 0, 0);
            nextIndex = this.GetIndex(next);
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
            nextIndex = this.GetIndex(next);
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

        if (current.y < this.height - 1)
        {
            next = current + new Vector3Int(0, 1, 0);
            nextIndex = this.GetIndex(next);
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
            nextIndex = this.GetIndex(next);
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

        if (current.z < this.breath - 1)
        {
            next = current + new Vector3Int(0, 0, 1);
            nextIndex = this.GetIndex(next);
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
            nextIndex = this.GetIndex(next);
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
            nextIndex = this.GetIndex(next);
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
            nextIndex = this.GetIndex(next);
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
            nextIndex = this.GetIndex(next);
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
        if (current.x < this.width - 1 && current.y < this.height - 1)
        {
            next = current + new Vector3Int(1, 1, 0);
            nextIndex = this.GetIndex(next);
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
        if (current.x < this.width - 1 && current.z < this.breath - 1)
        {
            next = current + new Vector3Int(1, 0, 1);
            nextIndex = this.GetIndex(next);
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
        if (current.x > 0 && current.y < this.height - 1)
        {
            next = current + new Vector3Int(-1, 1, 0);
            nextIndex = this.GetIndex(next);
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
        if (current.x > 0 && current.z < this.breath - 1)
        {
            next = current + new Vector3Int(-1, 0, 1);
            nextIndex = this.GetIndex(next);
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
        if (current.x < this.width - 1 && current.z > 0)
        {
            next = current + new Vector3Int(1, 0, -1);
            nextIndex = this.GetIndex(next);
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
        if (current.x < this.width - 1 && current.y > 0)
        {
            next = current + new Vector3Int(1, -1, 0);
            nextIndex = this.GetIndex(next);
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
        if (current.z > 0 - 1 && current.y < this.height - 1)
        {
            next = current + new Vector3Int(0, -1, 1);
            nextIndex = this.GetIndex(next);
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
        if (current.z < this.breath - 1 && current.y > 0)
        {
            next = current + new Vector3Int(0, -1, 1);
            nextIndex = this.GetIndex(next);
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

    private float GetCorrectedDistanceBetween(Vector3Int rhs, Vector3 lhs)
    {
        //rhs * lhs
        Vector3 a = new Vector3(rhs.x * pixelSpacingX, rhs.y * pixelSpacingY, rhs.z * sliceThickness * lhs.z * (sliceThickness != 0 ? sliceThickness : 1) * (spacingBetweenSlices != 0 ? spacingBetweenSlices : 1));
        Vector3 b = new Vector3(lhs.x * pixelSpacingX, lhs.y * pixelSpacingY, lhs.z * (sliceThickness != 0 ? sliceThickness : 1) * (spacingBetweenSlices != 0 ? spacingBetweenSlices : 1));

        return Vector3.Distance(a, b);
    }

    private float GetNormalizedDistanceBetween(Vector3Int rhs, Vector3 lhs)
    {
        float MaxDistance = Vector3.Distance(Vector3Int.zero, new Vector3Int(width, height, breath));

        return Vector3.Distance(rhs, lhs) / MaxDistance;
    }


    public float GetDistanceToNearestBoarder(int index)
    {
        return GetDistanceToNearestBoarder(this.GetPosition(index));
    }

    public float GetDistanceToNearestBoarder(Vector3 point)
    {
        // float of the closest point
        float closestDist = float.MaxValue;
        float closestNormalizedDistance = 1f;
        float MaxDistance = Vector3.Distance(Vector3Int.zero, new Vector3Int(width, height, breath));

        for (int index = 0; index < this.borderPixels.Length; index++)
        {
            float dist = GetCorrectedDistanceBetween(this.borderPixels[index], point);

            if (closestDist > dist)
            {
                // Logic for this function
                closestDist = dist;

                // Cacluate the normalized Distance
                closestNormalizedDistance =
                Vector3.Distance(this.borderPixels[index], point) / MaxDistance;
            }
        }

        return closestNormalizedDistance;
    }

    public Vector3 BruteForceGetNearestBorder(Vector3Int point)
    {
        int closest = 0;
        float closestDist = float.MaxValue;
        float MaxDistance = Vector3.Distance(Vector3Int.zero, new Vector3Int(width, height, breath));

        for (int index = 0; index < this.borderPixels.Length; index++)
        {
            float dist = Vector3.Distance(this.borderPixels[index], point);

            if (closestDist > dist)
            {
                closestDist = dist;
                closest = index;
            }
        }

        return this.borderPixels[closest];
    }

    public Vector3 BruteForceGetNearestBorderAsPercenatage(Vector3Int point)
    {
        int closest = 0;
        float closestDist = float.MaxValue;
        int index;
        for (index = 0; index < this.borderPixels.Length; index++)
        {
            float dist = Vector3.Distance(this.borderPixels[index], point);

            if (closestDist > dist)
            {
                closestDist = dist;
                closest = index;
            }
        }
        Debug.Log("Amount of Itterations: " + closest);
        return GetAsPercentage(this.borderPixels[closest]);
    }


    /// <summary>
    /// Should Rename...
    /// 
    /// Used as magnetic type of collision for volumetric collision. but is very slow
    /// </summary>
    /// <param name="point">
    /// A percenatage of the direction of the movement noticed by the user
    /// </param>
    /// <returns>
    /// The nearest valid point 
    /// </returns>
    public Vector3 BruteForceGetNearestBorderAsPercenatageUsingOnlyLargestBorderUsingAPointFormedAsPercenatage(Vector3 point)
    {
        int closest = 0;
        float closestDist = float.MaxValue;

        Vector3Int p = this.GetFromPercentage(point);

        for (int index = 0; index < this.borderPixelsOfLargestSegment.Length; index++)
        {
            float dist = Vector3.Distance(this.borderPixelsOfLargestSegment[index], p);

            if (closestDist > dist)
            {
                closestDist = dist;
                closest = index;
            }
        }

        return GetAsPercentage(this.borderPixels[closest]);
    }


    #endregion

    #region OctTree

    public octTree.OctTreeMedicalData GetAsOctTree()
    {
        return new octTree.OctTreeMedicalData(this.buffer, this.definition, this.largestSegment, this.RelatedSegments, width, height, breath);
    }

    #endregion

    #region smoothing

    public void AverageSmoothing(int size)
    {
        if (size % 2 == 0) throw new Exception("Can not generate a even number");

        // cacluate the offset for the start of the kernal loop
        int offset = size / 2;

        // determine a anew buffer for this object
        uint[] newBuffer = new uint[buffer.Length];

        for (int index = 0; index < buffer.Length; index++)
        {
            Vector3 current = this.GetPosition(index);
            uint divsor = 0;
            uint total = 0;

            // loop though all the kernals each time
            for (int z = 0; z < size; z++)
                for (int y = 0; y < size; y++)
                    for (int x = 0; x < size; x++)
                    {
                        Vector3 kernalPos = new Vector3(x - offset, y - offset, z - offset);
                        // if it exists then add the data to this kernal
                        if (this.Exists(current - kernalPos))
                        {
                            divsor++;
                            total += buffer[index];
                        }
                    }

            // average out the devisor and save it. 
            newBuffer[index] = (uint)Mathf.RoundToInt(Convert.ToSingle(total) / Convert.ToSingle(divsor));
        }

        this.buffer = newBuffer;
    }

    public void GaussianSmoothing(int size, float sigma, int maxValue = 256)
    {
        if (size % 2 == 0) throw new Exception("Can not generate a even number");

        //GaussianKernelGenerator.
        GaussianKernel.GaussianKernel3D kernal = GaussianKernel.GaussianKernelGenerator.Generate3DKernel(size, sigma);

        // cacluate the offset for the start of the kernal loop
        float offset = size / 2;
        float sumOfAll = 0;

        // determine a anew buffer for this object
        float[] newBuffer = new float[buffer.Length];

        for (int index = 0; index < buffer.Length; index++)
        {
            Vector3 current = this.GetPosition(index);
            float total = 0;

            // loop though all the kernals each time
            for (int z = 0; z < size; z++)
                for (int y = 0; y < size; y++)
                    for (int x = 0; x < size; x++)
                    {
                        // if it exists then add the data to this kernal
                        if (this.Exists(current - new Vector3(x - offset, y - offset, z - offset)))
                        {
                            int i = this.GetIndex(current - new Vector3(x - offset, y - offset, z - offset));
                            float KValue = Convert.ToSingle(kernal.Get(x, y, z));

                            if (KValue != 0)
                            {
                                if (i >= buffer.Length || i < 0)
                                {
                                    Debug.Log("Strange Error - index :" + i + " buffer Length :" + buffer.Length + (current - new Vector3(x - offset, y - offset, z - offset)));
                                }
                                else
                                {
                                    total += buffer[i] * KValue;
                                }
                            }
                        }
                    }

            // average out the devisor and save it. 
            newBuffer[index] = total;
            sumOfAll += total;
        }

        // normalise the values
        float highest = float.MinValue;
        float lowest = float.MaxValue;
        for (int i = 0; i < newBuffer.Length; i++)
        {
            newBuffer[i] /= sumOfAll;
            highest = Math.Max(newBuffer[i], highest);
            lowest = Math.Max(Math.Min(newBuffer[i], lowest), 0);
        }

        // set the values to the disired range
        float denominator = highest - lowest;
        for (int i = 0; i < newBuffer.Length; i++)
        {
            float current = (newBuffer[i] - lowest) / denominator;
            this.buffer[i] = System.Convert.ToUInt32(Math.Max(Mathf.RoundToInt(current * maxValue), 0));
        }
    }

    public void DifferenceOfGaussian(int sizeA, float sigmaA, int sizeB, int sigmaB, int maxValue = 256)
    {
        if (sizeA % 2 == 0 || sizeB % 2 == 0) throw new Exception("Can not generate a even number");

        //GaussianKernelGenerator.
        GaussianKernel.GaussianKernel3D kernalA = GaussianKernel.GaussianKernelGenerator.Generate3DKernel(sizeA, sigmaA);
        GaussianKernel.GaussianKernel3D kernalB = GaussianKernel.GaussianKernelGenerator.Generate3DKernel(sizeB, sigmaB);

        // cacluate the offset for the start of the kernal loop
        float offsetA = sizeA / 2;
        float offsetB = sizeB / 2;
        float sumOfAll = 0;

        // determine a anew buffer for this object
        float[] newBuffer = new float[buffer.Length];

        for (int index = 0; index < buffer.Length; index++)
        {
            Vector3 current = this.GetPosition(index);
            float totalA = 0;
            float totalB = 0;

            // loop though all the kernals each time
            for (int z = 0; z < sizeA; z++)
                for (int y = 0; y < sizeA; y++)
                    for (int x = 0; x < sizeA; x++)
                    {
                        // if it exists then add the data to this kernal
                        if (this.Exists(current - new Vector3(x - offsetA, y - offsetA, z - offsetA)))
                        {
                            int i = this.GetIndex(current - new Vector3(x - offsetA, y - offsetA, z - offsetA));
                            float KValue = Convert.ToSingle(kernalA.Get(x, y, z));
                            if (KValue != 0)
                                totalA += buffer[i] * KValue;
                        }
                    }

            // loop though all the kernals each time
            for (int z = 0; z < sizeB; z++)
                for (int y = 0; y < sizeB; y++)
                    for (int x = 0; x < sizeB; x++)
                    {
                        // if it exists then add the data to this kernal
                        if (this.Exists(current - new Vector3(x - offsetB, y - offsetB, z - offsetB)))
                        {
                            int i = this.GetIndex(current - new Vector3(x - offsetB, y - offsetB, z - offsetB));
                            float KValue = Convert.ToSingle(kernalB.Get(x, y, z));
                            if (KValue != 0)
                                totalB += buffer[i] * KValue;
                        }
                    }

            // average out the devisor and save it. 
            newBuffer[index] = totalA - totalB;
            sumOfAll += newBuffer[index];
        }

        // normalise the values
        float highest = float.MinValue;
        float lowest = float.MaxValue;
        for (int i = 0; i < newBuffer.Length; i++)
        {
            newBuffer[i] /= sumOfAll;
            highest = Math.Max(newBuffer[i], highest);
            lowest = Math.Max(Math.Min(newBuffer[i], lowest), 0);
        }

        // set the values to the disired range
        float denominator = highest - lowest;
        for (int i = 0; i < newBuffer.Length; i++)
        {
            float current = (newBuffer[i] - lowest) / denominator;
            this.buffer[i] = System.Convert.ToUInt32(Math.Max(Mathf.RoundToInt(current * maxValue), 0));
        }
    }

    #endregion

    #region edgeDetection

    /// <summary>
    /// Gets a sobel edge detection based on a 13 * 4 directional Algroithm. 
    /// </summary>
    /// <returns>
    /// a 1:1 replica of the buffer containing the edge results
    /// </returns>
    public float[] GetSobelBuffer()
    {
        float[] output = new float[this.buffer.Length];
        Sobel.VolumetricSobelKernel kernal = Sobel.VolumetricSobelKernel.Instance();
        float max = int.MinValue;
        float min = int.MaxValue;

        // detemine the raw values
        for (int index = 0; index < this.buffer.Length; index++)
        {
            float value = kernal.CalculateSobel(this.BuildVolumeKernal(index));
            output[index] = value;
            max = Math.Max(max, value);
            min = Math.Min(min, value);
        }

        Debug.Log("Sobel min (" + min + ") and Max (" + max + ") values found");

        if (max == 0)
            return new float[] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };

        // normalize the values into a set of floats between 1 and 0
        float _max = int.MinValue;
        float _min = int.MaxValue;
        for (int index = 0; index < output.Length; index++)
        {
            output[index] = (output[index] - min) / (max - min);
            _max = Math.Max(_max, output[index]);
            _min = Math.Min(_min, output[index]);
        }

        Debug.Log("Sobel min (" + _min + ") and Max (" + _max + ") values found");

        return output;
    }

    /// <summary>
    /// Builds a 3 by 3 volumetric kernal to send to the volumetric kernal
    /// </summary>
    /// <param name="index">
    /// The index in the buffer to check
    /// </param>
    /// <returns>
    /// A volumetric kernal object that is deisgned to be used for sobel.
    /// </returns>
    private int[] BuildVolumeKernal(int bufferIndex)
    {
        int[] output = new int[27];
        Vector3 Centerposition = this.GetPosition(bufferIndex);

        int index = 0;
        for (int z = -1; z < 2; z++)
            for (int y = -1; y < 2; y++)
                for (int x = -1; x < 2; x++, index++)
                {
                    output[index] = System.Convert.ToInt32(this.Get(Centerposition + new Vector3(x, y, z)));
                }

        return output;
    }

    #endregion

    #region normals

    // Define the Sobel filters for each axis
    static int[,,] sobelX = new int[3, 3, 3] {
                { { -1, 0, 1 }, { -2, 0, 2 }, { -1, 0, 1 } },
                { { -1, 0, 1 }, { -2, 0, 2 }, { -1, 0, 1 } },
                { { -1, 0, 1 }, { -2, 0, 2 }, { -1, 0, 1 } }
            };
    static int[,,] sobelY = new int[3, 3, 3] {
                { { -1, -2, -1 }, { 0, 0, 0 }, { 1, 2, 1 } },
                { { -1, -2, -1 }, { 0, 0, 0 }, { 1, 2, 1 } },
                { { -1, -2, -1 }, { 0, 0, 0 }, { 1, 2, 1 } }
            };
    static int[,,] sobelZ = new int[3, 3, 3] {
                { { -1, -2, -1 }, { -2, 0, 2 }, { -1, 2, 1 } },
                { { 0, 0, 0 }, { 0, 0, 0 }, { 0, 0, 0 } },
                { { 1, 2, 1 }, { 2, 0, 2 }, { 1, 2, 1 } }
            };

    /// <summary>
    /// Calculates the normal convolution of a voxel at a given position using a 3x3x3 Sobel filter.
    /// </summary>
    /// <param name="x">The current index within the buffer</param>
    /// <returns>A vector representing the normal of the voxel.</returns>
    public Vector4 CalculateNormalConvolution(int index)
    {
        if (index < 0 || index >= this.buffer.Length) 
            throw new System.ArgumentOutOfRangeException("Index was out of Range - index = " + index);
        return CalculateNormalConvolution(this.GetPositionAsInt(index));
    }

    /// <summary>
    /// Calculates the normal convolution of a voxel at a given position using a 3x3x3 Sobel filter.
    /// </summary>
    /// <param name="x">The x coordinate of the voxel.</param>
    /// <param name="y">The y coordinate of the voxel.</param>
    /// <param name="z">The z coordinate of the voxel.</param>
    /// <returns>A vector representing the normal of the voxel.</returns>
    public Vector4 CalculateNormalConvolution(Vector3Int location)
    {
        // Apply the Sobel filters to the volume
        double gradientX = 0;
        double gradientY = 0;
        double gradientZ = 0;
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                for (int k = 0; k < 3; k++)
                {
                    Vector3Int offset = new Vector3Int(i, j, k);
                    if (this.Exists(new Vector3Int(i, j, k) - offset))
                    {
                        gradientX += this.Get(location - offset) * sobelX[i, j, k];
                        gradientY += this.Get(location - offset) * sobelY[i, j, k];
                        gradientZ += this.Get(location - offset) * sobelZ[i, j, k];
                    }
                }
            }
        }

        // Cacluate then output the errors
        Vector4 output = new Vector4();
        output.w = System.Convert.ToSingle(Math.Sqrt(gradientX * gradientX + gradientY * gradientY + gradientZ * gradientZ));

        //double[] normal = new double[3];
        output.x = System.Convert.ToSingle(gradientX / output.w);
        output.y = System.Convert.ToSingle(gradientY / output.w);
        output.z = System.Convert.ToSingle(gradientZ / output.w);

        return output;
    }
    #endregion

    public static int[] ArrayAppend(int value, int[] array)
    {
        int[] newArray;

        if (array != null)
        {
            newArray = new int[array.Length + 1];

            for (int index = 0; index < array.Length; index++)
            {
                if (array[index] == value)
                    return array;

                newArray[index] = array[index];
            }
        }
        else
        {
            newArray = new int[1];
        }

        newArray[newArray.Length - 1] = value;

        return newArray;
    }

    public static int[] Merge(int[] left, int[] right)
    {
        if (left == null)
        {
            return right;
        }
        else if (right == null)
        {
            return left;
        }

        int[] array = new int[left.Length + right.Length];

        for (int index = 0; index < left.Length; index++)
        {
            array[index] = left[index];
        }
        for (int index = 0; index < right.Length; index++)
        {
            array[left.Length + index] = right[index];
        }

        return array;
    }

    public static bool ArrayContains(int value, int[] array)
    {
        for (int index = 0; index < array.Length; index++)
        {
            if (array[index] == value)
            {
                return true;
            }
        }
        return false;
    }

    public static void ChangeValue(int value, int changeTo, int[] array)
    {
        for (int index = 0; index < array.Length; index++)
        {
            if (array[index] == value)
            {
                array[index] = changeTo;
            }
        }
    }

    public Vector3 GetAsPercentage(Vector3 vector)
    {
        return new Vector3(
            vector.x / this.width,
            vector.y / this.height,
            vector.z / this.breath
            );
    }

    public Vector3 GetAsPercentage(Vector3Int vector)
    {
        return new Vector3(
            System.Convert.ToSingle(vector.x) / this.width,
            System.Convert.ToSingle(vector.y) / this.height,
            System.Convert.ToSingle(vector.z) / this.breath
            );
    }

    public Vector3Int GetFromPercentage(Vector3 vector)
    {
        return new Vector3Int(
            this.width - Mathf.Clamp(Mathf.RoundToInt(vector.x * this.width), 0, width),
            this.height - Mathf.Clamp(Mathf.RoundToInt(vector.y * this.height), 0, height),
            this.breath - Mathf.Clamp(Mathf.RoundToInt(vector.z * this.breath), 0, breath)
            );
    }

}


// 
// below is old code that is used to merge sections when they where created.
// It has been optimised a fair amount but it is really slow
// this was later found to be a caused by a floating point error by the lower loop
//

/*
        List<int[]> finalSegementionResults = new List<int[]>();
        List<int> merged = new List<int>();
        for (index = 0; index < segments.Count; index++)
        {
            // if it wont exist after this dont' do anything about it
            if (!merged.Contains(index))
            {
                // all of the segments
                int[] segment = segments[index];

                // loop though the pixels assoiated with this to see if they can be merged
                for(int i = 0; i < segment.Length; i++)
                {
                    if (this.definition[segment[i]] == TypeOfVoxel.inside && this.RelatedSegments[segment[i]] != null)
                    {
                        // fix the largest range
                        List<int> arraysToMerge = MergeNauborSegments(segment[i]);

                        // remove the duplicates
                        if (arraysToMerge.Count > 0)
                        {
                            // merge all the arrays found
                            for(int j = 0; j < arraysToMerge.Count; j++)
                            {
                                int[] other = segments[arraysToMerge[j]];

                                segment = Merge(segment, segments[arraysToMerge[j]]);

                                for(int k = 0; k < other.Length; k++)
                                {
                                    this.RelatedSegments[other[k]][0] = index;
                                }
                            }

                            //segment = segment.Distinct().ToArray();
                            // add the new values the the merged array
                            merged.AddRange(arraysToMerge);
                        }
                    }
                }

                // remove duplicates
                segment = segment.Distinct().ToArray();

                // add the new segment to the final output
                finalSegementionResults.Add(segment);

                // size of the largst segment
                if (sizeOfLargestSegment < segment.Length)
                {
                    this.sizeOfLargestSegment = segment.Length;
                    this.largestSegment = index;
                }
            }
        }

        this.amountOfSegments = finalSegementionResults.Count;
        */
/*
int index = 0;
int i = 0;
for(int z = 0; z < this.breath; z++)
    for (int y = 0; y < this.height; y++)
        for (int x = 0; x < this.width; x++)
        {
            if (!this.GetPosition(index).Equals(new Vector3(x, y, z)))
            {
                Debug.LogError("index: " + index + ", Vector: " + new Vector3(x, y, z));
                i++;
            }
            else if (!this.GetIndex(x, y, z).Equals(index))
            {
                Debug.LogError("2nd Option: " + "index: " + index + ", Vector: " + new Vector3(x, y, z) + ",Expected: " + this.GetPosition(index) + "index: " + this.GetIndex(x, y, z));

            i++;
        }
            if (i > 20)
            {
                return;
            }

            /*if (index > 16777255)
            {
                //Debug.Log("Test: " + "index: " + index + ", Vector: " + new Vector3(x, y, z) + ",Expected: " + this.GetPosition(index) + "index: " + this.GetIndex(new Vector3(x, y, z)));
                Debug.Log("Test: " + "index: " + index + ", Vector: " + new Vector3(x, y, z) + ",Expected: " + this.GetPosition(index) + "index: " + this.GetIndex(x, y, z));
                i++;
            index++;
        }

            }*/
