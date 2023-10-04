namespace Sobel
{
    public class SobelKernal
    {   
         /// <summary>
         /// the height width and depth (respectively). 
         /// </summary>
        public int size { get; private set; }

        /// <summary>
        /// Stores the raw buffer information for a given state. 
        /// </summary>
        public int[] buffer { get; private set; }

        /// <summary>
        /// Parent constuctor for all Gausian Kernel Objects
        /// </summary>
        /// <param name="buffer">The raw data in the format of a array of doubles</param>
        /// <param name="size">the height width and depth (respectively) as a int</param>
        public SobelKernal(int[] buffer, int size)
        {
            this.size = size;
            this.buffer = buffer;
        }

        public double Get(int x, int y)
        {
            return buffer[(y * size) + x];
        }
    }

    public class VolumetricSobelKernel
    {
        // the singleton object
        private static VolumetricSobelKernel instance;

        // logic data for all the different itterations
        private readonly int[][] cycles;

        // various kernels to test
        private SobelKernal h;
        private SobelKernal v;
        private SobelKernal dl;
        private SobelKernal dr;

        public int CalculateSobel(VolumetricKernal3D kernal)
        {
            int output = 0;

            for (int cycleIndex = 0; cycleIndex < cycles.Length; cycleIndex++)
            {
                int currentCycleSum = 0;

                for(int index = 0; index < cycles[cycleIndex].Length; index++)
                {
                    currentCycleSum += System.Math.Abs(HorizontalBuffer[index] * kernal.buffer[cycles[cycleIndex][index]]);
                    currentCycleSum += System.Math.Abs(VerticalBuffer[index] * kernal.buffer[cycles[cycleIndex][index]]);
                    currentCycleSum += System.Math.Abs(DiagonalLeftBuffer[index] * kernal.buffer[cycles[cycleIndex][index]]);
                    currentCycleSum += System.Math.Abs(DiagonalRightBuffer[index] * kernal.buffer[cycles[cycleIndex][index]]);
                }

                System.Math.Max(output, currentCycleSum);
            }
            return (int)System.Math.Round(output / 32f);
        }

        public float CalculateSobel(int[] kernal)
        {
            if (kernal.Length != 27) return -1;

            float output = 0;

            for (int cycleIndex = 0; cycleIndex < cycles.Length; cycleIndex++)
            {
                int currentCycleSum_HB = 0;
                int currentCycleSum_VB = 0;
                int currentCycleSum_DLB = 0;
                int currentCycleSum_DRB = 0;

                for (int index = 0; index < cycles[cycleIndex].Length; index++)
                {
                    currentCycleSum_HB += HorizontalBuffer[index] * kernal[cycles[cycleIndex][index]];
                    currentCycleSum_VB += VerticalBuffer[index] * kernal[cycles[cycleIndex][index]];
                    currentCycleSum_DLB += DiagonalLeftBuffer[index] * kernal[cycles[cycleIndex][index]];
                    currentCycleSum_DRB += DiagonalRightBuffer[index] * kernal[cycles[cycleIndex][index]];
                }

                output = System.Convert.ToSingle(System.Math.Max(output, System.Math.Sqrt(
                    (currentCycleSum_HB * currentCycleSum_HB) +
                    (currentCycleSum_VB * currentCycleSum_VB) +
                    (currentCycleSum_DLB * currentCycleSum_DLB) +
                    (currentCycleSum_DLB * currentCycleSum_DLB)
                )));
            }
            return output;
        }

        public static VolumetricSobelKernel Instance() {
            if (instance == null)
            {
                instance = new VolumetricSobelKernel();
            }
            return instance;
        }

        private VolumetricSobelKernel()
        {
            h = new SobelKernal(HorizontalBuffer, 3);
            v = new SobelKernal(VerticalBuffer, 3);
            dl = new SobelKernal(DiagonalLeftBuffer, 3);
            dr = new SobelKernal(DiagonalRightBuffer, 3);

            cycles = new int[][]{
                StandingPerpendicularIndex,
                StandingParallelIndex,
                StandingDiganalLeftIndex,
                StandingDiganalRightIndex,
                FlatIndex,
                FlatDiagonalRight,
                FlatDiagonalLeft,
                FloatDiagonalForward,
                FloatDiagonalBackward,
                DiagonalBaLeBo_FrRiTo,
                DiagonalFrRiBo_BaLeTo,
                DiagonalFrLeBo_BaRiTo,
                DiagonalBaRLeBo_FRRiTo
            };
        }

        #region indexsForCalculation

        private static int[] StandingPerpendicularIndex = { 3, 4, 5, 12, 13, 14, 21, 22, 23 };
        private static int[] StandingParallelIndex = { 1, 4, 7, 10, 13, 16, 19, 22, 25 };
        private static int[] StandingDiganalLeftIndex = { 0, 4, 8, 9, 13, 17, 18, 22, 26 };
        private static int[] StandingDiganalRightIndex = { 2, 4, 6, 11, 13, 15, 20, 22, 24 };

        private static int[] FlatIndex = { 9, 10, 11, 12, 13, 14, 15, 16, 17 };
        private static int[] FlatDiagonalRight = { 0, 3, 6, 10, 13, 16, 20, 23, 26 };
        private static int[] FlatDiagonalLeft = { 2, 5, 8, 10, 13, 16, 18, 21, 24 };
        private static int[] FloatDiagonalForward = { 6, 7, 8, 12, 13, 14, 18, 19, 20 };
        private static int[] FloatDiagonalBackward = { 0, 1, 2, 12, 13, 14, 24, 25, 26 };

        private static int[] DiagonalBaLeBo_FrRiTo = { 3, 6, 7, 9, 13, 17, 19, 20, 23 };
        private static int[] DiagonalFrRiBo_BaLeTo = { 1, 2, 5, 9, 13, 17, 21, 24, 25 };
        private static int[] DiagonalFrLeBo_BaRiTo = { 3, 0, 1, 15, 13, 11, 25, 26, 23 };
        private static int[] DiagonalBaRLeBo_FRRiTo = { 7, 8, 5, 15, 13, 11, 21, 18, 19 };

        #endregion

        #region KernalData

        static int[] HorizontalBuffer = {
                1, 2, 1,
                0 ,0, 0,
                -1, -2, -1
            };

        static int[] VerticalBuffer = {
                -1, 0, 1,
                -2, 0, 2,
                -1, 0, 1
            };

        static int[] DiagonalLeftBuffer = {
                0, 1, 2,
                -1, 0, 1,
                -2, -1, 0
            };

        static int[] DiagonalRightBuffer = {
                2, 1, 0,
                1, 0, -1,
                0, 1, -2
            };

        #endregion
    }

    public class VolumetricKernal3D
    {
        public int[] buffer = new int[27];
        public int size = 3;

        public int Get(int x, int y, int z)
        {
            return buffer[(z * (size * size)) + (y * size) + x];
        }

        public void Set(int x, int y, int z, int value)
        {
            buffer[(z * (size * size)) + (y * size) + x] = value;
        }

        public void SetBuffer(int[] newBuffer)
        {
            if (newBuffer.Length == buffer.Length)
            {
                this.buffer = newBuffer;
            }
        }

        public VolumetricKernal3D(int[] buffer)
        {
            if (buffer.Length == 27)
            {
                this.buffer = buffer;
            }
        }

    }

    public class VolumetricKernal2D
    {
        public int[] buffer = new int[27];
        public int size = 3;

        public int Get(int x, int y, int z)
        {
            return buffer[(z * (size * size)) + (y * size) + x];
        }

        public void Set(int x, int y, int z, int value)
        {
            buffer[(z * (size * size)) + (y * size) + x] = value;
        }
    }
}



