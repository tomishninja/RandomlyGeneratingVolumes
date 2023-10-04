/// <summary>
/// The following code was written by Thomas Clarke on 20th of April 2022
/// 
/// Licenced using the MIT Licence
/// 
/// The following name space handels the creation of Gaussian kernals in 2D and 3D
/// and a data object allowing O(1) access to the data.
/// 
/// </summary>
namespace GaussianKernel
{
    /// <summary>
    /// The abstract class of the Gaussian Kernel objects.
    /// Allows for the retrival of the underlying data. 
    /// All gaussian kernals are expected to be regular shapes. 
    /// </summary>
    public abstract class GaussianKernel
    {
        /// <summary>
        /// the height width and depth (respectively). 
        /// </summary>
        public int size { get; private set; }

        /// <summary>
        /// Stores the raw buffer information for a given state. 
        /// </summary>
        public double[] buffer { get; private set; }

        /// <summary>
        /// Parent constuctor for all Gausian Kernel Objects
        /// </summary>
        /// <param name="buffer">The raw data in the format of a array of doubles</param>
        /// <param name="size">the height width and depth (respectively) as a int</param>
        public GaussianKernel(double[] buffer, int size)
        {
            this.size = size;
            this.buffer = buffer;
        }
    }

    /// <summary>
    /// A 2D Gaussian Kernel
    /// </summary>
    public class GaussianKernel2D : GaussianKernel
    {
        /// <summary>
        /// Makes a 2D Gausian Kernel
        /// </summary>
        /// <param name="buffer">The raw data in the format of a array of doubles</param>
        /// <param name="size">the height width (respectively) as a int</param>
        public GaussianKernel2D(double[] buffer, int size) : base(buffer, size) { }

        /// <summary>
        /// Returns the pixel at the given position. 
        /// </summary>
        /// <param name="x">
        /// the amuont of columns in the value is in
        /// </param>
        /// <param name="y">
        /// the amount of rows the value is in
        /// </param>
        /// <returns>
        /// The value of the item at this point in the buffer
        /// </returns>
        public double Get(int x, int y)
        {
            return buffer[(y * size) + x];
        }


        public override string ToString()
        {
            int amountOfChars = 0;
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < amountOfChars; x++)
                {
                    sb.Append("-");
                }
                sb.AppendLine();

                amountOfChars = 0;
                for (int x = 0; x < size; x++)
                {
                    if (x != 0)
                    {
                        sb.Append("|");
                        amountOfChars++;
                    }

                    string s = this.Get(x, y).ToString("N5");
                    amountOfChars += s.Length;
                    sb.Append(s);
                }

                sb.AppendLine();

            }

            return sb.ToString();
        }
    }

    /// <summary>
    /// A 3D Gaussian Kernel
    /// </summary>
    public class GaussianKernel3D : GaussianKernel
    {

        /// <summary>
        /// Makes a 2D Gausian Kernel
        /// </summary>
        /// <param name="buffer">The raw data in the format of a array of doubles</param>
        /// <param name="size">the height width and depth (respectively) as a int</param>
        public GaussianKernel3D(double[] buffer, int size) : base(buffer, size) { }

        /// <summary>
        /// Returns the pixel at the given position. 
        /// </summary>
        /// <param name="x">
        /// the amuont of columns in the value is in
        /// </param>
        /// <param name="y">
        /// the amount of rows the value is in
        /// </param>
        /// <param name="z">
        /// the amount of slices deep the object is
        /// </param>
        /// <returns>
        /// The value of the item at this point in the buffer
        /// </returns>
        public double Get(int x, int y, int z)
        {
            return buffer[(z * (size * size)) + (y * size) + x];
        }

        public override string ToString()
        {
            int amountOfChars = 0;
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            for (int z = 0; z < size; z++)
            {
                if (z > 0)
                {
                    sb.AppendLine();
                    sb.AppendLine("z :" + z);
                }

                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < amountOfChars; x++)
                    {
                        sb.Append("-");
                    }
                    sb.AppendLine();

                    amountOfChars = 0;
                    for (int x = 0; x < size; x++)
                    {
                        if (x != 0)
                        {
                            sb.Append("|");
                            amountOfChars++;
                        }

                        string s = this.Get(x, y, z).ToString("N5");
                        amountOfChars += s.Length;
                        sb.Append(s);
                    }

                    sb.AppendLine();
                }
            }


            return sb.ToString();
        }
    }

    /// <summary>
    /// Static Functions for generating Gaussian Kernels
    /// </summary>
    public static class GaussianKernelGenerator
    {
        /// <summary>
        /// Creates a 2D Gaussian Kernel
        /// </summary>
        /// <param name="size">
        /// The height and width of your kernel
        /// </param>
        /// <param name="sigma">
        /// Controls how "fat" your cernel is going to be
        /// </param>
        /// <returns>
        /// A 2D Gaussian Kernel Object
        /// </returns>
        public static GaussianKernel2D Generate2DKernel(int size, double sigma = 1)
        {
            double sum = 0.0;
            double[] buffer = new double[(int)System.Math.Pow(size, 2)];
            int mean = size / 2;

            int index = 0;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++, index++)
                {
                    buffer[index] = System.Math.Exp(-0.5 * (System.Math.Pow((x - mean) / sigma, 2.0) + System.Math.Pow((y - mean) / sigma, 2.0)))
                          / (2 * System.Math.PI * sigma * sigma);

                    // Accumulate the kernel values
                    sum += buffer[index];
                }
            }

            // Normalize the kernel
            for (index = 0; index < buffer.Length; index++)
                buffer[index] /= sum;

            return new GaussianKernel2D(buffer, size);
        }

        /// <summary>
        /// Creates a 3D Gaussian Kernel
        /// </summary>
        /// <param name="size">
        /// The height, width and depth of your kernel
        /// </param>
        /// <param name="sigma">
        /// Controls how "fat" your kernel is going to be
        /// </param>
        /// <returns>
        /// A 2D Gaussian Kernel Object
        /// </returns>
        public static GaussianKernel3D Generate3DKernel(int size, double sigma = 1)
        {
            double sum = 0.0;
            double[] buffer = new double[(int)System.Math.Pow(size, 5)];
            int mean = size / 2;

            int index = 0;
            for (int z = 0; z < size; z++)
            {
                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++, index++)
                    {
                        buffer[index] = System.Math.Exp(-0.5 * (System.Math.Pow((x - mean) / sigma, 2.0) + System.Math.Pow((y - mean) / sigma, 2.0) + System.Math.Pow((z - mean) / sigma, 2.0)))
                              / (3 * System.Math.PI * sigma * sigma * sigma);

                        // Accumulate the kernel values
                        sum += buffer[index];
                    }
                }
            }


            // Normalize the kernel
            for (index = 0; index < buffer.Length; index++)
                buffer[index] /= sum;

            return new GaussianKernel3D(buffer, size);
        }
    }
}