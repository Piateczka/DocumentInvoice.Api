using Azure;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using DocumentInvoice.Domain;
using DocumentInvoice.Infrastructure;
using DocumentInvoice.Infrastructure.Repository;
using DocumentInvoice.Service.Command;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DocumentInvoice.Service.Handlers.Command
{
    public class DeleteTagDocumentCommandHandler : IRequestHandler<DeleteTagDocumentCommand, Unit>
    {
        private readonly IRepositoryFactory<DocumentInvoiceContext> _repository;
        private readonly IRepository<DocumentTag> _documentTagRepository;

        public DeleteTagDocumentCommandHandler(IRepositoryFactory<DocumentInvoiceContext> repository)
        {
            _repository = repository;
            _documentTagRepository = _repository.GetRepository<DocumentTag>();
        }
        public async Task<Unit> Handle(DeleteTagDocumentCommand request, CancellationToken cancellationToken)
        {
            DocumentTag documentTag;

            var documentActiveTags = await _documentTagRepository.Query
            .Where(x => x.Document.Id == request.DocumentId && x.IsActive).ToListAsync(cancellationToken);

            documentTag = documentActiveTags.FirstOrDefault(x => x.Id == request.TagId);

            if (documentTag == null)
            {
                throw new NotFoundApiException("Tag not found");
            }

            if (documentActiveTags.Count == 1)
            {
                //When we remove all tags from document we shoulde reset index to refresh azure search
                var documentTags = await _documentTagRepository.Query
                    .Where(x => x.Document.Id == request.DocumentId).ToListAsync(cancellationToken);
                _documentTagRepository.DeleteRange(documentTags);
            }
            else
            {
                documentTag.IsActive = false;
            }

            await _repository.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
