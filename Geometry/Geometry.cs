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
using System.Reflection;
using System.Runtime.InteropServices;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using IgnitionDX.Math;

namespace IgnitionDX.Graphics
{
    public interface IVertexDescription
    {
        Vector3 Position { get; }
    }

    public class Geometry<T> : IGeometry where T : struct, IVertexDescription
    {
        protected T[] _vertices;
        protected int[] _indices;
        protected PrimitiveTopology _primitiveTopology;
        protected RendererValue<SharpDX.Direct3D11.Buffer> _vertexBuffer = new RendererValue<SharpDX.Direct3D11.Buffer>(null);
        protected RendererValue<SharpDX.Direct3D11.Buffer> _indexBuffer = new RendererValue<SharpDX.Direct3D11.Buffer>(null);
        protected RendererValue<InputLayout> _inputLayout = new RendererValue<InputLayout>(null);

        public BoundingBox BoundingBox { get; set; }

        public bool IsIndexed
        {
            get { return _indices != null; }
        }

        public Geometry(T[] vertices)
            : this(vertices, null)
        {
        }

        public Geometry(T[] vertices, int[] indices)
            : this(vertices, indices, PrimitiveTopology.TriangleList)
        {
        }

        public Geometry(T[] vertices, int[] indices, PrimitiveTopology primitiveTopology)
        {
            this._vertices = vertices;
            this._indices = indices;
            this._primitiveTopology = primitiveTopology;

            UpdateBoundingBox();
        }

        public void UpdateBoundingBox()
        {
            BoundingBox bbox = new BoundingBox();
            for (int i = 0; i < _vertices.Length; i++)
            {
                bbox.Grow(_vertices[i].Position);
            }
            BoundingBox = bbox;
        }

        protected SharpDX.Direct3D11.Buffer CreateVertexBuffer(Renderer renderer)
        {
            //Logger.LogInfo(this, "Creating geometry with " + _vertices.Length + " vertices.");

            int dataSize = Marshal.SizeOf(typeof(T)) * _vertices.Length;

            BufferDescription bufferDescription = new BufferDescription();
            bufferDescription.Usage = ResourceUsage.Default;
            bufferDescription.SizeInBytes = dataSize;
            bufferDescription.BindFlags = BindFlags.VertexBuffer;

            SharpDX.Direct3D11.Buffer buffer = null;
            using (SharpDX.DataStream stream = new SharpDX.DataStream(dataSize, true, true))
            {
                for (int i = 0; i < _vertices.Length; i++)
                {
                    stream.Write<T>(_vertices[i]);
                }
                stream.Position = 0;

                buffer = new SharpDX.Direct3D11.Buffer(renderer.Device, stream, bufferDescription);
            }

            _vertexBuffer.Set(renderer, buffer);

            return buffer;
        }

        protected SharpDX.Direct3D11.Buffer CreateIndexBuffer(Renderer renderer)
        {
            int dataSize = Marshal.SizeOf(typeof(int)) * _indices.Length;

            BufferDescription bufferDescription = new BufferDescription();
            bufferDescription.Usage = ResourceUsage.Default;
            bufferDescription.SizeInBytes = dataSize;
            bufferDescription.BindFlags = BindFlags.IndexBuffer;

            SharpDX.Direct3D11.Buffer buffer = null;
            using (SharpDX.DataStream stream = new SharpDX.DataStream(dataSize, true, true))
            {
                for (int i = 0; i < _indices.Length; i++)
                {
                    stream.Write<int>(_indices[i]);
                }
                stream.Position = 0;

                buffer = new SharpDX.Direct3D11.Buffer(renderer.Device, stream, bufferDescription);
            }

            _indexBuffer.Set(renderer, buffer);

            return buffer;
        }

