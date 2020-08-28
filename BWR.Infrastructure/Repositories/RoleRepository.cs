using BWR.Domain.Model.Security;
using BWR.Domain.Repositories;
using BWR.Infrastructure.Common;
using BWR.Infrastructure.Context;
using BWR.ShareKernel.Interfaces;

namespace BWR.Infrastructure.Repositories
{
    public class RoleRepository : GenericRepository<Role>, IRoleRepository
    {
        public RoleRepository(IUnitOfWork<MainContext> unitOfWork)
        : base(unitOfWork.Context)
        {
        }

        public RoleRepository(MainContext context) : base(context)
        {

        }

    }
}
