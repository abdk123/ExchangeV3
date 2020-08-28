
using System.Collections.Generic;

namespace BWR.Application.Interfaces
{
    public interface IGrudAppService<TGet,TAdd,TEdit>
    {
        IList<TGet> GetAll();
        TGet GetById(int id);
        TGet Insert(TAdd dto);
        TGet Update(TEdit dto);
        TEdit GetForEdit(int id);
        void Delete(int id);
    }
}
