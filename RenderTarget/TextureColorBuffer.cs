/* MIT License (MIT)
 *
 * Copyright (c) 2020 Marc Roßbach
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

using IgnitionDX.Math;
using IgnitionDX.Utilities;
using SharpDX.Direct3D11;
using SharpDX.Mathematics.Interop;

namespace IgnitionDX.Graphics
{
    public class TextureColorBuffer : IColorBuffer, ITexture
    {
        private Texture2DDescription _textureDesc;
        private SamplerStateDescription _samplerDesc;

        private RendererValue<SharpDX.Direct3D11.Texture2D> _texture = new RendererValue<SharpDX.Direct3D11.Texture2D>(null);
        private RendererValue<RenderTargetView> _renderTargetView = new RendererValue<RenderTargetView>(null);
        private RendererValue<ShaderResourceView> _shaderResourceView = new RendererValue<ShaderResourceView>(null);
        private RendererValue<bool> _mipMapsDirty = new RendererValue<bool>(true);

        public int Width
        {
            get { return _textureDesc.Width; }
        }

        public int Height
        {
            get { return _textureDesc.Height; }
        }

        public bool IsMipMapped
        {
            get { return _textureDesc.MipLevels == 0; }
        }

        public TextureColorBuffer(SharpDX.DXGI.Format format, int width, int height, int samplesPerPixel, bool mipMapped)
        {
            _textureDesc = new Texture2DDescription();
            _textureDesc.Height = height;
            _textureDesc.Width = width;
            _textureDesc.Usage = ResourceUsage.Default;
            _textureDesc.BindFlags = BindFlags.ShaderResource | BindFlags.RenderTarget;
            _textureDesc.Format = format;
            _textureDesc.CpuAccessFlags = CpuAccessFlags.None;
            _textureDesc.MipLevels = mipMapped ? 0 : 1;
            _textureDesc.ArraySize = 1;
            _textureDesc.SampleDescription = new SharpDX.DXGI.SampleDescription(samplesPerPixel, 0);
            _textureDesc.OptionFlags = mipMapped ? ResourceOptionFlags.GenerateMipMaps : ResourceOptionFlags.None;
        }

        public TextureColorBuffer(Texture2DDescription textureDesc, SamplerStateDescription samplerDesc)
        {
            _textureDesc = textureDesc;
            _samplerDesc = samplerDesc;
        }

        public void Clear(Renderer renderer, Color4 color)
        {
            RenderTargetView rtView = GetRenderTargetView(renderer);
            renderer.DeviceContext.ClearRenderTargetView(rtView, new RawColor4(color.R, color.G, color.B, color.A));
        }

        public SharpDX.Direct3D11.Texture2D GetTexture(Renderer renderer)
        {
            Logger.LogInfo(this, "Creating color buffer.");

            SharpDX.Direct3D11.Texture2D tex = _texture.Get(renderer);
            if (tex == null)
            {
                tex = new SharpDX.Direct3D11.Texture2D(renderer.Device, _textureDesc);
                _texture.Set(renderer, tex, Width * Height * 4);
            }

            return tex;
        }

        public RenderTargetView GetRenderTargetView(Renderer renderer)
        {
            RenderTargetView view = _renderTargetView.Get(renderer);
            if (view == null)
            {
                view = new RenderTargetView(renderer.Device, GetTexture(renderer));
                _renderTargetView.Set(renderer, view);
            }

            return view;
        }

        public ShaderResourceView GetShaderResourceView(Renderer renderer)
        {
            ShaderResourceView view = _shaderResourceView.Get(renderer);
            if (view == null)
            {
                view = new ShaderResourceView(renderer.Device, GetTexture(renderer));
                _shaderResourceView.Set(renderer, view);
            }

            if (IsMipMapped && _mipMapsDirty.Get(renderer))
            {
                renderer.DeviceContext.GenerateMips(view);
                _mipMapsDirty.Set(renderer, false);
            }

            return view;
        }

        public void Preload(Renderer renderer)
        {
            this.GetRenderTargetView(renderer);
            this.GetShaderResourceView(renderer);
        }

        public void InvalidateMipMaps(Renderer renderer)
        {
            _mipMapsDirty.Set(renderer, true);
        }
    }
}
