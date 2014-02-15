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

namespace Core.Input
{
    /// <summary>
    /// Facade class providing access to input devices via a collection of
    /// KeyboardService, MouseService and GamePadService.
    /// </summary>
    public class InputSystem : Microsoft.Xna.Framework.GameComponent
    {
        // Keyboard service to check for keyboard input.
        KeyboardService mKeyboardInputService;

        // Mouse service to check mouse movements and button-clicks.
        MouseService mMouseService;

        // GamePad service for use with upto four Xbox 360 controllers.
        GamePadService mGamePadService;

        /// <summary>
        /// Public constructor.
        /// </summary>
        /// <param name="inGame">Reference to the active Game instance.</param>
        public InputSystem(Game inGame)
            : base(inGame)
        {
            // Instantiate the child objects.
            this.mKeyboardInputService = new KeyboardService(inGame);
            this.mMouseService = new MouseService(inGame);
            this.mGamePadService = new GamePadService(inGame);

            // Add this as a service.
            inGame.Services.AddService(typeof(InputSystem), this);
        }

        /// <summary>
        /// Update the child services.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            // Realistically, we'll want to update the mouse first as it
            // is the easiest to notice lag with.

            // Update the keyboard first.
            mKeyboardInputService.Update();

            // Secondly, update the gamepad.
            mGamePadService.Update();

            base.Update(gameTime);
        }

        /// <summary>
        /// The KeyboardService class contains input information
        /// regarding the connected Keyboard. Can be used to check
        /// which keys are pressed, released and held.
        /// </summary>
        public KeyboardService KeyboardService
        {
            get { return mKeyboardInputService; }
        }

        /// <summary>
        /// The GamePadService class contains input information
        /// regarding connected GamePads. Can be used to check for
        /// specific button-presses.
        /// </summary>
        public GamePadService GamePadService
        {
            get { return mGamePadService; }
        }
    }
}
