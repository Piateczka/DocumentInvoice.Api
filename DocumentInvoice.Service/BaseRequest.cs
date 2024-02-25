using DocumentInvoice.Service.DTO;
using MediatR;

namespace DocumentInvoice.Service
{
    public abstract class BaseRequest<TResponse> : IRequest<TResponse>
    {
        public RBACInfo RBACInfo { get; set; }
    }
}
