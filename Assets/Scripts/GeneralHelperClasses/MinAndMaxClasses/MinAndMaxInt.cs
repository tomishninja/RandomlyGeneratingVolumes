using UnityEngine;

[System.Serializable]
public class MinAndMaxInt
{
    [SerializeField] private int _min;
    [SerializeField] private int _max;

    public int min { get => _min; }
    public int max { get => _max; }

    public MinAndMaxInt(int min, int max)
    {
        Verify(min, max);
    }

    public void Verify(int min, int max)
    {
        if (min < max)
        {
            this._min = min;
            this._max = max;
        }
        else
        {
            this._max = min;
            this._min = max;
        }
    }
}
