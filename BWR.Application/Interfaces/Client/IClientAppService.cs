using BWR.Application.Dtos.Client;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using clientModel= BWR.Domain.Model.Clients ;
using BWR.Application.Dtos.Common;
namespace BWR.Application.Interfaces.Client
{
    public interface IClientAppService : IGrudAppService<ClientDto, ClientInsertDto, ClientUpdateDto>
    {
        IList<ClientDto> Get(Expression<Func<clientModel.Client, bool>> predicate) ;
        IList<Select2Dto<int>> GetSelect2(Expression<Func<clientModel.Client, bool>> predicate=null);
         //IList<ClientForDropdownDto> GetClientForDropdown(Expression<Func<clientModel.Client, bool>> predicate = null);
    }

  
}
