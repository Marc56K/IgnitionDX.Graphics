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

using SharpDX.Direct3D;
using SharpDX.Direct3D11;

namespace IgnitionDX.Graphics
{
    public class Renderer : SharpDX.Component
    {
        public SharpDX.Direct3D11.Device Device
        {
            get;
            private set;
        }

        public DeviceContext DeviceContext
        {
            get;
            private set;
        }

        public SharpDX.DXGI.Factory Factory
        {
            get;
            private set;
        }

#if DEBUG
        public DeviceDebug DeviceDebug
        {
            get;
            private set;
        }
#endif

        public Renderer()
        {
            SharpDX.Configuration.EnableReleaseOnFinalizer = true;

            this.Factory = ToDispose(new SharpDX.DXGI.Factory2());
            using (var adapter = this.Factory.Adapters[0])
            {
#if DEBUG
                try
                {
                    this.Device = ToDispose(new SharpDX.Direct3D11.Device(adapter, DeviceCreationFlags.Debug, FeatureLevel.Level_11_0, FeatureLevel.Level_10_1, FeatureLevel.Level_10_0, FeatureLevel.Level_9_3));
                    this.DeviceDebug = ToDispose(new DeviceDebug(this.Device));
                }
                catch (SharpDX.SharpDXException ex)
                {
                    if (ex.ResultCode.Code == -2005270483)
                    {
                        IgnitionDX.Utilities.Logger.LogWarning(this, "The D3D Debug-Runtime not installed. Creating device without debug flag.");
                        this.Device = ToDispose(new SharpDX.Direct3D11.Device(adapter, DeviceCreationFlags.None, FeatureLevel.Level_11_0, FeatureLevel.Level_10_1, FeatureLevel.Level_10_0, FeatureLevel.Level_9_3));
                    }
                }
#else
                this.Device = ToDispose(new SharpDX.Direct3D11.Device(adapter, DeviceCreationFlags.None, FeatureLevel.Level_11_0, FeatureLevel.Level_10_1, FeatureLevel.Level_10_0, FeatureLevel.Level_9_3));
#endif
                this.DeviceContext = ToDispose(this.Device.ImmediateContext);
            }
        }

        public ShaderProgram ActiveShaderProgram
        {
            get;
            internal set;
        }

        public VertexShader ActiveVertexShader
        {
            get;
            internal set;
        }

        public GeometryShader ActiveGeometryShader
        {
            get;
            internal set;
        }

        public PixelShader ActivePixelShader
        {
            get;
            internal set;
        }

        public IRenderTarget ActiveRenderTarget
        {
            get;
            internal set;
        }

        public BlendState ActiveBlendState
        {
            get;
            internal set;
        }

        public RasterizerState ActiveRasterizerState
        {
            get;
            internal set;
        }

        public DepthStencilState ActiveDepthStencilState
        {
            get;
            internal set;
        }
    }
}
