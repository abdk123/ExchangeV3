using BWR.Application.Dtos.BoxAction;

namespace BWR.Application.Interfaces.BoxAction
{
    public interface IBoxActionAppService
    {
        BoxActionUpdateDto GetForEdit(int moneyActionId);
        bool ExpenseFromTreasury(BoxActionExpensiveDto input);
        bool ReceiveToTreasury(BoxActionIncomeDto input);
        bool ExpenseFromTreasuryToClient(BoxActionClientDto dto);
        bool ReceiveFromClientToTreasury(BoxActionClientDto dto);
        bool ReceiveFromCompanyToTreasury(BoxActionCompanyDto dto);
        bool ExpenseFromTreasuryToCompany(BoxActionCompanyDto dto);
        bool FromClientToClient(BoxActionFromClientToClientDto dto);
        bool FromCompanyToClient(BoxActionFromCompanyToClientDto dto);
        bool FromClientToCompany(BoxActionFromClientToCompanyDto dto);
        bool FromCompanyToCompany(BoxActionFromCompanyToCompanyDto dto);
        bool ExpenseFromClientToPublic(BoxActionFromClientToPublicExpenesDto dto);
        bool ReceiveFromPublicToClient(BoxActionFromClientToPublicIncomeDto dto);
        bool ExpenseFromCompanyToPublic(BoxActionFromCompanyToPublicExpenesDto dto);
        bool ReceiveFromPublicToCompany(BoxActionFromCompanyToPublicIncomeDto dto);
        BoxActionInitialDto InitialInputData();
        
        bool EditExpenseFromTreasury(BoxActionExpensiveUpdateDto input);
        bool EditReceiveToTreasury(BoxActionIncomeUpdateDto input);
        bool EditExpenseFromTreasuryToClient(BoxActionClientUpdateDto dto);
        bool EditReceiveFromClientToTreasury(BoxActionClientUpdateDto dto);
        bool EditReceiveFromCompanyToTreasury(BoxActionCompanyUpdateDto dto);
        bool EditExpenseFromTreasuryToCompany(BoxActionCompanyUpdateDto dto);
        bool EditFromClientToClient(BoxActionFromClientToClientUpdateDto dto);
        bool EditFromCompanyToClient(BoxActionFromCompanyToClientUpdateDto dto);
        bool EditFromClientToCompany(BoxActionFromClientToCompanyUpdateDto dto);
        bool EditFromCompanyToCompany(BoxActionFromCompanyToCompanyUpdateDto dto);
        bool EditExpenseFromClientToPublic(BoxActionFromClientToPublicExpenesUpdateDto dto);
        bool EditReceiveFromPublicToClient(BoxActionFromClientToPublicIncomeUpdateDto dto);
        bool EditExpenseFromCompanyToPublic(BoxActionFromCompanyToPublicExpenesUpdateDto dto);
        bool EditReceiveFromPublicToCompany(BoxActionFromCompanyToPublicIncomeUpdateDto dto);

    }
}
