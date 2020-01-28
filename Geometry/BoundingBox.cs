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

using IgnitionDX.Math;

namespace IgnitionDX.Graphics
{
    public class BoundingBox
    {
        private Vector3? _min;
        private Vector3? _max;

        public Vector3? Min
        {
            get
            {
                return _min;
            }
            set
            {
                if (_min != value)
                {
                    _min = value;
                }
            }
        }

        public Vector3? Max
        {
            get
            {
                return _max;
            }
            set
            {
                if (_max != value)
                {
                    _max = value;
                }
            }
        }

        public bool IsValid
        {
            get
            {
                return _min.HasValue && _max.HasValue;
            }
        }

        public Vector3? Center
        {
            get
            {
                if (this.IsValid)
                {
                    return (Min.Value + Max.Value) * 0.5f;
                }

                return null;
            }
        }

        public float? Width
        {
            get
            {
                if (this.IsValid)
                {
                    return Max.Value.X - Min.Value.X;
                }

                return null;
            }
        }

        public float? Height
        {
            get
            {
                if (this.IsValid)
                {
                    return Max.Value.Y - Min.Value.Y;
                }

                return null;
            }
        }

        public float? Depth
        {
            get
            {
                if (this.IsValid)
                {
                    return Max.Value.Z - Min.Value.Z;
                }

                return null;
            }
        }

        public Matrix4? Transformation
        {
            get
            {
                if (this.IsValid)
                {
                    float w = this.Width.Value;
                    float h = this.Height.Value;
                    float d = this.Depth.Value;
                    Matrix4 scale = Matrix4.Scaling(w, h, d);
                    Matrix4 trans = Matrix4.Translation(this.Max.Value.X - w / 2, this.Max.Value.Y - h / 2, this.Max.Value.Z - d / 2);
                    return trans * scale;
                }

                return null;
            }
        }

        public BoundingBox()
        {
        }

        public BoundingBox(Vector3 min, Vector3 max)
        {
            Min = min;
            Max = max;
        }

        public void Grow(Vector3 p)
        {
            if (!Min.HasValue)
                Min = p;
            else
                Min = new Vector3(System.Math.Min(p.X, Min.Value.X), System.Math.Min(p.Y, Min.Value.Y), System.Math.Min(p.Z, Min.Value.Z));

            if (!Max.HasValue)
                Max = p;
            else
                Max = new Vector3(System.Math.Max(p.X, Max.Value.X), System.Math.Max(p.Y, Max.Value.Y), System.Math.Max(p.Z, Max.Value.Z));
        }

        public void Grow(Matrix4 trans, BoundingBox bbox)
        {
            if (bbox.Min.HasValue && bbox.Max.HasValue)
            {
                Vector4 p;

                p = new Vector4(bbox.Min.Value, 1);
                p = trans * p;
                p /= p.W;
                this.Grow(p.XYZ);

                p = new Vector4(bbox.Max.Value, 1);
                p = trans * p;
                p /= p.W;
                this.Grow(p.XYZ);

                p = new Vector4(bbox.Min.Value.X, bbox.Min.Value.Y, bbox.Max.Value.Z, 1);
                p = trans * p;
                p /= p.W;
                this.Grow(p.XYZ);

                p = new Vector4(bbox.Min.Value.X, bbox.Max.Value.Y, bbox.Max.Value.Z, 1);
                p = trans * p;
                p /= p.W;
                this.Grow(p.XYZ);

                p = new Vector4(bbox.Max.Value.X, bbox.Max.Value.Y, bbox.Max.Value.Z, 1);
                p = trans * p;
                p /= p.W;
                this.Grow(p.XYZ);

                p = new Vector4(bbox.Max.Value.X, bbox.Max.Value.Y, bbox.Min.Value.Z, 1);
                p = trans * p;
                p /= p.W;
                this.Grow(p.XYZ);

                p = new Vector4(bbox.Max.Value.X, bbox.Min.Value.Y, bbox.Min.Value.Z, 1);
                p = trans * p;
                p /= p.W;
                this.Grow(p.XYZ);

                p = new Vector4(bbox.Max.Value.X, bbox.Min.Value.Y, bbox.Max.Value.Z, 1);
                p = trans * p;
                p /= p.W;
                this.Grow(p.XYZ);
            }
        }
    }
}
