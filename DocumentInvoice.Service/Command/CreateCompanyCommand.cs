using DocumentInvoice.Service.DTO;
using MediatR;

namespace DocumentInvoice.Service.Command;

public class CreateCompanyCommand :IRequest<CreateCompanyResponse>
{
    public string Name { get; set; }
}
