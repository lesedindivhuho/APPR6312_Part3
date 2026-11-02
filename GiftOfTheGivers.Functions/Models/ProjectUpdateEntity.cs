using Azure;
using Azure.Data.Tables;

namespace GiftOfTheGivers.Functions.Models;

public class ProjectUpdateEntity : ITableEntity
{
    public string PartitionKey { get; set; } = string.Empty;
    public string RowKey { get; set; } = string.Empty;
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string PostedBy { get; set; } = string.Empty;
    public DateTime DatePosted { get; set; }
}
