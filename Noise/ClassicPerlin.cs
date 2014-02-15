using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Noise
{
    /// <summary>
    /// Implemented by all noise generation classes. To be used as a service.
    /// </summary>
    public interface INoiseGenerator
    {
        /// <summary>
        /// Generates 3D noise given three double precision floating-point values.
        /// </summary>
        /// <param name="x">X-axis value.</param>
        /// <param name="y">Y-axis value.</param>
        /// <param name="z">Z-axis value.</param>
        /// <returns>Original Perlin noise value in a double precision floating-point.</returns>
        double Generate(double x, double y, double z);

        /// <summary>
        /// Name of the algorithm used by the noise generator.
        /// </summary>
        string Name { get; }
    }

    public class ClassicPerlin : INoiseGenerator
    {
        protected int mSeed;
        protected Random mRand;
        protected int[] mPerm;

        /// <summary>
        /// Array of twelve gradient vectors for the 3D grid-cell.
        /// </summary>
        protected int[][] mGrad = new int[12][] 
            { 
                new int[] { 1, 1, 0 },
                new int[] { -1, 1, 0 },
                new int[] { 1, -1, 0 },
                new int[] { -1, -1, 0 },
                new int[] { 1, 0, 1 },
                new int[] { -1, 0, 1 },
                new int[] { 1, 0, -1 },
                new int[] { -1, 0,-1},
                new int[] { 0, 1, 1 },
                new int[] { 0, -1, 1 },
                new int[] { 0, 1, -1 },
                new int[] { 0, -1, -1 }
            };

        /// <summary>
        /// Public constructor.
        /// </summary>
        public ClassicPerlin(Game inGame, int inSeed)
        {
            inGame.Services.AddService(typeof(INoiseGenerator), this);
            this.CreatePermutationTable(inSeed);
        }

        /// <summary>
        /// Creates a table of permutation values used in the Perlin Noise algorithm.
        /// </summary>
        /// <param name="inSeed">Integer seed to use when generating numbers.</param>
        protected virtual void CreatePermutationTable(int inSeed)
        {
            // We store the seed as a value for potential Serialization.
            this.mSeed = inSeed;

            // The Random is a pseudo-random number generator.
            this.mRand = new Random(mSeed);

            // Re-use array-iteration variables over multiple for-loops if possible
            // to improve efficiency and avoid wasting memory.
            int iCount = 0;

            int[] pTable = new int[256];
            for (iCount = 0; iCount < pTable.Length; iCount++) { pTable[iCount] = -1; }

            // Loop through the values from 0...256, and then generate a new value
            // using the pseudo-random number generator and use its modulus as an
            // index into the permutation table array. If the array value is equal
            // to -1, store the iterator iCount there. It basically counts to 256
            // and stores each value in a pseudo-random array slot.
            for (iCount = 0; iCount < pTable.Length; iCount++)
            {
                while (true)
                {
                    int iP = mRand.Next() % pTable.Length;
                    if (pTable[iP] == -1)
                    {
                        pTable[iP] = iCount;
                        break;
                    }
                }
            }

            // To remove the need for index-wrapping, we double the length of the
            // permutation table and use bitwise AND to copy its values across.
            mPerm = new int[512];
            for (iCount = 0; iCount < 512; iCount++) { mPerm[iCount] = pTable[iCount & 255]; }
        }

        // Helper methods.

        /// <summary>
        /// Fast floor method to remove the fractional part of a floating-point number.
        /// </summary>
        protected static int FastFloor(double inValue) { return inValue > 0 ? (int)inValue : ((int)inValue) - 1; }

        /// <summary>
        /// Fade method allows for smoother transitions between grid-cells.
        /// </summary>
        protected static double Fade(double inValue) { return inValue * inValue * inValue * (inValue * (inValue * 6 - 15) + 10); }

        /// <summary>
        /// Dot-product.
        /// </summary>
        protected double Dot(int[] inGrid, double inX, double inY, double inZ) { return inGrid[0] * inX + inGrid[1] * inY + inGrid[2] * inZ; }

        /// <summary>
        /// Linear interpolation helper method. Fast when compared to other methods of interpolation, such as cosine interpolation.
        /// </summary>
        protected static double Lerp(double inA, double inB, double inT) { return (1 - inT) * inA + inT * inB; }

        public virtual double Generate(double inX, double inY, double inZ)
        {
            // Find the unit grid-cell locating our point.
            int X = FastFloor(inX),
                Y = FastFloor(inY),
                Z = FastFloor(inZ);

            // Get the relative X-Y-Z coordinates of the point within that grid-cell.
            double x = inX - X,
                   y = inY - Y,
                   z = inZ - Z;

            // Wrap the integer cells at 255. A smaller integer period can be introduced here.
            X = X & 255;
            Y = Y & 255;
            Z = Z & 255;

            // Calculate a set of eight hashed gradient indices.
            int grd000 = mPerm[X + mPerm[Y + mPerm[Z]]] % 12;
            int grd001 = mPerm[X + mPerm[Y + mPerm[Z + 1]]] % 12;
            int grd010 = mPerm[X + mPerm[Y + 1 + mPerm[Z]]] % 12;
            int grd011 = mPerm[X + mPerm[Y + 1 + mPerm[Z + 1]]] % 12;
            int grd100 = mPerm[X + 1 + mPerm[Y + mPerm[Z]]] % 12;
            int grd101 = mPerm[X + 1 + mPerm[Y + mPerm[Z + 1]]] % 12;
            int grd110 = mPerm[X + 1 + mPerm[Y + 1 + mPerm[Z]]] % 12;
            int grd111 = mPerm[X + 1 + mPerm[Y + 1 + mPerm[Z + 1]]] % 12;

            // Calculate noise contributions from each of the eight corners.
            double nc000 = Dot(mGrad[grd000], x, y, z);
            double nc100 = Dot(mGrad[grd100], x - 1, y, z);
            double nc010 = Dot(mGrad[grd010], x, y - 1, z);
            double nc110 = Dot(mGrad[grd110], x - 1, y - 1, z);
            double nc001 = Dot(mGrad[grd001], x, y, z - 1);
            double nc101 = Dot(mGrad[grd101], x - 1, y, z - 1);
            double nc011 = Dot(mGrad[grd011], x, y - 1, z - 1);
            double nc111 = Dot(mGrad[grd111], x - 1, y - 1, z - 1);

            // Compute the fade curve for each of the X, Y and Z values.
            double u = Fade(x),
                   v = Fade(y),
                   w = Fade(z);

            // Interpolate the eight corner values along the X-axis.
            double nx00 = Lerp(nc000, nc100, u),
                   nx01 = Lerp(nc001, nc101, u),
                   nx10 = Lerp(nc010, nc110, u),
                   nx11 = Lerp(nc011, nc111, u);

            // Interpolate the four results along the Y-axis.
            double nxy0 = Lerp(nx00, nx10, v),
                   nxy1 = Lerp(nx01, nx11, v);

            // Interpolate the last two results along the Z-axis.
            double nxyz = Lerp(nxy0, nxy1, w);
            return nxyz;
        }

        public double FBm(double inX, double inY, double inZ,
                          int inOctaves, float inLacunarity = 2.0f, float inGain = 0.5f)
        {
            double frequency = 1.0f;
            double amplitude = 0.5f;
            double total = 0.0f;

            for (int i = 0; i < inOctaves; i++)
            {
                total += Generate(inX * frequency, inY * frequency, inZ * frequency) * amplitude;
                frequency *= inLacunarity;
                amplitude *= inGain;
            }

            return total;
        }

        public double RidgedMultiFractal(double inX, double inY, double inZ,
                                         int inOctaves, double inOffset,
                                         float inLacunarity = 2.0f, float inGain = 0.5f)
        {
            double frequency = 1.0f;
            double amplitude = 0.5f;
            double total = 0.0f;
            double prev = 1.0f;

            for (int i = 0; i < inOctaves; i++)
            {
                double tempValue = Ridge(Generate(inX * frequency, inY * frequency, inZ * frequency), inOffset);
                total += tempValue * amplitude * prev;
                prev = tempValue;
                frequency *= inLacunarity;
                amplitude *= inGain;
            }

            return total;
        }

        /// <summary>
        /// Helper method for providing Ridged Multi-Fractal noise.
        /// </summary>
        protected static double Ridge(double inH, double inOffset)
        {
            inH = Math.Abs(inH);
            inH = inOffset - inH;
            inH = inH * inH;
            return inH;
        }

        public virtual void GenerateTexture(GraphicsDevice inGraphicsDevice,
                                    int inWidth, int inHeight,
                                    out Texture2D outTexture)
        {
            double xDivisor = 1f / inWidth;
            double yDivisor = 1f / inHeight;
            double finalNoiseValue = 0;
            Color[] tempColorArray = new Color[inWidth * inHeight];

            for (int x = 0; x < inWidth; x++)
            {
                for (int y = 0; y < inHeight; y++)
                {
                    double ridgeNoise = RidgedMultiFractal(x * xDivisor, y * yDivisor, 0.5f, 0, 128, 2.0f, 0.5f);
                    double fractal = FBm(x * xDivisor, y * yDivisor, 0f, 18);

                    finalNoiseValue = fractal;

                    //                    finalNoiseValue = finalNoiseValue * 0.5f + 0.5f;
                    tempColorArray[x + (y * inWidth)] = new Color((float)finalNoiseValue,
                                                                  (float)finalNoiseValue,
                                                                  (float)finalNoiseValue);
                }
            }

            outTexture = new Texture2D(inGraphicsDevice, inWidth, inHeight, false, SurfaceFormat.Color);
            outTexture.SetData<Color>(tempColorArray);

            return;
        }

        /// <summary>
        /// Name of the algorithm used by this INoiseGenerator.
        /// </summary>
        public string Name { get { return "ClassicPerlin"; } }

        /// <summary>
        /// Seed value.
        /// </summary>
        public int Seed { get { return mSeed; } }
    }
}
