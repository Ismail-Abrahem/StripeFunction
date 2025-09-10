using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using WalletApi.Configurations;
using WalletApi.Models;

namespace WalletApi.Repository
{
    public class WalletRepository
    {
            private readonly IMongoCollection<Wallet> _walletCollection;
            private readonly ILogger<WalletRepository> _logger;

        public WalletRepository(IOptions<MongoDBSettings> mongoDBSettings, ILogger<WalletRepository> logger)
        {
            _logger = logger;
            var settings = mongoDBSettings.Value;

            try
            {
                var client = new MongoClient(settings.ConnectionString);
                var database = client.GetDatabase(settings.DatabaseName);
                database.RunCommand((Command<BsonDocument>)"{ping:1}");
                _logger.LogInformation("MongoDB connection successful!");

                _walletCollection = database.GetCollection<Wallet>(settings.CollectionName);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "MongoDB connection failed");
                throw;
            }
        }

        // In WalletRepository.cs
        public async Task UpdateWalletBalance(string userId, decimal amount, string bearerToken)
        {
            var filter = Builders<Wallet>.Filter.Eq(w => w.UserId, userId);

            var transaction = new Transaction
            {
                Amount = amount,
                Type = "deposit",
                CreatedAt = DateTime.UtcNow,
                Description = "Stripe deposit"
            };

            var update = Builders<Wallet>.Update
                .Inc(w => w.Balance, amount)
                .Set(w => w.BearerToken, bearerToken)
                .Set(w => w.LastAdded, DateTime.UtcNow)
                .AddToSet(w => w.Transactions, transaction)
                .SetOnInsert(w => w.UserId, userId);

            try
            {
                await _walletCollection.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true });
            }
            catch (MongoWriteException ex) when (ex.WriteError.Code == 11000)
            {
                _logger.LogError("Duplicate UserId detected: {UserId}", userId);
            }
        }


        public async Task<decimal> GetWalletBalance(string userId)
        {
            var wallet = await _walletCollection
                .Find(w => w.UserId == userId)
                .FirstOrDefaultAsync();
            return wallet?.Balance ?? 0;
        }



    }

}

