using Azure.Search.Documents;
using DocumentInvoice.Infrastructure.Repository;
using DocumentInvoice.Infrastructure;
using DocumentInvoice.Service.DTO;
using DocumentInvoice.Service.Query;
using MediatR;
using DocumentInvoice.Domain;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace DocumentInvoice.Service.Handlers.Query
{
    public class FilterDocumentQueryHandler : IRequestHandler<FilterDocumentQuery, List<DocumentResponse>>
    {
        private readonly IRepositoryFactory<DocumentInvoiceContext> _repository;
        private readonly IRepository<Document> _documentRepository;
        private readonly IRepository<DocumentTag> _documentTagRepository;

        public FilterDocumentQueryHandler(IRepositoryFactory<DocumentInvoiceContext> repository)
        {
            _repository = repository;
            _documentRepository = _repository.GetRepository<Document>();
            _documentTagRepository = _repository.GetRepository<DocumentTag>();
        }
        public async Task<List<DocumentResponse>> Handle(FilterDocumentQuery request, CancellationToken cancellationToken)
        {
            IEnumerable<Document> documents = Enumerable.Empty<Document>();

            if (!string.IsNullOrEmpty(request.Tag))
            {
                var documentTag = await _documentTagRepository.Query
                    .Where(dt => dt.Tag == request.Tag)
                    .Select(dt => dt.Document).ToListAsync();
                documents = documents.Concat(documentTag);
            }

            if(request.Month > 0)
            {
                var documentsByMonth = await _documentRepository.Query
                    .Where(x => x.Month == request.Month.ToString()).ToListAsync();
                documents = documents.Concat(documentsByMonth);
            }

            if (request.Year > 0)
            {
                var documentsByYear = await _documentRepository.Query
                    .Where(x => x.Year == request.Year.ToString()).ToListAsync();
                documents = documents.Concat(documentsByYear);
            }

            if (!request.RBACInfo.IsAdminOrAccountant)
            {
                documents = documents.Where(x => request.RBACInfo.UserCompanyIdList.Contains(x.CompanyId));
            }

            var documentResponse = documents.Distinct().Select(x => new DocumentResponse
            {
                Id = x.Id,
                Commnet = x.Comment,
                DocumentCategory = (Enums.DocumentCategory?)x.DocumentCategory,
                DocumentStatus = (Enums.DocumentStatus?)x.DocumentStatus,
                DocumentName = x.DocumentName,
            }).ToList();

            return documentResponse;

        }
    }
}
