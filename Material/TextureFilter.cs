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
    public class TextureFilter
    {
        private RendererValue<SamplerState> _samplerState = new RendererValue<SamplerState>(null);
        private SamplerStateDescription _description;

        public TextureFilter(SamplerStateDescription desc)
        {
            _description = desc;
        }

        public SamplerState GetSamplerState(Renderer renderer)
        {
            SamplerState state = _samplerState.Get(renderer);
            if (state == null)
            {
                state = new SamplerState(renderer.Device, _description);
                _samplerState.Set(renderer, state);
            }

            return state;
        }

        public void Preload(Renderer renderer)
        {
            this.GetSamplerState(renderer);
        }

        #region Factory

        public static TextureFilter CreateLinearMipmapped(TextureAddressMode uvwAdressMode)
        {
            return new TextureFilter(new SamplerStateDescription()
            {
                Filter = Filter.MinMagMipLinear,
                AddressU = uvwAdressMode,
                AddressV = uvwAdressMode,
                AddressW = uvwAdressMode,
                ComparisonFunction = Comparison.Always,
                MinimumLod = 0,
                MaximumLod = float.MaxValue,
                MaximumAnisotropy = 16
            });
        }

        public static TextureFilter CreateLinear(TextureAddressMode uvwAdressMode)
        {
            return new TextureFilter(new SamplerStateDescription()
            {
                Filter = Filter.MinMagMipLinear,
                AddressU = uvwAdressMode,
                AddressV = uvwAdressMode,
                AddressW = uvwAdressMode,
                ComparisonFunction = Comparison.Always,
                MinimumLod = 0,
                MaximumLod = 0,
                MaximumAnisotropy = 16
            });
        }

        public static TextureFilter CreateNearest(TextureAddressMode uvwAdressMode)
        {
            return new TextureFilter(new SamplerStateDescription()
            {
                Filter = Filter.MinMagMipPoint,
                AddressU = uvwAdressMode,
                AddressV = uvwAdressMode,
                AddressW = uvwAdressMode,
                ComparisonFunction = Comparison.Always,
                MinimumLod = 0,
                MaximumLod = 0,
                MaximumAnisotropy = 16
            });
        }

        public static TextureFilter CreatePCF()
        {
            return new TextureFilter(new SamplerStateDescription()
            {
                Filter = Filter.ComparisonMinMagLinearMipPoint,
                AddressU = TextureAddressMode.Clamp,
                AddressV = TextureAddressMode.Clamp,
                AddressW = TextureAddressMode.Clamp,
                ComparisonFunction = Comparison.Less,
                MinimumLod = 0,
                MaximumLod = 0,
                MaximumAnisotropy = 0,
            });
        }

        #endregion
    }
}
