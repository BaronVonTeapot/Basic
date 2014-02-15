using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Library
{
    /// <summary>
    /// Light Pre-Pass Rendering allows large numbers of lights to be drawn quickly and
    /// easily in one pass. It is advantageous over forward-lighting, which typically
    /// requires each light to be drawn sequentially.
    /// 
    /// Similar, but not identical, to Deferred Rendering because there is no G-Buffer
    /// texture: it uses the back-buffer instead, which is easier on machines with a
    /// small amount of available memory.
    /// </summary>
    public class PrePassRenderer
    {
        // Reference to the Game instance.
        Game pGame;

        // Screen dimensions used to construct our render textures, and shadow
        // rendering values: distance to far-plane and square shadow-map size.
        int mViewWidth, mViewHeight;

        // Render textures used to store our lighting data.
        RenderTarget2D tDepth, tNormal, tLight;

        // Effects.
        Effect mDepthNormalEffect, mLightingEffect;

        // Meshes representing different types of lights.
        Model mPointLight;

        // Render states saved here and restored after we finish.
        BlendState mSavedBlendState;
        DepthStencilState mSavedDepthStencilState;

        // Matrices used when creating the light map.
        Matrix mWorldViewProjection, mViewProjection, mInverseViewProjection;

        /// <summary>
        /// Public constructor.
        /// </summary>
        /// <param name="inGraphicsDevice">GraphicsDevice to use for rendering.</param>
        /// <param name="inContentManager">ContentManager to load models, textures and effects.</param>
        public PrePassRenderer(Game inGame, GraphicsDevice inGraphicsDevice, ContentManager inContentManager)
        {
            pGame = inGame;

            // Screen dimensions are the same for each render texture.
            mViewWidth = inGraphicsDevice.Viewport.Width;
            mViewHeight = inGraphicsDevice.Viewport.Height;

            // Create the render-textures.
            tDepth = new RenderTarget2D(inGraphicsDevice, mViewWidth, mViewHeight,
                                         false, SurfaceFormat.Single, DepthFormat.Depth24);

            tNormal = new RenderTarget2D(inGraphicsDevice, mViewWidth, mViewHeight,
                                         false, SurfaceFormat.Color, DepthFormat.Depth24);

            tLight = new RenderTarget2D(inGraphicsDevice, mViewWidth, mViewHeight,
                                         false, SurfaceFormat.Color, DepthFormat.Depth24);

            mDepthNormalEffect = inContentManager.Load<Effect>("Effect\\LPPDepthNormal");
            mDepthNormalEffect.CurrentTechnique = mDepthNormalEffect.Techniques["DepthNormal"];

            mLightingEffect = inContentManager.Load<Effect>("Effect\\LPPPointLight");
            mPointLight = inContentManager.Load<Model>("Model\\pointLightMesh");
            mPointLight.Meshes[0].MeshParts[0].Effect = mLightingEffect;

            // Make sure we use the right effect technique.
            mLightingEffect.CurrentTechnique = mLightingEffect.Techniques["PointLight"];
            mLightingEffect.Parameters["ViewportWidth"].SetValue(mViewWidth);
            mLightingEffect.Parameters["ViewportHeight"].SetValue(mViewHeight);
        }

        /// <summary>
        /// Draw using Light Pre-Pass Rendering techniques.
        /// </summary>
        /// <param name="inGraphicsDevice">GraphicsDevice to use for rendering.</param>
        /// <param name="inView">View matrix.</param>
        /// <param name="inProjection">Projection matrix.</param>
        /// <param name="inCameraPosition">Camera position vector.</param>
        /// <param name="inModels">Collection of models to render.</param>
        /// <param name="inLights">Collection of lights to render.</param>
        public virtual void Draw(GraphicsDevice inGraphicsDevice, Matrix inView, Matrix inProjection, Vector3 inCameraPosition,
                                 ICollection<ModelInstance> inModels, ICollection<PrePointLight> inLights)
        {
            // First save the Z-Buffer positions and normals of each model we want to draw to the
            // Normal and Depth textures.
            this.DrawDepthNormal(inGraphicsDevice, inView, inProjection, inCameraPosition, inModels);

            // Next draw the lights by rendering a scaled spherical mesh across the scene into a light
            // texture, otherwise known as a light-map. Currently we only do point-lights.
            this.DrawPointLights(inGraphicsDevice, inView, inProjection, inCameraPosition, inLights);

            // Finally, apply settings to the models' effects for rendering using the light texture. 
            this.PrepareMainPass(inModels);
        }

        /// <summary>
        /// Creates the Depth and Normal textures.
        /// </summary>
        /// <param name="inGraphicsDevice">GraphicsDevice to use for rendering.</param>
        /// <param name="inView">View matrix.</param>
        /// <param name="inProjection">Projection matrix.</param>
        /// <param name="inCameraPosition">Camera position vector.</param>
        /// <param name="inModels">Collection of models to create depth and normal maps for.</param>
        protected virtual void DrawDepthNormal(GraphicsDevice inGraphicsDevice, Matrix inView, Matrix inProjection,
                                               Vector3 inCameraPosition, ICollection<ModelInstance> inModels)
        {
            // Prepare the render-textures for drawing and clear to white (infinite depth).
            inGraphicsDevice.SetRenderTargets(tNormal, tDepth);
            inGraphicsDevice.Clear(Color.White);

            // Draw each model instance using the DepthNormal shader technique. This is rather slow
            // because each model has to cache its effects and change them. I'd rather switch to a
            // different technique, forcing each ModelInstance to cache the name of its current
            // technique. A string is easier to store than a whole file.
            foreach (ModelInstance modelInstance in inModels)
            {
                modelInstance.CacheEffects();
                modelInstance.SetModelEffect(mDepthNormalEffect, false);
                modelInstance.DrawBasic(inGraphicsDevice, inView,
                                        inProjection, inCameraPosition);
                modelInstance.RestoreCached();
            }

            // Resolve the render-textures.
            inGraphicsDevice.SetRenderTargets(null);
        }

        /// <summary>
        /// Creates a Light texture.
        /// </summary>
        /// <param name="inGraphicsDevice">GraphicsDevice to use for rendering.</param>
        /// <param name="inView">View matrix.</param>
        /// <param name="inProjection">Projection matrix.</param>
        /// <param name="inCameraPosition">Camera position.</param>
        /// <param name="inLights">Collection of point lights.</param>
        protected virtual void DrawPointLights(GraphicsDevice inGraphicsDevice, Matrix inView, Matrix inProjection,
                                               Vector3 inCameraPosition, ICollection<PrePointLight> inLights)
        {
            // If there are no lights to draw, return.
            if (inLights.Count == 0) { return; }

            // Apply the depth and normal textures to our effect.
            mLightingEffect.Parameters["t_Depth"].SetValue(tDepth);
            mLightingEffect.Parameters["t_Normal"].SetValue(tNormal);

            // Rendering the lights requires the inverse ViewProjection matrix.
            mViewProjection = Matrix.Multiply(inView, inProjection);
            mInverseViewProjection = Matrix.Invert(mViewProjection);
            mLightingEffect.Parameters["matInverseViewProjection"].SetValue(mInverseViewProjection);

            // Prepare our light render-texture and clear it to black.
            inGraphicsDevice.SetRenderTarget(tLight);
            inGraphicsDevice.Clear(Color.Black);

            // Record render states here so they can be restored afterwards.
            mSavedBlendState = inGraphicsDevice.BlendState;
            mSavedDepthStencilState = inGraphicsDevice.DepthStencilState;
            inGraphicsDevice.BlendState = BlendState.Additive;
            inGraphicsDevice.DepthStencilState = DepthStencilState.None;

            foreach (PrePointLight pointLight in inLights)
            {
                // Apply the properties of each light to the lighting effect.
                pointLight.SetEffectParameters(mLightingEffect);

                // Calculate the WorldViewProjection matrix and apply it.
                mWorldViewProjection = (Matrix.CreateScale(pointLight.Attenuation) *
                                        Matrix.CreateTranslation(pointLight.Position)) *
                                        mViewProjection;
                mLightingEffect.Parameters["matWorldViewProjection"].SetValue(mWorldViewProjection);

                // If we're inside the mesh, we will only see the triangles drawn with the reverse winding-order,
                // meaning that normally they'd be culled. So swap cull-modes temporarily.
                float dist = Vector3.Distance(inCameraPosition, pointLight.Position);
                if (dist < pointLight.Attenuation) { inGraphicsDevice.RasterizerState = RasterizerState.CullClockwise; }

                // Draw the mesh and reset the RasterizerState to what it was previously.
                mPointLight.Meshes[0].Draw();
                inGraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            }

            inGraphicsDevice.BlendState = mSavedBlendState;
            inGraphicsDevice.DepthStencilState = mSavedDepthStencilState;
            inGraphicsDevice.SetRenderTargets(null);
        }

        /// <summary>
        /// Prepares a collection of ModelInstance objects for rendering with a light-map.
        /// </summary>
        /// <param name="inModels">Collection of models to prepare.</param>
        protected virtual void PrepareMainPass(ICollection<ModelInstance> inModels)
        {
            foreach (ModelInstance modelInstance in inModels)
            {
                foreach (ModelMesh modelMesh in modelInstance.Model.Meshes)
                {
                    foreach (ModelMeshPart meshPart in modelMesh.MeshParts)
                    {
                        if (meshPart.Effect.Parameters["LightTexture"] != null) {
                            meshPart.Effect.Parameters["LightTexture"].SetValue((Texture2D)tLight);
                        }
                        if (meshPart.Effect.Parameters["ViewportWidth"] != null) {
                            meshPart.Effect.Parameters["ViewportWidth"].SetValue(mViewWidth);
                        }
                        if (meshPart.Effect.Parameters["ViewportHeight"] != null) {
                            meshPart.Effect.Parameters["ViewportHeight"].SetValue(mViewHeight);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Depth texture.
        /// </summary>
        public Texture2D DepthTexture { get { return (Texture2D)tDepth; } }
        public RenderTarget2D Depth { get { return tDepth; } }

        /// <summary>
        /// Normal texture.
        /// </summary>
        public Texture2D NormalTexture { get { return (Texture2D)tNormal; } }
        public RenderTarget2D Normal { get { return tNormal; } }

        /// <summary>
        /// Light map.
        /// </summary>
        public Texture2D LightTexture { get { return (Texture2D)tLight; } }
        public RenderTarget2D Light { get { return tLight; } }
    }
}