using Azure;
using Azure.Data.Tables;
using System;

namespace CoffeeNchill.Models
{
    public class MenuItem : ITableEntity
    {
        // Required by ITableEntity — Table Storage's built-in partitioning key
        public string PartitionKey { get; set; } = string.Empty;

        // Required by ITableEntity — unique row identifier within a partition
        public string RowKey { get; set; } = string.Empty;

        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        // Your actual menu fields
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public double Price { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}