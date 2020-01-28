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
namespace IgnitionDX.Graphics
{
    public class RendererValue<T> : SharpDX.Component, IRendererValue
    {
        private T _defaultValue;
        private Dictionary<Renderer, T> _values;
        private Dictionary<Renderer, long> _memPressure;

        public RendererValue(T defaultValue)
        {
            _values = new Dictionary<Renderer, T>();
            _memPressure = new Dictionary<Renderer, long>();
            _defaultValue = defaultValue;

            if (_defaultValue is IDisposable || _defaultValue is SharpDX.Component)
            {
                ToDispose(_defaultValue);
            }
        }

        public void Set(Renderer renderer, T value, long memPressure = 0)
        {
            lock (_values)
            {
                if (_values.ContainsKey(renderer))
                {
                    Release(renderer);
                }
                _values[renderer] = value;
                if (value is IDisposable || value is SharpDX.Component)
                {
                    if (memPressure > 0)
                    {
                        _memPressure[renderer] = memPressure;
                        GC.AddMemoryPressure(memPressure);
                    }
                    ToDispose(value);
                }
            }
        }

        public T Get(Renderer renderer)
        {
            lock (_values)
            {
                if (_values.ContainsKey(renderer))
                {
                    return _values[renderer];
                }
            }

            return _defaultValue;
        }

        public void Release(Renderer renderer)
        {
            lock (_values)
            {
                if (_values.ContainsKey(renderer))
                {
                    T value = _values[renderer];
                    _values.Remove(renderer);
                    if (value is IDisposable || value is SharpDX.Component)
                    {
                        RemoveAndDispose(ref value);
                        if (_memPressure.ContainsKey(renderer))
                        {
                            GC.RemoveMemoryPressure(_memPressure[renderer]);
                            _memPressure.Remove(renderer);
                        }
                    }
                }
            }
        }

        public void ReleaseAll()
        {
            lock (_values)
            {
                foreach (KeyValuePair<Renderer, T> value in _values)
                {
                    if (value.Value is IDisposable || value.Value is SharpDX.Component)
                    {
                        T val = value.Value;
                        RemoveAndDispose(ref val);
                        if (_memPressure.ContainsKey(value.Key))
                        {
                            GC.RemoveMemoryPressure(_memPressure[value.Key]);
                            _memPressure.Remove(value.Key);
                        }
                    }
                }
                _values.Clear();
            }
        }

        ~RendererValue()
        {
            if (!IsDisposed)
            {
                this.Dispose();
            }

            foreach (var memPressure in _memPressure.Values)
            {
                GC.RemoveMemoryPressure(memPressure);
            }
        }
    }
}