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

using System.Collections.Generic;
using SharpDX.Direct3D11;
using SharpDX.Mathematics.Interop;

namespace IgnitionDX.Graphics
{
    public class RenderTarget : IRenderTarget
    {
        private RendererValue<IRenderTarget> _lastRenderTarget = new RendererValue<IRenderTarget>(null);
        private RendererValue<RenderTargetView[]> _renderTargetViews = new RendererValue<RenderTargetView[]>(null);
        private RendererValue<RawViewportF[]> _viewports = new RendererValue<RawViewportF[]>(null);

        public List<IColorBuffer> ColorBuffer { get; private set; }
        public DepthStencilBuffer DepthStencilBuffer { get; set; }

        public RasterizerState RasterizerState { get; set; }
        public DepthStencilState DepthStencilState { get; set; }
        public BlendState BlendState { get; set; }

        public RenderTarget()
        {
            ColorBuffer = new List<IColorBuffer>();
        }

        public virtual void Bind(Renderer renderer)
        {
            if (renderer.ActiveRenderTarget != _lastRenderTarget.Get(renderer))
            {
                _lastRenderTarget.Set(renderer, renderer.ActiveRenderTarget);
            }

            renderer.ActiveRenderTarget = this;

            RenderTargetView[] rtViews = _renderTargetViews.Get(renderer);
            if (rtViews == null || rtViews.Length != ColorBuffer.Count)
            {
                rtViews = new RenderTargetView[ColorBuffer.Count];
                _renderTargetViews.Set(renderer, rtViews);
            }

            RawViewportF[] viewports = _viewports.Get(renderer);
            if (viewports == null || viewports.Length != ColorBuffer.Count)
            {
                viewports = new RawViewportF[ColorBuffer.Count];
                _viewports.Set(renderer, viewports);
            }

            for (int i = 0; i < rtViews.Length; i++)
            {
                rtViews[i] = ColorBuffer[i].GetRenderTargetView(renderer);
                viewports[i] = new RawViewportF { X = 0, Y = 0, Width = ColorBuffer[i].Width, Height = ColorBuffer[i].Height, MinDepth = 0, MaxDepth = 1 };

                if (ColorBuffer[i] is TextureColorBuffer)
                {
                    (ColorBuffer[i] as TextureColorBuffer).InvalidateMipMaps(renderer);
                }
            }

            if (DepthStencilBuffer == null)
                renderer.DeviceContext.OutputMerger.SetTargets(rtViews.Length > 0 ? rtViews : null);
            else
                renderer.DeviceContext.OutputMerger.SetTargets(DepthStencilBuffer.GetDepthStencilView(renderer), rtViews.Length > 0 ? rtViews : null);

            renderer.DeviceContext.Rasterizer.SetViewports(viewports, viewports.Length);

            if (this.RasterizerState != null)
                this.RasterizerState.Bind(renderer);

            if (this.DepthStencilState != null)
                this.DepthStencilState.Bind(renderer);

            if (this.BlendState != null)
                this.BlendState.Bind(renderer);
        }


        public void Unbind(Renderer renderer)
        {
            if (this.RasterizerState != null)
                this.RasterizerState.Unbind(renderer);

            if (this.DepthStencilState != null)
                this.DepthStencilState.Unbind(renderer);

            if (this.BlendState != null)
                this.BlendState.Unbind(renderer);
            
            var lastRenderTarget = _lastRenderTarget.Get(renderer);
            if (lastRenderTarget != null)
            {
                renderer.ActiveRenderTarget = lastRenderTarget;
                lastRenderTarget.Bind(renderer);
            }
            else
            {
                renderer.ActiveRenderTarget = null;
                renderer.DeviceContext.OutputMerger.SetTargets((RenderTargetView)null);
            }
        }
    }
}
