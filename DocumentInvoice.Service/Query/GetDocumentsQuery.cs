using DocumentInvoice.Service.DTO;
using MediatR;

namespace DocumentInvoice.Service.Query;

public class GetDocumentsQuery : Base<List<DocumentResponse>>
{
}
