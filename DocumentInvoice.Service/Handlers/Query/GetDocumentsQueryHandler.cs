using DocumentInvoice.Domain;
using DocumentInvoice.Infrastructure;
using DocumentInvoice.Infrastructure.Repository;
using DocumentInvoice.Service.DTO;
using DocumentInvoice.Service.Query;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DocumentInvoice.Service.Handlers.Query;

public class GetDocumentsQueryHandler : IRequestHandler<GetDocumentsQuery, List<DocumentResponse>>
{
    private readonly IRepositoryFactory<DocumentInvoiceContext> _repository;
    private readonly IRepository<Document> _documentRepository;

    public GetDocumentsQueryHandler(IRepositoryFactory<DocumentInvoiceContext> repository)
    {
        _repository = repository;
        _documentRepository = _repository.GetRepository<Document>();
    }
    public async Task<List<DocumentResponse>> Handle(GetDocumentsQuery request, CancellationToken cancellationToken)
    {
        List<DocumentResponse> documentsList = new List<DocumentResponse>();
        List<Document> documents = new List<Document>();
        if (request.IsAdmin)
        {
            documents = await _documentRepository.Query.ToListAsync(cancellationToken);
        }
        else
        {
            documents = await _documentRepository.Query.Where(x => request.CompanyId.Contains(x.CompanyId)).ToListAsync(cancellationToken);
        }


        foreach (var document in documents)
        {
            documentsList.Add(new DocumentResponse
            {
                Id = document.Id,
                DocumentName = document.DocumentName,
                DocumentCategory = (Enums.DocumentCategory)document.DocumentCategory,
                DocumentStatus = (Enums.DocumentStatus)document.DocumentStatus,
                Commnet = document.Comment
            });
        }

        return documentsList;
    }
}
