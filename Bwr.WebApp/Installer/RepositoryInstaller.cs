using System;
using BWR.Application.AppServices.Branch;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Microsoft.AspNet.Identity;
using BWR.Infrastructure.Common;
using BWR.Infrastructure.Context;
using IdentityUser = Bwr.WebApp.Identity.IdentityUser;
using BWR.ShareKernel.Interfaces;
using Bwr.WebApp.Identity;
using BWR.Domain.Repositories;
using BWR.Infrastructure.Repositories;
using BWR.Application.Interfaces.Setting;
using BWR.Application.AppServices.Setting;
using BWR.Application.Interfaces.Shared;
using Bwr.WebApp.Models.Security;
using BWR.Application.AppServices.Companies;
using BWR.Application.Interfaces.Company;
using BWR.Application.Interfaces.Security;
using BWR.Application.AppServices.Security;
using BWR.Application.Interfaces.Client;
using BWR.Application.AppServices.Clients;
using BWR.Application.Interfaces.Branch;
using BWR.Application.AppServices.Treasuries;
using BWR.Application.Interfaces.Treasury;
using BWR.Application.Interfaces.TreasuryMoneyAction;
using BWR.Application.Interfaces.Common;
using BWR.Application.AppServices.Common;
using BWR.Application.Interfaces.Transaction;
using BWR.Application.AppServices.Transactions;
using BWR.Application.Interfaces.BranchCashFlow;
using BWR.Application.AppServices.Branches;
using BWR.Application.AppServices.BoxActions;
using BWR.Application.Interfaces.BoxAction;
using BWR.Application.Interfaces;

namespace Bwr.WebApp.Installer
{
    public class RepositoryInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(Component.For(typeof(IGenericRepository<>)).ImplementedBy(typeof(GenericRepository<>)).LifestyleTransient());
            container.Register(Component.For(typeof(IUnitOfWork<MainContext>)).ImplementedBy(typeof(UnitOfWork<MainContext>)).LifestyleTransient());
            container.Register(Component.For(typeof(IUserStore<IdentityUser, Guid>)).ImplementedBy(typeof(UserStore)).LifestyleTransient());
            container.Register(Component.For(typeof(UserManager<IdentityUser, Guid>)).LifestyleTransient());
            container.Register(Component.For(typeof(RoleManager<IdentityRole, Guid>)).LifestyleTransient());
            container.Register(Component.For(typeof(RoleStore)).LifestyleTransient());
            container.Register(Component.For<IAppSession>().ImplementedBy<AppSession>());
            //=========
            container.Register(Component.For<IRoleRepository>().ImplementedBy<RoleRepository>().LifestyleTransient());
            container.Register(Component.For<IUserRepository>().ImplementedBy<UserRepository>().LifestyleTransient());
            container.Register(Component.For<IExternalLoginRepository>().ImplementedBy<ExternalLoginRepository>().LifestyleTransient());

            //==========
            container.Register(Component.For<ICountryAppService>().ImplementedBy<CountryAppService>().LifestyleTransient());
            container.Register(Component.For<IProvinceAppService>().ImplementedBy<ProvinceAppService>().LifestyleTransient());
            container.Register(Component.For<ICoinAppService>().ImplementedBy<CoinAppService>().LifestyleTransient());
            container.Register(Component.For<ICompanyAppService>().ImplementedBy<CompanyAppService>().LifestyleTransient());
            container.Register(Component.For<ICompanyCashAppService>().ImplementedBy<CompanyCashAppService>().LifestyleTransient());
            container.Register(Component.For<ICompanyCountryAppService>().ImplementedBy<CompanyCountryAppService>().LifestyleTransient());
            container.Register(Component.For<IPublicIncomeAppService>().ImplementedBy<PublicIncomeAppService>().LifestyleTransient());
            container.Register(Component.For<IPublicExpenseAppService>().ImplementedBy<PublicExpenseAppService>().LifestyleTransient());
            container.Register(Component.For<IAttachmentAppService>().ImplementedBy<AttachmentAppService>().LifestyleTransient());
            container.Register(Component.For<IRoleAppService>().ImplementedBy<RoleAppService>().LifestyleTransient());
            container.Register(Component.For<IUserAppService>().ImplementedBy<UserAppService>().LifestyleTransient());
            container.Register(Component.For<IPermissionAppService>().ImplementedBy<PermissionAppService>().LifestyleTransient());
            container.Register(Component.For<IMoneyActionAppService>().ImplementedBy<MoneyActionAppService>().LifestyleTransient());
            container.Register(Component.For<IClientAppService>().ImplementedBy<ClientAppService>().LifestyleTransient());
            container.Register(Component.For<IClientCashAppService>().ImplementedBy<ClientCashAppService>().LifestyleTransient());
            container.Register(Component.For<IClientPhoneAppService>().ImplementedBy<ClientPhoneAppService>().LifestyleTransient());
            container.Register(Component.For<IBranchCommissionAppService>().ImplementedBy<BranchCommissionAppService>().LifestyleTransient());
            container.Register(Component.For<ICompanyCommissionAppService>().ImplementedBy<CompanyCommissionAppService>().LifestyleTransient());
            container.Register(Component.For<ICompanyCashFlowAppService>().ImplementedBy<CompanyCashFlowAppService>().LifestyleTransient());
            container.Register(Component.For<ITreasuryAppService>().ImplementedBy<TreasuryAppService>().LifestyleTransient());
            container.Register(Component.For<ITreasuryCashAppService>().ImplementedBy<TreasuryCashAppService>().LifestyleTransient());
            container.Register(Component.For<IBranchCashAppService>().ImplementedBy<BranchCashAppService>().LifestyleTransient());
            container.Register(Component.For<ITreasuryMoneyActionAppService>().ImplementedBy<TreasuryMoneyActionAppService>().LifestyleTransient());
            container.Register(Component.For<IOuterTransactionAppService>().ImplementedBy<OuterTransactionAppService>().LifestyleTransient());
            container.Register(Component.For<IInnerTransactionAppService>().ImplementedBy<InnerTransactionAppService>().LifestyleTransient());
            container.Register(Component.For<ITransactionAppService>().ImplementedBy<TransactionAppService>().LifestyleTransient());
            container.Register(Component.For<IClientAttatchmentAppService>().ImplementedBy<ClientAttatchmentAppService>().LifestyleTransient());
            container.Register(Component.For<IClientCashFlowAppService>().ImplementedBy<ClientCashFlowAppService>().LifestyleTransient());
            container.Register(Component.For<IBranchCashFlowAppService>().ImplementedBy<BranchCashFlowAppService>().LifestyleTransient());
            container.Register(Component.For<IBoxActionAppService>().ImplementedBy<BoxActionAppService>().LifestyleTransient());
            container.Register(Component.For<IStatementAppService>().ImplementedBy<StatementAppService>().LifestyleTransient());
            container.Register(Component.For<IExchangeAppService>().ImplementedBy<ExchangeAppService>().LifestyleTransient());

            //
            //
            //

        }
    }

    
}