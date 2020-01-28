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
    public class BlendState
    {
        private RendererValue<BlendState> _lastBlendState = new RendererValue<BlendState>(null);
        private RendererValue<SharpDX.Direct3D11.BlendState> _blendState = new RendererValue<SharpDX.Direct3D11.BlendState>(null);
        private SharpDX.Direct3D11.BlendStateDescription _description;

        public BlendState(SharpDX.Direct3D11.BlendStateDescription desc)
        {
            _description = desc;
        }

        public void Preload(Renderer renderer)
        {
            if (_blendState.Get(renderer) == null)
            {
                _blendState.Set(renderer, new SharpDX.Direct3D11.BlendState(renderer.Device, _description));
            }
        }

        public void Bind(Renderer renderer)
        {
            if (renderer.ActiveBlendState != _lastBlendState.Get(renderer))
            {
                _lastBlendState.Set(renderer, renderer.ActiveBlendState);
            }

            renderer.ActiveBlendState = this;
            
            this.Preload(renderer);
            renderer.DeviceContext.OutputMerger.BlendState = _blendState.Get(renderer);
        }

        public void Unbind(Renderer renderer)
        {
            var lastBlendState = _lastBlendState.Get(renderer);
            if (lastBlendState != null)
            {
                renderer.ActiveBlendState = lastBlendState;
                lastBlendState.Bind(renderer);
            }
            else
            {
                renderer.ActiveBlendState = null;
                renderer.DeviceContext.OutputMerger.BlendState = null;
            }
        }

        #region Factory

        public static BlendState CreateDefaultBlendState()
        {
            SharpDX.Direct3D11.BlendStateDescription desc = new SharpDX.Direct3D11.BlendStateDescription();
            for (int i = 0; i < desc.RenderTarget.Length; i++)
            {
                desc.RenderTarget[i].IsBlendEnabled = true;
                desc.RenderTarget[i].SourceBlend = SharpDX.Direct3D11.BlendOption.SourceAlpha;
                desc.RenderTarget[i].DestinationBlend = SharpDX.Direct3D11.BlendOption.InverseSourceAlpha;
                desc.RenderTarget[i].BlendOperation = SharpDX.Direct3D11.BlendOperation.Add;
                desc.RenderTarget[i].SourceAlphaBlend = SharpDX.Direct3D11.BlendOption.InverseDestinationAlpha;
                desc.RenderTarget[i].DestinationAlphaBlend = SharpDX.Direct3D11.BlendOption.One;
                desc.RenderTarget[i].AlphaBlendOperation = SharpDX.Direct3D11.BlendOperation.Add;
                desc.RenderTarget[i].RenderTargetWriteMask = SharpDX.Direct3D11.ColorWriteMaskFlags.All;
            }

            return new BlendState(desc);
        }

        public static BlendState CreateAdditiveBlendState()
        {
            SharpDX.Direct3D11.BlendStateDescription desc = new SharpDX.Direct3D11.BlendStateDescription();
            for (int i = 0; i < desc.RenderTarget.Length; i++)
            {
                desc.RenderTarget[i].IsBlendEnabled = true;
                desc.RenderTarget[i].SourceBlend = SharpDX.Direct3D11.BlendOption.SourceAlpha;
                desc.RenderTarget[i].DestinationBlend = SharpDX.Direct3D11.BlendOption.One;
                desc.RenderTarget[i].BlendOperation = SharpDX.Direct3D11.BlendOperation.Add;
                desc.RenderTarget[i].SourceAlphaBlend = SharpDX.Direct3D11.BlendOption.One;
                desc.RenderTarget[i].DestinationAlphaBlend = SharpDX.Direct3D11.BlendOption.One;
                desc.RenderTarget[i].AlphaBlendOperation = SharpDX.Direct3D11.BlendOperation.Add;
                desc.RenderTarget[i].RenderTargetWriteMask = SharpDX.Direct3D11.ColorWriteMaskFlags.All;
            }

            return new BlendState(desc);
        }

        public static BlendState CreateSourceColorBlendState(bool alphaToCoverageEnable)
        {
            SharpDX.Direct3D11.BlendStateDescription desc = new SharpDX.Direct3D11.BlendStateDescription();
            desc.AlphaToCoverageEnable = alphaToCoverageEnable;
            for (int i = 0; i < desc.RenderTarget.Length; i++)
            {
                desc.RenderTarget[i].IsBlendEnabled = true;
                desc.RenderTarget[i].SourceBlend = SharpDX.Direct3D11.BlendOption.One;
                desc.RenderTarget[i].DestinationBlend = SharpDX.Direct3D11.BlendOption.Zero;
                desc.RenderTarget[i].BlendOperation = SharpDX.Direct3D11.BlendOperation.Add;
                desc.RenderTarget[i].SourceAlphaBlend = SharpDX.Direct3D11.BlendOption.One;
                desc.RenderTarget[i].DestinationAlphaBlend = SharpDX.Direct3D11.BlendOption.Zero;
                desc.RenderTarget[i].AlphaBlendOperation = SharpDX.Direct3D11.BlendOperation.Add;
                desc.RenderTarget[i].RenderTargetWriteMask = SharpDX.Direct3D11.ColorWriteMaskFlags.All;
            }

            return new BlendState(desc);
        }

        #endregion
    }
}