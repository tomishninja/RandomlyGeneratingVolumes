public class LatinBalanceSquareHandler<T> : IterationOrderHandler<T>
{
    T[] permutations;

    public LatinBalanceSquareHandler(T[] conditions)
    {
        permutations = conditions;
    }

    public override T[] Get(int iteration)
    {
        return PermutationGenerator<T>.BalancedLatinSquare(permutations, iteration);
    }
}
