using BWR.Application.Common;
using BWR.Application.Dtos.Branch;
using BWR.Application.Dtos.Setting.Country;
using BWR.Application.Dtos.Transaction;
using BWR.Domain.Model.Clients;
using System.Collections.Generic;
using BWR.Application.Dtos.Client.ClientCashFlow;

namespace BWR.Application.Dtos.Client
{
    public class ClientDto : EntityDto
    {
        public ClientDto()
        {
            ClientAttatchments = new List<ClientAttatchmentDto>();
            ClientCashFlows = new List<ClientCashFlowDto>();
            ClientCashes = new List<ClientCashDto>();
            ClientPhones = new List<ClientPhoneDto>();
            Transactions = new List<TransactionDto>();
        }


        public string FullName { get; set; }
        public string Address { get; set; }
        public bool IsEnabled { get; set; }
        public int BranchId { get; set; }
        public ClientType ClientType { get; set; }
        public int? CountryId { get; set; }

        public CountryDto Country { get; set; }
        public BranchDto Branch { get; set; }

        public IList<ClientAttatchmentDto> ClientAttatchments { get; set; }
        public IList<ClientCashFlowDto> ClientCashFlows { get; set; }
        public IList<ClientCashDto> ClientCashes { get; set; }
        public IList<ClientPhoneDto> ClientPhones { get; set; }
        public IList<TransactionDto> Transactions { get; set; }


    }
}

