using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;
using Vortice.Mathematics;
using static Vortice.D3DCompiler.Compiler;

namespace HuiDesktop.DirectComposition
{
    interface IBindable
    {
        void Bind(ID3D11DeviceContext ctx);
        void Unbind();
    }

    class Binder : IDisposable
    {
        private readonly IBindable bindable;

        public Binder(ID3D11DeviceContext ctx, IBindable bindable)
        {
            this.bindable = bindable;
            bindable.Bind(ctx);
        }

        public void Dispose()
        {
            bindable.Unbind();
        }
    }

    class Geometry : IBindable
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

    class Effect : IBindable
    {
        readonly ID3D11InputLayout layout;
        readonly ID3D11VertexShader vertexShader;
        readonly ID3D11PixelShader pixelShader;

        public Effect(ID3D11InputLayout layout, ID3D11VertexShader vertexShader, ID3D11PixelShader pixelShader)
        {
            this.layout = layout;
            this.vertexShader = vertexShader;
            this.pixelShader = pixelShader;
        }

        public void Bind(ID3D11DeviceContext ctx)
        {
            ctx.IASetInputLayout(layout);
            ctx.VSSetShader(vertexShader);
            ctx.PSSetShader(pixelShader);
        }

        public void Unbind()
        {
        }
    }

    class Texture2D : IBindable
    {
        public readonly IntPtr sharedHandle;
        readonly ID3D11Texture2D texture;
        readonly ID3D11ShaderResourceView shaderResourceView;

        public Texture2D(ID3D11Device device, IntPtr sharedHandle)
        {
            this.sharedHandle = sharedHandle;
            texture = device.OpenSharedResource<ID3D11Texture2D>(sharedHandle);
            var p = texture.Description;
            ShaderResourceViewDescription? desc = null;
            if ((p.BindFlags & BindFlags.ShaderResource) != 0)
            {
                desc = new ShaderResourceViewDescription
                {
                    Format = Format.B8G8R8A8_UNorm,
                    ViewDimension = ShaderResourceViewDimension.Texture2D,
                    Texture2D = new Texture2DShaderResourceView { MipLevels = 1, MostDetailedMip = 0 }
                };
            }
            shaderResourceView = device.CreateShaderResourceView(texture.QueryInterface<ID3D11Resource>(), desc);
        }

        public void Bind(ID3D11DeviceContext ctx)
        {
            ctx.PSSetShaderResources(0, 1, new ID3D11ShaderResourceView[] { shaderResourceView });
        }

        public void Unbind()
        {
        }
    }

    class SwapChain : IBindable
    {
        ID3D11RenderTargetView renderTargetView;
        ID3D11BlendState blendState;
        ID3D11SamplerState samplerState;

        public SwapChain(ID3D11DeviceContext ctx, ID3D11Device device, ID3D11RenderTargetView renderTargetView, float width, float height)
        {
            this.renderTargetView = renderTargetView;
            ctx.OMSetRenderTargets(renderTargetView);
            ctx.RSSetViewport(new Viewport
            {
                Width = width,
                Height = height,
                MinDepth = 0,
                MaxDepth = 1,
                X = 0,
                Y = 0
            });
            samplerState = device.CreateSamplerState(new SamplerDescription
            {
                AddressU = TextureAddressMode.Clamp,
                AddressV = TextureAddressMode.Clamp,
                AddressW = TextureAddressMode.Clamp,
                ComparisonFunction = ComparisonFunction.Never,
                MinLOD = 0,
                MaxLOD = float.MaxValue,
                Filter = Filter.MinMagMipLinear
            });
            var desc = new BlendDescription
            {
                AlphaToCoverageEnable = false,
                IndependentBlendEnable = false
            };
            for (int x = 0; x < desc.RenderTarget.Length; ++x)
            {
                desc.RenderTarget[x] = new RenderTargetBlendDescription
                {
                    IsBlendEnabled = true,
                    SourceBlend = Blend.One,
                    DestinationBlend = Blend.InverseSourceAlpha,
                    SourceBlendAlpha = Blend.One,
                    DestinationBlendAlpha = Blend.InverseSourceAlpha,
                    BlendOperation = BlendOperation.Add,
                    BlendOperationAlpha = BlendOperation.Add,
                    RenderTargetWriteMask = ColorWriteEnable.All
                };
            }
            blendState = device.CreateBlendState(desc);
        }

        public void Resize(ID3D11DeviceContext ctx, IDXGISwapChain swapChain, int width, int height, ref IDXGISurface2 buffer)
        {
            ctx.OMSetRenderTargets(0, Array.Empty<ID3D11RenderTargetView>());
            renderTargetView.Dispose();
            renderTargetView = null;
            buffer.Dispose();
            ctx.ClearState();
            ctx.Flush();
            swapChain.ResizeBuffers(0, width, height, Format.B8G8R8A8_UNorm, SwapChainFlags.None).CheckError();
            buffer = swapChain.GetBuffer<IDXGISurface2>(0);
            renderTargetView = ctx.Device.CreateRenderTargetView(buffer.QueryInterface<ID3D11Resource>(),
                                                                 new RenderTargetViewDescription
                                                                 {
                                                                     ViewDimension = RenderTargetViewDimension.Texture2D,
                                                                     Texture2D = new Texture2DRenderTargetView { MipSlice = 0 },
                                                                     Format = Format.B8G8R8A8_UNorm
                                                                 });
            ctx.OMSetRenderTargets(renderTargetView);
            ctx.RSSetViewport(new Viewport { X = 0, Y = 0, Width = width, Height = height });
        }

