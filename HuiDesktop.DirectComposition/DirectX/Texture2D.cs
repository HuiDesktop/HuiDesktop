using System;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace HuiDesktop.DirectComposition.DirectX
{
    sealed class MutexLocker : IDisposable
    {
        private readonly ulong key;
        private readonly IDXGIKeyedMutex keyedMutex;

        public MutexLocker(ulong key, IDXGIKeyedMutex keyedMutex, int waitMilliseconds = int.MaxValue)
        {
            this.key = key;
            this.keyedMutex = keyedMutex;

            if (null != keyedMutex)
            {
                keyedMutex.AcquireSync(key, waitMilliseconds);
            }
        }

        public void Dispose()
        {
            keyedMutex.ReleaseSync(key);
        }
    }

    public class Texture2D : IBindable
    {
        private ID3D11Texture2D texture;
        private ID3D11ShaderResourceView shaderResourceView;
        //private IDXGIKeyedMutex keyedMutex;

        public int Width => texture.Description.Width;
        public int Height => texture.Description.Height;
        public Format Format => texture.Description.Format;
        public IntPtr SharedHandler { get; }
        //public bool HasMutex => keyedMutex is not null;

        public Texture2D(ID3D11Texture2D texture, ID3D11ShaderResourceView shaderResourceView, IntPtr sharedHandler = default)
        {
            this.texture = texture;
            this.shaderResourceView = shaderResourceView;
            this.SharedHandler = sharedHandler;

            //keyedMutex = texture.QueryInterfaceOrNull<IDXGIKeyedMutex>();
        }

        public void Bind(ID3D11DeviceContext ctx)
        {
            if (null != shaderResourceView)
            {
                ctx.PSSetShaderResource(0, shaderResourceView);
            }
        }

        public void Unbind()
        {
        }
    }
}
