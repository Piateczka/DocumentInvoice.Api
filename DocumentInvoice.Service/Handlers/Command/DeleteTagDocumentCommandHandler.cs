using Azure.Storage.Blobs;
using DocumentInvoice.Domain;
using DocumentInvoice.Infrastructure;
using DocumentInvoice.Infrastructure.Repository;
using DocumentInvoice.Service.Command;
using MediatR;
using Microsoft.EntityFrameworkCore;

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

            documentTag = await _documentTagRepository.Query
                .FirstOrDefaultAsync(x => x.Id == request.TagId);

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
