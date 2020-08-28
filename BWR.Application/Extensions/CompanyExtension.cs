
using BWR.Application.Dtos.Company;
using BWR.Domain.Model.Companies;
using System.Collections.Generic;

namespace BWR.Application.Extensions
{
    public static class CompanyExtension
    {
        public static bool HaveCountryById(this Company company,int Id)
        {
            if (company.CompanyCountries.Count == 0)
                return false;
            
            foreach (var item in company.CompanyCountries)
            {
                if (item.CountryId == Id)
                    return true;
            }
            return false;
        }

        public static bool HaveCountryById(this CompanyDto company, int Id)
        {
            if (company.CompanyCountries.Count == 0)
                return false;
            
            foreach (var item in company.CompanyCountries)
            {
                if (item.CountryId == Id)
                    return true;
            }
            return false;
        }
    }
}
