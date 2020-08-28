using BWR.Domain.Model.Security;
using BWR.Domain.Repositories;
using BWR.Infrastructure.Common;
using BWR.Infrastructure.Context;
using BWR.ShareKernel.Interfaces;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BWR.Infrastructure.Repositories
{
    public class ExternalLoginRepository : GenericRepository<ExternalLogin>, IExternalLoginRepository
    {
        public ExternalLoginRepository(IUnitOfWork<MainContext> unitOfWork)
        : base(unitOfWork.Context)
        {
        }
        public ExternalLoginRepository(MainContext context) : base(context)
        {

        }

        public ExternalLogin GetByProviderAndKey(string loginProvider, string providerKey)
        {
            return Entities.FirstOrDefault(x => x.LoginProvider == loginProvider && x.ProviderKey == providerKey);
        }

        public Task<ExternalLogin> GetByProviderAndKeyAsync(string loginProvider, string providerKey)
        {
            return Entities.FirstOrDefaultAsync(x => x.LoginProvider == loginProvider && x.ProviderKey == providerKey);
        }

        public Task<ExternalLogin> GetByProviderAndKeyAsync(CancellationToken cancellationToken, string loginProvider, string providerKey)
        {
            return Entities.FirstOrDefaultAsync(x => x.LoginProvider == loginProvider && x.ProviderKey == providerKey, cancellationToken);
        }
    }
}
