using System.Collections.Generic;

/// <summary>
/// A collection of static funcitons that compute various permientaions for various uses.
/// Several of these are ports from various tutorials that have been
/// ported to C# or rewirtten slightly for more general use (See documention for links to the original).
/// </summary>
/// <typeparam name="T">
/// The gereric type that you want to find a paraterter for
/// </typeparam>
public class PermutationGenerator<T>
{

    #region BalancedLatinSquare
    /// <summary>
    /// Generates a blanced latin squared array for the given itteration of its use. 
    /// </summary>
    /// <example>
    /// How to use:
    /// var conditions = ["A", "B", "C", "D"];
    /// balancedLatinSquare(conditions, 0)  //=> ["A", "B", "D", "C"]
    /// balancedLatinSquare(conditions, 1)  //=> ["B", "C", "A", "D"]
    /// balancedLatinSquare(conditions, 2)  //=> ["C", "D", "B", "A"]
    /// </example>
    /// <param name="conditions">
    /// A array of conditions required to be counterballanced
    /// </param>
    /// <param name="itteration">
    /// The current itteration required
    /// </param>
    /// <returns>
    /// The balanced latin square for the partipant.
    /// </returns>
    public static T[] BalancedLatinSquare(T[] conditions, int itteration)
    {
        T[] result = new T[conditions.Length];
        // Based on "Bradley, J. V. Complete counterbalancing of immediate sequential effects in a Latin square design. J. Amer. Statist. Ass.,.1958, 53, 525-528. "
        for (int i = 0, j = 0, h = 0; i < conditions.Length; i++)
        {
            var val = 0;
            if (i < 2 || i % 2 != 0)
            {
                val = j++;
            }
            else
            {
                val = conditions.Length - h - 1;
                ++h;
            }

            var idx = (val + itteration) % conditions.Length;
            result[i] = conditions[idx];
        }

        // if a odd number of conditions exists and the pariticpant id is odd then reverse the order to prevent any matches.
        if (conditions.Length % 2 != 0 && itteration % 2 != 0)
        {
            System.Array.Reverse(result);
        }

        return result;
    }

    /// <summary>
    /// Computes all of the possible balanced latin square candidates as a double array ( or matrix)
    /// </summary>
    /// <param name="conditions">
    /// The condions to that need to be itteratiated though
    /// </param>
    /// <returns>
    /// a double array with all of the required itterations for each participant. 
    /// The first element of this will be as long as the amount of partipants required
    /// (equal to the amount of conditions if equal, double of the amount if it is odd).
    /// </returns>
    public static T[][] GetAllBalancedLatinSquareItterations(T[] conditions)
    {
        // Work out the length of the blanalnce latin square (the length of contditions if it is even or else double)
        int amountOfItterationsRequired = conditions.Length % 2 == 0 ? conditions.Length : conditions.Length * 2;
        T[][] output = new T[amountOfItterationsRequired][];

        // Computer all of the purutations
        for(int index = 0; index < amountOfItterationsRequired; index++)
        {
            output[index] = BalancedLatinSquare(conditions, index);
        }

        return output;
    }

    #endregion

    #region Permutations 

    /// <summary>
    /// A list 
    /// </summary>
    /// <param name="conditions">
    /// A array of various conditions of a generic type that need to be sorted in to every possible permentation.
    /// </param>
    /// <returns>
    /// A List of a list object that has all of its permutations contained within it. 
    /// </returns>
    /// <see cref="https://www.chadgolden.com/blog/finding-all-the-permutations-of-an-array-in-c-sharp"/>
    public static IList<IList<T>> Permute(T[] conditions)
    {
        var list = new List<IList<T>>();
        return DoPermute(conditions, 0, conditions.Length - 1, list);
    }

    /// <summary>
    /// Performs the permuation logic using regression
    /// </summary>
    /// <param name="conditions">
    /// Possible conditions that need to be rearranged
    /// </param>
    /// <param name="start">
    /// the index to start at
    /// </param>
    /// <param name="end">
    /// the final index for this perminenation
    /// </param>
    /// <param name="list">
    /// the current list of veriables that has been used
    /// </param>
    /// <returns>
    /// The new list with the updated data in it
    /// </returns>
    /// <see cref="https://www.chadgolden.com/blog/finding-all-the-permutations-of-an-array-in-c-sharp"/>
    static IList<IList<T>> DoPermute(T[] conditions, int start, int end, IList<IList<T>> list)
    {
        if (start == end)
        {
            // We have one of our possible n! solutions,
            // add it to the list.
            list.Add(new List<T>(conditions));
        }
        else
        {
            for (var i = start; i <= end; i++)
            {
                Swap(ref conditions[start], ref conditions[i]);
                DoPermute(conditions, start + 1, end, list);
                Swap(ref conditions[start], ref conditions[i]);
            }
        }

        return list;
    }

    /// <summary>
    /// Changes the values of A and B for each other
    /// </summary>
    /// <param name="a">
    /// First object that needs to be swaped
    /// </param>
    /// <param name="b">
    /// The second object that needs to be swaped
    /// </param>
    /// <see cref="https://www.chadgolden.com/blog/finding-all-the-permutations-of-an-array-in-c-sharp"/>
    static void Swap(ref T a, ref T b)
    {
        T temp = a;
        a = b;
        b = temp;
    }

    /// <summary>
    /// A quick shuffleing 
    /// </summary>
    /// <param name="array"></param>
    /// <param name="seed"></param>
    /// <returns></returns>
    public static T[] Shuffle(T[] array, int seed = int.MinValue)
    {
        if (seed != int.MinValue)
            UnityEngine.Random.InitState((seed ^ 15485863) * 24862048);

        T temp;
        for (int i = 0; i < array.Length - 1; i++)
        {
            int rnd = UnityEngine.Random.Range(i, array.Length);
            temp = array[rnd];
            array[rnd] = array[i];
            array[i] = temp;
        }

        return array;
    }

    #endregion
}
