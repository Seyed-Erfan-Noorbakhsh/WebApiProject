namespace Shop_ProjForWeb.Core.Domain.Entities;


public class User : BaseEntity
{
    public string FullName { get; set; }
    public bool IsVip { get; set; }
}
