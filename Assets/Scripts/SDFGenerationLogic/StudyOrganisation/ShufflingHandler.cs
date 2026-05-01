public class ShufflingHandler<T> : IterationOrderHandler<T>
{
    T[] conditions;

    public ShufflingHandler(T[] conditions)
    {
        this.conditions = conditions;
    }

    public override T[] Get(int iteration)
    {
        return PermutationGenerator<T>.Shuffle(conditions);
    }
}
