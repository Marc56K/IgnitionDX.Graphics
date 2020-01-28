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

using SharpDX.Direct3D11;

namespace IgnitionDX.Graphics
{
    public class DepthStencilState
    {
        private RendererValue<DepthStencilState> _lastDepthStencilState = new RendererValue<DepthStencilState>(null);
        private RendererValue<SharpDX.Direct3D11.DepthStencilState> _depthStencilState = new RendererValue<SharpDX.Direct3D11.DepthStencilState>(null);
        private DepthStencilStateDescription _description;

        public DepthStencilState(DepthStencilStateDescription desc)
        {
            _description = desc;
        }

        public void Preload(Renderer renderer)
        {
            if (_depthStencilState.Get(renderer) == null)
            {
                _depthStencilState.Set(renderer, new SharpDX.Direct3D11.DepthStencilState(renderer.Device, _description));
            }
        }

        public void Bind(Renderer renderer)
        {
            if (renderer.ActiveDepthStencilState != _lastDepthStencilState.Get(renderer))
            {
                _lastDepthStencilState.Set(renderer, renderer.ActiveDepthStencilState);
            }

            renderer.ActiveDepthStencilState = this;

            this.Preload(renderer);
            renderer.DeviceContext.OutputMerger.DepthStencilState = _depthStencilState.Get(renderer);
        }

        public void Unbind(Renderer renderer)
        {
            var lastDepthStencilState = _lastDepthStencilState.Get(renderer);
            if (lastDepthStencilState != null)
            {
                renderer.ActiveDepthStencilState = lastDepthStencilState;
                lastDepthStencilState.Bind(renderer);
            }
            else
            {
                renderer.ActiveDepthStencilState = null;
                renderer.DeviceContext.OutputMerger.DepthStencilState = null;
            }
        }

        #region Factory

        public static DepthStencilState CreateDefaultDepthStencilState()
        {
            return DepthStencilState.CreateDepthStencilState(true, SharpDX.Direct3D11.Comparison.LessEqual, false);
        }

        public static DepthStencilState CreateDepthStencilState(bool enableDepthTest, Comparison depthComparison, bool readOnlyDepthBuffer)
        {
            DepthStencilStateDescription dsDesc = new DepthStencilStateDescription();
            dsDesc.IsDepthEnabled = enableDepthTest;
            dsDesc.DepthWriteMask = readOnlyDepthBuffer ? DepthWriteMask.Zero : DepthWriteMask.All;
            dsDesc.DepthComparison = depthComparison;
            dsDesc.IsStencilEnabled = false;

            return new DepthStencilState(dsDesc);
        }

        #endregion
    }
}
