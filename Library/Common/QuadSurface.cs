using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Library
{
    public class ScreenAlignedQuad : IDisposable
    {
        int mWidth, mHeight;
        RenderTarget2D mRenderTarget;
        VertexBuffer mVertexBuffer;
        bool mInitialized = false;
        bool mSet = false;

        /// <summary>
        /// Public constructor.
        /// </summary>
        public ScreenAlignedQuad() { }

        #region Initialize Method.
        /// <summary>
        /// Initializes the Quad and prepares it for rendering.
        /// </summary>
        /// <param name="inGraphicsDevice">Reference to the GraphicsDevice.</param>
        /// <param name="inWidth">Width of the quad.</param>
        /// <param name="inHeight">Height of the quad.</param>
        public virtual void Initialize(GraphicsDevice inGraphicsDevice,
                                          int inWidth, int inHeight)
        {
            // The texture must be at least 1 x 1 or it's useless.
            if (inWidth < 1 || inHeight < 1)
            {
                throw new InvalidOperationException
                    ("Dimensions are too small.");
            }

            // If the GraphicsDevice is null, we cannot use it and must exit.
            if (inGraphicsDevice == null)
            {
                throw new ArgumentNullException
                    ("GraphicsDevice cannot be null.");
            }

            // If this has already been initialized, exit now.
            if (mInitialized) { return; }

            mWidth = inWidth;
            mHeight = inHeight;

            // Create the render texture.
            mRenderTarget = new RenderTarget2D(inGraphicsDevice, mWidth, mHeight, false,
                                         SurfaceFormat.Color, DepthFormat.None);
            // Create the vertex buffer.
            mVertexBuffer = new VertexBuffer(inGraphicsDevice,
                                             VertexPositionTexture.VertexDeclaration,
                                             4, BufferUsage.WriteOnly);

            // One half-pixel used because we want the center of the pixel, not the edge.
            float ps = 1f / (mWidth - 1);

            // Define the vertices within a temporary array.
            VertexPositionTexture[] tempVertexArray = new VertexPositionTexture[4];
            tempVertexArray[0] = new VertexPositionTexture()
            {
                Position = new Vector3(-1f, 1f, 0f),
                TextureCoordinate = new Vector2(0, 1f)
            };
            tempVertexArray[1] = new VertexPositionTexture()
            {
                Position = new Vector3(1f, 1f, 0f),
                TextureCoordinate = new Vector2(1 + ps, 1)
            };
            tempVertexArray[2] = new VertexPositionTexture()
            {
                Position = new Vector3(-1f, -1f, 0f),
                TextureCoordinate = new Vector2(0f, 0 - ps)
            };
            tempVertexArray[3] = new VertexPositionTexture()
            {
                Position = new Vector3(1f, -1f, 0f),
                TextureCoordinate = new Vector2(1f + ps, 0 - ps)
            };

            // Set the vertices onto the buffer.
            mVertexBuffer.SetData<VertexPositionTexture>(tempVertexArray);

            mInitialized = true;
        }
        #endregion

        #region Begin, Clear, Draw, Resolve, End Methods.
        /// <summary>
        /// Begin use of the Quad. Will exit if Begin has already been called.
        /// </summary>
        /// <param name="inGraphicsDevice">Reference to the GraphicsDevice to use.</param>
        public virtual void Begin(GraphicsDevice inGraphicsDevice, BlendState inBlendState,
                                  DepthStencilState inDepthStencilState)
        {
            if (mSet) { return; }

            inGraphicsDevice.SetRenderTarget(mRenderTarget);
            inGraphicsDevice.BlendState = inBlendState;
            inGraphicsDevice.DepthStencilState = inDepthStencilState;

            mSet = true;
        }

        /// <summary>
        /// Simpler Begin method. Begins use of the Quad. Will exit if 
        /// Begin has already been called.
        /// </summary>
        /// <param name="inGraphicsDevice"></param>
        public virtual void Begin(GraphicsDevice inGraphicsDevice)
        {
            Begin(inGraphicsDevice, BlendState.Opaque, DepthStencilState.None);
        }

        /// <summary>
        /// Clears the render texture, ready for drawing.
        /// </summary>
        /// <param name="inGraphicsDevice">Reference to the GraphicsDevice to use.</param>
        public virtual void Clear(GraphicsDevice inGraphicsDevice)
        {
            if (!mSet) { return; }

            inGraphicsDevice.Clear(Color.CornflowerBlue);
        }

        /// <summary>
        /// Draws the quad.
        /// </summary>
        /// <param name="inGraphicsDevice">Reference to the GraphicsDevice to use.</param>
        public virtual void DrawQuad(GraphicsDevice inGraphicsDevice)
        {
            if (!mSet) { return; }

            inGraphicsDevice.SetVertexBuffer(mVertexBuffer);
            inGraphicsDevice.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
            inGraphicsDevice.SetVertexBuffer(null);
        }

        /// <summary>
        /// Resolves the RenderTarget to a Texture2D.
        /// </summary>
        /// <param name="outRenderTexture"></param>
        public virtual void Resolve(out Texture2D outRenderTexture)
        {
            outRenderTexture = (Texture2D)mRenderTarget;
        }

        /// <summary>
        /// End use of the Quad.
        /// </summary>
        /// <param name="inGraphicsDevice">Reference to the GraphicsDevice to use.</param>
        public virtual void End(GraphicsDevice inGraphicsDevice)
        {
            if (!mSet) { return; }
            inGraphicsDevice.SetRenderTarget(null);

            mSet = false;
        }
        #endregion

        #region IDisposable implementation.
        /// <summary>
        /// Public implementation of IDisposal interface.
        /// </summary>
        public virtual void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Protected implementation of IDisposal interface.
        /// </summary>
        /// <param name="disposing">Dispose of IDisposal child objects.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (mVertexBuffer != null)
                    mVertexBuffer.Dispose();

                if (mRenderTarget != null)
                    mRenderTarget.Dispose();
            }
        }
        #endregion

        #region Properties.
        /// <summary>
        /// Render texture as Texture2D.
        /// </summary>
        public Texture2D Texture { get { return (Texture2D)mRenderTarget; } }

        /// <summary>
        /// RenderTarget used by the Quad.
        /// </summary>
        public RenderTarget2D RenderTarget { get { return mRenderTarget; } }

        /// <summary>
        /// Width of the Quad.
        /// </summary>
        public int Width { get { return mWidth; } }

        /// <summary>
        /// Height of the Quad.
        /// </summary>
        public int Height { get { return mHeight; } }

        /// <summary>
        /// Is the Screen-Aligned Quad set?
        /// </summary>
        public bool Set { get { return mSet; } }
        #endregion
    }
}
