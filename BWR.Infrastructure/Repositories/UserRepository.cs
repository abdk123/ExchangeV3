using BWR.Domain.Model.Security;
using BWR.Domain.Repositories;
using BWR.Infrastructure.Common;
using BWR.Infrastructure.Context;
using BWR.ShareKernel.Interfaces;

namespace BWR.Infrastructure.Repositories
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        public UserRepository(IUnitOfWork<MainContext> unitOfWork)
        : base(unitOfWork.Context)
        {
        }

        public UserRepository(MainContext context) : base(context)
        {

        }

    }
}
