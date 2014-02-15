using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using Library.Common;

namespace Library.Scenes
{
    /// <summary>
    /// Scene class containing geometry, sounds, textures and logic.
    /// </summary>
    public class Scene : DrawableGameComponent
    {
        List<PrePointLight> mLights = new List<PrePointLight>();
        List<ModelInstance> mModels = new List<ModelInstance>();
        PrePassRenderer mRenderer;
        FreeCamera mCamera;
        Effect mEffect;

        // These need to be handled by the CameraController class!
        KeyboardState mCurrKeyboard;
        KeyboardState mPrevKeyboard;
        MouseState mCurrMouse;
        MouseState mPrevMouse;
        float mDeltaX, mDeltaY, mX, mY;
        Vector3 mCamTranslation;
        float mElapsedSeconds;

        /// <summary>
        /// Public constructor.
        /// </summary>
        /// <param name="inGame">Reference to active Game instance.</param>
        public Scene(Game inGame)
            : base(inGame)
        {
        }

        /// <summary>
        /// Load content here.
        /// </summary>
        protected override void LoadContent()
        {
            // Load our teapot and ground models.
            Model teapotModel = Game.Content.Load<Model>("Model\\teapot");
            Model groundModel = Game.Content.Load<Model>("Model\\ground");
            Texture2D bricks = Game.Content.Load<Texture2D>("Texture\\brick");
            Texture2D checkers = Game.Content.Load<Texture2D>("Texture\\checker");
            
            mEffect = Game.Content.Load<Effect>("Effect\\Model");
            mEffect.Parameters["t_Basic"].SetValue(bricks);
            mEffect.Parameters["TextureEnabled"].SetValue(true);

            mCamera = new FreeCamera("Main", MathHelper.PiOver4, 
                                     this.GraphicsDevice.Viewport.AspectRatio, 
                                     new Vector3(1000f, 650f, 1000f), 
                                     MathHelper.ToRadians(-50f), MathHelper.ToRadians(-5f));

            ModelInstance teapot = new ModelInstance(teapotModel, Vector3.Zero, Vector3.Zero, new Vector3(60f));
            teapot.SetModelMaterial(new Material());
            teapot.SetModelEffect(mEffect, true);
            teapot.SetSingleParameter("t_Basic", bricks);
            teapot.SetSingleParameter("TextureEnabled", true);

            ModelInstance ground = new ModelInstance(groundModel, Vector3.Zero, Vector3.Zero, Vector3.One);
            ground.SetModelMaterial(new Material());
            ground.SetModelEffect(mEffect, true);

            // Create a single teapot and just one instance of the ground model.
            mModels.Add(teapot);
            mModels.Add(ground);

            foreach (ModelInstance modelInstance in mModels) { modelInstance.SetModelEffect(mEffect, true); }

            mRenderer = new PrePassRenderer(this.Game, this.GraphicsDevice, this.Game.Content);

            mLights.Add(new PrePointLight(new Vector3(-1000, 1000, 0), Color.Red, 1500));
            mLights.Add(new PrePointLight(new Vector3(1000, 1000, 0), Color.Orange, 1500));
            mLights.Add(new PrePointLight(new Vector3(0, 1000, 1000), Color.Yellow, 1500));
            mLights.Add(new PrePointLight(new Vector3(0, 1000, -1000), Color.Green, 1500));
            mLights.Add(new PrePointLight(new Vector3(1000, 1000, 1000), Color.Blue, 1500));
            mLights.Add(new PrePointLight(new Vector3(-1000, 1000, 1000), Color.Indigo, 1500));
            mLights.Add(new PrePointLight(new Vector3(1000, 1000, -1000), Color.Violet, 1500));
            mLights.Add(new PrePointLight(new Vector3(-1000, 1000, -1000), Color.White, 1500));

            // Input.
            mCamera.Update();

            mX = -MathHelper.Pi / 10f;
            mY = MathHelper.PiOver2;

            Mouse.SetPosition(this.GraphicsDevice.Viewport.Width / 2,
                              this.GraphicsDevice.Viewport.Height / 2);
            mPrevMouse = Mouse.GetState();

            base.LoadContent();
        }

        /// <summary>
        /// Update method.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            mElapsedSeconds = (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            // Get the current keyboard and mouse status.
            mCurrKeyboard = Keyboard.GetState();
            mCurrMouse = Mouse.GetState();

            if (mCurrMouse != mPrevMouse)
            {
                // Calculate and store the difference in mouse movement.
                mDeltaX = (float)mCurrMouse.X - (float)mPrevMouse.X;
                mDeltaY = (float)mCurrMouse.Y - (float)mPrevMouse.Y;

                mX -= mDeltaX;
                mY -= mDeltaY;

                Mouse.SetPosition(this.GraphicsDevice.Viewport.Width / 2,
                  this.GraphicsDevice.Viewport.Height / 2);

                // Rotate the camera.
                mCamera.Rotate(mX * 0.005f, mY * 0.005f);
            }

            // Update the camera.
            this.UpdateCameraPosition(gameTime);

            // Store the keyboard and mouse data for use next frame.
            mPrevKeyboard = mCurrKeyboard;
            mPrevMouse = mCurrMouse;

            base.Update(gameTime);
        }

        /// <summary>
        /// Draw method.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Draw(GameTime gameTime)
        {
//            mRenderer.Draw(this.GraphicsDevice, mCamera.View, mCamera.Projection, mCamera.Position, mModels, mLights);

            this.GraphicsDevice.Clear(Color.Black);

            // Draw all models
            foreach (ModelInstance model in mModels) {
                if (mCamera.BoundingVolumeIsInView(model.BoundingSphere)) {
                    model.DrawBasic(this.GraphicsDevice, mCamera.View, mCamera.Projection, mCamera.Position);
                }
            }

            base.Draw(gameTime);
        }

        /// <summary>
        /// Instead of having these unfocused methods dangling over the scene, I'm
        /// going to create a CameraController class to contain them, and instead
        /// create pairs of Camera & CameraController objects to update.
        /// </summary>
        /// <param name="gameTime"></param>
        protected virtual void UpdateCameraPosition(GameTime gameTime)
        {
            // Reset the translation value.
            mCamTranslation = Vector3.Zero;

            // Determine which direction, if any, to move the camera.
            if (mCurrKeyboard.IsKeyDown(Keys.W)) { mCamTranslation += Vector3.Forward; }
            if (mCurrKeyboard.IsKeyDown(Keys.S)) { mCamTranslation += Vector3.Backward; }
            if (mCurrKeyboard.IsKeyDown(Keys.A)) { mCamTranslation += Vector3.Left; }
            if (mCurrKeyboard.IsKeyDown(Keys.D)) { mCamTranslation += Vector3.Right; }

            // Move the camera.
            mCamTranslation *= 0.5f * mElapsedSeconds;
            mCamera.Move(mCamTranslation);

            // Update the camera.
            mCamera.Update();
        }
    }
}
