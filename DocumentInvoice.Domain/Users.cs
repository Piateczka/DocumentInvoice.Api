using System.ComponentModel.DataAnnotations.Schema;

namespace DocumentInvoice.Domain;

public class Users
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    [ForeignKey("RoleId")]
    public Roles Role { get; set; }
    public List<UserCompaniesAccess> UserAccessList { get; set; }
}
