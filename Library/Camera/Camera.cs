using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Library
{
    public abstract class Camera
    {
        protected Matrix mView;
        protected Matrix mProjection;
        protected Matrix mViewProjection;
        BoundingFrustum mFrustum;

        float mNearClip;
        float mFarClip;
        float mFieldOfView;
        float mAspectRatio;

        string mName;
        bool mEnabled = false;

        /// <summary>
        /// Primary public constructor.
        /// </summary>
        /// <param name="inName">Name of the camera. Used for identification purposes.</param>
        /// <param name="inNearClip">Distance to the near clipping-plane.</param>
        /// <param name="inFarClip">Distance to the far clipping-plane.</param>
        /// <param name="inFieldOfView">Field of view.</param>
        /// <param name="inAspectRatio">Aspect-ratio.</param>
        public Camera(String inName,
                      float inNearClip, float inFarClip,
                      float inFieldOfView, float inAspectRatio)
        {
            this.mName = inName;
            this.mNearClip = inNearClip;
            this.mFarClip = inFarClip;

            this.CreatePerspectiveAlt(inFieldOfView, inAspectRatio);
            this.CreateFrustum();
        }

        /// <summary>
        /// Secondary public constructor with fewer parameters.
        /// </summary>
        public Camera(String inName, float inFieldOfView, float inAspectRatio)
            : this(inName, 0.1f, 100000.0f, inFieldOfView, inAspectRatio) { }

        /// <summary>
        /// Virtual update method. Static cameras will not use this, but all
        /// moving dynamic cameras will require update methods to update their
        /// View matrices.
        /// </summary>
        public virtual void Update() { }

        /// <summary>
        /// Alternative method to create the perspective-projection matrix.
        /// </summary>
        /// <param name="inFieldOfView">Field of view in radians.</param>
        /// <param name="inAspectRatio">Horizontal aspect ratio.</param>
        protected virtual void CreatePerspectiveAlt(float inFieldOfView, float inAspectRatio)
        {
            this.mFieldOfView = inFieldOfView;
            this.mAspectRatio = inAspectRatio;

            this.Projection = Matrix.CreatePerspectiveFieldOfView(inFieldOfView, inAspectRatio,
                                                                  mNearClip, mFarClip);
        }

        /// <summary>
        /// Creates a Perspective Projection matrix.
        /// </summary>
        /// <param name="inFieldOfView">Field of view for the perspective projection.</param>
        /// <param name="inAspectRatio">Aspect-ratio decided by width and height of the viewport.</param>
        protected virtual void CreatePerspective(float inFieldOfView, float inAspectRatio)
        {
            this.mFieldOfView = inFieldOfView;
            this.mAspectRatio = inAspectRatio;

            float aspectInv = 1.0f / mAspectRatio;
            float e = 1.0f / (float)Math.Tan(mFieldOfView / 2f);
            float fovy = 2.0f * (float)Math.Atan(aspectInv / e);
            float xScale = 1.0f / (float)Math.Tan(0.5f * fovy);
            float yScale = xScale / aspectInv;

            mProjection.M11 = xScale;
            mProjection.M12 = 0f;
            mProjection.M13 = 0f;
            mProjection.M14 = 0f;

            mProjection.M21 = 0f;
            mProjection.M22 = yScale;
            mProjection.M23 = 0f;
            mProjection.M24 = 0f;

            mProjection.M31 = 0f;
            mProjection.M32 = 0f;
            mProjection.M33 = (mFarClip + mNearClip) / (mNearClip - mFarClip);
            mProjection.M34 = -1.0f;

            mProjection.M41 = 0f;
            mProjection.M42 = 0f;
            mProjection.M43 = (2f * mFarClip * mNearClip) / (mNearClip - mFarClip);
            mProjection.M44 = 0f;
        }

        /// <summary>
        /// Creates the BoundingFrustum.
        /// </summary>
        protected virtual void CreateFrustum()
        {
            Matrix.Multiply(ref mView, ref mProjection, out mViewProjection);

            if (mFrustum == null)
            { mFrustum = new BoundingFrustum(mViewProjection); }
            else { mFrustum.Matrix = mViewProjection; }
        }

        /// <summary>
        /// Checks a bounding volume against the View frustum.
        /// </summary>
        /// <param name="sphere">BoundingSphere to test.</param>
        public bool BoundingVolumeIsInView(BoundingSphere sphere)
        {
            return (Frustum.Contains(sphere) != ContainmentType.Disjoint);
        }

        /// <summary>
        /// Checks a bounding volume against the View frustum.
        /// </summary>
        /// <param name="box">BoundingBox to test.</param>
        public bool BoundingVolumeIsInView(BoundingBox box)
        {
            return (Frustum.Contains(box) != ContainmentType.Disjoint);
        }

        /// <summary>
        /// Gets the Projection matrix.
        /// </summary>
        public void GetProjectionMatrix(out Matrix outProjection) { outProjection = mProjection; }
        /// <summary>
        /// Sets the Projection matrix.
        /// </summary>
        public void SetProjectionMatrix(ref Matrix inProjection) { mProjection = inProjection; }
        /// <summary>
        /// Gets the View matrix.
        /// </summary>
        public void GetViewMatrix(out Matrix outView) { outView = mView; }
        /// <summary>
        /// Sets the View matrix.
        /// </summary>
        public void SetViewMatrix(ref Matrix inView) { mView = inView; }
        /// <summary>
        /// Gets the View-Projection matrix.
        /// </summary>
        public void GetViewProjectionMatrix(out Matrix outViewProjection) { outViewProjection = mViewProjection; }
        /// <summary>
        /// Gets the BoundingFrustum.
        /// </summary>
        public void GetFrustum(out BoundingFrustum outFrustum) { outFrustum = mFrustum; }

        /// <summary>
        /// String identifier of the camera.
        /// </summary>
        public String Name
        {
            get { return mName; }
        }

        /// <summary>
        /// View matrix.
        /// </summary>
        public Matrix View
        {
            get { return mView; }
            protected set
            {
                mView = value;
                this.CreateFrustum();
            }
        }

        /// <summary>
        /// Projection matrix.
        /// </summary>
        public Matrix Projection
        {
            get { return mProjection; }
            protected set
            {
                mProjection = value;
                this.CreateFrustum();
            }
        }

        /// <summary>
        /// View and Projection matrices multiplied together.
        /// </summary>
        public Matrix ViewProjection
        {
            get
            {
                mViewProjection = Matrix.Multiply(View, Projection);
                return mViewProjection;
            }
        }

        /// <summary>
        /// Inverse of View and Projection matrices multiplied together.
        /// </summary>
        public Matrix InverseViewProjection
        {
            get
            {
                Matrix invViewProj = Matrix.Invert(Matrix.Multiply(View, Projection));
                return invViewProj;
            }
        }

        /// <summary>
        /// BoundingFrustum used in frustum-culling.
        /// </summary>
        public BoundingFrustum Frustum
        {
            get { return mFrustum; }
            protected set { mFrustum = value; }
        }

        /// <summary>
        /// Near clipping-plane.
        /// </summary>
        public float NearClip
        {
            get { return mNearClip; }
            set
            {
                mNearClip = value;
                this.CreatePerspectiveAlt(FieldOfView, AspectRatio);
            }
        }

        /// <summary>
        /// Far clipping-plane.
        /// </summary>
        public float FarClip
        {
            get { return mFarClip; }
            set
            {
                mFarClip = value;
                this.CreatePerspectiveAlt(FieldOfView, AspectRatio);
            }
        }

        /// <summary>
        /// Field of view.
        /// </summary>
        public float FieldOfView
        {
            get { return mFieldOfView; }
            set
            {
                this.CreatePerspective(value, mAspectRatio);
                this.CreateFrustum();
            }
        }

        /// <summary>
        /// Aspect ratio.
        /// </summary>
        public float AspectRatio
        {
            get { return mAspectRatio; }
            set
            {
                this.CreatePerspective(mFieldOfView, value);
                this.CreateFrustum();
            }
        }

        /// <summary>
        /// Camera status.
        /// </summary>
        public bool Enabled
        {
            get { return mEnabled; }
            set { mEnabled = value; }
        }
    }
}
