using Azure.Storage.Blobs;
using DocumentInvoice.Infrastructure.Repository;
using DocumentInvoice.Infrastructure;
using DocumentInvoice.Service.Command;
using MediatR;
using DocumentInvoice.Domain;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata;
using System.Xml.Linq;

namespace DocumentInvoice.Service.Handlers.Command
{
    public class DeleteTagDocumentCommandHandler : IRequestHandler<DeleteTagDocumentCommand, Unit>
    {
        private readonly IRepositoryFactory<DocumentInvoiceContext> _repository;
        private readonly IRepository<DocumentTag> _documentTagRepository;

        public DeleteTagDocumentCommandHandler(IRepositoryFactory<DocumentInvoiceContext> repository, BlobServiceClient blobServiceClient)
        {
            _repository = repository;
            _documentTagRepository = _repository.GetRepository<DocumentTag>();
        }
        public async Task<Unit> Handle(DeleteTagDocumentCommand request, CancellationToken cancellationToken)
        {
            DocumentTag documentTag;
            if (request.IsAdmin)
            {
                documentTag = await _documentTagRepository.Query
                    .FirstOrDefaultAsync(x => x.Id == request.TagId);
            }
            else
            {
                documentTag = await _documentTagRepository.Query.Include(d=>d.Document)
                    .FirstOrDefaultAsync(x => request.CompanyId.Contains(x.Document.CompanyId) && x.Id == request.DocumentId);
            }

            if (documentTag == null)
            {
                throw new NotFoundApiException("Tag not found");
            }

            _documentTagRepository.Delete(documentTag);
            await _repository.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
