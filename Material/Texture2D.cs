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

using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using SharpDX.Direct3D11;
using IgnitionDX.Utilities;
using System.Runtime.InteropServices;

namespace IgnitionDX.Graphics
{
    public class Texture2D : ITexture
    {
        private string _name;
        private SharpDX.DataStream _dataStream;
        private int _dataStreamRowPitch = 0;

        private RendererValue<byte[]> _dataBuffer = new RendererValue<byte[]>(null);
        private RendererValue<int> _dataBufferOffset = new RendererValue<int>(0);
        private RendererValue<int> _dataBufferRowPitch = new RendererValue<int>(0);

        private RendererValue<ShaderResourceView> _textureResView = new RendererValue<ShaderResourceView>(null);
        private RendererValue<SharpDX.Direct3D11.Texture2D> _texture = new RendererValue<SharpDX.Direct3D11.Texture2D>(null);
        private RendererValue<bool> _textureIsDirty = new RendererValue<bool>(true);

        public int Width { get; private set; }
        public int Height { get; private set; }
        public SharpDX.DXGI.Format Format { get; private set; }
        public bool AutoMipmapped { get; private set; }
        public bool IsSemiTransparent { get; set; }

        public Texture2D(int width, int height, SharpDX.DXGI.Format format, bool autoMipmapped)
        {
            this.Width = width;
            this.Height = height;
            this.Format = format;
            this.AutoMipmapped = autoMipmapped;
        }

        public Texture2D(string fileName)
        {
            if (File.Exists(fileName))
            {
                _name = fileName;
                Bitmap bmp = new Bitmap(fileName, false);
                this.IsSemiTransparent = bmp.PixelFormat == PixelFormat.Format32bppArgb || fileName.ToLowerInvariant().EndsWith(".gif");
                this.Width = bmp.Width;
                this.Height = bmp.Height;
                this.Format = SharpDX.DXGI.Format.B8G8R8A8_UNorm;
                this.AutoMipmapped = true;
                Update(bmp);
            }
            else
            {
                throw new FileNotFoundException(fileName);
            }
        }

        public Texture2D(string name, Stream stream)
        {
            _name = name;
            Bitmap bmp = new Bitmap(stream, false);
            this.IsSemiTransparent = bmp.PixelFormat == PixelFormat.Format32bppArgb || name.ToLowerInvariant().EndsWith(".gif");
            this.Width = bmp.Width;
            this.Height = bmp.Height;
            this.Format = SharpDX.DXGI.Format.B8G8R8A8_UNorm;
            this.AutoMipmapped = true;
            Update(bmp);
        }

        public void Update(Bitmap bmp)
        {
            _textureIsDirty.ReleaseAll();
            _dataBuffer.ReleaseAll();

            int dataSize = bmp.Width * bmp.Height * 4;
            if (_dataStream != null && _dataStream.Length != dataSize)
            {
                _dataStream.Dispose();
                _dataStream = null;
            }

            if (_dataStream == null)
            {
                _dataStream = new SharpDX.DataStream(dataSize, true, true);
            }

            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            _dataStreamRowPitch = bmpData.Stride;
            _dataStream.Position = 0;
            _dataStream.WriteRange(bmpData.Scan0, dataSize);
            bmp.UnlockBits(bmpData);
        }

        public void Update(Renderer renderer, byte[] buffer, int bufferOffset, int rowPitch)
        {
            _textureIsDirty.Release(renderer);
            _dataStream = null;

            _dataBuffer.Set(renderer, buffer);
            _dataBufferOffset.Set(renderer, bufferOffset);
            _dataBufferRowPitch.Set(renderer, rowPitch);

            Preload(renderer);
        }

        #region ITexture Member

        public void Preload(Renderer renderer)
        {
            this.GetShaderResourceView(renderer);
        }

        private void UploadTextureData(Renderer renderer)
        {
            if (_textureIsDirty.Get(renderer))
            {
                _textureIsDirty.Set(renderer, false);

                var tex = _texture.Get(renderer);
                
                // upload level 0
                SharpDX.DataBox box = new SharpDX.DataBox();
                box.SlicePitch = 0;

                if (_dataBuffer.Get(renderer) != null)
                {
                    // upload from byte array
                    box.RowPitch = _dataBufferRowPitch.Get(renderer);
                    GCHandle dataHandle = GCHandle.Alloc(_dataBuffer.Get(renderer), GCHandleType.Pinned);
                    box.DataPointer = Marshal.UnsafeAddrOfPinnedArrayElement(_dataBuffer.Get(renderer), _dataBufferOffset.Get(renderer));
                    renderer.DeviceContext.UpdateSubresource(box, tex, 0);
                    dataHandle.Free();
                }
                else if (_dataStream != null)
                {
                    // upload from data stream
                    lock (_dataStream)
                    {
                        box.RowPitch = _dataStreamRowPitch;
                        _dataStream.Position = 0;
                        box.DataPointer = _dataStream.DataPointer;
                        renderer.DeviceContext.UpdateSubresource(box, tex, 0);
                    }
                }
                else
                {
                    return;
                }
                
                if (AutoMipmapped)
                {
                    // generate mipmap levels
                    renderer.DeviceContext.GenerateMips(_textureResView.Get(renderer));
                }
            }
        }

        public ShaderResourceView GetShaderResourceView(Renderer renderer)
        {
            ShaderResourceView resView = _textureResView.Get(renderer);

            if (resView == null)
            {
                Texture2DDescription desc = new Texture2DDescription();
                desc.Height = this.Height;
                desc.Width = this.Width;
                desc.Usage = ResourceUsage.Default;
                desc.Format = this.Format;
                desc.CpuAccessFlags = CpuAccessFlags.None;
                desc.ArraySize = 1;
                desc.SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0);

                if (AutoMipmapped)
                {
                    desc.BindFlags = BindFlags.ShaderResource | BindFlags.RenderTarget;
                    desc.OptionFlags = ResourceOptionFlags.GenerateMipMaps;
                    desc.MipLevels = 0;
                }
                else
                {
                    desc.BindFlags = BindFlags.ShaderResource;
                    desc.OptionFlags = ResourceOptionFlags.None;
                    desc.MipLevels = 1;
                }

                var texture = new SharpDX.Direct3D11.Texture2D(renderer.Device, desc);
                _texture.Set(renderer, texture, Width * Height * 4);

                // create shader resource view  
                resView = new ShaderResourceView(renderer.Device, texture);
                _textureResView.Set(renderer, resView);
            }

            UploadTextureData(renderer);

            return resView;
        }

        #endregion
    }
}
