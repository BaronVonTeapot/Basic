using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Library.Materials
{
    /// <summary>
    /// Projected texturing material.
    /// </summary>
    public class ProjectedTexture : Material
    {
        Vector3 mProjectorPosition, mProjectorTarget, mProjectorUp = Vector3.Up;
        Texture2D tProjected;
        float mScale, mHalfWidth, mHalfHeight;
        bool bProjectorEnabled;
        
        /// <summary>
        /// Secondary public constructor.
        /// </summary>
        /// <param name="inTexture">Texture to project.</param>
        public ProjectedTexture(Texture2D inTexture)
            : this(inTexture, new Vector3(0f, 0f, 0f), Vector3.Forward) { }

        /// <summary>
        /// Primary public constructor.
        /// </summary>
        /// <param name="inTexture">Texture to project.</param>
        /// <param name="inProjectorPosition">Projector position.</param>
        /// <param name="inProjectorTarget">Projector target.</param>
        public ProjectedTexture(Texture2D inTexture, Vector3 inProjectorPosition, Vector3 inProjectorTarget)
        {
            this.mProjectorPosition = inProjectorPosition;
            this.mProjectorTarget = inProjectorTarget;
            this.tProjected = inTexture;
            this.bProjectorEnabled = true;
        }

        /// <summary>
        /// Apply the parameters to an effect.
        /// </summary>
        /// <param name="inEffect">Effect to apply to.</param>
        public override void SetEffectParameters(Effect inEffect)
        {
            if (inEffect.Parameters["ProjectorEnabled"] != null)
            {
                inEffect.Parameters["ProjectorEnabled"].SetValue(bProjectorEnabled);
            }

            if (!bProjectorEnabled) { return; }
            Matrix matProjection = Matrix.CreateOrthographicOffCenter(-mHalfWidth  * mScale, mHalfWidth  * mScale,
                                                                      -mHalfHeight * mScale, mHalfHeight * mScale,
                                                                      -10000f, 10000f);
            Matrix matView = Matrix.CreateLookAt(mProjectorPosition, 
                                                 mProjectorTarget, mProjectorUp);

            if (inEffect.Parameters["ProjectorViewProjection"] != null) {
                inEffect.Parameters["ProjectorViewProjection"].SetValue(Matrix.Multiply(matView, matProjection));
            }
            if (inEffect.Parameters["ProjectedTexture"] != null) {
                inEffect.Parameters["ProjectedTexture"].SetValue(tProjected);
            }

            base.SetEffectParameters(inEffect);
        }

        /// <summary>
        /// Projector position.
        /// </summary>
        public Vector3 ProjectorPosition
        {
            get { return mProjectorPosition; }
            set { mProjectorPosition = value; }
        }

        /// <summary>
        /// Projector target.
        /// </summary>
        public Vector3 ProjectorTarget
        {
            get { return mProjectorTarget; }
            set { mProjectorTarget = value; }
        }

        /// <summary>
        /// Scale.
        /// </summary>
        public float Scale
        {
            get { return mScale; }
            set { mScale = value; }
        }

        /// <summary>
        /// Enable or disable the projector.
        /// </summary>
        public bool ProjectorEnabled
        {
            get { return bProjectorEnabled; }
            set { bProjectorEnabled = value; }
        }

        /// <summary>
        /// Up vector.
        /// </summary>
        public Vector3 ProjectorUp
        {
            get { return mProjectorUp; }
            set { mProjectorUp = value; }
        }
    }
}
