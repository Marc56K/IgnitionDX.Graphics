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

using System;
using System.IO;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;
using System.Reflection;
using IgnitionDX.Math;

namespace IgnitionDX.Graphics
{
    public class GeometryShader : Shader
    {
        private Type _streamOutType = null;
        private RendererValue<SharpDX.Direct3D11.GeometryShader> _geometryShader = new RendererValue<SharpDX.Direct3D11.GeometryShader>(null);

        public GeometryShader(string shaderSourceFile, Profile profile)
            : base(shaderSourceFile, profile)
        {
        }

        public GeometryShader(string name, Stream shaderSource, Profile profile)
            : base(name, shaderSource, profile)
        {
        }

        public GeometryShader(string shaderSourceFile, Profile profile, Type streamOutType)
            : base(shaderSourceFile, profile)
        {
            _streamOutType = streamOutType;
        }

        public GeometryShader(string name, Stream shaderSource, Profile profile, Type streamOutType)
            : base(name, shaderSource, profile)
        {
            _streamOutType = streamOutType;
        }

        public InputLayout CreateInputLayout(Renderer renderer, InputElement[] inputElementDescriptions)
        {
            ShaderSignature signature = ShaderSignature.GetInputSignature(_shaderByteCode);
            return new InputLayout(renderer.Device, signature, inputElementDescriptions);
        }

        public override void Preload(Renderer renderer)
        {
            if (_geometryShader.Get(renderer) == null)
            {
                SharpDX.Direct3D11.GeometryShader shader;

                if (_streamOutType == null)
                {
                    shader = new SharpDX.Direct3D11.GeometryShader(renderer.Device, _shaderByteCode);
                }
                else
                {
                    FieldInfo[] infos = _streamOutType.GetFields(BindingFlags.Public | BindingFlags.Instance);
                    StreamOutputElement[] elements = new StreamOutputElement[infos.Length];
                    int[] strides = new int[1] { 0 };

                    for (int i = 0; i < infos.Length; i++)
                    {
                        FieldInfo info = infos[i];
                        elements[i] = new StreamOutputElement();
                        elements[i].SemanticName = info.Name;

                        if (info.FieldType == typeof(float))
                            elements[i].ComponentCount = 1;
                        else if (info.FieldType == typeof(Vector2))
                            elements[i].ComponentCount = 2;
                        else if (info.FieldType == typeof(Vector3))
                            elements[i].ComponentCount = 3;
                        else if (info.FieldType == typeof(Vector4))
                            elements[i].ComponentCount = 4;
                        else
                            throw new Exception("Unknown element type: " + info.FieldType.ToString());

                        strides[0] += sizeof(float) * elements[i].ComponentCount; // TODO ...
                    }

                    shader = new SharpDX.Direct3D11.GeometryShader(renderer.Device, _shaderByteCode, elements, strides, 0);
                }
                _geometryShader.Set(renderer, shader, _shaderByteCode.Data.LongLength);
            }
        }

        public override void BindShader(Renderer renderer)
        {
            if (renderer.ActiveGeometryShader == this)
            {
                return;
            }

            renderer.ActiveGeometryShader = this;

            this.Preload(renderer);

            renderer.DeviceContext.GeometryShader.Set(_geometryShader.Get(renderer));
        }

        public override void UnbindShader(Renderer renderer)
        {
            if (renderer.ActiveGeometryShader == this)
            {
                renderer.DeviceContext.GeometryShader.Set(null);
                renderer.ActiveGeometryShader = null;
            }
        }

        public override void BindConstantBuffer(Renderer renderer, string name, SharpDX.Direct3D11.Buffer buffer)
        {
            if (_constantBuffersInfos.ContainsKey(name))
            {
                renderer.DeviceContext.GeometryShader.SetConstantBuffer(_constantBuffersInfos[name].BindingDescription.BindPoint, buffer);
            }
        }

        public override void BindShaderResource(Renderer renderer, string name, ShaderResourceView resource)
        {
            if (_shaderResourceInfos.ContainsKey(name))
            {
                renderer.DeviceContext.GeometryShader.SetShaderResource(_shaderResourceInfos[name].BindPoint, resource);
            }
        }

        public override void BindSamplerState(Renderer renderer, string name, SamplerState state)
        {
            if (_samplerStateInfos.ContainsKey(name))
            {
                renderer.DeviceContext.GeometryShader.SetSampler(_samplerStateInfos[name].BindPoint, state);
            }
        }
    }
}
