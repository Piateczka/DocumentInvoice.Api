using MediatR;

namespace DocumentInvoice.Service
{
    public abstract class BaseRequest<TResponse> : IRequest<TResponse>
    {
        public List<int>? CompanyId { get; set; }
        public bool IsAdmin { get; set; }
    }
}
