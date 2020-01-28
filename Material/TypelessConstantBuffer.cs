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
using System.Runtime.InteropServices;
using SharpDX.Direct3D11;
using IgnitionDX.Utilities;

namespace IgnitionDX.Graphics
{
    public class TypelessConstantBuffer : IConstantBuffer
    {
        protected RendererValue<SharpDX.Direct3D11.Buffer> _constantBuffer = new RendererValue<SharpDX.Direct3D11.Buffer>(null);
        protected RendererValue<bool> _constantBufferDirty = new RendererValue<bool>(true);
        protected RendererValue<int> _constantBufferSize = new RendererValue<int>(0);

        protected byte[] _dataBuffer = null;
        protected byte[] _valueBuffer = null;

        public TypelessConstantBuffer()
        {
        }

        protected int AlignedSize(int size)
        {
            if (size % 16 != 0)
            {
                return size + 16 - (size % 16);
            }
            return size;
        }

        protected void Invalidate()
        {
            _constantBufferDirty.ReleaseAll();
        }

        protected SharpDX.Direct3D11.Buffer Create(Renderer renderer)
        {
            BufferDescription desc = new BufferDescription();
            desc.Usage = ResourceUsage.Default;
            desc.SizeInBytes = _dataBuffer.Length;
            desc.BindFlags = BindFlags.ConstantBuffer;
            desc.CpuAccessFlags = CpuAccessFlags.None;

            var cBuffer = new SharpDX.Direct3D11.Buffer(renderer.Device, desc);
            renderer.DeviceContext.UpdateSubresource<byte>(_dataBuffer, cBuffer);

            _constantBuffer.Set(renderer, cBuffer, _dataBuffer.Length);
            _constantBufferDirty.Set(renderer, false);
            _constantBufferSize.Set(renderer, _dataBuffer.Length);

            return cBuffer;
        }

        public SharpDX.Direct3D11.Buffer GetBuffer(Renderer renderer)
        {
            if (_dataBuffer == null)
            {
                return null;
            }

            SharpDX.Direct3D11.Buffer cBuffer = _constantBuffer.Get(renderer);
            if (cBuffer == null)
            {
                // create new buffer
                cBuffer = this.Create(renderer);
            }
            else if (_constantBufferDirty.Get(renderer))
            {
                if (_constantBufferSize.Get(renderer) != _dataBuffer.Length)
                {
                    // recreate buffer with new size
                    cBuffer = this.Create(renderer);
                }
                else
                {
                    // update existing buffer
                    renderer.DeviceContext.UpdateSubresource<byte>(_dataBuffer, cBuffer);
                }

                _constantBufferDirty.Set(renderer, false);
            }

            return cBuffer;
        }

        public void Update<V>(int offset, V value) where V : struct
        {
            int valueSize = Marshal.SizeOf(typeof(V));
            int newBufferSize = AlignedSize(offset + valueSize + 128);

            bool resized = false;
            if (_dataBuffer == null)
            {
                // create new data buffer
                _dataBuffer = new byte[newBufferSize];
                resized = true;
            }
            else if (_dataBuffer.Length < newBufferSize)
            {
                // resize data buffer
                byte[] newDataBuffer = new byte[newBufferSize];
                Array.Copy(_dataBuffer, newDataBuffer, _dataBuffer.Length);
                _dataBuffer = newDataBuffer;
                resized = true;
            }

            if (_valueBuffer == null || _valueBuffer.Length < valueSize)
            {
                // create temporary byte array
                _valueBuffer = new byte[valueSize];
            }

            // copy value to temporary byte array
            GCHandle valueHandle = GCHandle.Alloc(_valueBuffer, GCHandleType.Pinned);
            Marshal.StructureToPtr(value, valueHandle.AddrOfPinnedObject(), false);

            GCHandle dataHandle = GCHandle.Alloc(_dataBuffer, GCHandleType.Pinned);
            IntPtr valuePtr = valueHandle.AddrOfPinnedObject();
            IntPtr dataPtr = Marshal.UnsafeAddrOfPinnedArrayElement(_dataBuffer, offset);

            if (resized || Win32Helper.MemCmp(valuePtr, dataPtr, valueSize) != 0)
            {
                // copy value to data buffer
                Win32Helper.MemCopy(dataPtr, valuePtr, valueSize);

                // invalidate gpu resource
                this.Invalidate();
            }

            dataHandle.Free();
            valueHandle.Free();
        }

        public void Preload(Renderer renderer)
        {
            this.GetBuffer(renderer);
        }
    }
}
