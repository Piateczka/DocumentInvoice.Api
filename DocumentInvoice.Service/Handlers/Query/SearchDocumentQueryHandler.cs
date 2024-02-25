using Azure.Search.Documents;
using Azure;
using Azure.Search.Documents.Indexes;
using DocumentInvoice.Service.Query;
using MediatR;
using Azure.Search.Documents.Models;
using DocumentInvoice.Service.DTO;
using DocumentInvoice.Domain;
using DocumentInvoice.Infrastructure.Repository;
using DocumentInvoice.Infrastructure;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;

namespace DocumentInvoice.Service.Handlers.Query;

public class SearchDocumentQueryHandler : IRequestHandler<SearchDocumentQuery, List<DocumentResponse>>
{
    private readonly IRepositoryFactory<DocumentInvoiceContext> _repository;
    private readonly IRepository<Document> _documentRepository;
    private readonly SearchClient _searchClient;

    public SearchDocumentQueryHandler(IRepositoryFactory<DocumentInvoiceContext> repository, SearchClient searchClient)
    {
        _repository = repository;
        _documentRepository = _repository.GetRepository<Document>();
        _searchClient = searchClient;
    }
    public async Task<List<DocumentResponse>> Handle(SearchDocumentQuery request, CancellationToken cancellationToken)
    {
        // Perform the search
        SearchResults<DocumentSearch> response = _searchClient.Search<DocumentSearch>(request.Query);

        var documentIds = response.GetResults().Select(x => int.Parse(x.Document.Id)).ToList();
        List< DocumentResponse> documents = null;
        if(request.RBACInfo.IsAdminOrAccountant)
        {
            documents = await _documentRepository.Query.Where(x => documentIds.Contains(x.Id)).Select(x => new DocumentResponse
            {
                Id = x.Id,
                Commnet = x.Comment,
                DocumentCategory = (Enums.DocumentCategory)x.DocumentCategory,
                DocumentName = x.DocumentName,
                DocumentStatus = (Enums.DocumentStatus)x.DocumentStatus,
            }).ToListAsync(cancellationToken);
        }
        else
        {
            documents = await _documentRepository.Query.Where(x => documentIds.Contains(x.Id) && request.RBACInfo.UserCompanyIdList.Contains(x.Customer.Id)).Select(x => new DocumentResponse
            {
                Id = x.Id,
                Commnet = x.Comment,
                DocumentCategory = (Enums.DocumentCategory)x.DocumentCategory,
                DocumentName = x.DocumentName,
                DocumentStatus = (Enums.DocumentStatus)x.DocumentStatus,
            }).ToListAsync(cancellationToken);
        }

        return documents;
    }
}
