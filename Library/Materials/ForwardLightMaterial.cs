using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Library
{
    public class ForwardLightMaterial : Material
    {
        Vector3 mAmbientColor;
        Vector3 mSpecularColor;
        Vector3 mLightColor;
        Vector3 mLightDirection;

        /// <summary>
        /// Empty constructor.
        /// </summary>
        public ForwardLightMaterial()
            : this(new Vector3(0.1f, 0.1f, 0.1f), new Vector3(1f, 1f, 1f),
                   new Vector3(0.9f, 0.9f, 0.9f), new Vector3(1f, 1f, 1f))
        {
            this.mAmbientColor = new Vector3(0.1f, 0.1f, 0.1f);
            this.mSpecularColor = new Vector3(1.0f, 1.0f, 1.0f);
            this.mLightColor = new Vector3(0.9f, 0.9f, 0.9f);
            this.mLightDirection = new Vector3(1.0f, 1.0f, 1.0f);
        }

        /// <summary>
        /// Public constructor.
        /// </summary>
        /// <param name="inAmbientColor">Ambient light color.</param>
        /// <param name="inSpecularColor">Specular highlight color.</param>
        /// <param name="inLightColor">Light color.</param>
        /// <param name="inLightDirection">Light direction.</param>
        public ForwardLightMaterial(Vector3 inAmbientColor, Vector3 inSpecularColor,
                                    Vector3 inLightColor, Vector3 inLightDirection)
        {
            this.mAmbientColor = inAmbientColor;
            this.mSpecularColor = inSpecularColor;
            this.mLightColor = inLightColor;
            this.mLightDirection = inLightDirection;
        }

        /// <summary>
        /// Applies the material values to an Effect.
        /// </summary>
        /// <param name="inEffect">Effect to apply parameters to.</param>
        public override void SetEffectParameters(Effect inEffect)
        {
            if (inEffect.Parameters["AmbientColor"] != null) {
                inEffect.Parameters["AmbientColor"].SetValue(mAmbientColor);
            }

            if (inEffect.Parameters["SpecularColor"] != null) {
                inEffect.Parameters["SpecularColor"].SetValue(mSpecularColor);
            }

            if (inEffect.Parameters["LightColor"] != null) {
                inEffect.Parameters["LightColor"].SetValue(mLightColor);
            }

            if (inEffect.Parameters["LightDirection"] != null) {
                inEffect.Parameters["LightDirection"].SetValue(mLightDirection);
            }

            base.SetEffectParameters(inEffect);
        }

        /// <summary>
        /// Ambient light color.
        /// </summary>
        public Vector3 AmbientColor
        {
            get { return mAmbientColor; }
            set { mAmbientColor = value; }
        }

        /// <summary>
        /// Specular highlight color.
        /// </summary>
        public Vector3 SpecularColor
        {
            get { return mSpecularColor; }
            set { mSpecularColor = value; }
        }

        /// <summary>
        /// Light color.
        /// </summary>
        public Vector3 LightColor
        {
            get { return mLightColor; }
            set { mLightColor = value; }
        }

        /// <summary>
        /// Light direction.
        /// </summary>
        public Vector3 LightDirection
        {
            get { return mLightDirection; }
            set { mLightDirection = value; }
        }
    }
}
