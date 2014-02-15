using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Core.Input;

namespace Library
{
    /// <summary>
    /// Camera controller class.
    /// </summary>
    public class FreeCameraController : GameComponent
    {
        /// <summary>
        /// Actions that apply to the camera.
        /// </summary>
        public enum Actions
        {
            Crouch,
            Jump,
            MoveForwards,
            MoveBackwards,
            StrafeLeft,
            StrafeRight,
            Run,
            Walk
        }

        /// <summary>
        /// Posture affects the height of the camera and movement speed.
        /// </summary>
        public enum Posture
        {
            Standing,
            Crouching,
            Rising,
            Jumping
        }

        // Camera controlled by this instance.
        FreeCamera mCamera;

        // Global direction vectors: right, up and forward.
        Vector3 gVecX, gVecY, gVecZ;

        // Projection matrix values.
        float mNearClip, mFarClip, mFieldOfView, mAspect;

        // Acceleration and velocity values.
        float mAccelX, mAccelY, mAccelZ;
        float mVelocityX, mVelocityY, mVelocityZ;

        // Movement speed multipliers for walking and running.
        float mRunMultiplier, mWalkMultiplier;

        // Height multiplier for when crouching.
        float mCrouchHeightMultiplier;

        Rectangle mClientBounds;
        int mWindowWidth, mWindowHeight;

        // Boolean movement flags.
        bool bForwardsPressed, bBackwardsPressed, bStrafeLeftPressed, bStrafeRightPressed;

        // Current posture.
        Posture ePosture;

        // Movement vectors.
        Vector3 mAcceleration, mCurrentVelocity, mVelocity, mWalkVelocity, mRunVelocity;

        // Keyboard input service used to check if keys are pressed.
        InputSystem mInput;
        KeyboardService mKeyboardService;
        PlayerIndex mPlayerInControl;
        Dictionary<Actions, Keys> mActionKeys = new Dictionary<Actions, Keys>();

        // Mouse state objects in lieu of a MouseInputService.
        MouseState mPrevMouse, mCurrMouse;

        /// <summary>
        /// Public constructor.
        /// </summary>
        /// <param name="inGame">Reference to the active Game instance.</param>
        public FreeCameraController(Game inGame, FreeCamera inCamera)
            : base(inGame)
        {
            this.UpdateOrder = 1;

            this.mCamera = inCamera;

            mActionKeys.Add(Actions.Crouch, Keys.LeftControl);
            mActionKeys.Add(Actions.Walk, Keys.Tab);
            mActionKeys.Add(Actions.Jump, Keys.Space);
            mActionKeys.Add(Actions.MoveForwards, Keys.W);
            mActionKeys.Add(Actions.MoveBackwards, Keys.S);
            mActionKeys.Add(Actions.StrafeLeft, Keys.A);
            mActionKeys.Add(Actions.StrafeRight, Keys.D);
            mActionKeys.Add(Actions.Run, Keys.LeftShift);

            mClientBounds = inGame.Window.ClientBounds;
            mWindowWidth = mClientBounds.Width;
            mWindowHeight = mClientBounds.Height;
        }

        public override void Initialize()
        {
            // Input service.
            mInput = (InputSystem)Game.Services.GetService(typeof(InputSystem));
            mKeyboardService = mInput.KeyboardService;
            // TODO: mMouseService = mInput.MouseService;

            mPrevMouse = mCurrMouse = Mouse.GetState();

            base.Initialize();
        }

        /// <summary>
        /// Applies values to global vectors.
        /// </summary>
        /// <param name="inVecX">X-Axis / Right.</param>
        /// <param name="inVecY">Y-Axis / Up.</param>
        /// <param name="inVecZ">Z-Axis / Forward.</param>
        public void SetGlobalVectors(Vector3 inVecX, Vector3 inVecY, Vector3 inVecZ)
        {
            this.gVecX = inVecX;
            this.gVecY = inVecY;
            this.gVecZ = inVecZ;
        }