        protected InputLayout CreateInputLayout(Renderer renderer)
        {
            //Logger.LogInfo(this, "Creating InputLayout.");

            FieldInfo[] infos = typeof(T).GetFields(BindingFlags.Public | BindingFlags.Instance);
            InputElement[] descriptions = new InputElement[infos.Length];

            for (int i = 0; i < infos.Length; i++)
            {
                FieldInfo info = infos[i];
                descriptions[i] = new InputElement();
                descriptions[i].SemanticName = info.Name;
                descriptions[i].SemanticIndex = 0;
                descriptions[i].Slot = 0;
                descriptions[i].AlignedByteOffset = InputElement.AppendAligned;
                descriptions[i].Classification = InputClassification.PerVertexData;
                descriptions[i].InstanceDataStepRate = 0;

                if (info.FieldType == typeof(float))
                    descriptions[i].Format = SharpDX.DXGI.Format.R32_Float;
                else if (info.FieldType == typeof(Vector2))
                    descriptions[i].Format = SharpDX.DXGI.Format.R32G32_Float;
                else if (info.FieldType == typeof(Vector3))
                    descriptions[i].Format = SharpDX.DXGI.Format.R32G32B32_Float;
                else if (info.FieldType == typeof(Vector4) || info.FieldType == typeof(Color4))
                    descriptions[i].Format = SharpDX.DXGI.Format.R32G32B32A32_Float;
                else
                    throw new Exception("Unknown attribute type: " + info.FieldType.ToString());
            }

            InputLayout layout = renderer.ActiveShaderProgram.CreateInputLayout(renderer, descriptions);
            _inputLayout.Set(renderer, layout);

            return layout;
        }

        public bool IsCreated(Renderer renderer)
        {
            return _vertexBuffer.Get(renderer) != null;
        }

        public void Preload(Renderer renderer)
        {
            if (_vertexBuffer.Get(renderer) == null)
            {
                this.CreateVertexBuffer(renderer);
            }

            if (IsIndexed && _indexBuffer.Get(renderer) == null)
            {
                this.CreateIndexBuffer(renderer);
            }
        }

        private void Bind(Renderer renderer)
        {
            if (renderer.ActiveShaderProgram == null)
            {
                throw new Exception("There is no material to render this geometry.");
            }

            this.Preload(renderer);

            InputLayout layout = _inputLayout.Get(renderer);
            if (layout == null)
            {
                layout = this.CreateInputLayout(renderer); // TODO preload
            }

            renderer.DeviceContext.InputAssembler.InputLayout = layout;
            renderer.DeviceContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(_vertexBuffer.Get(renderer), Marshal.SizeOf(typeof(T)), 0));
            renderer.DeviceContext.InputAssembler.PrimitiveTopology = this._primitiveTopology;

            if (IsIndexed)
            {
                renderer.DeviceContext.InputAssembler.SetIndexBuffer(_indexBuffer.Get(renderer), SharpDX.DXGI.Format.R32_UInt, 0);
            }
        }

        public void Render(Renderer renderer)
        {
            Bind(renderer);

            if (IsIndexed)
            {
                renderer.DeviceContext.DrawIndexed(_indices.Length, 0, 0);
            }
            else
            {
                renderer.DeviceContext.Draw(_vertices.Length, 0);
            }
        }

        public void RenderInstanced(Renderer renderer, int numInstances)
        {
            Bind(renderer);

            if (IsIndexed)
            {
                renderer.DeviceContext.DrawIndexedInstanced(_indices.Length, numInstances, 0, 0, 0);
            }
            else
            {
                renderer.DeviceContext.DrawInstanced(_vertices.Length, numInstances, 0, 0);
            }
        }

