using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vortice.Direct3D11;

namespace HuiDesktop.DirectComposition.DirectX
{
    public sealed class Binder : IDisposable
    {
        private readonly IBindable resource;

        public Binder(IBindable resource, ID3D11DeviceContext ctx)
        {
            this.resource = resource;
            resource.Bind(ctx);
        }

        public void Dispose()
        {
            resource.Unbind();
        }
    }

    public interface IBindable
    {
        void Bind(ID3D11DeviceContext ctx);
        void Unbind();
    }
}