        /// <summary>
        /// Maps an action to a specific key.
        /// </summary>
        public void MapActionToKey(Actions inAction, Keys inKey)
        {
            mActionKeys[inAction] = inKey;
        }

        /// <summary>
        /// Determines which way to move the camera based on player input.
        /// The returned values are in the range [-1,1].
        /// </summary>
        /// <param name="direction">The direction vector.</param>
        private void GetMovementDirection(out Vector3 outDirection)
        {
            // Reset the direction vector to zero.
            outDirection.X = 0.0f;
            outDirection.Y = 0.0f;
            outDirection.Z = 0.0f;

            // Movement actions are tricky, because we don't simply want to check whether a
            // key has been pressed and released, and checking if it had been held would take
            // two frames instead of one to respond.

            // Check if the forwards key is pressed. If so, move forwards.
            if (mKeyboardService.CurrentlyDown(mActionKeys[Actions.MoveForwards]))
            {
                if (!bForwardsPressed)
                {
                    bForwardsPressed = true;
                    mCurrentVelocity.Z = 0.0f;
                }

                outDirection.Z += 1.0f;
            }
            else { bForwardsPressed = false; }

            // Check if the backwards key is pressed. If so, move backwards.
            if (mKeyboardService.CurrentlyDown(mActionKeys[Actions.MoveBackwards]))
            {
                if (!bBackwardsPressed)
                {
                    bBackwardsPressed = true;
                    mCurrentVelocity.Z = 0.0f;
                }

                outDirection.Z -= 1.0f;
            }
            else { bBackwardsPressed = false; }

            // Check if the strafe-left key is pressed. If so, move left.
            if (mKeyboardService.CurrentlyDown(mActionKeys[Actions.StrafeLeft]))
            {
                if (!bStrafeLeftPressed)
                {
                    bStrafeLeftPressed = true;
                    mCurrentVelocity.X = 0.0f;
                }

                outDirection.X -= 1.0f;
            }
            else { bStrafeLeftPressed = false; }

            // Check if the strafe-right key is pressed. If so, move right.
            if (mKeyboardService.CurrentlyDown(mActionKeys[Actions.StrafeRight]))
            {
                if (!bStrafeRightPressed)
                {
                    bStrafeRightPressed = true;
                    mCurrentVelocity.X = 0.0f;
                }

                outDirection.X += 1.0f;
            }
            else { bStrafeRightPressed = false; }

            if (mKeyboardService.CurrentlyDown(mActionKeys[Actions.Crouch]))
            {
                switch (ePosture)
                {
                    case Posture.Standing:
                        ePosture = Posture.Crouching;
                        outDirection.Y -= 1.0f;
                        mCurrentVelocity.Y = 0.0f;
                        break;

                    case Posture.Crouching:
                        outDirection.Y -= 1.0f;
                        break;

                    case Posture.Rising:
                        outDirection.Y += 1.0f;
                        break;

                    default:
                        break;
                }
            }
            else
            {
                switch (ePosture)
                {
                    case Posture.Crouching:
                        ePosture = Posture.Rising;
                        outDirection.Y += 1.0f;
                        mCurrentVelocity.Y = 0.0f;
                        break;

                    case Posture.Rising:
                        outDirection.Y += 1.0f;
                        break;

                    default:
                        break;
                }
            }

            if (mKeyboardService.IsKeyPressed(mActionKeys[Actions.Jump]))
            {
                switch (ePosture)
                {
                    case Posture.Standing:
                        ePosture = Posture.Jumping;
                        mCurrentVelocity.Y = mVelocity.Y;
                        outDirection.Y += 1.0f;
                        break;
                    case Posture.Jumping:
                        outDirection.Y += 1.0f;
                        break;
                    default:
                        break;
                }
            }
            else
            {
                if (ePosture == Posture.Jumping) { outDirection.Y += 1.0f; }
            }
        }
    }
}
