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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX.Direct3D11;
using System.Runtime.InteropServices;

namespace IgnitionDX.Graphics
{
    public class StreamOutputBuffer : IRenderTarget
    {
        private RendererValue<IRenderTarget> _lastRenderTarget = new RendererValue<IRenderTarget>(null);
        private int _size;
        private RendererValue<Query> _query = new RendererValue<Query>(null);
        private RendererValue<SharpDX.Direct3D11.Buffer> _gpuBuffer = new RendererValue<SharpDX.Direct3D11.Buffer>(null);
        private RendererValue<SharpDX.Direct3D11.Buffer> _cpuBuffer = new RendererValue<SharpDX.Direct3D11.Buffer>(null);

        public long NumPrimitivesWritten { get; private set; }
        public long PrimitivesStorageNeeded { get; private set; }

        public StreamOutputBuffer(int size)
        {
            _size = size;
        }
        
        public void Bind(Renderer renderer)
        {
            if (renderer.ActiveRenderTarget != _lastRenderTarget.Get(renderer))
            {
                _lastRenderTarget.Set(renderer, renderer.ActiveRenderTarget);
            }

            renderer.ActiveRenderTarget = this;

            SharpDX.Direct3D11.Buffer gpuBuffer = _gpuBuffer.Get(renderer);
            if (gpuBuffer == null)
            {
                BufferDescription bufferDesc = new BufferDescription();
                bufferDesc.Usage = ResourceUsage.Default;
                bufferDesc.SizeInBytes = _size;
                bufferDesc.BindFlags = BindFlags.StreamOutput;

                gpuBuffer = new SharpDX.Direct3D11.Buffer(renderer.Device, bufferDesc);
                _gpuBuffer.Set(renderer, gpuBuffer, _size);
            }

            Query query = _query.Get(renderer);
            if (query == null)
            {
                QueryDescription queryDesc = new QueryDescription();
                queryDesc.Flags = QueryFlags.None;
                queryDesc.Type = QueryType.StreamOutputStatistics;
                query = new SharpDX.Direct3D11.Query(renderer.Device, queryDesc);

                _query.Set(renderer, query);
            }

            NumPrimitivesWritten = 0;
            PrimitivesStorageNeeded = 0;
            renderer.DeviceContext.Begin(query);

            renderer.DeviceContext.StreamOutput.SetTarget(gpuBuffer, 0);
        }

        public void Unbind(Renderer renderer)
        {
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

            Query query = _query.Get(renderer);
            if (query != null)
            {
                renderer.DeviceContext.End(query);
                while (!renderer.DeviceContext.IsDataAvailable(query))
                {
                    // wait for GPU
                }
                StreamOutputStatistics stats = renderer.DeviceContext.GetData<StreamOutputStatistics>(query);
                this.NumPrimitivesWritten = stats.NumPrimitivesWritten;
                this.PrimitivesStorageNeeded = stats.PrimitivesStorageNeeded;
            }

            renderer.DeviceContext.StreamOutput.SetTargets(null);
        }

        public T[] Download<T>(Renderer renderer) where T : struct
        {
            if (renderer.ActiveRenderTarget == this)
            {
                Unbind(renderer);
            }
            
            SharpDX.Direct3D11.Buffer cpuBuffer = _cpuBuffer.Get(renderer);
            if (cpuBuffer == null)
            {
                BufferDescription bufferDescription = new BufferDescription();
                bufferDescription.Usage = ResourceUsage.Staging;
                bufferDescription.SizeInBytes = _size;
                bufferDescription.CpuAccessFlags = CpuAccessFlags.Read;

                cpuBuffer = new SharpDX.Direct3D11.Buffer(renderer.Device, bufferDescription);
                _cpuBuffer.Set(renderer, cpuBuffer, _size);
            }

            SharpDX.Direct3D11.Buffer gpuBuffer = _gpuBuffer.Get(renderer);
            if (gpuBuffer != null)
            {
                int tSize = Marshal.SizeOf(typeof(T));
                int numElements = (int)System.Math.Ceiling((double)_size / tSize);
                T[] result = new T[numElements];
                
                renderer.DeviceContext.CopyResource(gpuBuffer, cpuBuffer);

                var box = renderer.DeviceContext.MapSubresource(cpuBuffer, 0, MapMode.Read, MapFlags.None);

                byte[] buffer = new byte[_size];
                Marshal.Copy(box.DataPointer, buffer, 0, _size);

                GCHandle bufferHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);

                int j = 0;
                for (int i = 0; i < _size; i += Marshal.SizeOf(typeof(T)))
                {
                    IntPtr ptr = Marshal.UnsafeAddrOfPinnedArrayElement(buffer, i);
                    result[j++] = (T)Marshal.PtrToStructure(ptr, typeof(T));
                }

                bufferHandle.Free();
                renderer.DeviceContext.UnmapSubresource(cpuBuffer, 0);
                
                return result;
            }

            return null;
        }
    }
}
