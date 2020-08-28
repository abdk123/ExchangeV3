using BWR.Domain.Model.Companies;

namespace BWR.Application.Extensions
{
    public static class CompanyCashFlowExtension
    {
        public static decimal? Commission(this CompanyCashFlow companyCashFlow)
        {
            return companyCashFlow.MoenyAction.Comission(companyCashFlow.CompanyId);
        }

        public static decimal SecounCommission(this CompanyCashFlow companyCashFlow)
        {
            return companyCashFlow.MoenyAction.SecounCompanyCommission(companyCashFlow.CompanyId);
        }

        public static string ReceiverName(this CompanyCashFlow companyCashFlow)
        {
            return companyCashFlow.MoenyAction.ReciverName();
        }

        public static string SenderName(this CompanyCashFlow companyCashFlow)
        {
            return companyCashFlow.MoenyAction.SenderName();
        }

        public static string CountryName(this CompanyCashFlow companyCashFlow)
        {
            return companyCashFlow.MoenyAction.CountryName();
        }

        public static string CompanyName(this CompanyCashFlow companyCashFlow)
        {
            return companyCashFlow.Company.Name;
        }
    }
}
