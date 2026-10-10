using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using TaskManagementServiceBusApi.AI.AIKnowledgeData.DTO;
using TaskManagementServiceBusApi.AICoreService.DTO;
using TaskManagementServiceBusApi.AICoreService.Service;
using TaskManagementServiceBusApi.Worker.DTO;

namespace TaskManagementServiceBusApi.AI.AIKnowledgeData.Service
{
    public class AIKnowledgeService(IAIService aiService, ILogger<AIKnowledgeService> logger) : IAIKnowledgeService
    {
        private readonly IAIService _aiService = aiService;
        private readonly ILogger<AIKnowledgeService> _logger = logger;
        public async Task<AIKnowledgeResponse> UpsertAIKnowledgeAsync(AIKnowledgeMessage data)
        {
            try
            {
                var metaData = new Dictionary<string, string>();
                metaData = BuildMetaData(data.Type, new ActivityContext(
                    Actor: data.Type switch
                    {
                        ActivityType.TaskCreated => data.UserCreated,
                        _ => data.UserUpdated
                    },
                    TaskTitle: data.TaskName,
                    From: "",
                    To: "",
                    Comment: "",
                    Assignee: data.Username,
                    NewDueDate: data.ExpirationDate
                ));
                
                if (data.Type == ActivityType.TaskCreated)
                {
                    var aiKnowledge = new AICoreService.DTO.CreateAIKnowledgeRequest()
                    {
                        EventId = data.EventId,
                        TaskId = data.Id,
                        ProjectId = data.ProjectId,
                        Type = data.Type,
                        Content = "",
                        Metadata = metaData
                    };
                    _logger.LogInformation("Processing aiKnowledge: {aiKnowledge}", aiKnowledge);
                    var result = await _aiService.CreateAIKnowledge(aiKnowledge);
                    return result;
                }
                else
                {
                    var aiKnowledge = new AICoreService.DTO.UpdateAIKnowledgeRequest()
                    {
                        TaskId = data.Id,
                        ProjectId = data.ProjectId,
                        Type = data.Type,
                        Content = "",
                        Metadata = metaData
                    };
                    _logger.LogInformation("Processing aiKnowledge: {aiKnowledge}", aiKnowledge);
                    var result = await _aiService.UpdateAIKnowledge(aiKnowledge);
                    return result;
                }
            }
            catch (System.Exception)
            {

                throw;
            }
        }

        private Dictionary<string, string> BuildMetaData(ActivityType type, ActivityContext ctx)
        {
            // Every activity type needs these two (the default branch of BuildSummaryText uses them too).
            var metaData = new Dictionary<string, string>
            {
                [MetaKeys.Actor] = Require(ctx.Actor, nameof(ctx.Actor), type),
                [MetaKeys.TaskTitle] = Require(ctx.TaskTitle, nameof(ctx.TaskTitle), type)
            };

            switch (type)
            {
                case ActivityType.TaskCreated:
                    // No extra fields.
                    break;

                case ActivityType.StatusChanged:
                    metaData[MetaKeys.From] = Require(ctx.From, nameof(ctx.From), type);
                    metaData[MetaKeys.To] = Require(ctx.To, nameof(ctx.To), type);
                    break;

                case ActivityType.Commented:
                    metaData[MetaKeys.Comment] = Require(ctx.Comment, nameof(ctx.Comment), type);
                    break;

                case ActivityType.Assigned:
                    metaData[MetaKeys.Assignee] = Require(ctx.Assignee, nameof(ctx.Assignee), type);
                    break;

                case ActivityType.DueDateChanged:
                    if (ctx.NewDueDate is null)
                        throw MissingValue(nameof(ctx.NewDueDate), type);

                    metaData[MetaKeys.NewDueDate] =
                        ctx.NewDueDate.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                    break;

                default:
                    // Unknown types fall back to the generic "updated task" summary.
                    break;
            }

            return metaData;
        }

        private static string Require(string? value, string name, ActivityType type)
        => string.IsNullOrWhiteSpace(value) ? throw MissingValue(name, type) : value;

        private static ArgumentException MissingValue(string name, ActivityType type)
            => new($"'{name}' is required for activity type {type}.", name);
    }

    public static class MetaKeys
    {
        public const string Actor = "actor";
        public const string TaskTitle = "taskTitle";
        public const string From = "from";
        public const string To = "to";
        public const string Comment = "comment";
        public const string Assignee = "assignee";
        public const string NewDueDate = "newDueDate";
    }

    // Everything a caller might know about the activity. Only the fields
    // relevant to the ActivityType need to be filled in.
    public sealed record ActivityContext(
        string Actor,
        string TaskTitle,
        string? From = null,
        string? To = null,
        string? Comment = null,
        string? Assignee = null,
        DateTime? NewDueDate = null);
}