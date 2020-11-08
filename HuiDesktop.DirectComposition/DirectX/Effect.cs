using Vortice.Direct3D11;
using Vortice.DXGI;

namespace HuiDesktop.DirectComposition.DirectX
{
    public class Effect : IBindable
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

    partial class Device
    {
        public Effect CreateEffect(string vertexCode, string vertexEntry, string vertexModel, string pixelCode, string pixelEntry, string pixelModel)
        {
            var vsBlob = CompileShader(vertexCode, vertexEntry, vertexModel);
            var vshader = nativeDevice.CreateVertexShader(vsBlob.BufferPointer, vsBlob.BufferSize);
            var layout = nativeDevice.CreateInputLayout(new InputElementDescription[]
            {
                new InputElementDescription
                {
                    SemanticName = "POSITION",
                    SemanticIndex = 0,
                    Format = Format.R32G32B32_Float,
                    Slot = 0,
                    AlignedByteOffset = 0,
                    Classification = InputClassification.PerVertexData,
                    InstanceDataStepRate = 0
                },
                new InputElementDescription
                {
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 0,
                    Format = Format.R32G32_Float,
                    Slot = 0,
                    AlignedByteOffset = 12,
                    Classification = InputClassification.PerVertexData,
                    InstanceDataStepRate = 0
                }
            }, vsBlob);
            var psBlob = CompileShader(pixelCode, pixelEntry, pixelModel);
            var pshader = nativeDevice.CreatePixelShader(psBlob.BufferPointer, psBlob.BufferSize);
            return new Effect(layout, vshader, pshader);
        }

        public Effect CreateDefaultEffect()
        {
            return CreateEffect(VertexCode, "main", "vs_4_0", PixelCode, "main", "ps_4_0");
        }
    }
}
