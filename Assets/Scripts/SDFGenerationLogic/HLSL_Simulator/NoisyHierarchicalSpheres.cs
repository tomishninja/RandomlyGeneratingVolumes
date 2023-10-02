using GenoratingRandomSDF;
using System.Collections.Generic;
using UnityEngine;

namespace hLSL_Simulator
{
    public class NoisyHierarchicalSpheres
    {
        readonly int randomCoords;
        HashingMatrix hashingMatrix;
        readonly float sDFTollerance;
        float NoiseMultipler = 1f;
        private ShapeHandeler shapes;

        public NoisyHierarchicalSpheres(ref ShapeHandeler shapes, ref HashingMatrix hashingMatrix, float SDFTollerance = 0, int randomCoords = 8, float noiseMultipler = 1f)
        {
            this.randomCoords = randomCoords;
            this.shapes = shapes;
            this.hashingMatrix = hashingMatrix;
            this.sDFTollerance = SDFTollerance;
            NoiseMultipler = noiseMultipler;
        }

        public bool CheckSDFFirstLayer(Vector3 pos)
        {
            int index = -1;
            float d = SDF(pos, out index);
            return d < 0;
        }

        public void SetShapes(ShapeHandeler shapes)
        {
            this.shapes = shapes;
        }

        public void SetHashingMatrix(HashingMatrix hashingMatrix)
        {
            this.hashingMatrix = hashingMatrix;
        }

        // Cacluates the index value as well
        public Stack<int> GetAllValidSDFs(Vector3 p)
        {
            Stack<int> output = new Stack<int>();
            float n = Noise(p);
            AbstractGeometricShape[] shapeArray = shapes.GetShapesAsArray();

            for (int i = 0; i < shapeArray.Length; i++)
            {
                if (shapeArray[i] != null && !shapeArray[i].IsDefault() && shapeArray[i].radius > 0.001)
                {
                    float d = Sphere(p, shapeArray[i].positon, shapeArray[i].radius) - n;
                    if (d < sDFTollerance)
                    {
                        output.Push(i);
                    }
                }
            }

            // return the closest distance
            return output;
        }



        // Cacluates the index value as well
        public Stack<int> GetAllValidSDFs(Vector3 p, float tollerance)
        {
            Stack<int> output = new Stack<int>();
            float n = Noise(p);
            AbstractGeometricShape[] shapeArray = shapes.GetShapesAsArray();

            for (int i = 0; i < shapeArray.Length; i++)
            {
                if (shapeArray[i] != null && !shapeArray[i].IsDefault() && shapeArray[i].radius > 0.01)
                {
                    float d = Sphere(p, shapeArray[i].positon, shapeArray[i].radius) - n;
                    if (d < tollerance)
                    {
                        output.Push(i);
                    }
                }
            }

            // return the closest distance
            return output;
        }

        // Cacluates the index value as well
        public float SDF(Vector3 p, out int index, float tollerance)
        {
            AbstractGeometricShape[] shapeArray = shapes.GetShapesAsArray();

            if (shapeArray == null || shapeArray.Length < 1)
            {
                index = -1;
                return float.PositiveInfinity;
            }

            float n = Noise(p);
            float bestDistance = Sphere(p, shapeArray[0].positon, shapeArray[0].radius) - n;
            index = 0;

            for (int i = 1; i < shapeArray.Length; i++)
            {
                if (shapeArray[i] != null)
                {
                    float d = Sphere(p, shapeArray[i].positon, shapeArray[i].radius) - n;

                    bool isBest = d < tollerance && d > bestDistance;

                    index = isBest ? i : index;
                    bestDistance = isBest ? d : bestDistance;
                }
            }

            // return the closest distance
            return bestDistance;
        }

        // Cacluates the index value as well
        public float SDF(Vector3 p, out int index)
        {
            AbstractGeometricShape[] shapeArray = shapes.GetShapesAsArray();

            if (shapeArray == null || shapeArray.Length < 1)
            {
                index = -1;
                return float.PositiveInfinity;
            }

            float n = Noise(p);
            float bestDistance = Sphere(p, shapeArray[0].positon, shapeArray[0].radius) - n;
            index = 0;

            for (int i = 1; i < shapeArray.Length; i++)
            {
                if (shapeArray[i] != null)
                {
                    float d = Sphere(p, shapeArray[i].positon, shapeArray[i].radius) - n;

                    bool isBest = d < 0 && d > bestDistance;

                    index = isBest ? i : index;
                    bestDistance = isBest ? d : bestDistance;
                }
            }

            // return the closest distance
            return bestDistance;
        }


