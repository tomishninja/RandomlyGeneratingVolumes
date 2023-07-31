using UnityEngine;

[System.Serializable]
public class MinAndMaxInt
{
    [SerializeField] private int _min;
    [SerializeField] private int _max;

    public int min { get => _min; set => _min = value; }
    public int max { get => _max; set => _max = value; }

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
