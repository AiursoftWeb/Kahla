using System.Text.Json;
using Aiursoft.AiurProtocol;
using Aiursoft.Kahla.SDK.Events;
using Aiursoft.Kahla.SDK.Events.Abstractions;
using Aiursoft.Kahla.SDK.Services;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Aiursoft.Kahla.SDK;

public static class Extensions
{
    public static IServiceCollection AddKahlaService(this IServiceCollection services, string endPointUrl)
    {
        services.AddAiurProtocolClient();
        services.Configure<KahlaServerConfig>(options => options.Instance = endPointUrl);
        services.AddScoped<KahlaServerAccess>();
        return services;
    }
    
    private static readonly JsonSerializerSettings Settings = new()
    {
        TypeNameHandling = TypeNameHandling.Auto,
        DateTimeZoneHandling = DateTimeZoneHandling.Utc,
        ContractResolver = new CamelCasePropertyNamesContractResolver()
    };
    
    public static string Serialize<T>(T model)
    {
        return JsonConvert.SerializeObject(model, Settings);
    }

    public static T Deserialize<T>(string json)
    {
        return JsonConvert.DeserializeObject<T>(json, Settings)!;
    }
    
    public static KahlaEvent DeserializeKahlaEvent(string json)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        
        using var jsonDocument = JsonDocument.Parse(json);
        var root = jsonDocument.RootElement;

        // Check if the 'Type' property exists
        if (root.TryGetProperty("type", out var typeElement))
        {
            // Get the integer value of the 'Type' property
            var typeValue = typeElement.GetInt32();
            var eventType = (EventType)typeValue;

            // Map the event type to the corresponding class type
            var targetType = eventType switch
            {
                EventType.NewMessage => typeof(NewMessageEvent),
                EventType.ThreadDissolved => typeof(ThreadDissolvedEvent),
                EventType.YouBeenKicked => typeof(YouBeenKickedEvent),
                EventType.YouLeft => typeof(YouLeftEvent),
                EventType.CreateScratched => typeof(CreateScratchedEvent),
                EventType.YouDirectJoined => typeof(YouDirectJoinedEvent),
                EventType.YourHardInviteFinished => typeof(YourHardInviteFinishedEvent),
                EventType.YouWasHardInvited => typeof(YouWasHardInvitedEvent),
                EventType.YouCompletedSoftInvited => typeof(YouCompletedSoftInvitedEvent),
                EventType.ThreadPropertyChanged => typeof(ThreadPropertyChangedEvent),
                _ => typeof(KahlaEvent) // Default to base class if type is unknown
            };

            // Deserialize the JSON into the target type
            var kahlaEvent = (KahlaEvent)System.Text.Json.JsonSerializer.Deserialize(json, targetType, options)!;
            return kahlaEvent;
        }

        throw new System.Text.Json.JsonException("The 'Type' property was not found in the JSON.");
    }
}