public class LatinBallanceSquareHandeler<T> : ItterationOrderHandeler<T>
{
    T[] permutations;

    public LatinBallanceSquareHandeler(T[] conditions)
    {
        permutations = conditions;
    }

    public override T[] Get(int itteration)
    {
        return PermutationGenerator<T>.BalancedLatinSquare(permutations, itteration);
    }
}
