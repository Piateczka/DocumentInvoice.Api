using DocumentInvoice.Domain;
using DocumentInvoice.Infrastructure;
using DocumentInvoice.Infrastructure.Repository;
using DocumentInvoice.Service.Command;
using DocumentInvoice.Service.DTO;
using MediatR;

namespace DocumentInvoice.Service.Handlers.Command
{
    public class CreateCustomerCommandHandler : IRequestHandler<CreateCompanyCommand, CreateCompanyResponse>
    {

        private readonly IRepositoryFactory<DocumentInvoiceContext> _repositoryFactory;
        private readonly IRepository<Company> _customerRepo;

        public CreateCustomerCommandHandler(IRepositoryFactory<DocumentInvoiceContext> repositoryFactory)
        {
            _repositoryFactory = repositoryFactory;
            _customerRepo = _repositoryFactory.GetRepository<Company>();
        }

        public async Task<CreateCompanyResponse> Handle(CreateCompanyCommand request, CancellationToken cancellationToken)
        {
            var company = new Company
            {
                Name = request.Name,
                ContainerName = request.Name.Replace(" ", "").ToLower()
            };
            await _customerRepo.AddAsync(company, cancellationToken);

            await _repositoryFactory.SaveChangesAsync(cancellationToken);

            return new CreateCompanyResponse
            {
                Id = company.Id,
                CompanyName = company.Name,
            };


        }
    }
}
