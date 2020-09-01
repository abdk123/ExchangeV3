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
            if (clearing.ToClientId != null && (clearing.ToClientId != objectId || requester != Requester.Agent))
            {
                return note + "/" + clearing.ToClient.FullName;
            }
            if (clearing.FromClientId != null && (clearing.FromClientId != objectId || requester != Requester.Agent))
            {
                return note + "/" + clearing.FromClient.FullName;
            }
            if (clearing.ToCompanyId != null && (clearing.ToCompanyId != objectId || requester != Requester.Company))
            {
                return note + "/" + clearing.ToCompany.Name;
            }
            if (clearing.FromCompanyId != null && (clearing.FromCompanyId != objectId || requester != Requester.Company))
            {
                return note + "/" + clearing.FromCompany.Name;
            }
            return "GetNoteClearing";
        }
        public static string GetSenderName(this Clearing clearing,Requester requester ,int requeserId)
        {
            if (requester == Requester.Company)
            {
                if (clearing.ToCompanyId == requeserId)
                {
                    if (clearing.FromClient != null)
                    {
                        return clearing.FromClient.FullName;
                    }
                }
                if( clearing.FromCompanyId == requeserId)
                {
                    if (clearing.ToClient != null)
                    {
                        return clearing.Note + "/" + clearing.ToClient.FullName;
                    }
                }
            }
            return "";
        }
        public static string ReciverName(this Clearing clearing ,Requester requester, int requesterId)
        {
            if(requester == Requester.Company)
            {
                if (clearing.ToCompanyId == requesterId)
                {
                    if (clearing.FromClient != null)
                    {
                        return clearing.Note;
                    }
                }
                if (clearing.FromCompanyId == requesterId)
                {
                    return clearing.Note;
                }
            }
            return "";
        }
    }
}
