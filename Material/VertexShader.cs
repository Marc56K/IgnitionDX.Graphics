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
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;
namespace IgnitionDX.Graphics
{
    public class VertexShader : Shader
    {
        private RendererValue<SharpDX.Direct3D11.VertexShader> _vertexShader = new RendererValue<SharpDX.Direct3D11.VertexShader>(null);

        public VertexShader(string shaderSourceFile, Profile profile)
            : base(shaderSourceFile, profile)
        {
        }

        public VertexShader(string name, Stream shaderSource, Profile profile)
            : base(name, shaderSource, profile)
        {
        }

        public InputLayout CreateInputLayout(Renderer renderer, InputElement[] inputElementDescriptions)
        {
            ShaderSignature signature = ShaderSignature.GetInputSignature(_shaderByteCode);
            return new InputLayout(renderer.Device, signature, inputElementDescriptions);
        }

        public override void Preload(Renderer renderer)
        {
            if (_vertexShader.Get(renderer) == null)
            {
                _vertexShader.Set(renderer, new SharpDX.Direct3D11.VertexShader(renderer.Device, _shaderByteCode), _shaderByteCode.Data.LongLength);
            }
        }

        public override void BindShader(Renderer renderer)
        {
            if (renderer.ActiveVertexShader == this)
            {
                return;
            }

            renderer.ActiveVertexShader = this;

            this.Preload(renderer);

            renderer.DeviceContext.VertexShader.Set(_vertexShader.Get(renderer));
        }

        public override void UnbindShader(Renderer renderer)
        {
            if (renderer.ActiveVertexShader == this)
            {
                renderer.DeviceContext.VertexShader.Set(null);
                renderer.ActiveVertexShader = null;
            }
        }

        public override void BindConstantBuffer(Renderer renderer, string name, SharpDX.Direct3D11.Buffer buffer)
        {
            if (_constantBuffersInfos.ContainsKey(name))
            {
                renderer.DeviceContext.VertexShader.SetConstantBuffer(_constantBuffersInfos[name].BindingDescription.BindPoint, buffer);
            }
        }

        public override void BindShaderResource(Renderer renderer, string name, ShaderResourceView resource)
        {
            if (_shaderResourceInfos.ContainsKey(name))
            {
                renderer.DeviceContext.VertexShader.SetShaderResource(_shaderResourceInfos[name].BindPoint, resource);
            }
        }

        public override void BindSamplerState(Renderer renderer, string name, SamplerState state)
        {
            if (_samplerStateInfos.ContainsKey(name))
            {
                renderer.DeviceContext.VertexShader.SetSampler(_samplerStateInfos[name].BindPoint, state);
            }
        }
    }
}
