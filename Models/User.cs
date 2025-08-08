using MongoDB.Bson;
public class Transaction
{
    public string _id { get; set; }
    public string userId { get; set; }
    public string beneficiaryNickname { get; set; }
    public double amount { get; set; }
    public string timestamp { get; set; }
    public string direction { get; set; }
}

public class Beneficiary
{
    public string _id { get; set; }
    public string name { get; set; }
    public string nickname { get; set; }
    public double limit { get; set; }
}

public class User
{
    public string _id { get; set; }
    public string name { get; set; }
    public string phone { get; set; }
    public double balance { get; set; }
    public double topupLimit { get; set; }
    public bool isverified { get; set; }
    public List<Beneficiary> beneficiaries { get; set; }
    public List<Transaction> transactions { get; set; }
}

public class AccountCollection
{
    public ObjectId Id { get; set; }
    public List<User> Users { get; set; }
}
public class AddBeneficiaryRequest
{
    public string userPhone { get; set; }
    public string beneficiaryPhone { get; set; }
    public string nickname { get; set; }
}
public class DeleteBeneficiaryRequest
{
    public string userId { get; set; }
    public string beneficiaryId { get; set; }
}
public class TopupRequest
{
    public string userId { get; set; }
    public string beneficiaryId { get; set; }
    public int amount { get; set; }
}