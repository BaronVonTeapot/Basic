using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Core.Input
{
    /// <summary>
    /// Simple GamePad input service.
    /// </summary>
    public class GamePadService
    {
        // Reference to the active Game instance.
        Game pGame;

        // Maximum number of Xbox 360 controllers to use.
        private const int MaximumInputs = 4;

        private readonly GamePadState[] mCurrentStates;
        private readonly GamePadState[] mPreviousStates;
        private readonly bool[] mConnected;

        /// <summary>
        /// Public constructor.
        /// </summary>
        public GamePadService(Game inGame)
        {
            pGame = inGame;
            mCurrentStates = new GamePadState[MaximumInputs];
            mPreviousStates = new GamePadState[MaximumInputs];
            mConnected = new bool[MaximumInputs];
        }

        /// <summary>
        /// Update method ensures GamePad status is accurate.
        /// </summary>
        public void Update()
        {
            // Save the state information to use next frame.
            mPreviousStates[0] = mCurrentStates[0];
            mPreviousStates[1] = mCurrentStates[1];
            mPreviousStates[2] = mCurrentStates[2];
            mPreviousStates[3] = mCurrentStates[3];

            // Get the state information for this frame.
            mCurrentStates[0] = GamePad.GetState(PlayerIndex.One);
            mCurrentStates[1] = GamePad.GetState(PlayerIndex.Two);
            mCurrentStates[2] = GamePad.GetState(PlayerIndex.Three);
            mCurrentStates[3] = GamePad.GetState(PlayerIndex.Four);

            // Keep track of whether a gamepad has been connected, so that we
            // can react if it is unplugged.
            if (mCurrentStates[0].IsConnected) { mConnected[0] = true; }
            if (mCurrentStates[1].IsConnected) { mConnected[1] = true; }
            if (mCurrentStates[2].IsConnected) { mConnected[2] = true; }
            if (mCurrentStates[3].IsConnected) { mConnected[3] = true; }
        }

        /// <summary>
        /// Helper for checking if a button was newly pressed during this update.
        /// The controllingPlayer parameter specifies which player to read input for.
        /// If this is null, it will accept input from any player. When a button press
        /// is detected, the output playerIndex reports which player pressed it.
        /// </summary>
        public bool IsNewButtonPress(Buttons inButton, PlayerIndex? inControllingPlayer, out PlayerIndex outPlayerIndex)
        {
            if (inControllingPlayer.HasValue)
            {
                outPlayerIndex = inControllingPlayer.Value;

                int i = (int)outPlayerIndex;

                return (mCurrentStates[i].IsButtonDown(inButton) && 
                        mPreviousStates[i].IsButtonUp(inButton));
            }
            else
            {
                return (IsNewButtonPress(inButton, PlayerIndex.One, out outPlayerIndex)     ||
                        IsNewButtonPress(inButton, PlayerIndex.Two, out outPlayerIndex)     ||
                        IsNewButtonPress(inButton, PlayerIndex.Three, out outPlayerIndex)   ||
                        IsNewButtonPress(inButton, PlayerIndex.Four, out outPlayerIndex));
            }
        }

        /// <summary>
        /// Helper for checking if a button was newly released during this update.
        /// The controllingPlayer parameter specifies which player to read input for.
        /// If this is null, it will accept input from any player. When a button press
        /// is detected, the output playerIndex reports which player pressed it.
        /// </summary>
        public bool IsNewButtonRelease(Buttons inButton, PlayerIndex? inControllingPlayer, 
                                       out PlayerIndex outPlayerIndex)
        {
            if (inControllingPlayer.HasValue)
            {
                outPlayerIndex = inControllingPlayer.Value;
                int i = (int)outPlayerIndex;

                return (mCurrentStates[i].IsButtonUp(inButton) &&
                        mPreviousStates[i].IsButtonDown(inButton));
            }
            else
            {
                return (IsNewButtonRelease(inButton, PlayerIndex.One, out outPlayerIndex)   ||
                    IsNewButtonRelease(inButton, PlayerIndex.Two, out outPlayerIndex)       ||
                    IsNewButtonRelease(inButton, PlayerIndex.Three, out outPlayerIndex)     ||
                    IsNewButtonRelease(inButton, PlayerIndex.Four, out outPlayerIndex));
            }
        }

        /// <summary>
        /// Helper for checking if a button is pressed. Not the same as IsNewButtonPress,
        /// which checks if a button was pressed during the latest update. This method checks
        /// the current state of the button, and if down returns true.
        /// </summary>
        public bool IsButtonDown(Buttons inButton, PlayerIndex? inControllingPlayer, 
                                 out PlayerIndex outPlayerIndex)
        {
            if (inControllingPlayer.HasValue)
            {
                outPlayerIndex = inControllingPlayer.Value;
                int i = (int)outPlayerIndex;

                return mCurrentStates[i].IsButtonDown(inButton);
            }
            else
            {
                return (IsButtonDown(inButton, PlayerIndex.One, out outPlayerIndex)     ||
                        IsButtonDown(inButton, PlayerIndex.Two, out outPlayerIndex)     ||
                        IsButtonDown(inButton, PlayerIndex.Three, out outPlayerIndex)   ||
                        IsButtonDown(inButton, PlayerIndex.Four, out outPlayerIndex));
            }
        }

        /// <summary>
        /// Helper identical to IsButtonDown, but instead checks a button against the GamePadState
        /// for the previous update.
        /// </summary>
        public bool WasButtonDown(Buttons inButton, PlayerIndex? inControllingPlayer, out PlayerIndex outPlayerIndex)
        {
            if (inControllingPlayer.HasValue)
            {
                outPlayerIndex = inControllingPlayer.Value;
                int i = (int)outPlayerIndex;

                return mPreviousStates[i].IsButtonDown(inButton);
            }
            else
            {
                return (WasButtonDown(inButton, PlayerIndex.One, out outPlayerIndex) ||
                        WasButtonDown(inButton, PlayerIndex.Two, out outPlayerIndex) ||
                        WasButtonDown(inButton, PlayerIndex.Three, out outPlayerIndex) ||
                        WasButtonDown(inButton, PlayerIndex.Four, out outPlayerIndex));
            }
        }

        /// <summary>
        /// Helper for checking if a button is not pressed. Not the same as IsNewButtonRelease,
        /// which checks if a button was released during the latest update. This method checks the
        /// current state of the button, and if up returns true.
        /// </summary>
        public bool IsButtonUp(Buttons inButton, PlayerIndex? inControllingPlayer, out PlayerIndex outPlayerIndex)
        {
            if (inControllingPlayer.HasValue)
            {
                outPlayerIndex = inControllingPlayer.Value;
                int i = (int)outPlayerIndex;

                return mPreviousStates[i].IsButtonUp(inButton);
            }
            else
            {
                return (IsButtonUp(inButton, PlayerIndex.One, out outPlayerIndex)       ||
                        IsButtonUp(inButton, PlayerIndex.Two, out outPlayerIndex)       ||
                        IsButtonUp(inButton, PlayerIndex.Three, out outPlayerIndex)     ||
                        IsButtonUp(inButton, PlayerIndex.Four, out outPlayerIndex));
            }
        }

        /// <summary>
        /// Helper identical to IsButtonUp, but instead checks a button against the GamePadState
        /// for the previous update.
        /// </summary>
        public bool WasButtonUp(Buttons inButton, PlayerIndex? inControllingPlayer, out PlayerIndex outPlayerIndex)
        {
            if (inControllingPlayer.HasValue)
            {
                outPlayerIndex = inControllingPlayer.Value;
                int i = (int)outPlayerIndex;

                return mCurrentStates[i].IsButtonUp(inButton);
            }
            else
            {
                return (WasButtonUp(inButton, PlayerIndex.One, out outPlayerIndex) ||
                        WasButtonUp(inButton, PlayerIndex.Two, out outPlayerIndex) ||
                        WasButtonUp(inButton, PlayerIndex.Three, out outPlayerIndex) ||
                        WasButtonUp(inButton, PlayerIndex.Four, out outPlayerIndex));
            }
        }

        public bool IsConnected(PlayerIndex inPlayerIndex)
        {
            int i = (int)inPlayerIndex;
            return mConnected[i];
        }

        /// <summary>
        /// Gets the cached current GamePadState for with a particular PlayerIndex.
        /// </summary>
        public GamePadState GetCurrentState(PlayerIndex inPlayerIndex)
        {
            int i = (int)inPlayerIndex;
            return mCurrentStates[i];
        }

        /// <summary>
        /// Gets the cached previous GamePadState for a particular PlayerIndex.
        /// </summary>
        public GamePadState GetPreviousState(PlayerIndex inPlayerIndex)
        {
            int i = (int)inPlayerIndex;
            return mPreviousStates[i];
        }

        /// <summary>
        /// Array of GamePadState objects for the current frame.
        /// </summary>
        public GamePadState[] CurrentStates
        {
            get { return mCurrentStates; }
        }

        /// <summary>
        /// Array of GamePadState objects for the previous frame.
        /// </summary>
        public GamePadState[] PreviousStates
        {
            get { return mPreviousStates; }
        }
    }
}
