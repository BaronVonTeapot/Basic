using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Core.Graphics
{
    public static class ScreenCapture
    {
        /// <summary>
        /// Do we capture as .PNG or .JPEG?
        /// </summary>
        public enum ScreenCaptureType
        {
            UsePNG = 0,
            UseJPEG = 1
        }

        public static bool CaptureJpeg(GraphicsDevice inGraphicsDevice, String inFilename)
        {
            inFilename = inFilename + ".jpeg";

            // Store the current BackBuffer data in a new array of Color values. This
            // will take what is currently on the BackBuffer; everything current being
            // drawn to the screen.
            Color[] colorData = new Color[inGraphicsDevice.Viewport.Width *
                                          inGraphicsDevice.Viewport.Height];
            inGraphicsDevice.GetBackBufferData<Color>(colorData);

            // Next set the colors into a Texture, ready for saving.
            Texture2D backBufferTexture = new Texture2D(inGraphicsDevice,
                                                        inGraphicsDevice.Viewport.Width,
                                                        inGraphicsDevice.Viewport.Height);
            backBufferTexture.SetData<Color>(colorData, 0, colorData.Length);

            // Create the file after checking whether it exists. This requires a means
            // of altering the intended filename so that it cannot overwrite an existing
            // screen-capture, for instance suffixing an incremental digit onto the file
            // name, but this would have to be saved to avoid overwritten files when the
            // game crashes and the count is lost.
            if (!File.Exists(inFilename))
            {
                using (FileStream fileStream = File.Create(inFilename))
                {
                    backBufferTexture.SaveAsJpeg(fileStream,
                             inGraphicsDevice.Viewport.Width,
                             inGraphicsDevice.Viewport.Height);

                    fileStream.Flush();
                }

                return true;
            }

            return false;
        }

        public static bool CapturePng(GraphicsDevice inGraphicsDevice, String inFilename)
        {
            inFilename = inFilename + ".png";

            // Store the current BackBuffer data in a new array of Color values. This
            // will take what is currently on the BackBuffer; everything current being
            // drawn to the screen.
            Color[] colorData = new Color[inGraphicsDevice.Viewport.Width *
                                          inGraphicsDevice.Viewport.Height];
            inGraphicsDevice.GetBackBufferData<Color>(colorData);

            // Next set the colors into a Texture, ready for saving.
            Texture2D backBufferTexture = new Texture2D(inGraphicsDevice,
                                                        inGraphicsDevice.Viewport.Width,
                                                        inGraphicsDevice.Viewport.Height);
            backBufferTexture.SetData<Color>(colorData, 0, colorData.Length);

            // Create the file after checking whether it exists. This requires a means
            // of altering the intended filename so that it cannot overwrite an existing
            // screen-capture, for instance suffixing an incremental digit onto the file
            // name, but this would have to be saved to avoid overwritten files when the
            // game crashes and the count is lost.
            if (!File.Exists(inFilename))
            {
                using (FileStream fileStream = File.Create(inFilename))
                {
                    backBufferTexture.SaveAsPng(fileStream,
                             inGraphicsDevice.Viewport.Width,
                             inGraphicsDevice.Viewport.Height);

                    fileStream.Flush();
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Stores the current contents of the back buffer as a .PNG file.
        /// </summary>
        /// <param name="inGraphicsDevice">GraphicsDevice to grab the back buffer data from.</param>
        /// <param name="inFilename">String containing the name of the file to save.</param>
        /// <returns>True if the file is saved successfully, otherwise false.</returns>
        public static bool Capture(GraphicsDevice inGraphicsDevice,
                                   String inFilename, ScreenCaptureType inCaptureExtension)
        {
            if (inCaptureExtension == ScreenCaptureType.UseJPEG) { inFilename = inFilename + ".jpg"; }
            if (inCaptureExtension == ScreenCaptureType.UsePNG) { inFilename = inFilename + ".png"; }

            // Store the current BackBuffer data in a new array of Color values. This
            // will take what is currently on the BackBuffer; everything current being
            // drawn to the screen.
            Color[] colorData = new Color[inGraphicsDevice.Viewport.Width *
                                          inGraphicsDevice.Viewport.Height];
            inGraphicsDevice.GetBackBufferData<Color>(colorData);

            // Next set the colors into a Texture, ready for saving.
            Texture2D backBufferTexture = new Texture2D(inGraphicsDevice,
                                                        inGraphicsDevice.Viewport.Width,
                                                        inGraphicsDevice.Viewport.Height);
            backBufferTexture.SetData<Color>(colorData, 0, colorData.Length);

            // Create the file after checking whether it exists. This requires a means
            // of altering the intended filename so that it cannot overwrite an existing
            // screen-capture, for instance suffixing an incremental digit onto the file
            // name, but this would have to be saved to avoid overwritten files when the
            // game crashes and the count is lost.
            if (!File.Exists(inFilename))
            {
                using (FileStream fileStream = File.Create(inFilename))
                {
                    // The choice passed in as the 3rd parameter just exists for the sake
                    // of providing options. If one is clearly advantageous to the other
                    // I'll hard-code it to use that instead. But, for now, we allow the
                    // choice between JPEG and PNG. PNG files have transparency.
                    switch (inCaptureExtension)
                    {
                        case ScreenCaptureType.UseJPEG:
                            backBufferTexture.SaveAsJpeg(fileStream,
                                                         inGraphicsDevice.Viewport.Width,
                                                         inGraphicsDevice.Viewport.Height);
                            break;
                        case ScreenCaptureType.UsePNG:
                            backBufferTexture.SaveAsPng(fileStream,
                                                        inGraphicsDevice.Viewport.Width,
                                                        inGraphicsDevice.Viewport.Height);
                            break;
                    }

                    fileStream.Flush();
                }

                return true;
            }

            return false;
        }
    }
}