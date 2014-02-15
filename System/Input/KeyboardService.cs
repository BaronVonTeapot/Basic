using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Core.Input
{
    /// <summary>
    /// Simple Keyboard input service.
    /// </summary>
    public class KeyboardService
    {
        // Reference to the active Game instance.
        Game pGame;

        KeyboardState mPreviousState;
        KeyboardState mCurrentState;

        /// <summary>
        /// Public constructor.
        /// </summary>
        public KeyboardService(Game inGame)
        {
            pGame = inGame;
            mPreviousState = Keyboard.GetState();
            mCurrentState = Keyboard.GetState();
        }

        /// <summary>
        /// Update method called each frame.
        /// </summary>
        public virtual void Update()
        {
            mPreviousState = mCurrentState;
            mCurrentState = Keyboard.GetState();
        }

        /// <summary>
        /// Helper method to determine status of a single key.
        /// </summary>
        private bool IsKeyDown(KeyboardState inState, Keys inKey)   { return inState.IsKeyDown(inKey); }

        /// <summary>
        /// Helper method to determine status of a single key.
        /// </summary>
        private bool IsKeyUp(KeyboardState inState, Keys inKey)     { return inState.IsKeyUp(inKey); }

        /// <summary>
        /// Checks whether a specific key is pressed.
        /// </summary>
        /// <param name="inKey">Key to check.</param>
        public bool IsKeyPressed(Keys inKey)    
        { 
            return (this.IsKeyDown(mCurrentState, inKey) &&
                    this.IsKeyUp(mPreviousState, inKey)); 
        }

        /// <summary>
        /// Checks whether a specific key is released.
        /// </summary>
        /// <param name="inKey">Key to check.</param>
        public bool IsKeyReleased(Keys inKey)   
        { 
            return (this.IsKeyUp(mCurrentState, inKey) &&
                    this.IsKeyDown(mPreviousState, inKey)); }

        /// <summary>
        /// Checks whether a specific key is held.
        /// </summary>
        /// <param name="inKey">Key to check.</param>
        public bool IsKeyHeld(Keys inKey)       
        { 
            return (this.IsKeyDown(mCurrentState, inKey) && 
                    this.IsKeyDown(mPreviousState, inKey)); }

        /// <summary>
        /// Is a specific key currently down?
        /// </summary>
        public bool CurrentlyDown(Keys inKey) { return this.IsKeyDown(mCurrentState, inKey); }

        /// <summary>
        /// Is a specific key currently up?
        /// </summary>
        public bool CurrentlyUp(Keys inKey) { return this.IsKeyUp(mCurrentState, inKey); }

        /// <summary>
        /// Was a specific key previously down?
        /// </summary>
        public bool PreviouslyDown(Keys inKey) { return this.IsKeyDown(mPreviousState, inKey); }

        /// <summary>
        /// Was a specific key previously up?
        /// </summary>
        public bool PreviouslyUp(Keys inKey) { return this.IsKeyUp(mPreviousState, inKey); }

        /// <summary>
        /// Keyboard state for current frame.
        /// </summary>
        public KeyboardState CurrentState { get { return mCurrentState; } }

        /// <summary>
        /// Keyboard state for previous frame.
        /// </summary>
        public KeyboardState PreviousState { get { return mPreviousState; } }
    }
}