using DocumentInvoice.Infrastructure.Repository;
using DocumentInvoice.Infrastructure;
using DocumentInvoice.Service.Command;
using DocumentInvoice.Service.DTO;
using MediatR;
using DocumentInvoice.Domain;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using Microsoft.EntityFrameworkCore;
using DocumentInvoice.Service.Enums;

namespace DocumentInvoice.Service.Handlers.Command;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, CreateUserResponse>
{
    private readonly IRepositoryFactory<DocumentInvoiceContext> _repositoryFactory;
    private readonly IRepository<Users> _userRepo;
    private readonly IRepository<Roles> _roleRepo;
    private readonly IRepository<Company> _companyRepo;
    private readonly IRepository<UserCompaniesAccess> _userCompaniesAccessRepo;

    public CreateUserCommandHandler(IRepositoryFactory<DocumentInvoiceContext> repositoryFactory)
    {
        _repositoryFactory = repositoryFactory;
        _userRepo = _repositoryFactory.GetRepository<Users>();
        _companyRepo = _repositoryFactory.GetRepository<Company>();
        _userCompaniesAccessRepo = _repositoryFactory.GetRepository<UserCompaniesAccess>();
        _roleRepo = _repositoryFactory.GetRepository<Roles>();
    }

    public async Task<CreateUserResponse> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var role = await _roleRepo.Query.FirstOrDefaultAsync(x => x.Id == (int)request.Role);
        if(role == null)
        {
            throw new Exception($"{request.Role} role not exists");
        }

        var user = new Users
        {
            Email = request.Email,
            Name = request.UserName,
            Role = role
        };

        if(role.Name == UserRole.User.ToString())
        {
            var companies = await _companyRepo.Query.Where(x => request.CompaniesId.Contains(x.Id)).ToListAsync(cancellationToken);

            var userCompaniesAccesses = companies.Select(company => new UserCompaniesAccess
            {
                User = user,
                Company = company
            }).ToList();

            await _userRepo.AddAsync(user, cancellationToken);
            await _userCompaniesAccessRepo.AddRangeAsync(userCompaniesAccesses, cancellationToken);
        }

        await _userRepo.AddAsync(user, cancellationToken);

        await _repositoryFactory.SaveChangesAsync(cancellationToken);

        return new CreateUserResponse
        {
            UserName = user.Name,
            Email = user.Email,
        };


    }
}
