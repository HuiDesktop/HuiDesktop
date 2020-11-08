using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vortice.Direct3D11;
using Vortice.DXGI;
using Vortice.Mathematics;

namespace HuiDesktop.DirectComposition.DirectX
{
    public class SwapChain : IBindable
    {
        private ID3D11SamplerState samplerState;
        private ID3D11BlendState blendState;
        private IDXGISwapChain swapchain;
        private ID3D11RenderTargetView renderTargetView;
        private ID3D11Texture2D backBuffer; //TODO: Release before resize

        public SwapChain(ID3D11SamplerState samplerState, ID3D11BlendState blendState, IDXGISwapChain swapchain, ID3D11RenderTargetView renderTargetView)
        {
            this.samplerState = samplerState;
            this.blendState = blendState;
            this.swapchain = swapchain;
            this.renderTargetView = renderTargetView;
            backBuffer = swapchain.GetBuffer<ID3D11Texture2D>(0);
        }

        public void Clear(ID3D11DeviceContext ctx)
        {
            ctx.ClearRenderTargetView(renderTargetView, new Color4(0, 0, 1, 1));
        }

        public void Present(int syncInterval)
        {
            swapchain.Present(syncInterval, PresentFlags.None);
        }

        public void Bind(ID3D11DeviceContext ctx)
        {
            ctx.OMSetRenderTargets(renderTargetView);
            if (blendState != null)
            {
                ctx.OMSetBlendState(blendState, new Color4(0, 0, 0, 0), -1);
            }
            if (samplerState != null)
            {
                ctx.PSSetSampler(0, samplerState);
            }
        }

        public void Unbind()
        {
        }

        public void SetDCVisualContent(DCompositionVisual visual)
        {
            visual.SetContent(swapchain).CheckError();
        }

        public void CopyRegion(ID3D11DeviceContext ctx, ID3D11Resource dest, Rectangle rect)
        {
            ctx.CopySubresourceRegion(dest, 0, 0, 0, 0, backBuffer, 0, new(rect.X, rect.Y, 0, rect.X + rect.Width, rect.Y + rect.Height, 1));
        }
    }

    partial class Device
    {
        public SwapChain CreateSwapChain(ID3D11DeviceContext ctx, int width, int height)
        {
            if (width <= 0)
            {
                throw new ArgumentException($"{nameof(width)} shoule greater than 0");
            }
            if (height <= 0)
            {
                throw new ArgumentException($"{nameof(height)} shoule greater than 0");
            }

            var dxgi_device = nativeDevice.QueryInterface<IDXGIDevice>();
            dxgi_device.GetAdapter(out var adapter).CheckError();
            var dxgi_factory = adapter.GetParent<IDXGIFactory2>();

            var swapchain = dxgi_factory.CreateSwapChainForComposition(dxgi_device, new SwapChainDescription1
            {
                Format = Format.B8G8R8A8_UNorm,
                Usage = Vortice.DXGI.Usage.RenderTargetOutput,
                SwapEffect = SwapEffect.FlipSequential,
                BufferCount = 2,
                SampleDescription = new SampleDescription { Count = 1 },
                AlphaMode = Vortice.DXGI.AlphaMode.Premultiplied,
                Width = width,
                Height = height
            });
            var back_buffer = swapchain.GetBuffer<IDXGISurface2>(0);

            var rtv = nativeDevice.CreateRenderTargetView(back_buffer.QueryInterface<ID3D11Texture2D>());
            ctx.OMSetRenderTargets(rtv);
            ctx.RSSetViewport(new Viewport
            {
                Width = width,
                Height = height,
                MinDepth = 0,
                MaxDepth = 1,
                X = 0,
                Y = 0
            });

            var samplerState = nativeDevice.CreateSamplerState(new SamplerDescription
            {
                AddressU = TextureAddressMode.Clamp,
                AddressV = TextureAddressMode.Clamp,
                AddressW = TextureAddressMode.Clamp,
                ComparisonFunction = ComparisonFunction.Never,
                MinLOD = 0,
                MaxLOD = float.MaxValue,
                Filter = Filter.MinMagMipLinear
            });

            //pre-multiplied alpha
            var blend_description = new BlendDescription
            {
                AlphaToCoverageEnable = false,
                IndependentBlendEnable = false
            };
            for (int x = 0; x < blend_description.RenderTarget.Length; ++x)
            {
                blend_description.RenderTarget[x] = new RenderTargetBlendDescription
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
            var blendState = nativeDevice.CreateBlendState(blend_description);

            return new SwapChain(samplerState, blendState, swapchain, rtv);
        }
    }
}
