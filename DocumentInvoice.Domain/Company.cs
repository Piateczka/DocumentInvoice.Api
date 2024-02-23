namespace DocumentInvoice.Domain;

public class Company
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string ContainerName { get; set; }
    public List<UserCompaniesAccess> UserAccessList { get; set; }
}
