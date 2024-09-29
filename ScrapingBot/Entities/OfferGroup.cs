using Azure;
using Azure.Data.Tables;
using System;

namespace ScrapingBot.Entities;

public class OfferGroup : ITableEntity {
    public string ExperienceLevel { get; set; }
    public string Stack { get; set; }
    public int OfferCount { get; set; }
    public string PartitionKey { get; set; }
    public string RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
}
