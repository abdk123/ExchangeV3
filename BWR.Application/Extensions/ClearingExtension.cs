using BWR.Domain.Model.Common;
using BWR.Domain.Model.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BWR.Application.Extensions
{
    public static class ClearingExtension
    {
        public static string GetTypeName(this Clearing clearing, Requester requester, int objectId)
        {
            if (requester == Requester.Agent)
            {
                if (clearing.FromClientId == objectId)
                {
                    if (clearing.IsIncome)
                    {
                        return "قبض";
                    }
                    return "صرف";
                }
                if (clearing.ToClientId == objectId)
                {
                    if (clearing.IsIncome)
                    {
                        return "صرف له";
                    }
                    return "قبض منه";
                }
            }
            if (requester == Requester.Company)
            {

                if (clearing.FromCompanyId == objectId)
                {
                    if (clearing.IsIncome)
                    {
                        return "قبض";
                    }
                    return "صرف";
                }

                if (clearing.ToCompanyId == objectId)
                {
                    if (clearing.IsIncome)
                    {
                        return "صرف له";
                    }
                    return "قبض منه";
                }
            }
            return "GetTypeName";
        }

        public static string GetNote(this Clearing clearing, Requester requester, int objectId)
        {
            string note = clearing.Note;
            if (clearing.ToClient != null && (clearing.ToClient.Id != objectId || requester != Requester.Agent))
            {
                return note + "\\" + clearing.ToClient.FullName;
            }
            if (clearing.FromClient != null && (clearing.FromClient.Id != objectId || requester != Requester.Agent))
            {
                return note + "\\" + clearing.FromClient.FullName;
            }
            if (clearing.ToCompany != null && (clearing.ToCompany.Id != objectId || requester != Requester.Company))
            {
                return note + "\\" + clearing.ToCompany.Name;
            }
            if (clearing.FromCompany != null && (clearing.FromCompany.Id != objectId || requester != Requester.Company))
            {
                return note + "\\" + clearing.FromCompany.Name;
            }
            return "GetNoteClearing";
        }
    }
}
