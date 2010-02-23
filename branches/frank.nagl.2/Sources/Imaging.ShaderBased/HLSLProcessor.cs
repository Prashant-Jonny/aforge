﻿// AForge Shader-Based Image Processing Library
// AForge.NET framework
// http://www.aforgenet.com/framework/
//
// Copyright © Frank Nagl, 2009-2010
// admin@franknagl.de
//
namespace AForge.Imaging.ShaderBased
{
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using System.Windows.Forms;
    using System.Drawing;
    using Color = Microsoft.Xna.Framework.Graphics.Color;
    using AForge.Imaging.ShaderBased.HLSLFilter;

    /// <summary>
    /// HLSL based <c>I</c>mage <c>P</c>rocessing framework. 
    /// Implements the <see cref="IShaderProcessor"/> interface.
    /// </summary>
    public class HLSLProcessor : IShaderProcessor
    {
        /// <summary>Windows control using for online rendering process.</summary>
        protected Control renderControl;
        GraphicsDevice graphics;
        /// <summary>
        /// Gets or sets the shader filter to apply to.
        /// </summary>
        /// <value>The shader filter to apply to.</value>
        public HLSLBaseFilter Filter { get; set; }
        Texture2D texture;
        SpriteBatch spriteBatch;
        TextureInformation info;
        RenderTarget2D rt;

        /// <summary>
        /// Initializes a new instance of the <see cref="HLSLProcessor"/> class.
        /// </summary>
        public HLSLProcessor()
        {
            Filter = new HLSLOriginal();
        }

        private void Init(int w, int h)
        {
            renderControl.Width = w;
            renderControl.Height = h;

            PresentationParameters pp = new PresentationParameters();
            pp.IsFullScreen = false;
            pp.SwapEffect = SwapEffect.Discard;
            pp.BackBufferWidth = w;
            pp.BackBufferHeight = h;
            pp.EnableAutoDepthStencil = true;
            pp.AutoDepthStencilFormat = DepthFormat.Depth24Stencil8;
            pp.PresentOptions = PresentOptions.None;
            pp.RenderTargetUsage = RenderTargetUsage.PreserveContents;

            graphics = new GraphicsDevice(GraphicsAdapter.DefaultAdapter,
                DeviceType.Hardware, renderControl.Handle, pp);
            spriteBatch = new SpriteBatch(graphics);
            rt = new RenderTarget2D(graphics, w, h, 0, SurfaceFormat.Color);
        }

        #region IShaderProcessor Member

        /// <summary>
        /// Starts the HLSL image processor.
        /// </summary>
        /// <param name="file">Source image's filename.</param>
        /// <remarks>Inits all necessary devices and
        /// informations for shader based image processing.</remarks>
        public void Begin(string file)
        {
            renderControl = new Control();
            Begin(file, renderControl);
        }

        /// <summary>
        /// Starts the HLSL image processor.
        /// </summary>
        /// <param name="file">Source image's filename.</param>
        /// <param name="control">Windows control using for online rendering process.</param>
        /// <remarks>Inits all necessary devices and
        /// informations for shader based image processing.</remarks>
        public void Begin(string file, Control control)
        {        
            this.renderControl = control;
            info = Texture2D.GetTextureInformation(file);
            Init(info.Width, info.Height);
            texture = Texture2D.FromFile(graphics, file);            
        }

        /// <summary>
        /// Starts the HLSL image processor.
        /// </summary>
        /// <param name="bitmap">Source image to process.</param>
        /// <remarks>Inits all necessary devices and
        /// informations for shader based image processing.</remarks>
        public void Begin(Bitmap bitmap)
        {
            renderControl = new Control();
            Begin(bitmap, renderControl);
        }

        /// <summary>
        /// Starts the HLSL image processor.
        /// </summary>
        /// <param name="bitmap">Source image to process.</param>
        /// <param name="control">Windows control using for online rendering process.</param>
        /// <remarks>Inits all necessary devices and
        /// informations for shader based image processing.</remarks>
        public void Begin(Bitmap bitmap, Control control)
        {
            this.renderControl = control;
            Init(bitmap.Width, bitmap.Height);
            ImageConverter.BitmapToTexture(bitmap, graphics, out texture, out info);
        }

        /// <summary>
        /// Stops render process. Disposes the used windows control and the graphics device.
        /// </summary>
        public void End()
        {
            renderControl.Dispose();
            graphics.Dispose();
        }

        /// <summary>
        /// Renders online the image processing results into a windows control.
        /// </summary>
        public void Render()
        {
            graphics.Clear(Color.Black);
            spriteBatch.Begin(SpriteBlendMode.None, SpriteSortMode.Immediate, SaveStateMode.SaveState);
            Filter.RenderEffect(graphics, info);
            spriteBatch.Draw(texture, Vector2.Zero, Color.White);
            spriteBatch.End();
            graphics.Present();
        }

        /// <summary>
        /// Renders offline the image processing routines and stores the
        /// result image into a texture.
        /// </summary>
        /// <returns>Result texture.</returns>
        public Texture2D RenderToTexture()
        {
            graphics.Clear(Color.Black);
            graphics.SetRenderTarget(0, rt);
            spriteBatch.Begin(SpriteBlendMode.None, SpriteSortMode.Immediate, SaveStateMode.SaveState);
            Filter.RenderEffect(graphics, info);
            spriteBatch.Draw(texture, Vector2.Zero, Color.White);
            spriteBatch.End();
            graphics.SetRenderTarget(0, null);
            return rt.GetTexture();            
        }

        /// <summary>
        /// Renders offline the image processing routines and stores the
        /// result image into a new image.
        /// </summary>
        /// <returns>Result image.</returns>
        public Bitmap RenderToBitmap()
        {            
            graphics.Clear(Color.Black);
            graphics.SetRenderTarget(0, rt);
            spriteBatch.Begin(SpriteBlendMode.None, SpriteSortMode.Immediate, SaveStateMode.SaveState);
            Filter.RenderEffect(graphics, info);
            spriteBatch.Draw(texture, Vector2.Zero, Color.White);
            spriteBatch.End();
            graphics.SetRenderTarget(0, null);
            Texture2D tex = rt.GetTexture();

            return ImageConverter.TextureToRGB(tex);
        }
        #endregion IShaderProcessor Member
    }
}
