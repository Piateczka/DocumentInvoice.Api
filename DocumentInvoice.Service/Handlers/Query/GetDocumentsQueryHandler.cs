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
    private readonly IRepository<DocumentTag> _documentTagRepository;

    public GetDocumentsQueryHandler(IRepositoryFactory<DocumentInvoiceContext> repository)
    {
        _repository = repository;
        _documentRepository = _repository.GetRepository<Document>();
        _documentTagRepository = _repository.GetRepository<DocumentTag>();
    }
    public async Task<List<DocumentResponse>> Handle(GetDocumentsQuery request, CancellationToken cancellationToken)
    {
        List<DocumentResponse> documents = new List<DocumentResponse>();
        if (request.RBACInfo.IsAdminOrAccountant)
        {
            documents = await _documentRepository.Query.GroupJoin(
                _documentTagRepository.Query,
                x => x.Id,
                y => y.Document.Id,
                (x, y) => new { Document = x, Tags = y })
                .Select(x => new DocumentResponse
                {
                    Id = x.Document.Id,
                    Commnet = x.Document.Comment,
                    DocumentCategory = (Enums.DocumentCategory)x.Document.DocumentCategory,
                    DocumentName = x.Document.DocumentName,
                    DocumentStatus = (Enums.DocumentStatus)x.Document.DocumentStatus,
                    ContainerName = x.Document.Container,
                    Tags = x.Tags.Where(t => t.IsActive).Select(x => new TagResponse
                    {
                        Tag = x.Tag,
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
                .Where(a => request.RBACInfo.UserCompanyIdList.Contains(a.Document.Id)).Select(x => new DocumentResponse
                {
                    Id = x.Document.Id,
                    Commnet = x.Document.Comment,
                    DocumentCategory = (Enums.DocumentCategory)x.Document.DocumentCategory,
                    DocumentName = x.Document.DocumentName,
                    DocumentStatus = (Enums.DocumentStatus)x.Document.DocumentStatus,
                    ContainerName = x.Document.Container,
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
