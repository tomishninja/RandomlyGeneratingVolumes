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
    public ItterationOrderHandeler<T> BuildPermuationCreator(PermutationType type, T[] conditions)
    {
        switch (type)
        {
            case PermutationType.Permeation:
                return new PermutationsHandeler<T>(conditions);
            case PermutationType.BallancedLatinSquare:
                return new LatinBallanceSquareHandeler<T>(conditions);
            case PermutationType.Shuffle:
                return new ShufflingHandeler<T>(conditions);
            default:
                return null;
        }
    }
}
