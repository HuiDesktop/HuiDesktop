using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;
using static Vortice.Direct3D11.D3D11;
using Vortice.D3DCompiler;

namespace HuiDesktop.DirectComposition.DirectX
{
    public partial class Device
    {
        #region Default Effect Code
        private const string VertexCode = @"struct VS_INPUT
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
        private const string PixelCode = @"Texture2D tex0 : register(t0);
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
        #endregion
        private static readonly FeatureLevel[] featureLevels = new FeatureLevel[]
        {
            FeatureLevel.Level_11_0,
            FeatureLevel.Level_11_1
        };

        private readonly ID3D11Device device;
        //private readonly ID3D11DeviceContext ctx;

        //public ID3D11DeviceContext ImmedidateContext => ctx;

        public Device(out ID3D11DeviceContext ctx)
        {
            D3D11CreateDevice(adapterPtr: IntPtr.Zero,
                              driverType: DriverType.Hardware,
                              flags: DeviceCreationFlags.BgraSupport,
                              featureLevels: featureLevels,
                              device: out device,
                              immediateContext: out ctx).CheckError();
        }

        public Texture2D OpenSharedResource(IntPtr sharedHandle)
        {
            var texture = device.OpenSharedResource<ID3D11Texture2D>(sharedHandle);
            ShaderResourceViewDescription? desc = null;
            if ((texture.Description.BindFlags & BindFlags.ShaderResource) != 0)
            {
                desc = new ShaderResourceViewDescription
                {
                    Format = Format.B8G8R8A8_UNorm,
                    ViewDimension = ShaderResourceViewDimension.Texture2D,
                    Texture2D = new Texture2DShaderResourceView { MipLevels = 1, MostDetailedMip = 0 }
                };
            }
            var shaderResourceView = device.CreateShaderResourceView(texture.QueryInterface<ID3D11Resource>(), desc);
            return new Texture2D(texture, shaderResourceView, sharedHandle);
        }

        public Blob CompileShader(string source, string entry, string model)
        {
            Compiler.Compile(source, entry, string.Empty, model, out Blob blob, out _).CheckError();
            return blob;
        }

        public void InitDC(IntPtr handle, SwapChain swapChain)
        {
            var dc_device = new DCompositionDevice(device.QueryInterface<IDXGIDevice>());
            var dc_target = dc_device.CreateTargetForHwnd(handle, true);
            var dc_visual = dc_device.CreateVisual();
            swapChain.SetDCVisualContent(dc_visual);
            dc_target.SetRoot(dc_visual).CheckError();
            dc_device.Commit().CheckError();
        }
    }
}
