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
    public class MaterialPass
    {
        private class NamedConstantBuffer
        {
            public string Name { get; set; }
            public IConstantBuffer ConstantBuffer { get; set; }
        }

        private class NamedTexture
        {
            public string Name { get; set; }
            public ITexture Texture { get; set; }
        }

        private class NamedTextureFilter
        {
            public string Name { get; set; }
            public TextureFilter TextureFilter { get; set; }
        }

        private Dictionary<string, NamedConstantBuffer> _constantBuffers = new Dictionary<string, NamedConstantBuffer>();
        private Dictionary<string, NamedTexture> _textures = new Dictionary<string, NamedTexture>();
        private Dictionary<string, NamedTextureFilter> _filters = new Dictionary<string, NamedTextureFilter>();
        private ShaderProgram _program;

        public MaterialPass()
        {
        }

        public void Preload(Renderer renderer)
        {
            foreach (var cBuffer in _constantBuffers.Values)
            {
                cBuffer.ConstantBuffer.Preload(renderer);
            }

            foreach (var tex in _textures.Values)
            {
                tex.Texture.Preload(renderer);
            }

            foreach (var filter in _filters.Values)
            {
                filter.TextureFilter.Preload(renderer);
            }

            if (_program != null)
            {
                _program.Preload(renderer);
            }
        }

        public void Bind(Renderer renderer)
        {
            if (_program != null)
            {
                _program.Bind(renderer);

                foreach (NamedConstantBuffer cBuffer in _constantBuffers.Values)
                    _program.BindConstantBuffer(renderer, cBuffer.Name, cBuffer.ConstantBuffer.GetBuffer(renderer));

                foreach (NamedTexture tex in _textures.Values)
                    _program.BindShaderResource(renderer, tex.Name, tex.Texture.GetShaderResourceView(renderer));

                foreach (NamedTextureFilter filter in _filters.Values)
                    _program.BindSamplerState(renderer, filter.Name, filter.TextureFilter.GetSamplerState(renderer));
            }
        }

        public void Unbind(Renderer renderer)
        {
            if (_program != null)
            {
                _program.Unbind(renderer);

                foreach (NamedConstantBuffer cBuffer in _constantBuffers.Values)
                    _program.BindConstantBuffer(renderer, cBuffer.Name, null);

                foreach (NamedTexture tex in _textures.Values)
                    _program.BindShaderResource(renderer, tex.Name, null);

                foreach (NamedTextureFilter filter in _filters.Values)
                    _program.BindSamplerState(renderer, filter.Name, null);
            }
        }

        public void SetShaders(params Shader[] shaders)
        {
            _program = new ShaderProgram(shaders);
        }

        public void SetConstantBuffer(string bufferName, IConstantBuffer buffer)
        {
            if (_constantBuffers.ContainsKey(bufferName))
                _constantBuffers.Remove(bufferName);

            _constantBuffers.Add(bufferName, new NamedConstantBuffer() { Name = bufferName, ConstantBuffer = buffer });
        }

        public void SetConstantBufferParam<T>(string bufferName, string bufferParamName, T value) where T : struct
        {
            if (_program != null)
            {
                var desc = _program.GetConstantBufferParamDescription(bufferName, bufferParamName);

                if (desc == null)
                {
                    return;
                }

                if (!_constantBuffers.ContainsKey(bufferName))
                {
                    SetConstantBuffer(bufferName, new TypelessConstantBuffer());
                }

                _constantBuffers[bufferName].ConstantBuffer.Update<T>(desc.Value.StartOffset, value);
            }
        }

        public void SetTexture(string textureName, ITexture texture)
        {
            if (_textures.ContainsKey(textureName))
                _textures.Remove(textureName);

            _textures.Add(textureName, new NamedTexture() { Name = textureName, Texture = texture });
        }

        public void SetTextureFilter(string samplerName, TextureFilter filter)
        {
            if (_filters.ContainsKey(samplerName))
                _filters.Remove(samplerName);

            _filters.Add(samplerName, new NamedTextureFilter() { Name = samplerName, TextureFilter = filter });
        }
    }
}
