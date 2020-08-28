using AutoMapper;
using BWR.Application.Dtos.Setting.Attachment;
using BWR.Application.Interfaces.Setting;
using BWR.Application.Interfaces.Shared;
using BWR.Infrastructure.Context;
using BWR.Infrastructure.Exceptions;
using BWR.ShareKernel.Exceptions;
using BWR.ShareKernel.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using Attachment = BWR.Domain.Model.Settings.Attachment;

namespace BWR.Application.AppServices.Setting
{
    public class AttachmentAppService : IAttachmentAppService
    {
        private readonly IUnitOfWork<MainContext> _unitOfWork;
        private readonly IAppSession _appSession;

        public AttachmentAppService(IUnitOfWork<MainContext> unitOfWork, IAppSession appSession)
        {
            _unitOfWork = unitOfWork;
            _appSession = appSession;
        }

        public IList<AttachmentDto> GetAll()
        {
            var attachmentDtos = new List<AttachmentDto>();
            try
            {
                var attachments = _unitOfWork.GenericRepository<Attachment>().GetAll().ToList();
                attachmentDtos = Mapper.Map<List<Attachment>, List<AttachmentDto>>(attachments);
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }

            return attachmentDtos;
        }

        public AttachmentDto GetById(int id)
        {
            AttachmentDto attachmentDto = null;
            try
            {
                var attachment = _unitOfWork.GenericRepository<Attachment>().GetById(id);
                if (attachment != null)
                {
                    attachmentDto = Mapper.Map<Attachment, AttachmentDto>(attachment);
                }

            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }

            return attachmentDto;
        }

        public IList<AttachmentForDropdownDto> GetForDropdown(string name)
        {
            var attachmentDtos = new List<AttachmentForDropdownDto>();
            try
            {
                var attachments = _unitOfWork.GenericRepository<Attachment>().FindBy(x => x.Name.StartsWith(name)).ToList();
                Mapper.Map<List<Attachment>, List<AttachmentForDropdownDto>>(attachments, attachmentDtos);
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }

            return attachmentDtos;
        }

        public AttachmentUpdateDto GetForEdit(int id)
        {
            AttachmentUpdateDto attachmentDto = null;
            try
            {
                var attachment = _unitOfWork.GenericRepository<Attachment>().GetById(id);
                if (attachment != null)
                {
                    attachmentDto = Mapper.Map<Attachment, AttachmentUpdateDto>(attachment);
                }

            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }

            return attachmentDto;
        }

        public AttachmentDto Insert(AttachmentInsertDto dto)
        {
            AttachmentDto attachmentDto = null;
            try
            {
                var attachment = Mapper.Map<AttachmentInsertDto, Attachment>(dto);
                attachment.CreatedBy = _appSession.GetUserName();
                attachment.IsEnabled = true;
                _unitOfWork.CreateTransaction();

                _unitOfWork.GenericRepository<Attachment>().Insert(attachment);
                _unitOfWork.Save();

                _unitOfWork.Commit();

                attachmentDto = Mapper.Map<Attachment, AttachmentDto>(attachment);
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
                _unitOfWork.Rollback();
            }
            return attachmentDto;
        }

        public AttachmentDto Update(AttachmentUpdateDto dto)
        {
            AttachmentDto attachmentDto = null;
            try
            {
                var attachment = _unitOfWork.GenericRepository<Attachment>().GetById(dto.Id);
                Mapper.Map<AttachmentUpdateDto, Attachment>(dto, attachment);
                attachment.ModifiedBy = _appSession.GetUserName();
                _unitOfWork.CreateTransaction();

                _unitOfWork.GenericRepository<Attachment>().Update(attachment);
                _unitOfWork.Save();

                _unitOfWork.Commit();

                attachmentDto = Mapper.Map<Attachment, AttachmentDto>(attachment);
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }
            return attachmentDto;
        }

        public void Delete(int id)
        {
            try
            {
                var attachment = _unitOfWork.GenericRepository<Attachment>().GetById(id);
                if (attachment != null)
                {
                    _unitOfWork.GenericRepository<Attachment>().Delete(attachment);
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
                var attachment = _unitOfWork.GenericRepository<Attachment>()
                    .FindBy(x => x.Name.Trim().Equals(name.Trim()) && x.Id != id)
                    .FirstOrDefault();
                if (attachment != null)
                    return true;
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }

            return false;
        }
    }
}