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

using System;
using SharpDX.Direct3D11;
using IgnitionDX.Utilities;

namespace IgnitionDX.Graphics
{
    public class DepthStencilBuffer : ITexture
    {
        private int _samplesPerPixel;
        private bool _useAsShaderResource;
        private Texture2DDescription _textureDesc;

        private RendererValue<SharpDX.Direct3D11.Texture2D> _texture = new RendererValue<SharpDX.Direct3D11.Texture2D>(null);
        private RendererValue<DepthStencilView> _depthStencilView = new RendererValue<DepthStencilView>(null);
        private RendererValue<ShaderResourceView> _shaderResourceView = new RendererValue<ShaderResourceView>(null);

        public int Width
        {
            get { return _textureDesc.Width; }
        }

        public int Height
        {
            get { return _textureDesc.Height; }
        }

        public DepthStencilBuffer(int width, int height, int samplesPerPixel, bool useAsShaderResource)
        {
            _samplesPerPixel = samplesPerPixel;
            _useAsShaderResource = useAsShaderResource;

            _textureDesc = new Texture2DDescription();
            _textureDesc.Height = height;
            _textureDesc.Width = width;
            _textureDesc.Usage = ResourceUsage.Default;
            _textureDesc.BindFlags = _useAsShaderResource ? BindFlags.ShaderResource | BindFlags.DepthStencil : BindFlags.DepthStencil;
            _textureDesc.Format = _useAsShaderResource ? SharpDX.DXGI.Format.R24G8_Typeless : SharpDX.DXGI.Format.D24_UNorm_S8_UInt;
            _textureDesc.CpuAccessFlags = CpuAccessFlags.None;
            _textureDesc.MipLevels = 1;
            _textureDesc.ArraySize = 1;
            _textureDesc.SampleDescription = new SharpDX.DXGI.SampleDescription(samplesPerPixel, 0);
            _textureDesc.OptionFlags = ResourceOptionFlags.None;
        }

        public void Clear(Renderer renderer, float? depth, byte? stencil)
        {
            if (depth.HasValue && stencil.HasValue)
                renderer.DeviceContext.ClearDepthStencilView(_depthStencilView.Get(renderer), DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, depth.Value, stencil.Value);
            else if (depth.HasValue)
                renderer.DeviceContext.ClearDepthStencilView(_depthStencilView.Get(renderer), DepthStencilClearFlags.Depth, depth.Value, 0);
            else if (stencil.HasValue)
                renderer.DeviceContext.ClearDepthStencilView(_depthStencilView.Get(renderer), DepthStencilClearFlags.Stencil, 0, stencil.Value);
        }

        public SharpDX.Direct3D11.Texture2D GetTexture(Renderer renderer)
        {
            Logger.LogInfo(this, "Creating depth stencil buffer.");

            SharpDX.Direct3D11.Texture2D tex = _texture.Get(renderer);
            if (tex == null)
            {
                if (_samplesPerPixel > 1 && _useAsShaderResource && renderer.Device.FeatureLevel < SharpDX.Direct3D.FeatureLevel.Level_10_1)
                {
                    Logger.LogError(this, String.Format("Using multisampled depth stencil buffers as shader resources is not supported in DirectX {0}", renderer.Device.FeatureLevel));
                }

                tex = new SharpDX.Direct3D11.Texture2D(renderer.Device, _textureDesc);
                _texture.Set(renderer, tex, Width * Height * 4);
            }

            return tex;
        }

        public DepthStencilView GetDepthStencilView(Renderer renderer)
        {
            DepthStencilView view = _depthStencilView.Get(renderer);
            if (view == null)
            {
                if (_useAsShaderResource)
                {
                    DepthStencilViewDescription dsvDesc = new DepthStencilViewDescription();
                    dsvDesc.Format = SharpDX.DXGI.Format.D24_UNorm_S8_UInt;
                    dsvDesc.Dimension = DepthStencilViewDimension.Texture2D;
                    dsvDesc.Texture2D.MipSlice = 0;

                    view = new DepthStencilView(renderer.Device, GetTexture(renderer), dsvDesc);
                }
                else
                {
                    view = new DepthStencilView(renderer.Device, GetTexture(renderer));
                }
                _depthStencilView.Set(renderer, view);
            }

            return view;
        }

        public ShaderResourceView GetShaderResourceView(Renderer renderer)
        {
            if (!_useAsShaderResource)
            {
                return null;
            }

            ShaderResourceView view = _shaderResourceView.Get(renderer);
            if (view == null)
            {
                ShaderResourceViewDescription srvDesc = new ShaderResourceViewDescription();
                srvDesc.Format = SharpDX.DXGI.Format.R24_UNorm_X8_Typeless;
                srvDesc.Dimension = SharpDX.Direct3D.ShaderResourceViewDimension.Texture2D;
                srvDesc.Texture2D.MipLevels = 1;

                view = new ShaderResourceView(renderer.Device, GetTexture(renderer), srvDesc);
                _shaderResourceView.Set(renderer, view);
            }

            return view;
        }

        public void Preload(Renderer renderer)
        {
            this.GetDepthStencilView(renderer);
            this.GetShaderResourceView(renderer);
        }
    }
}
