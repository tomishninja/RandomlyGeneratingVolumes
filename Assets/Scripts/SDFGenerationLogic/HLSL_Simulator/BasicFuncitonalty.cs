using UnityEngine;

namespace hLSL_Simulator
{
    public static class BasicFuncitonalty
    {
        public static float Frac(float value)
        {
            return value - Mathf.Floor(value);
        }

        public static Vector3 Frac(Vector3 value)
        {
            return new Vector3(Frac(value.x), Frac(value.y), Frac(value.z));
        }

        public static Vector3 Floor(Vector3 value)
        {
            return new Vector3(
                Mathf.Floor(value.x),
                Mathf.Floor(value.y),
                Mathf.Floor(value.z)
                );
        }

        public static Vector3 Mutliply(Vector3 lhs, float rhs)
        {
            return new Vector3(
                lhs.x * rhs,
                lhs.y * rhs,
                lhs.z * rhs
                );
        }

        public static Vector3 Division(float lhs, Vector3 rhs)
        {
            return new Vector3(
                lhs / rhs.x,
                lhs / rhs.y,
                lhs / rhs.z
                );
        }

        public static Vector3 ComponentWiseDivision(Vector3 lhs, Vector3 rhs)
        {
            return new Vector3(
                lhs.x / rhs.x,
                lhs.y / rhs.y,
                lhs.z / rhs.z
                );
        }

        public static Vector3 ComponentWiseMultiplication(Vector3 lhs, Vector3 rhs)
        {
            return new Vector3(
                lhs.x * rhs.x,
                lhs.y * rhs.y,
                lhs.z * rhs.z
                );
        }

        public static Vector3 Add(Vector3 lhs, float rhs)
        {
            return new Vector3(
                lhs.x + rhs,
                lhs.y + rhs,
                lhs.z + rhs
                );
        }

        public static Vector3 Subtract(Vector3 lhs, float rhs)
        {
            return new Vector3(
                lhs.x - rhs,
                lhs.y - rhs,
                lhs.z - rhs
                );
        }
    }
}

