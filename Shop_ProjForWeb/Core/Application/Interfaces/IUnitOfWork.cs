using System.Threading;
using System.Threading.Tasks;

namespace Shop_ProjForWeb.Core.Application.Interfaces
{
    public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
