using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
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
        public async Task<AIKnowledgeResponse> UpsertAIKnowledgeAsync(AIKnowledgeMessage message)
        {
            try
            {
                var activityType = ConvertActionToActityType(message.Action);
                var metaData = BuildMetaData(message);

                _logger.LogInformation("Metadata: {metaData}", metaData);

                if (message.Action == DTO.Action.TaskCreated)
                {
                    var aiKnowledge = new AICoreService.DTO.CreateAIKnowledgeRequest()
                    {
                        EventId = message.EventId,
                        TaskId = message.TaskId,
                        ProjectId = message.ProjectId,
                        Type = activityType,
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
                        TaskId = message.TaskId,
                        ProjectId = message.ProjectId,
                        Type = activityType,
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

        private static Metadata BuildMetaData(AIKnowledgeMessage message)
        {
            Dictionary<string, PropertyInfo> MetadataProps =
               typeof(Metadata)
                   .GetProperties()
                   .Where(p => p.PropertyType == typeof(TaskManagementServiceBusApi.AI.AIKnowledgeData.DTO.Changes)) // the metadata Changes (From/To)
                   .ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);
            var metadata = new Metadata()
            {
                Actor = message.Actor,
            };

            if(message.Action == DTO.Action.TaskCreated)
            {
                metadata.Title = new AICoreService.DTO.Changes()
                {
                    From = message.TaskTitle,
                    To = message.TaskTitle
                };
                return metadata;
            }

            foreach (var change in message.Changes)
            {
                if (!MetadataProps.TryGetValue(change.Field, out var prop)) continue;

                var target = (AICoreService.DTO.Changes)prop.GetValue(metadata)!;
                target.From = ToStringValue(change.OldValue);
                target.To = ToStringValue(change.NewValue);
            }
            return metadata;
        }

        private static string ToStringValue(object? value) =>
            value switch
            {
                null => string.Empty,
                IFormattable f => f.ToString(null, CultureInfo.InvariantCulture),
                _ => value.ToString() ?? string.Empty
            };

        private static string Require(string? value, string name, ActivityType type)
        => string.IsNullOrWhiteSpace(value) ? throw MissingValue(name, type) : value;

        private static ArgumentException MissingValue(string name, ActivityType type)
            => new($"'{name}' is required for activity type {type}.", name);

        private static ActivityType ConvertActionToActityType(DTO.Action action)
        {
            return action switch
            {
                DTO.Action.TaskCreated => ActivityType.TaskCreated,
                DTO.Action.TaskStatusChanged => ActivityType.StatusChanged,
                DTO.Action.TaskAssigned => ActivityType.Assigned,
                DTO.Action.TaskCommentAdded => ActivityType.Commented,
                DTO.Action.TaskDueDateChanged => ActivityType.DueDateChanged,
                _ => ActivityType.Unknown,
            };
        }
    }
}