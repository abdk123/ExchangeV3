using BWR.Domain.Model.Security;
using BWR.ShareKernel.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace BWR.Domain.Repositories
{
    public interface IExternalLoginRepository : IGenericRepository<ExternalLogin>
    {
        ExternalLogin GetByProviderAndKey(string loginProvider, string providerKey);
        Task<ExternalLogin> GetByProviderAndKeyAsync(string loginProvider, string providerKey);
        Task<ExternalLogin> GetByProviderAndKeyAsync(CancellationToken cancellationToken, string loginProvider, string providerKey);
    }
}