        public void Bind(ID3D11DeviceContext ctx)
        {
            ctx.OMSetRenderTargets(renderTargetView);
            if (blendState != null)
            {
                ctx.OMSetBlendState(blendState, new Color4(0,0,0,0), -1);
            }
            if (samplerState != null)
            {
                ctx.PSSetSampler(0, samplerState);
            }

            ctx.ClearRenderTargetView(renderTargetView, new Color4(0, 0, 1, 1));
        }

        public void Unbind()
        {
        }
    }

    class Layer
    {
        readonly ID3D11Device device;
        Geometry geometry;
        Effect effect;

        public Layer(ID3D11Device device)
        {
            this.device = device;
            effect = device.CreateDefaultEffect();
        }

        public void RenderTexture(ID3D11DeviceContext ctx, Texture2D texture)
        {
            using (new Binder(ctx, geometry))
            using (new Binder(ctx, effect))
            using (new Binder(ctx, texture))
            {
                geometry.Draw(ctx);
            }
        }

        public void Move(System.Drawing.Rectangle rect)
        {
            geometry = device.CreateQuad(rect.X, rect.Y, rect.Width, rect.Height, true);
        }
    }

    static class RenderHelper
    {
        static void Swap<T>(ref T x, ref T y)
        {
            T z = x;
            x = y;
            y = z;
        }

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

        private static Blob CompileShader(string source, string entry, string model)
        {
            Compile(source, entry, string.Empty, model, out Blob blob, out Blob errorBlob).CheckError();
            if (errorBlob != null) errorBlob.Release();
            return blob;
        }

        public static Effect CreateEffect(this ID3D11Device device, string vertexCode, string vertexEntry, string vertexModel, string pixelCode, string pixelEntry, string pixelModel)
        {
            var vsBlob = CompileShader(vertexCode, vertexEntry, vertexModel);
            var vshader = device.CreateVertexShader(vsBlob.BufferPointer, vsBlob.BufferSize);
            var layout = device.CreateInputLayout(new InputElementDescription[]
            {
                new InputElementDescription
                {
                    SemanticName = "POSITION",
                    SemanticIndex = 0,
                    Format = Vortice.DXGI.Format.R32G32B32_Float,
                    Slot = 0,
                    AlignedByteOffset = 0,
                    Classification = InputClassification.PerVertexData,
                    InstanceDataStepRate = 0
                },
                new InputElementDescription
                {
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 0,
                    Format = Vortice.DXGI.Format.R32G32_Float,
                    Slot = 0,
                    AlignedByteOffset = 12,
                    Classification = InputClassification.PerVertexData,
                    InstanceDataStepRate = 0
                }
            }, vsBlob);
            var psBlob = CompileShader(pixelCode, pixelEntry, pixelModel);
            var pshader = device.CreatePixelShader(psBlob.BufferPointer, psBlob.BufferSize);
            return new Effect(layout, vshader, pshader);
        }

        public static Effect CreateDefaultEffect(this ID3D11Device device)
        {
            var vsh =
@"struct VS_INPUT
{
	float4 pos : POSITION;
	float2 tex : TEXCOORD0;
};

struct VS_OUTPUT
{
	float4 pos : SV_POSITION;
	float2 tex : TEXCOORD0;
};

VS_OUTPUT main(VS_INPUT input)
{
	VS_OUTPUT output;
	output.pos = input.pos;
	output.tex = input.tex;
	return output;
}";
            var psh =
@"Texture2D tex0 : register(t0);
SamplerState samp0 : register(s0);

struct VS_OUTPUT
{
    float4 pos : SV_POSITION;
    float2 tex : TEXCOORD0;
};

float4 main(VS_OUTPUT input) : SV_Target
{
    return tex0.Sample(samp0, input.tex);
}";
            return device.CreateEffect(vsh, "main", "vs_4_0", psh, "main", "ps_4_0");
        }

        public static Geometry CreateQuad(this ID3D11Device device, float x, float y, float width, float height, bool flip)
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

            var desc = new BufferDescription
            {
                Usage = Vortice.Direct3D11.Usage.Default,
                BindFlags = BindFlags.VertexBuffer,
                CpuAccessFlags = CpuAccessFlags.None,
                SizeInBytes = Unsafe.SizeOf<SimpleVertex>() * 4
            };

            var srd = new SubresourceData();
            unsafe
            {
                fixed (SimpleVertex* ptr = &vertices[0])
                {
                    srd.DataPointer = new IntPtr(ptr);
                    var buffer = device.CreateBuffer(desc, srd);
                    return new Geometry(PrimitiveTopology.TriangleStrip, buffer, Unsafe.SizeOf<SimpleVertex>(), 4);
                }
            }
        }
    }
}
