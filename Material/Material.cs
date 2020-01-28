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

namespace IgnitionDX.Graphics
{
    public class Material
    {
        protected Dictionary<string, MaterialPass> _passes = new Dictionary<string, MaterialPass>();

        public string MaterialClass { get; set; }

        public Material()
        {
            MaterialClass = string.Empty;
        }

        public void SetShaders(string materialPass, params Shader[] shaders)
        {
            GetOrCreatePass(materialPass).SetShaders(shaders);
        }

        public void SetTexture(string materialPass, string textureName, ITexture texture)
        {
            GetOrCreatePass(materialPass).SetTexture(textureName, texture);
        }

        public void SetTextureFilter(string materialPass, string samplerName, TextureFilter filter)
        {
            GetOrCreatePass(materialPass).SetTextureFilter(samplerName, filter);
        }

        public void SetConstantBuffer(string materialPass, string bufferName, IConstantBuffer cBuffer)
        {
            GetOrCreatePass(materialPass).SetConstantBuffer(bufferName, cBuffer);
        }

        public void SetConstantBufferParam<T>(string materialPass, string bufferName, string bufferParamName, T value) where T : struct
        {
            GetOrCreatePass(materialPass).SetConstantBufferParam<T>(bufferName, bufferParamName, value);
        }

        public void Preload(Renderer renderer)
        {
            foreach (var pass in _passes.Values)
            {
                pass.Preload(renderer);
            }
        }

        public bool Bind(Renderer renderer, string materialPass)
        {
            if (_passes.ContainsKey(materialPass))
            {
                _passes[materialPass].Bind(renderer);
                return true;
            }
            return false;
        }

        public void Unbind(Renderer renderer, string materialPass)
        {
            if (_passes.ContainsKey(materialPass))
            {
                _passes[materialPass].Unbind(renderer);
            }
        }

        protected virtual MaterialPass GetOrCreatePass(string name)
        {
            if (!_passes.ContainsKey(name))
            {
                MaterialPass pass = new MaterialPass();
                _passes.Add(name, pass);
                return pass;
            }

            return _passes[name];
        }
    }
}
