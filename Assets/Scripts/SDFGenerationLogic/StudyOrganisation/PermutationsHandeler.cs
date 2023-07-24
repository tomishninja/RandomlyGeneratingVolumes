
public class PermutationsHandeler<T> : ItterationOrderHandeler<T>
{
    T[][] permutations;

    public PermutationsHandeler(T[] conditions)
    {
        var temp = PermutationGenerator<T>.Permute(conditions);
        permutations = new T[temp.Count][];

        for (int index = 0; index < permutations.Length; index++)
        {
            permutations[index] = new T[temp[index].Count];
            temp[index].CopyTo(permutations[index], 0);
        }
    }

    public override T[] Get(int itteration)
    {
        return this.permutations[itteration % this.permutations.Length];
    }
}
