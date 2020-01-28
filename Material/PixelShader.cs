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

using System.IO;
using SharpDX.Direct3D11;

namespace IgnitionDX.Graphics
{
    public class PixelShader : Shader
    {
        private RendererValue<SharpDX.Direct3D11.PixelShader> _pixelShader = new RendererValue<SharpDX.Direct3D11.PixelShader>(null);

        public PixelShader(string shaderSourceFile, Profile profile)
            : base(shaderSourceFile, profile)
        {
        }

        public PixelShader(string name, Stream shaderSource, Profile profile)
            : base(name, shaderSource, profile)
        {
        }

        public override void Preload(Renderer renderer)
        {
            if (_pixelShader.Get(renderer) == null)
            {
                _pixelShader.Set(renderer, new SharpDX.Direct3D11.PixelShader(renderer.Device, _shaderByteCode), _shaderByteCode.Data.LongLength);
            }
        }

        public override void BindShader(Renderer renderer)
        {
            if (renderer.ActivePixelShader == this)
            {
                return;
            }

            renderer.ActivePixelShader = this;

            this.Preload(renderer);

            renderer.DeviceContext.PixelShader.Set(_pixelShader.Get(renderer));
        }

        public override void UnbindShader(Renderer renderer)
        {
            if (renderer.ActivePixelShader == this)
            {
                renderer.DeviceContext.PixelShader.Set(null);
                renderer.ActivePixelShader = null;
            }
        }

        public override void BindConstantBuffer(Renderer renderer, string name, SharpDX.Direct3D11.Buffer buffer)
        {
            if (_constantBuffersInfos.ContainsKey(name))
            {
                renderer.DeviceContext.PixelShader.SetConstantBuffer(_constantBuffersInfos[name].BindingDescription.BindPoint, buffer);
            }
        }

        public override void BindShaderResource(Renderer renderer, string name, ShaderResourceView resource)
        {
            if (_shaderResourceInfos.ContainsKey(name))
            {
                renderer.DeviceContext.PixelShader.SetShaderResource(_shaderResourceInfos[name].BindPoint, resource);
            }
        }

        public override void BindSamplerState(Renderer renderer, string name, SamplerState state)
        {
            if (_samplerStateInfos.ContainsKey(name))
            {
                renderer.DeviceContext.PixelShader.SetSampler(_samplerStateInfos[name].BindPoint, state);
            }
        }
    }
}
