using System.ComponentModel.DataAnnotations.Schema;

namespace Shop_ProjForWeb.Core.Domain.Entities;

public class User : BaseEntity
{
    public required string FullName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }

}