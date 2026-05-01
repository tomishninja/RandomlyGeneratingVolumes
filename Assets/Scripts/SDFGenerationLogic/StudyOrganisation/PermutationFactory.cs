using UnityEngine;

public enum PermutationType
{
    Permeation = 0,
    BallancedLatinSquare,
    Shuffle
}

[System.Serializable]
public class PermutationFactory<T> : MonoBehaviour
{
    public IterationOrderHandler<T> BuildPermuationCreator(PermutationType type, T[] conditions)
    {
        switch (type)
        {
            case PermutationType.Permeation:
                return new PermutationsHandler<T>(conditions);
            case PermutationType.BallancedLatinSquare:
                return new LatinBalanceSquareHandler<T>(conditions);
            case PermutationType.Shuffle:
                return new ShufflingHandler<T>(conditions);
            default:
                return null;
        }
    }
}
