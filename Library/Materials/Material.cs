using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Library
{
    /// <summary>
    /// Material class allows us to set values on shaders.
    /// </summary>
    public class Material
    {
        protected float mHalfWidth, mHalfHeight;

        public virtual void SetEffectParameters(Effect inEffect)
        {
            if (inEffect.Parameters["g_HalfWidth"] != null) {
                inEffect.Parameters["g_HalfWidth"].SetValue(mHalfWidth);
            }

            if (inEffect.Parameters["g_HalfHeight"] != null) {
                inEffect.Parameters["g_HalfHeight"].SetValue(mHalfHeight);
            }
        }
    }
}
