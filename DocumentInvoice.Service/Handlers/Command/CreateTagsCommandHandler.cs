using DocumentInvoice.Domain;
using DocumentInvoice.Infrastructure;
using DocumentInvoice.Infrastructure.Repository;
using DocumentInvoice.Service.Command;
using DocumentInvoice.Service.DTO;
using DocumentInvoice.Service.Enums;
using MediatR;

namespace DocumentInvoice.Service.Handlers.Command
{
    public class CreateTagsCommandHandler : IRequestHandler<CreateTagsCommand, DocumentResponse>
    {
        private readonly IRepositoryFactory<DocumentInvoiceContext> _repository;
        private readonly IRepository<Document> _documentRepository;
        private readonly IRepository<DocumentTag> _documentTagRepository;

        public CreateTagsCommandHandler(IRepositoryFactory<DocumentInvoiceContext> repository)
        {
            _repository = repository;
            _documentRepository = repository.GetRepository<Document>();
            _documentTagRepository = repository.GetRepository<DocumentTag>();
        }

        public async Task<DocumentResponse> Handle(CreateTagsCommand request, CancellationToken cancellationToken)
        {
            var document = _documentRepository.Query.Where(x => x.Id == request.DocumentId).FirstOrDefault();

            if (document == null)
            {
                throw new KeyNotFoundException("Document not exists");
            }
            List<DocumentTag> tags = new List<DocumentTag>();
            foreach (var tag in request.Tags)
            {
                tags.Add(new DocumentTag
                {
                    Document = document,
                    Tag = tag,
                    IsActive = true
                });
            }

            await _documentTagRepository.AddRangeAsync(tags, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);

            return new DocumentResponse
            {
                Id = document.Id,
                Commnet = document.Comment,
                DocumentCategory = (DocumentCategory)document.DocumentCategory,
                DocumentName = document.DocumentName,
                DocumentStatus = (DocumentStatus)document.DocumentStatus,
                Tags = tags.Select(x => new TagResponse
                {
                    Tag = x.Tag,
                    TagId = x.Id
                }).ToArray()
            };
        }
    }
}
