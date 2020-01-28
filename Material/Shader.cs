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
using System.IO;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;
using IgnitionDX.Utilities;

namespace IgnitionDX.Graphics
{
    public abstract class Shader
    {
        public class ConstantBufferInfo
        {
            public InputBindingDescription BindingDescription { get; set; }
            public Dictionary<string, ShaderVariableDescription> VariableDescriptions { get; private set; }
            public ConstantBufferInfo()
            {
                VariableDescriptions = new Dictionary<string, ShaderVariableDescription>();
            }
        }

        protected ShaderBytecode _shaderByteCode;
        protected Dictionary<string, ConstantBufferInfo> _constantBuffersInfos = new Dictionary<string, ConstantBufferInfo>();
        protected Dictionary<string, InputBindingDescription> _shaderResourceInfos = new Dictionary<string, InputBindingDescription>();
        protected Dictionary<string, InputBindingDescription> _samplerStateInfos = new Dictionary<string, InputBindingDescription>();

        public enum Profile
        {
            cs_4_0, cs_4_1, cs_5_0, ds_5_0, fx_2_0, fx_4_0, fx_4_1, fx_5_0, gs_4_0,
            gs_4_1, gs_5_0, hs_5_0, ps_2_0, ps_2_a, ps_2_b, ps_2_sw, ps_3_0, ps_3_sw,
            ps_4_0, ps_4_0_level_9_1, ps_4_0_level_9_3, ps_4_0_level_9_0, ps_4_1,
            ps_5_0, tx_1_0, vs_1_1, vs_2_0, vs_2_a, vs_2_sw, vs_3_0, vs_3_sw, vs_4_0,
            vs_4_0_level_9_1, vs_4_0_level_9_3, vs_4_0_level_9_0, vs_4_1, vs_5_0
        }

        public Shader(string shaderSourceFile, Profile profile)
        {
            using (var stream = new System.IO.StreamReader(shaderSourceFile))
            {
                Compile(shaderSourceFile, stream, profile);
            }
        }

        public Shader(string name, Stream shaderSource, Profile profile)
        {
            using (var stream = new System.IO.StreamReader(shaderSource))
            {
                Compile(name, stream, profile);
            }
        }

        protected void Compile(string name, StreamReader shaderSource, Profile profile)
        {
            Logger.LogInfo(this, "Compiling " + name + " with profile " + profile + ".");

            _shaderByteCode = ShaderBytecode.Compile(shaderSource.ReadToEnd(), "main", profile.ToString(), ShaderFlags.None, EffectFlags.None, null, null, name);

            using (ShaderReflection sr = new ShaderReflection(_shaderByteCode))
            {
                for (int i = 0; i < sr.Description.BoundResources; i++)
                {
                    InputBindingDescription desc = sr.GetResourceBindingDescription(i);
                    switch (desc.Type)
                    {
                        case ShaderInputType.ConstantBuffer:
                            {
                                ConstantBufferInfo info = new ConstantBufferInfo();
                                info.BindingDescription = desc;
                                var buffer = sr.GetConstantBuffer(desc.Name);
                                for (int v = 0; v < buffer.Description.VariableCount; v++)
                                {
                                    var variable = buffer.GetVariable(v);
                                    info.VariableDescriptions.Add(variable.Description.Name, variable.Description);
                                }
                                _constantBuffersInfos.Add(desc.Name, info);
                            }
                            break;
                        case ShaderInputType.Texture:
                            _shaderResourceInfos.Add(desc.Name, desc);
                            break;
                        case ShaderInputType.Sampler:
                            _samplerStateInfos.Add(desc.Name, desc);
                            break;
                    }
                }
            }
        }

        public ShaderVariableDescription? GetConstantBufferParamDescription(string bufferName, string bufferParamName)
        {
            if (!_constantBuffersInfos.ContainsKey(bufferName))
            {
                return null;
            }

            var bufferInfo = _constantBuffersInfos[bufferName];

            if (!bufferInfo.VariableDescriptions.ContainsKey(bufferParamName))
            {
                return null;
            }

            return bufferInfo.VariableDescriptions[bufferParamName];
        }

        public abstract void Preload(Renderer renderer);

        public abstract void BindShader(Renderer renderer);

        public abstract void BindConstantBuffer(Renderer renderer, string name, SharpDX.Direct3D11.Buffer buffer);

        public abstract void BindShaderResource(Renderer renderer, string name, ShaderResourceView resource);

        public abstract void BindSamplerState(Renderer renderer, string name, SamplerState state);

        public abstract void UnbindShader(Renderer renderer);
    }
}
