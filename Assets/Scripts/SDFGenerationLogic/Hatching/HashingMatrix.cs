using UnityEngine;


[System.Serializable]
public class HashingMatrix
{
    public Vector3 lineA;
    public Vector3 lineB;
    public Vector3 lineC;

    public bool IsModifiable = true;

    /// <summary>
    /// Returns a new HashingMatrix with the same values as this one.   
    /// </summary>
    /// <returns>
    /// A new HashingMatrix with the same values as this one.
    /// </returns>
    public HashingMatrix Clone()
    {
        HashingMatrix output = new HashingMatrix();
        output.lineA = this.lineA;
        output.lineB = this.lineB;
        output.lineC = this.lineC;
        return output;
    }

    /// <summary>
    /// Returns a new HashingMatrix with random values between 0 and 1000.
    /// </summary>
    /// <param name="min">
    /// The lowest possible value for the hash in this matrix
    /// </param>
    /// <param name="max">
    /// The highest possible value for the hash in this matrix
    /// </param>
    /// <returns>
    /// A new HashingMatrix with random values between 0 and 1000.
    /// </returns>
    public static HashingMatrix InitalizeRandomHashingMatrix(float min = 0f, float max = 1000f)
    {
            HashingMatrix output = new HashingMatrix();
            output.lineA = RandomLine(min, max);
            output.lineB = RandomLine(min, max);
            output.lineC = RandomLine(min, max);
            return output;
    }

    /// <summary>
    /// Returns a random set of values for a line in a hashing matrix.
    /// </summary>
    /// <param name="min">
    /// The lowest possible value for the hash in this matrix
    /// </param>
    /// <param name="max">
    /// The highest possible value for the hash in this matrix
    /// </param>
    /// <returns>
    /// A vector3 with random values between min and max
    /// </returns>
    private static Vector3 RandomLine(float min, float max)
    {
        float range = Mathf.Abs(max - min);
        return new Vector3(
            (Random.value * range) + min,
            (Random.value * range) + min,
            (Random.value * range) + min
            );
    }

    /// <summary>
    /// Sets the values of this HashingMatrix to the values of the given Material.
    /// </summary>
    /// <param name="mat">
    /// A Unity Material that requires a 3 by 3 hashing matrix
    /// </param>
    /// <param name="lineA">
    /// A string that represents the name of the first line of the hashing matrix in the given Material.
    /// </param>
    /// <param name="lineB">
    /// A string that represents the name of the second line of the hashing matrix in the given Material.
    /// </param>
    /// <param name="lineC">
    /// A string that represents the name of the Third line of the hashing matrix in the given Material.
    /// </param>
    public void SetInShader(Material mat, string lineA, string lineB, string lineC)
    {
        mat.SetVector(lineA, this.lineA);
        mat.SetVector(lineB, this.lineB);
        mat.SetVector(lineC, this.lineC);
    }

    public override string ToString()
    {
        string output = this.lineA.ToString("F5");
        output += ",";
        output += this.lineB.ToString("F5");
        output += ",";
        output += this.lineC.ToString("F5");
        return output;
    }
}