        public Vector3? Pick(PickRay ray)
        {
            Vector3? closestPoint = null;
            float closestDist = 0;
            if (_primitiveTopology == PrimitiveTopology.TriangleList)
            {
                for (int i = 0; i < (IsIndexed ? _indices.Length : _vertices.Length); i += 3)
                {
                    Vector3 p0;
                    Vector3 p1;
                    Vector3 p2;

                    if (IsIndexed)
                    {
                        p0 = _vertices[_indices[i + 0]].Position;
                        p1 = _vertices[_indices[i + 1]].Position;
                        p2 = _vertices[_indices[i + 2]].Position;
                    }
                    else
                    {
                        p0 = _vertices[i + 0].Position;
                        p1 = _vertices[i + 1].Position;
                        p2 = _vertices[i + 2].Position;
                    }

                    Vector3? point = RayTriIntersect(ray.Origin, ray.Direction, p0, p1, p2);

                    if (point != null)
                    {
                        float dist = (ray.Origin - point.Value).Length;
                        if (closestPoint == null || dist < closestDist)
                        {
                            closestPoint = point;
                            closestDist = dist;
                        }
                    }
                }
            }
            else if (_primitiveTopology == PrimitiveTopology.LineList)
            {
                for (int i = 0; i < (IsIndexed ? _indices.Length : _vertices.Length); i += 2)
                {
                    Vector3 p0;
                    Vector3 p1;

                    if (IsIndexed)
                    {
                        p0 = _vertices[_indices[i + 0]].Position;
                        p1 = _vertices[_indices[i + 1]].Position;
                    }
                    else
                    {
                        p0 = _vertices[i + 0].Position;
                        p1 = _vertices[i + 1].Position;
                    }

                    Vector3? point = RayLineIntersect(ray.Origin, ray.Direction, p0, p1);

                    if (point != null)
                    {
                        float dist = (ray.Origin - point.Value).Length;
                        if (closestPoint == null || dist < closestDist)
                        {
                            closestPoint = point;
                            closestDist = dist;
                        }
                    }
                }
            }
            else
            {
                // not implemented
            }

            return closestPoint;
        }

        private Vector3? RayTriIntersect(Vector3 orig, Vector3 dir, Vector3 vert0, Vector3 vert1, Vector3 vert2)
        {
            // http://www.cs.virginia.edu/~gfx/Courses/2003/ImageSynthesis/papers/Acceleration/Fast%20MinimumStorage%20RayTriangle%20Intersection.pdf

            float epsilon = 0.000001f;

            // find vectors for two edges sharing vert0
            Vector3 edge1 = vert1 - vert0;
            Vector3 edge2 = vert2 - vert0;

            // begin calculating determinant - also used to calculate U parameter
            Vector3 pvec = dir.Cross(edge2);

            // if determinant is near zero, ray lies in plane of triangle
            float det = edge1.Dot(pvec);

            if (det > -epsilon && det < epsilon)
                return null;
            float inv_det = 1f / det;

            // calculate distance from vert0 to ray origin
            Vector3 tvec = orig - vert0;

            // calculate U parameter and test bounds
            float u = tvec.Dot(pvec) * inv_det;
            if (u < 0 || u > 1)
                return null;

            // prepare to test V parameter
            Vector3 qvec = tvec.Cross(edge1);

            // calculate V parameter and test bounds
            float v = dir.Dot(qvec) * inv_det;
            if (v < 0 || u + v > 1)
                return null;

            // calculate intersection point
            Vector3 e = edge1.Cross(edge2);
            float d = e.Dot(vert0);
            float s = (d - e.Dot(orig)) / e.Dot(dir);
            if (s >= 0)
                return orig + (dir * s);

            return null;
        }

        private Vector3? RayLineIntersect(Vector3 orig, Vector3 dir, Vector3 vert0, Vector3 vert1)
        {
            Vector3 orig2 = vert0;
            Vector3 dir2 = vert1 - vert0;
            Vector3 n = dir2.Cross(dir);
            float dist = System.Math.Abs((orig2 - orig).Dot(n)) / n.Length;

            if (dist < 0.01f)
            {
                float[,] m = new float[3, 3];
                m[0, 0] = dir.X;
                m[0, 1] = -dir2.X;
                m[0, 2] = n.X;
                m[1, 0] = dir.Y;
                m[1, 1] = -dir2.Y;
                m[1, 2] = n.Y;
                m[2, 0] = dir.Z;
                m[2, 1] = -dir2.Z;
                m[2, 2] = n.Z;

                float[] r = new float[3];
                r[0] = orig2.X - orig.X;
                r[1] = orig2.Y - orig.Y;
                r[2] = orig2.Z - orig.Z;

                float[] result = new float[3];
                if (IgnitionDX.Math.Solver.SolveLinearEquation(m, r, ref result))
                {
                    if (result[1] >= 0 && result[1] <= 1)
                    {
                        return orig2 + result[1] * dir2;
                    }
                }
            }

            return null;
        }
    }
}
