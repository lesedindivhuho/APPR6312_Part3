using System.Text.Json;
using Azure.Data.Tables;
using GiftOfTheGivers.Functions.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace GiftOfTheGivers.Functions.Functions;

public class LogProjectUpdate
{
    private const int MaxTitleLength = 150;
    private const int MaxMessageLength = 2000;

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private readonly TableClient _table;
    private readonly ILogger<LogProjectUpdate> _logger;

    public LogProjectUpdate(TableServiceClient tableService, ILogger<LogProjectUpdate> logger)
    {
        _table = tableService.GetTableClient("ProjectUpdates");
        _logger = logger;
    }

    [Function(nameof(LogProjectUpdate))]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
    {
        _logger.LogInformation("Project update received.");

        ProjectUpdateRequest? request;
        try
        {
            request = await JsonSerializer.DeserializeAsync<ProjectUpdateRequest>(req.Body, JsonOptions);
        }
        catch (JsonException)
        {
            _logger.LogWarning("Project update rejected: body is not valid JSON.");
            return new BadRequestObjectResult(new { message = "Request body must be valid JSON with title and message." });
        }

        var errors = Validate(request);
        if (errors.Count > 0)
        {
            _logger.LogWarning("Project update rejected: {Errors}", string.Join(" ", errors));
            return new BadRequestObjectResult(new { message = "Invalid project update.", errors });
        }

        var postedAt = DateTime.UtcNow;
        var entity = new ProjectUpdateEntity
        {
            PartitionKey = postedAt.ToString("yyyy-MM"),
            RowKey = $"{postedAt:yyyyMMddHHmmssfff}-{Guid.NewGuid():N}",
            Title = request!.Title!.Trim(),
            Message = request.Message!.Trim(),
            PostedBy = string.IsNullOrWhiteSpace(request.PostedBy) ? "Employee" : request.PostedBy.Trim(),
            DatePosted = postedAt
        };

        try
        {
            await _table.CreateIfNotExistsAsync();
            await _table.AddEntityAsync(entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not save project update '{Title}' to table storage.", entity.Title);
            return new ObjectResult(new { message = "Storage is currently unavailable. The update was not logged." })
            {
                StatusCode = StatusCodes.Status503ServiceUnavailable
            };
        }

        _logger.LogInformation("Project update '{Title}' saved with id {RowKey}.", entity.Title, entity.RowKey);

        return new ObjectResult(new
        {
            id = entity.RowKey,
            entity.Title,
            entity.PostedBy,
            entity.DatePosted
        })
        {
            StatusCode = StatusCodes.Status201Created
        };
    }

    private static List<string> Validate(ProjectUpdateRequest? request)
    {
        var errors = new List<string>();

        if (request == null)
        {
            errors.Add("Request body is required.");
            return errors;
        }

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            errors.Add("Title is required.");
        }
        else if (request.Title.Trim().Length > MaxTitleLength)
        {
            errors.Add($"Title cannot be longer than {MaxTitleLength} characters.");
        }

        if (string.IsNullOrWhiteSpace(request.Message))
        {
            errors.Add("Message is required.");
        }
        else if (request.Message.Trim().Length > MaxMessageLength)
        {
            errors.Add($"Message cannot be longer than {MaxMessageLength} characters.");
        }

        return errors;
    }
}
