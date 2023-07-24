public class ShufflingHandeler<T> : ItterationOrderHandeler<T>
{
    T[] conditions;

    public ShufflingHandeler(T[] conditions)
    {
        this.conditions = conditions;
    }

    public override T[] Get(int itteration)
    {
        return PermutationGenerator<T>.Shuffle(conditions);
    }
}
