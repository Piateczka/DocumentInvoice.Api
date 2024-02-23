using System.ComponentModel.DataAnnotations.Schema;

namespace DocumentInvoice.Domain;

public class UserRoles
{
    public int Id { get; set; }
    [ForeignKey("User")]
    public int UserId { get; set; }
    public Users User { get; set; }

    [ForeignKey("Roles")]
    public int RoleId { get; set; }
    public Roles Roles { get; set; }
}
