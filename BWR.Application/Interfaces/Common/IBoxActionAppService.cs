using BWR.Application.Dtos.BoxAction;

namespace BWR.Application.Interfaces.BoxAction
{
    public interface IBoxActionAppService
    {
        bool PayExpenciveFromMainBox(BoxActionExpensiveDto input);
        bool ReciverIncomeToMainBox(BoxActionIncomeDto input);
        bool PayForClientFromMainBox(BoxActionClientDto dto);
        bool ReciveFromClientToMainBox(BoxActionClientDto dto);
        bool ReciveFromCompanyToMainBox(BoxActionCompanyDto dto);
        bool PayForCompanyFromMainBox(BoxActionCompanyDto dto);
        bool FromClientToClient(BoxActionFromClientToClientDto dto);
        bool FromCompanyToClient(BoxActionFromCompanyToClientDto dto);
        bool FromClientToCompany(BoxActionFromClientToCompanyDto dto);
        bool FromCompanyToCompany(BoxActionFromCompanyToCompanyDto dto);

        BoxActionInitialDto InitialInputData();
        
    }
}
