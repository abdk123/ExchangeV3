using AutoMapper;
using BWR.Application.Dtos.Branch;
using BWR.Application.Dtos.Client;
using BWR.Application.Dtos.Company;
using BWR.Application.Dtos.Setting.Coin;
using BWR.Application.Dtos.Treasury;
using BWR.Application.Interfaces.Branch;
using BWR.Application.Interfaces.Client;
using BWR.Application.Interfaces.Company;
using BWR.Application.Interfaces.Setting;
using BWR.Application.Interfaces.Shared;
using BWR.Application.Interfaces.Treasury;
using BWR.Domain.Model.Branches;
using BWR.Domain.Model.Clients;
using BWR.Domain.Model.Companies;
using BWR.Domain.Model.Settings;
using BWR.Domain.Model.Treasures;
using BWR.Infrastructure.Context;
using BWR.Infrastructure.Exceptions;
using BWR.ShareKernel.Exceptions;
using BWR.ShareKernel.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BWR.Application.AppServices.Setting
{
    public class CoinAppService : ICoinAppService
    {
        private readonly IUnitOfWork<MainContext> _unitOfWork;
        private readonly IBranchCashAppService _branchCashAppService;
        private readonly ITreasuryCashAppService _treasuryCashAppService;
        private readonly ICompanyCashAppService _companyCashAppService;
        private readonly IClientCashAppService _clientCashAppService;
        private readonly IAppSession _appSession;

        public CoinAppService(IUnitOfWork<MainContext> unitOfWork,
            IBranchCashAppService branchCashAppService,
            ITreasuryCashAppService treasuryCashAppService,
            ICompanyCashAppService companyCashAppService,
            IClientCashAppService clientCashAppService,
            IAppSession appSession)
        {
            _unitOfWork = unitOfWork;
            _branchCashAppService = branchCashAppService;
            _treasuryCashAppService = treasuryCashAppService;
            _companyCashAppService = companyCashAppService;
            _clientCashAppService = clientCashAppService;
            _appSession = appSession;
        }

        public IList<CoinDto> GetAll()
        {
            var coinsDtos = new List<CoinDto>();
            try
            {
                var coins = _unitOfWork.GenericRepository<Coin>().GetAll().ToList();
                coinsDtos = Mapper.Map<List<Coin>, List<CoinDto>>(coins);
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }

            return coinsDtos;
        }

        public CoinDto GetById(int id)
        {
            CoinDto coinDto = null;
            try
            {
                var coin = _unitOfWork.GenericRepository<Coin>().GetById(id);
                if (coin != null)
                {
                    coinDto = Mapper.Map<Coin, CoinDto>(coin);
                }

            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }

            return coinDto;
        }

        public IList<CoinForDropdownDto> GetForDropdown(string name)
        {
            var coinsDtos = new List<CoinForDropdownDto>();
            try
            {
                var coins = _unitOfWork.GenericRepository<Coin>().FindBy(x => x.Name.StartsWith(name)).ToList();
                Mapper.Map<List<Coin>, List<CoinForDropdownDto>>(coins, coinsDtos);
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }

            return coinsDtos;
        }

        public CoinUpdateDto GetForEdit(int id)
        {
            CoinUpdateDto coinDto = null;
            try
            {
                var coin = _unitOfWork.GenericRepository<Coin>().GetById(id);
                if (coin != null)
                {
                    coinDto = Mapper.Map<Coin, CoinUpdateDto>(coin);
                }

            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }

            return coinDto;
        }

        public CoinDto Insert(CoinInsertDto dto)
        {
            CoinDto coinDto = null;
            try
            {
                var coin = Mapper.Map<CoinInsertDto, Coin>(dto);
                coin.CreatedBy = _appSession.GetUserName();
                coin.IsEnabled = true;
                _unitOfWork.CreateTransaction();

                _unitOfWork.GenericRepository<Coin>().Insert(coin);

                CreateBranchCashForAllBranches(coin);
                CreateTreasuryCashForAllTreasures(coin);
                CreateCompanyCashForAllCompanies(coin);
                CreateClientCashForAllClients(coin);

                _unitOfWork.Save();

                _unitOfWork.Commit();

                coinDto = Mapper.Map<Coin, CoinDto>(coin);
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
                _unitOfWork.Rollback();
            }
            
            return coinDto;
        }

        
        public CoinDto Update(CoinUpdateDto dto)
        {
            CoinDto coinDto = null;
            try
            {
                var coin = _unitOfWork.GenericRepository<Coin>().GetById(dto.Id);
                Mapper.Map<CoinUpdateDto, Coin>(dto, coin);
                coin.ModifiedBy = _appSession.GetUserName();
                _unitOfWork.CreateTransaction();

                _unitOfWork.GenericRepository<Coin>().Update(coin);
                _unitOfWork.Save();

                _unitOfWork.Commit();

                coinDto = Mapper.Map<Coin, CoinDto>(coin);
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }
            return coinDto;
        }

        public void Delete(int id)
        {
            try
            {
                var coin = _unitOfWork.GenericRepository<Coin>().GetById(id);
                if (coin != null)
                {
                    _unitOfWork.GenericRepository<Coin>().Delete(coin);
                    _unitOfWork.Save();
                }
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }
        }

        public bool CheckIfExist(string name, int id)
        {
            try
            {
                var coin = _unitOfWork.GenericRepository<Coin>()
                    .FindBy(x => x.Name.Trim().Equals(name.Trim()) && x.Id != id)
                    .FirstOrDefault();
                if (coin != null)
                    return true;
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }

            return false;
        }

        
        private void CreateBranchCashForAllBranches(Coin coin)
        {
            var branchs = _unitOfWork.GenericRepository<BWR.Domain.Model.Branches.Branch>().GetAll();
            foreach (var branch in branchs)
            {
                var branchCash = new BranchCash()
                {
                    Coin = coin,
                    BranchId = branch.Id,
                    IsEnabled = true,
                    CreatedBy = _appSession.GetUserName()
                };

                _unitOfWork.GenericRepository<BWR.Domain.Model.Branches.BranchCash>().Insert(branchCash);
            }
        }

        private void CreateTreasuryCashForAllTreasures(Coin coin)
        {
            var treasuries = _unitOfWork.GenericRepository<Treasury>().GetAll();
            foreach (var treasury in treasuries)
            {
                var treasuryCash = new TreasuryCash()
                {
                    Coin = coin,
                    TreasuryId = treasury.Id,
                    IsEnabled = true,
                    CreatedBy = _appSession.GetUserName()
                };

                _unitOfWork.GenericRepository<TreasuryCash>().Insert(treasuryCash);
            }
        }

        private void CreateCompanyCashForAllCompanies(Coin coin)
        {
            var companies = _unitOfWork.GenericRepository<Company>().GetAll();
            foreach (var company in companies)
            {
                var companyCash = new CompanyCash()
                {
                    Coin = coin,
                    CompanyId = company.Id,
                    IsEnabled = true,
                    CreatedBy = _appSession.GetUserName()
                };

                _unitOfWork.GenericRepository<CompanyCash>().Insert(companyCash);
            }
        }

        private void CreateClientCashForAllClients(Coin coin)
        {
            var clients = _unitOfWork.GenericRepository<Client>().FindBy(x => x.ClientType == ClientType.Client);
            foreach (var client in clients)
            {
                var clientCash = new ClientCash()
                {
                    Coin = coin,
                    ClientId = client.Id,
                    IsEnabled = true,
                    CreatedBy = _appSession.GetUserName()
                };

                _unitOfWork.GenericRepository<ClientCash>().Insert(clientCash);
            }
        }

    }

}
