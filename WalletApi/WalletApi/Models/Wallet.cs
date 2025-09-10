using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace WalletApi.Models
{
    public class Wallet
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty; 
        
        [BsonElement("BearerToken")]
        public string BearerToken { get; set; } = string.Empty;

        [BsonElement("UserId")]
        public string UserId { get; set; } = string.Empty;

        [BsonElement("Balance")]
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal Balance { get; set; } = 0m;

        [BsonElement("LastAdded")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime LastAdded { get; set; } = DateTime.UtcNow;

        [BsonElement("Transactions")]
        public List<Transaction> Transactions { get; set; } = new();


    }

    public class Transaction
    {
        [BsonElement("Amount")]
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal Amount { get; set; }

        [BsonElement("Type")]
        public string Type { get; set; } = "deposit";

        [BsonElement("CreatedAt")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("Description")]
        public string Description { get; set; } = "Stripe deposit";
    }
}