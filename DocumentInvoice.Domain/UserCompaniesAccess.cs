namespace DocumentInvoice.Domain;

public class UserCompaniesAccess
{
    public int UserId { get; set; }
    public int CompanyId { get; set; }
    public Users User { get; set; }
    public Company Company { get; set; }
}
