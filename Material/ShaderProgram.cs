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
    public class ShaderProgram
    {
        private VertexShader _vertexShader;
        private Shader[] _shaders;

        public ShaderProgram(params Shader[] shaders)
        {
            _shaders = shaders;

            // search the vertex shader for faster creation of input layouts
            for (int i = 0; i < _shaders.Length; i++)
            {
                if (_shaders[i] is VertexShader)
                {
                    _vertexShader = _shaders[i] as VertexShader;
                    break;
                }
            }
        }

        public InputLayout CreateInputLayout(Renderer renderer, InputElement[] inputElementDescriptions)
        {
            if (_vertexShader != null)
            {
                return _vertexShader.CreateInputLayout(renderer, inputElementDescriptions);
            }

            return null;
        }

        public SharpDX.D3DCompiler.ShaderVariableDescription? GetConstantBufferParamDescription(string bufferName, string bufferParamName)
        {
            for (int i = 0; i < _shaders.Length; i++)
            {
                var desc = _shaders[i].GetConstantBufferParamDescription(bufferName, bufferParamName);
                if (desc != null)
                    return desc;
            }

            return null;
        }

        public void Preload(Renderer renderer)
        {
            for (int i = 0; i < _shaders.Length; i++)
            {
                _shaders[i].Preload(renderer);
            }
        }

        public void Bind(Renderer renderer)
        {
            if (renderer.ActiveShaderProgram == this)
            {
                return;
            }

            renderer.ActiveShaderProgram = this;

            for (int i = 0; i < _shaders.Length; i++)
            {
                _shaders[i].BindShader(renderer);
            }
        }

        public void Unbind(Renderer renderer)
        {
            if (renderer.ActiveShaderProgram == this)
            {
                for (int i = 0; i < _shaders.Length; i++)
                {
                    _shaders[i].UnbindShader(renderer);
                }

                renderer.ActiveShaderProgram = null;
            }
        }

        public void BindConstantBuffer(Renderer renderer, string name, SharpDX.Direct3D11.Buffer buffer)
        {
            for (int i = 0; i < _shaders.Length; i++)
            {
                _shaders[i].BindConstantBuffer(renderer, name, buffer);
            }
        }

        public void BindShaderResource(Renderer renderer, string name, ShaderResourceView resource)
        {
            for (int i = 0; i < _shaders.Length; i++)
            {
                _shaders[i].BindShaderResource(renderer, name, resource);
            }
        }

        public void BindSamplerState(Renderer renderer, string name, SamplerState state)
        {
            for (int i = 0; i < _shaders.Length; i++)
            {
                _shaders[i].BindSamplerState(renderer, name, state);
            }
        }
    }
}