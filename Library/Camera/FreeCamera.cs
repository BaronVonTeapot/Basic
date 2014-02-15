using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Library
{
    /// <summary>
    /// A free, first-person camera using a perspective projection matrix.
    /// </summary>
    public class FreeCamera : Camera
    {
        // Stores the rotations.
        Matrix mRotation;

        // Position in world-space.
        Vector3 mEye;

        // Camera axes.
        Vector3 mXAxis, mYAxis, mZAxis;
        Vector3 mXGlobal, mYGlobal, mZGlobal;
        Vector3 mTranslation;

        // Rotation values.
        float mPitch, mYaw;

        /// <summary>
        /// Primary public constructor.
        /// </summary>
        /// <param name="inName">Name of the camera. Used for identification purposes.</param>
        /// <param name="inNearClip">Distance to the near clipping-plane.</param>
        /// <param name="inFarClip">Distance to the far clipping-plane.</param>
        /// <param name="inFieldOfView">Field of view.</param>
        /// <param name="inAspectRatio">Aspect-ratio.</param>
        /// <param name="inPosition">Camera position.</param>
        /// <param name="inPitch">Pitch value in radians.</param>
        /// <param name="inYaw">Yaw value in radians.</param>
        public FreeCamera(String inName,
                          float inNearClip, float inFarClip, float inFieldOfView, float inAspectRatio,
                          Vector3 inPosition, float inYaw, float inPitch)
            : base(inName, inNearClip, inFarClip, inFieldOfView, inAspectRatio)
        {
            this.mEye = inPosition;
            this.mYaw = inYaw;
            this.mPitch = inPitch;

            // Store our global direction vectors.
            this.mXGlobal = Vector3.Right;
            this.mYGlobal = Vector3.Up;
            this.mZGlobal = Vector3.Forward;

            // Set the X, Y and Z axes to zero, to be set in Update().
            this.mXAxis = Vector3.Zero;
            this.mYAxis = Vector3.Zero;
            this.mZAxis = Vector3.Zero;
        }

        /// <summary>
        /// Secondary public constructor with fewer parameters.
        /// </summary>
        public FreeCamera(String inName,
                          float inFieldOfView, float inAspectRatio,
                          Vector3 inPosition, float inYaw, float inPitch)
            : this(inName, 0.1f, 100000.0f, inFieldOfView, inAspectRatio, inPosition, inPitch, inYaw) { }

        /// <summary>
        /// Rotation method.
        /// </summary>
        /// <param name="inPitchDelta">Difference in rotation about the X-Axis.</param>
        /// <param name="inYawDelta">Difference in rotation about the Y-Axis.</param>
        public virtual void Rotate(float inYawDelta, float inPitchDelta)
        {
            this.mYaw += inYawDelta;
            this.mPitch += inPitchDelta;
        }

        /// <summary>
        /// Movement method translates the camera along its X, Y and Z axes.
        /// </summary>
        /// <param name="inXDelta">X-Axis delta value.</param>
        /// <param name="inYDelta">Y-Axis delta value.</param>
        /// <param name="inZDelta">Z-Axis delta value.</param>
        public virtual void Move(float inXDelta, float inYDelta, float inZDelta)
        {
            if (inXDelta != 0.0f) { mTranslation += Vector3.Right * inXDelta; }
            if (inYDelta != 0.0f) { mTranslation += Vector3.Up * inYDelta; }
            if (inZDelta != 0.0f) { mTranslation += Vector3.Forward * inZDelta; }
        }

        public virtual void Move(Vector3 inTranslation)
        {
            this.mTranslation += inTranslation;
        }

        public override void Update()
        {
            // Calculate our rotation matrix.
            mRotation = Matrix.CreateFromYawPitchRoll(mYaw, mPitch, 0f);

            mTranslation = Vector3.Transform(mTranslation, mRotation);
            Position += mTranslation;
            mTranslation = Vector3.Zero;


            // Z-axis.
            mZAxis = Vector3.Transform(mZGlobal, mRotation);
            Vector3.Normalize(ref mZAxis, out mZAxis);

            // Y-axis.
            mYAxis = Vector3.Transform(mYGlobal, mRotation);
            Vector3.Normalize(ref mYAxis, out mYAxis);

            // X-axis.
            mXAxis = Vector3.Cross(mYAxis, mZAxis);
            Vector3.Normalize(ref mXAxis, out mXAxis);

            // Create the View matrix.
            View = Matrix.CreateLookAt(mEye, mEye + mZAxis, mYAxis);

            base.Update();
        }

        /// <summary>
        /// X-Axis (right) vector.
        /// </summary>
        public Vector3 Right { get { return mXAxis; } }

        /// <summary>
        /// Y-axis (up) vector.
        /// </summary>
        public Vector3 Up { get { return mYAxis; } }

        /// <summary>
        /// Z-axis (forward) vector.
        /// </summary>
        public Vector3 Forward { get { return mZAxis; } }

        /// <summary>
        /// Position in world-space.
        /// </summary>
        public Vector3 Position
        {
            get { return mEye; }
            set { mEye = value; }
        }
    }
}