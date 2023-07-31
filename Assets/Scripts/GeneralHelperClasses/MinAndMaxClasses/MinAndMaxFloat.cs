using UnityEngine;

[System.Serializable]

public class MinAndMaxFloat
{
    [SerializeField] private float _min;
    [SerializeField] private float _max;

    public float min { get => _min; set => _min = value; }
    public float max { get => _max; set => _max = value; }

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

    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
            return false;

        MinAndMaxFloat other = (MinAndMaxFloat)obj;

        return other.min == this.min && other.max == this.max;
    }

    public override int GetHashCode()
    {
        return min.GetHashCode() * max.GetHashCode();
    }
}
