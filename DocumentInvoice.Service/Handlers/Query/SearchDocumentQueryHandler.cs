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
using System.Runtime.CompilerServices;

namespace DocumentInvoice.Service.Handlers.Query;

public class SearchDocumentQueryHandler : IRequestHandler<SearchDocumentQuery, List<DocumentResponse>>
{
    private readonly IRepositoryFactory<DocumentInvoiceContext> _repository;
    private readonly IRepository<Document> _documentRepository;
    private readonly IRepository<DocumentTag> _documentTagRepository;
    private readonly SearchClient _searchClient;

    public SearchDocumentQueryHandler(IRepositoryFactory<DocumentInvoiceContext> repository, SearchClient searchClient)
    {
        _repository = repository;
        _documentRepository = _repository.GetRepository<Document>();
        _documentTagRepository = _repository.GetRepository<DocumentTag>();
        _searchClient = searchClient;
    }
    public async Task<List<DocumentResponse>> Handle(SearchDocumentQuery request, CancellationToken cancellationToken)
    {
        // Perform the search
        SearchResults<DocumentSearch> response = _searchClient.Search<DocumentSearch>(request.Query);

        var documentIds = response.GetResults().Select(x => x.Document.Id).ToList();
        List<DocumentResponse> documents = null;
        if(request.RBACInfo.IsAdminOrAccountant)
        {
            documents = await _documentRepository.Query.GroupJoin(
                _documentTagRepository.Query,
                x=>x.Id,
                y=>y.Document.Id,
                (x,y)=> new {Document = x, Tags = y})
                .Where(a => documentIds.Contains(a.Document.Id)).Select(x => new DocumentResponse
                {
                    Id = x.Document.Id,
                    Commnet = x.Document.Comment,
                    DocumentCategory = (Enums.DocumentCategory)x.Document.DocumentCategory,
                    DocumentName = x.Document.DocumentName,
                    DocumentStatus = (Enums.DocumentStatus)x.Document.DocumentStatus,
                    Tags = x.Tags.Select(x=> new TagResponse 
                    { 
                        Tag= x.Tag,
                        TagId = x.Id
                    }).ToArray()
                }).ToListAsync(cancellationToken);
        }
        else
        {
            documents = await _documentRepository.Query.GroupJoin(
                _documentTagRepository.Query,
                x => x.Id,
                y => y.Document.Id,
                (x, y) => new { Document = x, Tags = y })
                .Where(a => documentIds.Contains(a.Document.Id) && request.RBACInfo.UserCompanyIdList.Contains(a.Document.Id)).Select(x => new DocumentResponse
                {
                    Id = x.Document.Id,
                    Commnet = x.Document.Comment,
                    DocumentCategory = (Enums.DocumentCategory)x.Document.DocumentCategory,
                    DocumentName = x.Document.DocumentName,
                    DocumentStatus = (Enums.DocumentStatus)x.Document.DocumentStatus,
                    Tags = x.Tags.Where(t => t.IsActive).Select(x => new TagResponse
                    {
                        Tag = x.Tag,
                        TagId = x.Id
                    }).ToArray()
                }).ToListAsync(cancellationToken);
        }

        return documents;
    }
}
