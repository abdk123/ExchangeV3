using BWR.Domain.Model.Branches;
using BWR.Domain.Model.Settings;
using BWR.Domain.Model.Transactions;
using BWR.ShareKernel.Common;
using BWR.ShareKernel.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace BWR.Domain.Model.Clients
{
    public class Client: Entity,IAggregateRoot
    {
        public Client()
        {
            ClientAttatchments = new List<ClientAttatchment>();
            ClientCashFlows = new List<ClientCashFlow>();
            ClientCashes = new List<ClientCash>();
            ClientPhones = new List<ClientPhone>();
            Transactions = new List<Transaction>();
        }

        
        public string FullName { get; set; }
        public string Address { get; set; }
        public bool IsEnabled { get; set; }

        public int BranchId { get; set; }
        [ForeignKey("BranchId")]
        public virtual Branch Branch { get; set; }

        public ClientType ClientType { get; set; }
        
        public int? CountryId { get; set; }
        [ForeignKey("CountryId")]
        public virtual Country Country { get; set; }

        
        public virtual IList<ClientAttatchment> ClientAttatchments { get; set; }
        public virtual IList<ClientCashFlow> ClientCashFlows { get; set; }
        public virtual IList<ClientCash> ClientCashes { get; set; }
        public virtual IList<ClientPhone> ClientPhones { get; set; }
        public virtual IList<Transaction> Transactions { get; set; }
       

    }
}
