using UnityEngine;

[System.Serializable]

public class MinAndMaxFloat
{
    [SerializeField] private float _min;
    [SerializeField] private float _max;

    public float min { get => _min; }
    public float max { get => _max; }

    public MinAndMaxFloat(float min, float max)
    {
        Verify(min, max);
    }

    public void Verify(float min, float max)
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
