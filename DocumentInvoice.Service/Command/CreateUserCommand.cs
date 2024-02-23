using DocumentInvoice.Service.DTO;
using DocumentInvoice.Service.Enums;
using MediatR;
using System.Text.Json.Serialization;

namespace DocumentInvoice.Service.Command;

public class CreateUserCommand : IRequest<CreateUserResponse>
{
    public string UserName { get; set; }
    public string Email { get; set; }
    public UserRole Role { get; set; }
    public List<int> CompaniesId { get; set; }
}
