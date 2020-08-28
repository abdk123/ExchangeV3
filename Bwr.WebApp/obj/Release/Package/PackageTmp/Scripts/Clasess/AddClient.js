class AddClient {
    constructor(FullName, CountryId, Address, Phones, IsEnabeld, Balances) {
        this.FullName = FullName;
        this.CountryId = CountryId;
        this.Address = Address;
        this.Phones = Phones;
        this.IsEnabeld = IsEnabeld;
        this.Balances = Balances;
    }

}
class ClientBalnce {
    constructor(CoinId, Balance) {
        this.CoinId = CoinId;
        this.Balance = Balance;
    }
}