using System.Threading;
using System.Threading.Tasks;
using Shop_ProjForWeb.Core.Application.Interfaces;
using Shop_ProjForWeb.Infrastructure.Persistent.DbContext;

namespace Shop_ProjForWeb.Infrastructure.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly SupermarketDbContext _dbContext;

        public UnitOfWork(SupermarketDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
