using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Library
{
    /// <summary>
    /// Material used for normal-mapping.
    /// </summary>
    public class NormalMapMaterial : ForwardLightMaterial
    {
        Texture2D tNormalMap;

        /// <summary>
        /// Secondary constructor.
        /// </summary>
        /// <param name="inNormalMap"></param>
        public NormalMapMaterial(Texture2D inNormalMap)
            : base()
        {
            this.tNormalMap = inNormalMap;
        }

        /// <summary>
        /// Primary constructor.
        /// </summary>
        /// <param name="inNormalMap">Normal map texture.</param>
        /// <param name="inAmbientColor">Ambient light color.</param>
        /// <param name="inSpecularColor">Specular highlight color.</param>
        /// <param name="inLightColor">Light color.</param>
        /// <param name="inLightDirection">Light direction.</param>
        public NormalMapMaterial(Texture2D inNormalMap, Vector3 inAmbientColor, Vector3 inSpecularColor,
                                 Vector3 inLightColor, Vector3 inLightDirection)
            : base(inAmbientColor, inSpecularColor, inLightColor, inLightDirection)
        {
            this.tNormalMap = inNormalMap;
        }

        /// <summary>
        /// Applies the material values to an Effect.
        /// </summary>
        /// <param name="inEffect">Effect to apply parameters to.</param>
        public override void SetEffectParameters(Effect inEffect)
        {
            if (inEffect.Parameters["t_NormalMap"] != null) {
                inEffect.Parameters["t_NormalMap"].SetValue(tNormalMap);
            }

            base.SetEffectParameters(inEffect);
        }

        /// <summary>
        /// Normal map.
        /// </summary>
        public Texture2D NormalMap
        {
            get { return tNormalMap; }
            set { tNormalMap = value; }
        }
    }
}
