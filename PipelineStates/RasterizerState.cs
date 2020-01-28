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

namespace IgnitionDX.Graphics
{
    public class RasterizerState
    {
        private RendererValue<RasterizerState> _lastRasterizerState = new RendererValue<RasterizerState>(null);
        private RendererValue<SharpDX.Direct3D11.RasterizerState> _rasterizerState = new RendererValue<SharpDX.Direct3D11.RasterizerState>(null);
        private SharpDX.Direct3D11.RasterizerStateDescription _description;

        public RasterizerState(SharpDX.Direct3D11.RasterizerStateDescription desc)
        {
            _description = desc;
        }

        public void Preload(Renderer renderer)
        {
            if (_rasterizerState.Get(renderer) == null)
            {
                _rasterizerState.Set(renderer, new SharpDX.Direct3D11.RasterizerState(renderer.Device, _description));
            }
        }

        public void Bind(Renderer renderer)
        {
            if (renderer.ActiveRasterizerState != _lastRasterizerState.Get(renderer))
            {
                _lastRasterizerState.Set(renderer, renderer.ActiveRasterizerState);
            }

            renderer.ActiveRasterizerState = this;
            
            this.Preload(renderer);
            renderer.DeviceContext.Rasterizer.State = _rasterizerState.Get(renderer);
        }

        public void Unbind(Renderer renderer)
        {
            var lastRasterizerState = _lastRasterizerState.Get(renderer);
            if (lastRasterizerState != null)
            {
                renderer.ActiveRasterizerState = lastRasterizerState;
                lastRasterizerState.Bind(renderer);
            }
            else
            {
                renderer.ActiveRasterizerState = null;
                renderer.DeviceContext.Rasterizer.State = null;
            }
        }

        #region Factory

        public static RasterizerState CreateDefaultRasterizerState()
        {
            return CreateRasterizerState(SharpDX.Direct3D11.CullMode.Back);
        }

        public static RasterizerState CreateRasterizerState(SharpDX.Direct3D11.CullMode cullMode)
        {
            return CreateRasterizerState(cullMode, 0, false);
        }

        public static RasterizerState CreateRasterizerState(SharpDX.Direct3D11.CullMode cullMode, float slopeScaledDepthBias, bool wireFrame)
        {
            return new RasterizerState(new SharpDX.Direct3D11.RasterizerStateDescription()
            {
                IsAntialiasedLineEnabled = false,
                CullMode = cullMode,
                DepthBias = 0,
                DepthBiasClamp = 0,
                IsDepthClipEnabled = true,
                FillMode = wireFrame ? SharpDX.Direct3D11.FillMode.Wireframe : SharpDX.Direct3D11.FillMode.Solid,
                IsFrontCounterClockwise = true,
                IsMultisampleEnabled = true,
                IsScissorEnabled = false,
                SlopeScaledDepthBias = slopeScaledDepthBias
            });
        }

        #endregion
    }
}
