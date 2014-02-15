using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Library.Common
{
    /// <summary>
    /// Basic prototype shadow-casting class.
    /// </summary>
    public class ShadowCaster
    {
        Vector3 mPosition, mTarget, mUp;
        Matrix mShadowView;

        /// <summary>
        /// Public constructor.
        /// </summary>
        /// <param name="inPosition">Caster position.</param>
        /// <param name="inTarget">Caster target.</param>
        /// <param name="inUp">Up vector.</param>
        public ShadowCaster(Vector3 inPosition, Vector3 inTarget, Vector3 inUp)
        {
            this.mPosition = inPosition;
            this.mTarget = inTarget;
            this.mUp = inUp;

            mShadowView = Matrix.CreateLookAt(mPosition, mTarget, mUp);
        }

        /// <summary>
        /// Position vector.
        /// </summary>
        public Vector3 Position
        {
            get { return mPosition; }
            set { mPosition = value; }
        }

        /// <summary>
        /// Target vector.
        /// </summary>
        public Vector3 Target
        {
            get { return mTarget; }
            set { mTarget = value; }
        }

        /// <summary>
        /// Up-vector.
        /// </summary>
        public Vector3 Up
        {
            get { return mUp; }
            set { mUp = value; }
        }

        /// <summary>
        /// View matrix used for lighting calculations.
        /// </summary>
        public Matrix ShadowViewMatrix
        {
            get { return mShadowView; }
        }
    }
}
