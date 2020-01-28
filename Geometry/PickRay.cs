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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IgnitionDX.Graphics
{
    public struct PickRay
    {
        public Vector3 Origin;
        public Vector3 Direction;

        public Vector3? GetIntersectionPoint(Vector4 plane)
        {
            float f = (plane.W - Origin.X * plane.X - Origin.Y * plane.Y - Origin.Z * plane.Z) / (Direction.X * plane.X + Direction.Y * plane.Y + Direction.Z * plane.Z);

            if (float.IsNaN(f))
                return null;

            return Origin + (f * Direction);
        }

        public static PickRay operator *(Matrix4 trans, PickRay ray)
        {
            PickRay result = new PickRay();
            result.Origin = (trans * new Vector4(ray.Origin, 1)).DivW.XYZ;
            result.Direction = (trans * new Vector4(ray.Direction, 0)).XYZ;

            return result;
        }
    }
}
