namespace Shop_ProjForWeb.Core.Application.Interfaces;

using Shop_ProjForWeb.Core.Domain.Entities;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id);
    Task UpdateAsync(User user);
}
