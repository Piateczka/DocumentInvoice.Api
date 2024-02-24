using DocumentInvoice.Service.DTO;
using MediatR;

namespace DocumentInvoice.Service.Query;

public class GetDocumentsQuery : BaseRequest<List<DocumentResponse>>
{
}
