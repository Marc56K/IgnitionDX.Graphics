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

namespace IgnitionDX.Graphics
{
    public class WindowRenderTarget : RenderTarget
    {
        private IntPtr _hWnd;
        private int _samplesPerPixel;
        private WindowColorBuffer _windowColorBuffer;

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
                    if (_windowColorBuffer != null)
                    {
                        _windowColorBuffer.HWnd = value;
                    }
                    _hWnd = value;
                }
            }
        }

        public WindowRenderTarget(IntPtr hWnd, int samplesPerPixel, bool createDepthStencilBuffer)
        {
            _hWnd = hWnd;
            _samplesPerPixel = samplesPerPixel;
            _windowColorBuffer = new WindowColorBuffer(_hWnd, _samplesPerPixel);
            this.ColorBuffer.Add(_windowColorBuffer);

            if (createDepthStencilBuffer)
            {
                this.DepthStencilBuffer = new DepthStencilBuffer(_windowColorBuffer.Width, _windowColorBuffer.Height, samplesPerPixel, false);
            }
        }

        public override void Bind(Renderer renderer)
        {
            UpdateBufferSize(renderer);
            base.Bind(renderer);
        }

        public void Present(Renderer renderer, bool waitForVSync)
        {
            _windowColorBuffer.Present(renderer, waitForVSync);
        }

        public void UpdateBufferSize(Renderer renderer)
        {
            _windowColorBuffer.UpdateBufferSize(renderer);

            if (this.DepthStencilBuffer != null)
            {
                if (_windowColorBuffer.Width != DepthStencilBuffer.Width || _windowColorBuffer.Height != DepthStencilBuffer.Height)
                {
                    this.DepthStencilBuffer = new DepthStencilBuffer(_windowColorBuffer.Width, _windowColorBuffer.Height, _samplesPerPixel, false);
                }
            }
        }
    }
}
