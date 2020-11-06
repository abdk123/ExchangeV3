
namespace BWR.Domain.Model.Enums
{
    public enum BoxActionType
    {
        None,
        
        ExpenseFromTreasury,
        ExpenseFromTreasuryToClient,
        ExpenseFromTreasuryToCompany,
        ExpenseFromClientToPublic,
        //ExpenseFromClientToCompany,
        //ExpenseFromClientToClient,
        ExpenseFromCompanyToPublic,
        //ExpenseFromCompanyToClient,
        //ExpenseFromCompanyToCompany,
        ReceiveToTreasury,
        //ReceiveFromTreasuryToClient,
        //ReceiveFromTreasuryToCompany,
        ReceiveFromClientToTreasury,
        ReceiveFromPublicToCompany,
        ReceiveFromPublicToClient,
        ReceiveFromCompanyToTreasury,
        //ReceiveFromCompanyToClient,
        //ReceiveFromCompanyToCompany,

    }
}
