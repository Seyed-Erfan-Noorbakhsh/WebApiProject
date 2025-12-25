namespace Shop_ProjForWeb.Core.Domain.Entities;


public class RolePermission : BaseEntity
{
    public int RoleId { get; set; }
    public int PermissionId { get; set; }
    
    // Navigation properties
    public Role Role { get; set; } = null!;
    public Permission Permission { get; set; } = null!;
}

