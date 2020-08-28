using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using BWR.Application.Dtos.Branch;
using BWR.Application.Dtos.Branch.BranchCommission;
using BWR.Application.Interfaces.Branch;
using BWR.Application.Interfaces.Shared;
using BWR.Domain.Model.Branches;
using BWR.Infrastructure.Context;
using BWR.Infrastructure.Exceptions;
using BWR.ShareKernel.Exceptions;
using BWR.ShareKernel.Interfaces;

namespace BWR.Application.AppServices.Branch
{
    public class BranchCommissionAppService : IBranchCommissionAppService
    {
        private readonly IUnitOfWork<MainContext> _unitOfWork;
        private readonly IAppSession _appSession;

        public BranchCommissionAppService(IUnitOfWork<MainContext> unitOfWork, IAppSession appSession)
        {
            _unitOfWork = unitOfWork;
            _appSession = appSession;
        }

        public IList<BranchCommissionDto> GetAll()
        {
            var branchCommissionsDto = new List<BranchCommissionDto>();
            try
            {
                var branchCommissions = _unitOfWork.GenericRepository<BranchCommission>().GetAll().ToList();
                branchCommissionsDto = Mapper.Map<List<BranchCommission>, List<BranchCommissionDto>>(branchCommissions);
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }

            return branchCommissionsDto;
        }

        public decimal CalcComission(BranchCommissionInputDto input)
        {
            decimal value = 0;

            try
            {
                var branchCommission = _unitOfWork.GenericRepository<BranchCommission>()
                    .FindBy(x => x.CoinId == input.CoinId &&
                    x.CountryId == input.CountryId &&
                    x.BranchId == input.BranchId &&
                    x.StartRange <= input.Amount &&
                    (x.EndRange == 0 || input.Amount <= x.EndRange))
                    .OrderByDescending(x => x.Id).FirstOrDefault();

                if (branchCommission == null)
                    value = 0;
                else if (branchCommission != null && branchCommission.Cost != 0)
                    value = branchCommission.Cost;
                else
                    value = branchCommission.Ratio > 0 ? ((input.Amount * branchCommission.Ratio) / 100) : 0;
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }

            return value;
        }

        public BranchCommissionDto GetById(int id)
        {
            BranchCommissionDto branchCommissionDto = null;
            try
            {
                var branchCommission = _unitOfWork.GenericRepository<BranchCommission>().GetById(id);
                if (branchCommission != null)
                {
                    branchCommissionDto = Mapper.Map<BranchCommission, BranchCommissionDto>(branchCommission);
                }

            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }

            return branchCommissionDto;
        }

        public BranchCommissionUpdateDto GetForEdit(int id)
        {
            BranchCommissionUpdateDto branchCommissionDto = null;
            try
            {
                var branchCommission = _unitOfWork.GenericRepository<BranchCommission>().GetById(id);
                if (branchCommission != null)
                {
                    branchCommissionDto = Mapper.Map<BranchCommission, BranchCommissionUpdateDto>(branchCommission);
                }

            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }

            return branchCommissionDto;
        }

        public BranchCommissionDto Insert(BranchCommissionInsertDto dto)
        {
            BranchCommissionDto branchCommissionDto = null;
            try
            {
                var branchCommission = Mapper.Map<BranchCommissionInsertDto, BranchCommission>(dto);
                branchCommission.CreatedBy = _appSession.GetUserName();
                branchCommission.IsEnabled = true;
                _unitOfWork.CreateTransaction();

                var branch = _unitOfWork.GenericRepository<Domain.Model.Branches.Branch>().GetAll().FirstOrDefault();
                branchCommission.BranchId = branch.Id;

                _unitOfWork.GenericRepository<BranchCommission>().Insert(branchCommission);
                _unitOfWork.Save();

                _unitOfWork.Commit();

                branchCommissionDto = Mapper.Map<BranchCommission, BranchCommissionDto>(branchCommission);
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
                _unitOfWork.Rollback();
            }
            return branchCommissionDto;
        }

        public BranchCommissionDto Update(BranchCommissionUpdateDto dto)
        {
            BranchCommissionDto branchCommissionDto = null;
            try
            {
                var branchCommission = _unitOfWork.GenericRepository<BranchCommission>().GetById(dto.Id);
                Mapper.Map<BranchCommissionUpdateDto, BranchCommission>(dto, branchCommission);
                branchCommission.ModifiedBy = _appSession.GetUserName();
                _unitOfWork.CreateTransaction();

                _unitOfWork.GenericRepository<BranchCommission>().Update(branchCommission);
                _unitOfWork.Save();

                _unitOfWork.Commit();

                branchCommissionDto = Mapper.Map<BranchCommission, BranchCommissionDto>(branchCommission);
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }
            return branchCommissionDto;
        }

        public void Delete(int id)
        {
            try
            {
                var branchCommission = _unitOfWork.GenericRepository<BranchCommission>().GetById(id);
                if (branchCommission != null)
                {
                    _unitOfWork.GenericRepository<BranchCommission>().Delete(branchCommission);
                    _unitOfWork.Save();
                }
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }
        }

        
    }
}
