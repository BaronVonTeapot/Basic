using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Core.Input
{
    public class MouseService
    {
        private const float Default_SmoothingSensitivity = 0.5f;
        private const int Default_SmoothingCacheSize = 10;

        // Reference to the active Game instance.
        Game pGame;

        // State objects for the current and previous updates.
        MouseState mCurrentState, mPreviousState;

        // Cache of movement values for filtering purposes.
        Vector2[] mSmoothingCache;
        int mSmoothingCacheSize;

        // Stores mouse movement.
        Vector2[] mMovement;
        Vector2 mMouseDelta;
        int mIndex;

        // Sensitivity of smoothed mouse movement.
        float mSmoothingSensitivity;

        // Determines whether smoothing is used or not.
        bool mEnableSmoothing = true;

        // Average values for mouse-filtering.
        float mAverageX, mAverageY, mAverageTotal, mCurrentWeight;

        // Rectangle representing the dimensions of the game window.
        Rectangle mClientBounds;
        int mCenterX, mCenterY, mDeltaX, mDeltaY;
        
        /// <summary>
        /// Public constructor.
        /// </summary>
        public MouseService(Game inGame)
        {
            pGame = inGame;

            // Mouse status objects updated each frame.
            mCurrentState = new MouseState();
            mPreviousState = new MouseState();

            // Contains mouse cursor position information.
            mMovement = new Vector2[2];
            mMovement[0].X = 0.0f;
            mMovement[0].Y = 0.0f;
            mMovement[1].X = 0.0f;
            mMovement[1].Y = 0.0f;
            mIndex = 0;

            // Mouse movement is smoothed and filtered in-game.
            mSmoothingSensitivity = Default_SmoothingSensitivity;
            mSmoothingCacheSize = Default_SmoothingCacheSize;
            mSmoothingCache = new Vector2[mSmoothingCacheSize];
        }

        /// <summary>
        /// Update method.
        /// </summary>
        public void Update()
        {
            // Update the MouseState variables.
            mPreviousState = mCurrentState;
            mCurrentState = Mouse.GetState();

            mClientBounds = pGame.Window.ClientBounds;
            mCenterX = mClientBounds.Width >> 1;
            mCenterY = mClientBounds.Height >> 1;

            mDeltaX = mCenterX - mCurrentState.X;
            mDeltaY = mCenterX - mCurrentState.Y;

            Mouse.SetPosition(mCenterX, mCenterY);

            if (mEnableSmoothing)
            {
                FilterMouse((float)mDeltaX, (float)mDeltaY);
                SmoothMouse(mMouseDelta.X, mMouseDelta.Y);
            }
            else
            {
                mMouseDelta.X = (float)mDeltaX;
                mMouseDelta.Y = (float)mDeltaY;
            }
        }

        /// <summary>
        /// Filters the mouse movement based upon a weighted sum of mouse
        /// movements recorded during previous updates.
        /// <para>
        ///  Nettle, Paul "Smooth Mouse Filtering", flipCode's Ask Midnight column.
        ///  http://www.flipcode.com/cgi-bin/fcarticles.cgi?show=64462
        /// </para>
        /// </summary>
        /// <param name="inX">Horizontal mouse distance from its previous location.</param>
        /// <param name="inY">Vertical mouse distance from its previous location.</param>
        private void FilterMouse(float inX, float inY)
        {
            // Shuffle all cache entries. Move newer entries to the front.
            int i;
            for (i = mSmoothingCache.Length - 1; i > 0; i--) 
            {
                mSmoothingCache[i].X = mSmoothingCache[i - 1].X;
                mSmoothingCache[i].Y = mSmoothingCache[i - 1].Y;
            }

            // Record the current mouse movement entry at the front of the cache.
            mSmoothingCache[0].X = inX;
            mSmoothingCache[0].Y = inY;

            mAverageX = mAverageY = 0.0f;
            mAverageTotal = 0.0f;
            mCurrentWeight = 1.0f;

            // Filter the mouse movement with the rest of the cache entries. Use a
            // weighted average where newer entries have more effect than older
            // entries (towards the bottom of the cache).
            for (i = 0; i < mSmoothingCache.Length; ++i)
            {
                mAverageX += mSmoothingCache[i].X * mCurrentWeight;
                mAverageY += mSmoothingCache[i].Y * mCurrentWeight;
                mAverageTotal += 1.0f * mCurrentWeight;
                mCurrentWeight *= mSmoothingSensitivity;
            }

            // Calculate the new smoothed movement.
            mMouseDelta.X = mAverageX / mAverageTotal;
            mMouseDelta.Y = mAverageY / mAverageTotal;
        }

        private void SmoothMouse(float inX, float inY)
        {
            mMovement[mIndex].X = inX;
            mMovement[mIndex].Y = inY;

            mMouseDelta.X = (mMovement[0].X + mMovement[1].X) * 0.5f;
            mMouseDelta.Y = (mMovement[0].Y + mMovement[1].Y) * 0.5f;

            mIndex ^= 1;
            mMovement[mIndex].X = 0.0f;
            mMovement[mIndex].Y = 0.0f;
        }

        /// <summary>
        /// Smoothed X-axis position.
        /// </summary>
        public float XDelta { get { return mMouseDelta.X; } }

        /// <summary>
        /// Smoothed Y-axis position.
        /// </summary>
        public float YDelta { get { return mMouseDelta.Y; } }

        /// <summary>
        /// Un-filtered X-axis position.
        /// </summary>
        public int RealX { get { return mCurrentState.X; } }

        /// <summary>
        /// Un-filtered Y-axis position.
        /// </summary>
        public int RealY { get { return mCurrentState.Y; } }
    }
}
