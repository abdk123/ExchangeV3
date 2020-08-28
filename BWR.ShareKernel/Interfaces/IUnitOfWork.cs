using System.Data.Entity;
using System.Threading.Tasks;

namespace BWR.ShareKernel.Interfaces
{
    public interface IUnitOfWork<out TContext> where TContext : DbContext, new()
    {
        TContext Context { get; }
        void CreateTransaction();
        void Commit();
        void Rollback();
        void Save();
        Task SaveAsync();
        IGenericRepository<TEntity> GenericRepository<TEntity>() where TEntity : class;
    }
}
