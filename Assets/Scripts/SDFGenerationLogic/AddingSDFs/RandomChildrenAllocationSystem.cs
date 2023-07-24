using System.Collections.Generic;

public class RandomChildrenAllocationSystem
{
    Dictionary<int, List<int[]>> lookUpTable = new Dictionary<int, List<int[]>>();
    int[] possibleAmountsOfChildren = null;
    int minAmountOfChildrenPossible = int.MaxValue;
    int maxAmountOfChildrenPossible = int.MinValue;

    // save the data from the input parameters to prevent other systems from creating duplicates.
    int minChildrenPerParent;
    int maxChildrenPerParent;
    int amountOfParents;

    // A method to help fix the current random value
    int currentRandomValue;

    public RandomChildrenAllocationSystem(int minChildren, int maxChildren, int amountOfParentObjects)
    {
        // Determine the amount of different combinations possible
        int AmountOfObjects = (int)System.Math.Round(System.Math.Pow(maxChildren - minChildren, amountOfParentObjects));
        int[][] differentCombinationsOfChildren = new int[AmountOfObjects][];
        int i = 0;
        // Generate all possible combinations of numbers that can be rolled
        differentCombinationsOfChildren = PopulateOptions(minChildren, maxChildren, amountOfParentObjects, differentCombinationsOfChildren, ref i);

        // Sort and catagorise the data collected in to a dictionary that will allow the look up of different purmulations based on amount of children created
        List<int> posibleAmountsOfChildren = new List<int>();
        for (int index = 0; index < differentCombinationsOfChildren.Length; index++)
        {
            int sumation = this.Sum(differentCombinationsOfChildren[index]);
            minAmountOfChildrenPossible = System.Math.Min(sumation, minAmountOfChildrenPossible);
            maxAmountOfChildrenPossible = System.Math.Max(sumation, maxAmountOfChildrenPossible);

            if (!posibleAmountsOfChildren.Contains(sumation))
            {
                posibleAmountsOfChildren.Add(sumation);
                lookUpTable.Add(sumation, new List<int[]>());
            }

            //Add the data to the look up table
            lookUpTable[sumation].Add(differentCombinationsOfChildren[index]);
        }

        // Save the data found to an array
        possibleAmountsOfChildren = posibleAmountsOfChildren.ToArray();

        minChildrenPerParent = minChildren;
        maxChildrenPerParent = maxChildren;
        amountOfParents = amountOfParentObjects;
    }

    /// <summary>
    /// Returns a random number of children that is valid in the data output
    /// </summary>
    /// <param name="persistQueue">
    /// False if you just want to get a random number,
    /// True if you want a random number to persist over some time.
    /// Use the Depersist Method to stop chose a new random number if you wish to persist the value
    /// </param>
    /// <returns></returns>
    public int GenerateARadomAmountOfChildren(bool persistQueue = false)
    {
        if (persistQueue == true && IsPossible(currentRandomValue))
        {
            return currentRandomValue;
        }

        int output = minAmountOfChildrenPossible;

        // find a range that is possible and works in this use case.
        do
        {
            output = UnityEngine.Random.Range(minAmountOfChildrenPossible, maxAmountOfChildrenPossible + 1);
        } while (!IsPossible(output));

        currentRandomValue = output;

        return output;
    }

    public void DepersistsRandomValue()
    {
        currentRandomValue = minAmountOfChildrenPossible;
        do
        {
            currentRandomValue--;
        } while (IsPossible(currentRandomValue));
    }

    public int[] GetRandomPermitationFor(int amount)
    {
        if (!IsPossible(amount)) return null;

        return lookUpTable[amount][UnityEngine.Random.Range(0, lookUpTable[amount].Count)];
    }

    public bool IsPossible(int amount)
    {
        for (int index = 0; index < possibleAmountsOfChildren.Length; index++)
        {
            if (possibleAmountsOfChildren[index] == amount)
            {
                return true;
            }
        }

        return false;
    }

    private int[][] PopulateOptions(int minChildren, int maxChildren, int amountOfParentObjects, 
        int[][] differentCombinationsOfChildren, ref int currentArrayIndex, 
        int[] currentDataSet = null, int currentParentIndex = 0)
    {
        if (currentDataSet == null)
            currentDataSet = new int[amountOfParentObjects];

        // loop though all of the data
        for (int index = minChildren; index < maxChildren; index++)
        {
            // each itteration update the data set value
            currentDataSet[currentParentIndex] = index;

            // check if this is a leaf item
            if (currentParentIndex + 1 < amountOfParentObjects)
                // if it is not a leaf value then move down to the next item
                differentCombinationsOfChildren = PopulateOptions(minChildren, maxChildren, amountOfParentObjects, differentCombinationsOfChildren, ref currentArrayIndex, currentDataSet, currentParentIndex + 1);
            else
            {
                // we only want to save the leaf objects
                differentCombinationsOfChildren[currentArrayIndex] = (int[])currentDataSet.Clone();
                currentArrayIndex++;
            }
        }

        return differentCombinationsOfChildren;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    private int Sum (int[] input)
    {
        if (input == null) return 0;

        int output = 0;

        for (int index = 0; index < input.Length; index++)
        {
            output += input[index];
        }

        return output;
    }

    public bool MatchesParameters(int min, int max, int amountOfParentObjects)
    {
        return minChildrenPerParent == min &&
            maxChildrenPerParent == max &&
            amountOfParents == amountOfParentObjects;
    }
}