        // https://iquilezles.org/articles/distfunctions/
        static float Sphere(Vector3 p, Vector3 c, float r)
        {
            return (p - c).magnitude - r;
        }

        //https://iquilezles.org/articles/gradientnoise/
        // returns 3D value noise
        float Noise(Vector3 x)
        {
            x *= randomCoords;
            // grid
            Vector3 p = BasicFuncitonalty.Floor(x);
            Vector3 w = BasicFuncitonalty.Frac(x);

            // quintic interpolant
            Vector3 u = BasicFuncitonalty.ComponentWiseMultiplication(
                BasicFuncitonalty.ComponentWiseMultiplication(
                BasicFuncitonalty.ComponentWiseMultiplication(w, w), w),
                BasicFuncitonalty.Add(BasicFuncitonalty.ComponentWiseMultiplication(w, BasicFuncitonalty.Subtract(BasicFuncitonalty.Mutliply(w, 6.0f), 15.0f)), 10.0f));


            // gradients
            Vector3 ga = Hash(p + new Vector3(0.0f, 0.0f, 0.0f));
            Vector3 gb = Hash(p + new Vector3(1.0f, 0.0f, 0.0f));
            Vector3 gc = Hash(p + new Vector3(0.0f, 1.0f, 0.0f));
            Vector3 gd = Hash(p + new Vector3(1.0f, 1.0f, 0.0f));
            Vector3 ge = Hash(p + new Vector3(0.0f, 0.0f, 1.0f));
            Vector3 gf = Hash(p + new Vector3(1.0f, 0.0f, 1.0f));
            Vector3 gg = Hash(p + new Vector3(0.0f, 1.0f, 1.0f));
            Vector3 gh = Hash(p + new Vector3(1.0f, 1.0f, 1.0f));

            // projections
            float va = Vector3.Dot(ga, w - new Vector3(0.0f, 0.0f, 0.0f));
            float vb = Vector3.Dot(gb, w - new Vector3(1.0f, 0.0f, 0.0f));
            float vc = Vector3.Dot(gc, w - new Vector3(0.0f, 1.0f, 0.0f));
            float vd = Vector3.Dot(gd, w - new Vector3(1.0f, 1.0f, 0.0f));
            float ve = Vector3.Dot(ge, w - new Vector3(0.0f, 0.0f, 1.0f));
            float vf = Vector3.Dot(gf, w - new Vector3(1.0f, 0.0f, 1.0f));
            float vg = Vector3.Dot(gg, w - new Vector3(0.0f, 1.0f, 1.0f));
            float vh = Vector3.Dot(gh, w - new Vector3(1.0f, 1.0f, 1.0f));

            // interpolation
            return (va +
                u.x * (vb - va) +
                u.y * (vc - va) +
                u.z * (ve - va) +
                u.x * u.y * (va - vb - vc + vd) +
                u.y * u.z * (va - vc - ve + vg) +
                u.z * u.x * (va - vb - ve + vf) +
                u.x * u.y * u.z * (-va + vb + vc - vd + ve - vf - vg + vh)) / (randomCoords * NoiseMultipler);
        }

        // https://www.shadertoy.com/view/4dffRH
        public Vector3 Hash(Vector3 p)
        {
            //p = new Vector3(Mathf.Sin(Vector3.Dot(p, new Vector3(127.1f, 311.7f, 74.7f))),
            //              Mathf.Sin(Vector3.Dot(p, new Vector3(269.5f, 183.3f, 246.1f))),
            //              Mathf.Sin(Vector3.Dot(p, new Vector3(113.5f, 271.9f, 124.6f))));

            p = new Vector3(Mathf.Sin(Vector3.Dot(p, hashingMatrix.lineA)),
                         Mathf.Sin(Vector3.Dot(p, hashingMatrix.lineB)),
                          Mathf.Sin(Vector3.Dot(p, hashingMatrix.lineC)));

            return BasicFuncitonalty.Add(BasicFuncitonalty.Mutliply(BasicFuncitonalty.Frac(BasicFuncitonalty.Mutliply(p, 43758.5453123f)), 2.0f), -1.0f);
        }

        public string GetHashAsString()
        {
            return this.hashingMatrix.ToString();
        }
    }
}
