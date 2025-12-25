using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Shop_ProjForWeb.Domain.Entities;
using Shop_ProjForWeb.Domain.Interfaces;
using Shop_ProjForWeb.Infrastructure.Data;

namespace Shop_ProjForWeb.Infrastructure.Repositories;

public class Repository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(Guid id)
    {
        return await _dbSet.FindAsync(id);
    }


    public async Task AddAsync(T entity)
        => await _dbSet.AddAsync(entity);

     public void Update(T entity)
        => _dbSet.Update(entity);
    
    public void Remove(T entity)
        => _dbSet.Remove(entity);
}

//     public async Task DeleteAsync(T entity)
//     {
//         entity.IsDeleted = true;
//         entity.UpdatedAt = DateTime.UtcNow;
//         await UpdateAsync(entity);
//     }

//     public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
//     {
//         return await _dbSet.AnyAsync(predicate);
//     }
// }

