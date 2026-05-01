
public class PermutationsHandler<T> : IterationOrderHandler<T>
{
    T[][] permutations;

    public PermutationsHandler(T[] conditions)
    {
        var temp = PermutationGenerator<T>.Permute(conditions);
        permutations = new T[temp.Count][];

        for (int index = 0; index < permutations.Length; index++)
        {
            permutations[index] = new T[temp[index].Count];
            temp[index].CopyTo(permutations[index], 0);
        }
    }

    public override T[] Get(int iteration)
    {
        return this.permutations[iteration % this.permutations.Length];
    }
}
