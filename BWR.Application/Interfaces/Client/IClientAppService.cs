using BWR.Application.Dtos.Client;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace BWR.Application.Interfaces.Client
{
    public interface IClientAppService : IGrudAppService<ClientDto, ClientInsertDto, ClientUpdateDto>
    {
        IList<ClientDto> Get(Expression<Func<BWR.Domain.Model.Clients.Client, bool>> predicate) ;
    }

  
}
