using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnswersFromGeneration
{
    public static AnswersFromGeneration GenerateAnswers(HierachicalObjects[] inputData, SphericalVolumeHierarchyLevelDetails[] levelInfo)
    {
        AnswersFromGeneration output = new AnswersFromGeneration(inputData.Length, levelInfo.Length);
        Dictionary<Color, ColorDetails> colorDictionary = new Dictionary<Color, ColorDetails>();

        // Save the required depth information
        //output.depthInfo = new DepthInformation(inputData, transform);

        for (int index = 0; index < inputData.Length; index++)
        {
            int level = inputData[index].AmountOfParents();

            // Write up the details regarding the hyeracy based on the amount of parents the child had
            if (level < output.hierachyDetails.Length)
            {
                // Create a new object if one dosn't exist
                if (output.hierachyDetails[level] == null)
                {
                    // Saves the current implmention
                    output.hierachyDetails[level] = new HierarchyDetails(level, inputData[index].color);
                    output.hierachyDetails[level].smallestRegion = inputData[index];
                    output.hierachyDetails[level].largestRegion = inputData[index];
                    output.hierachyDetails[level].voxelsUsed = 0; // just paronoia
                }
                else
                {
                    // Adjust details in the hyeracy as needed
                    if (output.hierachyDetails[level].smallestRegion.AmountOfVoxelsWithin > inputData[index].AmountOfVoxelsWithin)
                        output.hierachyDetails[level].smallestRegion = inputData[index];

                    if (output.hierachyDetails[level].largestRegion.AmountOfVoxelsWithin < inputData[index].AmountOfVoxelsWithin)
                        output.hierachyDetails[level].largestRegion = inputData[index];
                }
                // Increment the amount of voxels as required
                output.hierachyDetails[level].voxelsUsed += inputData[index].AmountOfVoxelsWithin;
            }


            // Add Info to the color output
            if (colorDictionary.ContainsKey(inputData[index].color))
            {
                colorDictionary[inputData[index].color].amountOfVoxels += inputData[index].AmountOfVoxelsWithin;
                colorDictionary[inputData[index].color].amountOfRegions++;
            }
            else
            {
                colorDictionary[inputData[index].color] = new ColorDetails(inputData[index].color, inputData[index].AmountOfVoxelsWithin, 1);
            }
        }

        // Convert the color dictionary to a array
        output.informationAboutColors = new ColorDetails[colorDictionary.Count];
        colorDictionary.Values.CopyTo(output.informationAboutColors, 0);

        return output;
    }

    public static AnswersFromGeneration GenerateAnswers(HierachicalObjects[] inputData, SphericalVolumeHierarchyLevelDetails levelInfo, int amountContained, int amountUncontained, int amountOfContainers, int amountOfOuters, int amountOfCountables)
    {
        AnswersFromGeneration output = new AnswersFromGeneration(inputData.Length, 3, amountContained, amountUncontained, amountOfContainers, amountOfOuters, amountOfCountables);
        Dictionary<Color, ColorDetails> colorDictionary = new Dictionary<Color, ColorDetails>();

        // Save the required depth information
        //output.depthInfo = new DepthInformation(inputData, transform);

        for (int index = 0; index < inputData.Length; index++)
        {
            int level = inputData[index].AmountOfParents();

            // Add Info to the color output
            if (colorDictionary.ContainsKey(inputData[index].color))
            {
                colorDictionary[inputData[index].color].amountOfVoxels += inputData[index].AmountOfVoxelsWithin;
                colorDictionary[inputData[index].color].amountOfRegions++;
            }
            else
            {
                colorDictionary[inputData[index].color] = new ColorDetails(inputData[index].color, inputData[index].AmountOfVoxelsWithin, 1);
            }
        }

        // Convert the color dictionary to a array
        output.informationAboutColors = new ColorDetails[colorDictionary.Count];
        colorDictionary.Values.CopyTo(output.informationAboutColors, 0);

        return output;
    }

    private AnswersFromGeneration(int amountOfRegionsInTotal, int amountOfLevels, int amountContained, int amountUncontained, int amountOfContainers, int amountOfOuters, int amountOfCountables)
    {
        this.amountOfRegionsInTotal = amountOfRegionsInTotal;
        this.hierachyDetails = new HierarchyDetails[amountOfLevels];
        this.amountOfCountablesInsideAnother = amountContained;
        this.amountOfCountablesOutside = amountUncontained;
        this.amountOfContainers = amountOfContainers;
        this.amountOfOuters = amountOfOuters;
        this.amountOfCountables = amountOfCountables;
    }

    private AnswersFromGeneration(int amountOfRegionsInTotal, int amountOfLevels)
    {
        this.amountOfRegionsInTotal = amountOfRegionsInTotal;
        this.hierachyDetails = new HierarchyDetails[amountOfLevels];
        this.amountOfCountablesInsideAnother = -1;
        this.amountOfCountablesOutside = -1;
    }

    [SerializeField] ColorDetails[] informationAboutColors;

    [SerializeField] HierarchyDetails[] hierachyDetails;

    [SerializeField] DepthInformation depthInfo;

    [SerializeField] int amountOfRegionsInTotal;

    [SerializeField] int amountOfCountablesInsideAnother;
    [SerializeField] int amountOfCountablesOutside;
    [SerializeField] int amountOfContainers;
    [SerializeField] int amountOfOuters;
    [SerializeField] int amountOfCountables;

    public int AmountOfCountablesInsideAnother { get => amountOfCountablesInsideAnother; }
    public int AmountOfCountablesOutside { get => this.amountOfCountablesOutside; }
    public int AmountOfContainers { get => this.amountOfContainers; }
    public int AmountOfOuters { get => this.amountOfOuters; }
    public int AmountOfCountables { get => this.amountOfCountables; }

    [System.Serializable]
    public class HierarchyDetails
    {
        public HierarchyDetails(int level, Color color)
        {
            this.level = level;
            this.color = color;
        }

        [SerializeField] int level;
        [SerializeField] public int voxelsUsed = 0;
        [SerializeField] Color color;
        [SerializeField] public HierachicalObjects smallestRegion;
        [SerializeField] public HierachicalObjects largestRegion;
    }

    [System.Serializable]
    public class ColorDetails
    {
        public ColorDetails(Color color, int amountOfVoxels, int amountOfRegions)
        {
            this.color = color;
            this.amountOfVoxels = amountOfVoxels;
            this.amountOfRegions = amountOfRegions;
        }

        [SerializeField] public Color color;
        [SerializeField] public int amountOfVoxels = 0;
        [SerializeField] public int amountOfRegions = 0;
    }

    [System.Serializable]
    public class DepthInformation
    {
        [SerializeField] public DepthInformationShapeInfo[] faceFirstDepthPerceptionIndexs = null;
        [SerializeField] public DepthInformationShapeInfo[] centerfirstDepthPerceptionIndexs = null;

        public DepthInformation(HierachicalObjects[] data, Transform objectsTransform)
        {
            faceFirstDepthPerceptionIndexs = new DepthInformationShapeInfo[data.Length];
            centerfirstDepthPerceptionIndexs = new DepthInformationShapeInfo[data.Length];

            for (int index = 0; index < data.Length; index++)
            {
                faceFirstDepthPerceptionIndexs[index] = new FaceFirstDepthInfo(data[index], index, objectsTransform);
                centerfirstDepthPerceptionIndexs[index] = new CentralDepthInfo(data[index], index, objectsTransform);
            }

            System.Array.Sort(faceFirstDepthPerceptionIndexs, 0, faceFirstDepthPerceptionIndexs.Length, null);
            System.Array.Sort(centerfirstDepthPerceptionIndexs, 0, centerfirstDepthPerceptionIndexs.Length, null);
        }

        [System.Serializable]
        public class DepthInformationShapeInfo : HierachicalObjects
        {
            [SerializeField] public int originalIndex = -1;

            protected Transform transform;

            public DepthInformationShapeInfo(HierachicalObjects shapeInfo, int index, Transform transform)
            {
                this.originalIndex = index;
                this.AABB_Max = shapeInfo.AABB_Max;
                this.AABB_Min = shapeInfo.AABB_Min;
                this.AmountOfVoxelsWithin = shapeInfo.AmountOfVoxelsWithin;
                this.Parent = shapeInfo.Parent;
                this.Children = shapeInfo.Children;
                this.importance = shapeInfo.importance;
                this.positon = shapeInfo.positon;
                this.radius = shapeInfo.radius;
                this.color = shapeInfo.color;
                this.transform = transform;
            }

            public static float DistanceToAABB(Vector3 min, Vector3 max, Vector3 point)
            {
                Bounds bounds = new Bounds();
                bounds.SetMinMax(min, max);
                Vector3 closet = bounds.ClosestPoint(point);
                return Vector3.Distance(closet, point);
            }

            public override void AddDataToAverageVoxelPostion(Vector3 newPosition)
            {
                // DO nothing
            }

            public override int AmountOfParents()
            {
                if (this.Parent == null)
                {
                    return 0;
                }
                else
                {
                    return 1 + this.Parent.AmountOfParents();
                }
            }

            public override HierachicalObjects DraftNewChild(int childIndex, MinAndMaxFloat minAndMaxRadiusMultipler)
            {
                // should never be called
                throw new NotImplementedException();
            }

            public override HierachicalObjects DraftNewChild(int childIndex, MinAndMaxFloat containableRadiusRange, int AmountOfTimesToLookForABetterObject = 50, int StartingIndexToLookForBestRandomlyGeneratedPoint = 1, int StartingIndexToLookCheckForBestPositionAgainst = 0, float SphereTolerance = 0)
            {
                // should never be called
                throw new NotImplementedException();
            }

            public override float GetVolume()
            {
                return 0.0f;
            }

            public override bool HasAncestor(HierachicalObjects possibleAncestor)
            {
                return (this.Parent != null);
            }

            public override bool IsDefault()
            {
                // No default found
                return false;
            }

            public override HierachicalObjects MoveChild(int childIndex)
            {
                // should never be called
                throw new NotImplementedException();
            }

            public override float TotalPercentOfThisVolumeThatIsFree(int CurrentChildIndex = -1)
            {
                // should never be called
                return 0;
            }
        }

        [System.Serializable]
        public class FaceFirstDepthInfo : DepthInformationShapeInfo, IComparer, IComparable
        {
            public FaceFirstDepthInfo(HierachicalObjects shapeInfo, int index, Transform transform)
                : base(shapeInfo, index, transform) { }

            public int Compare(object x, object y)
            {
                if (x is FaceFirstDepthInfo && y is FaceFirstDepthInfo)
                {
                    FaceFirstDepthInfo lhs = (FaceFirstDepthInfo)x;
                    FaceFirstDepthInfo rhs = (FaceFirstDepthInfo)y;

                    // convert the points in to world space and then check how far away they are from zero
                    float lhsDistance = DistanceToAABB(
                        lhs.AABB_Min,
                        lhs.AABB_Max,
                        this.transform.InverseTransformPoint(Vector3.zero)
                        );

                    float rhsDistance = DistanceToAABB(
                        rhs.AABB_Min,
                        rhs.AABB_Max,
                        this.transform.InverseTransformPoint(Vector3.zero)
                        );

                    if (lhsDistance < rhsDistance)
                    {
                        return -1;
                    }
                    else if (lhsDistance > rhsDistance)
                    {
                        return 1;
                    }
                    else
                    {
                        return 0;
                    }
                }
                else
                {
                    throw new System.ArgumentException("Tried to compare two objects that where not childrend of type \"DepthInformationShapeInfo\"");
                }
            }

            public int CompareTo(object obj)
            {
                if (obj is FaceFirstDepthInfo)
                {
                    FaceFirstDepthInfo rhs = (FaceFirstDepthInfo)obj;

                    float lhsDistance = DistanceToAABB(
                        this.AABB_Min,
                        this.AABB_Max,
                        this.transform.InverseTransformPoint(Vector3.zero)
                        );

                    float rhsDistance = DistanceToAABB(
                        rhs.AABB_Min,
                        rhs.AABB_Max,
                        this.transform.InverseTransformPoint(Vector3.zero)
                        );

                    if (lhsDistance < rhsDistance)
                    {
                        return -1;
                    }
                    else if (lhsDistance > rhsDistance)
                    {
                        return 1;
                    }
                    else
                    {
                        return 0;
                    }
                }
                else
                {
                    throw new System.ArgumentException("Tried to compare two objects that where not childrend of type \"DepthInformationShapeInfo\"");
                }
            }
        }

        [System.Serializable]
        public class CentralDepthInfo : DepthInformationShapeInfo, IComparer, IComparable
        {
            public CentralDepthInfo(HierachicalObjects shapeInfo, int index, Transform transform)
                   : base(shapeInfo, index, transform) { }

            public int Compare(object x, object y)
            {
                if (x is CentralDepthInfo && y is CentralDepthInfo)
                {
                    CentralDepthInfo lhs = (CentralDepthInfo)x;
                    CentralDepthInfo rhs = (CentralDepthInfo)y;

                    if (lhs.positon.z < rhs.positon.z)
                    {
                        return -1;
                    }
                    else if (lhs.positon.z > rhs.positon.z)
                    {
                        return 1;
                    }
                    else
                    {
                        return 0;
                    }
                }
                else
                {
                    throw new System.ArgumentException("Tried to compare two objects that where not childrend of type \"DepthInformationShapeInfo\"");
                }
            }

            public int CompareTo(object obj)
            {
                if (obj is CentralDepthInfo)
                {
                    CentralDepthInfo rhs = (CentralDepthInfo)obj;

                    if (this.positon.z < rhs.positon.z)
                    {
                        return -1;
                    }
                    else if (this.positon.z > rhs.positon.z)
                    {
                        return 1;
                    }
                    else
                    {
                        return 0;
                    }
                }
                else
                {
                    throw new System.ArgumentException("Tried to compare two objects that where not childrend of type \"DepthInformationShapeInfo\"");
                }
            }
        }
    }

    public string ToJSON()
    {
        return JsonUtility.ToJson(this, true);
    }
}
