using MongoDB.Bson;
using MongoDB.Driver;
using Microsoft.Extensions.Options;

public class MongoDbService
{
    private readonly IMongoCollection<AccountCollection> _collection;

    public MongoDbService(IOptions<MongoDbSettings> settings)
    {
        var client = new MongoClient(settings.Value.ConnectionString);
        var database = client.GetDatabase(settings.Value.DatabaseName);
        _collection = database.GetCollection<AccountCollection>(settings.Value.CollectionName);
    }

    public async Task<List<User>> GetAllUsersAsync()
    {
        var doc = await _collection.Find(_ => true).FirstOrDefaultAsync();
        return doc?.Users ?? new List<User>();
    }

    public async Task<User> GetUserByPhoneAsync(string phone)
    {
        var doc = await _collection.Find(_ => true).FirstOrDefaultAsync();
        return doc?.Users.Find(u => u.phone == phone);
    }

    public async Task<object> SearchBenfAsync(string phone)
    {
        var doc = await _collection.Find(_ => true).FirstOrDefaultAsync();
        var user = doc?.Users.Find(u => u.phone == phone);
        return user != null ? new { name = user.name, id = user._id } : null;
    }
    public async Task UpdateUserAsync(User user)
    {
        var doc = await _collection.Find(_ => true).FirstOrDefaultAsync();

        if (doc != null)
        {
            var userIndex = doc.Users.FindIndex(u => u._id == user._id);
            if (userIndex != -1)
            {
                doc.Users[userIndex] = user; // update in memory
                await _collection.ReplaceOneAsync(d => d.Id == doc.Id, doc); // save back
            }
        }
    }

    public async Task<bool> DeleteUserAsync(string userId, string beneficiaryId)
    {
        // Get the document from Mongo
        var doc = await _collection.Find(_ => true).FirstOrDefaultAsync();

        if (doc != null)
        {
            // Find the user
            var user = doc.Users.FirstOrDefault(u => u._id == userId);
            var userIndex = doc.Users.FindIndex(u => u._id == userId);
            if (user != null)
            {
                // Remove beneficiary from user's list
                user.beneficiaries.RemoveAll(b => b._id == beneficiaryId);
                doc.Users[userIndex] = user; // update in memory
                // Replace the whole document in Mongo
                await _collection.ReplaceOneAsync(d => d.Id == doc.Id, doc); // save back
                return true;
            }
            else
            {
                return false;
            }
        }
        return false;
    }
    public async Task<bool> TopUpAsync(TopupRequest dto)
    {
        var doc = await _collection.Find(_ => true).FirstOrDefaultAsync();

        if (doc != null)
        {
            var user = doc.Users.FirstOrDefault(u => u._id == dto.userId);
            var userIndex = doc.Users.FindIndex(u => u._id == dto.userId);

            var beneficiary = doc.Users.FirstOrDefault(u => u._id == dto.beneficiaryId);
            var beneficiaryIndex = doc.Users.FindIndex(u => u._id == dto.beneficiaryId);

            if (userIndex != -1)
            {
                user.balance -= dto.amount;
                user.topupLimit -= dto.amount;
                user.beneficiaries.FirstOrDefault(b => b._id == dto.beneficiaryId).limit -= dto.amount;
                user.transactions.Add(new Transaction
                {
                    _id = user.transactions.Count.ToString(),
                    userId = dto.beneficiaryId,
                    beneficiaryNickname = beneficiary.name,
                    amount = dto.amount,
                    timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                    direction = "outgoing"
                });
                beneficiary.balance += dto.amount;
                beneficiary.transactions.Add(new Transaction
                {
                    _id = beneficiary.transactions.Count.ToString(),
                    userId = dto.userId,
                    beneficiaryNickname = user.name,
                    amount = dto.amount,
                    timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                    direction = "incoming"
                });
                doc.Users[userIndex] = user; // update in memory
                doc.Users[beneficiaryIndex] = beneficiary; // update in memory
                await _collection.ReplaceOneAsync(d => d.Id == doc.Id, doc); // save back
                return true;
            }
        }
        return false;
    }
}
