using System.Linq.Expressions;
using System.Security.Claims;
using Aiursoft.AiurProtocol.Exceptions;
using Aiursoft.AiurProtocol.Models;
using Aiursoft.CSTools.Tools;
using Aiursoft.DbTools.InMemory;
using Aiursoft.DbTools.MySql;
using Aiursoft.Kahla.Server.Data;
using Aiursoft.Kahla.Server.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Aiursoft.Kahla.Server;

public static class Extensions
{
    public static string GetUserId(this ClaimsPrincipal user)
    {
        var userId = user.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new AiurServerException(Code.Unauthorized, "You are not authorized to view this content.");
        }
        return userId;
    }
    
    public static async Task<KahlaUser> GetCurrentUser(this ControllerBase controller, UserManager<KahlaUser> userManager)
    {
        var user = await userManager.GetUserAsync(controller.User);
        if (user == null)
        {
            throw new AiurServerException(Code.Conflict, "The user you signed in was deleted from the database!");
        }
        return user;
    }
    
    public static IQueryable<T> WhereWhen<T>(
        this IQueryable<T> query,
        string? condition,
        Expression<Func<T, bool>> predicate)
    {
        return string.IsNullOrWhiteSpace(condition) ? query : query.Where(predicate);
    }
    
    public static IServiceCollection AddRelationalDatabase(this IServiceCollection services, string connectionString)
    {
        if (EntryExtends.IsInUnitTests())
        {
            Console.WriteLine("Unit test detected, using in-memory database.");
            services.AddAiurInMemoryDb<KahlaRelationalDbContext>();
        }
        else
        {
            Console.WriteLine("Production environment detected, using MySQL database.");
            
            // As tested, splitQuery: false has better performance.
            // This is because splitQuery = false can reduce the number of queries sent to the database from O(n) to O(1).
            services.AddAiurMySqlWithCache<KahlaRelationalDbContext>(connectionString, splitQuery: false);
        }
        
        return services;
    }
    
    public static int GetLimitedNumber(int min, int max, int suggested)
    {
        return Math.Max(min, Math.Min(max, suggested));
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
}