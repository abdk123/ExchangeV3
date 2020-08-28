class IncomeTransactionTableDTO {
    Transactions = [];    
    constructor(MainCompanyId, Note) {
        this.MainCompanyId = MainCompanyId;
        this.Note = Note;
    }
    
}
class IncomeTransacrionRow {
    constructor(coinId, amount, ourComission, sender, TypeOfPay, ReciverClinet, AgentId, AgentCommission, ReciverCompany) {
        this.CoinId = coinId;
        this.Amount = amount;
        this.OurComission = ourComission;
        this.TypeOfPay = TypeOfPay;
        this.Sender = sender;
        this.ReciverClinet = ReciverClinet;
        this.AgentId = AgentId;
        this.AgentCommission = AgentCommission;
        this.ReciverCompany = ReciverCompany;
    }
}
class ClientDTO {
    constructor(id, name, address, phone) {
        this.Id = id;
        this.Name = name;
        this.address = address;
        this.Phone = phone;
    }
}
class ReciverCompanyDTO {

    constructor(companyId, companyCommission, reciverClinet) {
        this.CompanyId = companyId;
        this.CompanyCommission = companyCommission;
        this.ReciverClinet = reciverClinet;
    }
}
