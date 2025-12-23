namespace Shop_ProjForWeb.Core.Application.Interfaces;

using Shop_ProjForWeb.Core.Domain.Entities;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id);
}
