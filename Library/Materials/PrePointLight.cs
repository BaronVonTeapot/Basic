using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Library
{
    public class PrePointLight : Material
    {
        Vector3 mPosition;
        Color mColor;
        float mAttenuation;

        /// <summary>
        /// Public constructor.
        /// </summary>
        public PrePointLight(Vector3 inPosition, Color inColor, float inAttenuation)
        {
            this.mPosition = inPosition;
            this.mColor = inColor;
            this.mAttenuation = inAttenuation;
        }

        /// <summary>
        /// Applies the material values to an Effect.
        /// </summary>
        /// <param name="inEffect">Effect to apply parameters to.</param>
        public override void SetEffectParameters(Effect inEffect)
        {
            if (inEffect.Parameters["LightPosition"] != null) {
                inEffect.Parameters["LightPosition"].SetValue(mPosition);
            }

            if (inEffect.Parameters["LightColor"] != null) {
                inEffect.Parameters["LightColor"].SetValue(mColor.ToVector3());
            }

            if (inEffect.Parameters["Attenuation"] != null) {
                inEffect.Parameters["Attenuation"].SetValue(mAttenuation);
            }

            base.SetEffectParameters(inEffect);
        }

        /// <summary>
        /// Position.
        /// </summary>
        public Vector3 Position
        {
            get { return mPosition; }
            set { mPosition = value; }
        }

        /// <summary>
        /// Color.
        /// </summary>
        public Color Color
        {
            get { return mColor; }
            set { mColor = value; }
        }

        /// <summary>
        /// Color as Vector3.
        /// </summary>
        public Vector3 ColorVector { get { return mColor.ToVector3(); } }

        /// <summary>
        /// Attenuation value.
        /// </summary>
        public float Attenuation
        {
            get { return mAttenuation; }
            set { mAttenuation = value; }
        }
    }
}