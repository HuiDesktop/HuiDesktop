using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Vortice.Direct3D;
using Vortice.Direct3D11;

namespace HuiDesktop.DirectComposition.DirectX
{
    public class Geometry : IBindable
    {
        readonly PrimitiveTopology primitiveTopology;
        readonly ID3D11Buffer buffer;
        readonly int strides;
        readonly int vertices;
        static readonly int[] offsets = new int[] { 0 };

        public Geometry(PrimitiveTopology primitiveTopology, ID3D11Buffer buffer, int strides, int vertices)
        {
            this.primitiveTopology = primitiveTopology;
            this.buffer = buffer;
            this.strides = strides;
            this.vertices = vertices;
        }

        public void Bind(ID3D11DeviceContext ctx)
        {
            ctx.IASetVertexBuffers(0, 1, new ID3D11Buffer[1] { buffer }, new int[] { strides }, offsets);
            ctx.IASetPrimitiveTopology(primitiveTopology);
        }

        public void Unbind()
        {
        }

        public void Draw(ID3D11DeviceContext ctx)
        {
            ctx.Draw(vertices, 0);
        }
    }

    partial class Device
    {
        struct SimpleVertex
        {
            public struct Pos
            {
                public float x, y, z;
            }

            public struct Tex
            {
                public float x, y;
            }

            public Pos pos;
            public Tex tex;

            public SimpleVertex(float x1, float x2, float x3, float x4, float x5)
            {
                pos.x = x1;
                pos.y = x2;
                pos.z = x3;
                tex.x = x4;
                tex.y = x5;
            }
        }

        static void Swap<T>(ref T x, ref T y)
        {
            T z = x;
            x = y;
            y = z;
        }

        public Geometry CreateQuad(float x, float y, float width, float height, bool flip)
        {
            width = 1;
            height = 1;
            x = x * 2.0f - 1.0f;
            y = 1.0f - y * 2.0f;
            float z = 1.0f;
            width *= 2.0f;
            height *= 2.0f;

            var vertices = new SimpleVertex[]
            {
                new SimpleVertex(x,y,z,0,0),
                new SimpleVertex(x+width, y,z,1,0),
                new SimpleVertex(x,y-height,z,0,1),
                new SimpleVertex(x+width,y-height,z,1,1)
            };

            if (flip)
            {
                Swap(ref vertices[2].tex, ref vertices[0].tex);
                Swap(ref vertices[3].tex, ref vertices[1].tex);
            }

            var bufferDescription = new BufferDescription
            {
                Usage = Usage.Default,
                BindFlags = BindFlags.VertexBuffer,
                CpuAccessFlags = CpuAccessFlags.None,
                SizeInBytes = Unsafe.SizeOf<SimpleVertex>() * 4
            };

            return new Geometry(primitiveTopology: PrimitiveTopology.TriangleStrip,
                                buffer: nativeDevice.CreateBuffer(vertices, bufferDescription),
                                strides: Unsafe.SizeOf<SimpleVertex>(),
                                vertices: 4);
        }
    }
}
