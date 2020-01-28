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
using System.Drawing;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using IgnitionDX.Math;
using IgnitionDX.Utilities;
using SharpDX.Mathematics.Interop;

namespace IgnitionDX.Graphics
{
    public class WindowColorBuffer : IColorBuffer
    {
        private int _samplesPerPixel;
        private IntPtr _hWnd;
        private int _width;
        private int _height;

        private RendererValue<RenderTargetView> _renderTargetView = new RendererValue<RenderTargetView>(null);
        private RendererValue<SwapChain> _swapChain = new RendererValue<SwapChain>(null);

        public int Width
        {
            get { return _width; }
        }

        public int Height
        {
            get { return _height; }
        }

        public IntPtr HWnd
        {
            get
            {
                return _hWnd;
            }
            set
            {
                if (value != _hWnd)
                {
                    _renderTargetView.ReleaseAll();
                    _swapChain.ReleaseAll();
                    _hWnd = value;

                    Rectangle rect = new Rectangle();
                    Win32Helper.GetWindowRect(_hWnd, out rect);
                    _width = System.Math.Max(1, rect.Width - rect.Left);
                    _height = System.Math.Max(1, rect.Height - rect.Top);
                }
            }
        }

        public WindowColorBuffer(IntPtr hWnd, int samplesPerPixel)
        {
            _samplesPerPixel = samplesPerPixel;
            this.HWnd = hWnd;
        }

        public SharpDX.Direct3D11.RenderTargetView GetRenderTargetView(Renderer renderer)
        {
            SwapChain swapChain = _swapChain.Get(renderer);
            if (swapChain == null)
            {
                Logger.LogInfo(this, "Creating SwapChain");

                SwapChainDescription swapChainDesc = new SwapChainDescription();
                swapChainDesc.BufferCount = 1;
                swapChainDesc.Usage = Usage.RenderTargetOutput;
                swapChainDesc.OutputHandle = _hWnd;
                swapChainDesc.IsWindowed = true;
                swapChainDesc.ModeDescription = new ModeDescription(0, 0, new Rational(60, 1), Format.R8G8B8A8_UNorm);
                swapChainDesc.SampleDescription = new SharpDX.DXGI.SampleDescription(_samplesPerPixel, 0);
                swapChainDesc.Flags = SwapChainFlags.AllowModeSwitch;
                swapChainDesc.SwapEffect = SwapEffect.Discard;

                swapChain = new SwapChain(renderer.Factory, renderer.Device, swapChainDesc);
                _swapChain.Set(renderer, swapChain);

                using (var resource = SharpDX.Direct3D11.Resource.FromSwapChain<SharpDX.Direct3D11.Texture2D>(swapChain, 0))
                {
                    _renderTargetView.Set(renderer, new RenderTargetView(renderer.Device, resource), Width * Height * 4);
                }
            }
            else
            {
                UpdateBufferSize(renderer);
            }

            return _renderTargetView.Get(renderer);
        }

        public void UpdateBufferSize(Renderer renderer)
        {
            Rectangle rect = new Rectangle();
            Win32Helper.GetWindowRect(_hWnd, out rect);
            int newWidth = System.Math.Max(1, rect.Width - rect.Left);
            int newHeight = System.Math.Max(1, rect.Height - rect.Top);
            
            SwapChain swapChain = _swapChain.Get(renderer);
            if (swapChain == null)
            {
                _width = newWidth;
                _height = newHeight;
            }
            else if (newWidth != this.Width || newHeight != this.Height)
            {
                _width = newWidth;
                _height = newHeight;

                _renderTargetView.Release(renderer);

                Logger.LogInfo(this, string.Format("Resizing SwapChain to {0}x{1}", _width, _height));

                swapChain.ResizeBuffers(1, newWidth, newHeight, Format.R8G8B8A8_UNorm, SwapChainFlags.AllowModeSwitch);
                using (var resource = SharpDX.Direct3D11.Resource.FromSwapChain<SharpDX.Direct3D11.Texture2D>(swapChain, 0))
                {
                    _renderTargetView.Set(renderer, new RenderTargetView(renderer.Device, resource), Width * Height * 4);
                }
            }
        }

        public void Clear(Renderer renderer, Color4 color)
        {
            RenderTargetView rtView = GetRenderTargetView(renderer);
            renderer.DeviceContext.ClearRenderTargetView(rtView, new RawColor4(color.R, color.G, color.B, color.A));
        }

        public void Present(Renderer renderer, bool waitForVSync)
        {
            SwapChain swapChain = _swapChain.Get(renderer);
            if (swapChain != null)
            {
                swapChain.Present(waitForVSync ? 1 : 0, 0);
            }
        }
    }
}
