using System.Linq.Expressions;
using System.Security.Claims;
using Aiursoft.AiurProtocol.Exceptions;
using Aiursoft.AiurProtocol.Models;
using Aiursoft.CSTools.Tools;
using Aiursoft.DbTools.InMemory;
using Aiursoft.DbTools.MySql;
using Aiursoft.Kahla.SDK.Models.Entities;
using Aiursoft.Kahla.Server.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

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
        if (string.IsNullOrWhiteSpace(condition))
        {
            return query;
        }
        return query.Where(predicate);
    }
    
    public static IServiceCollection AddDatabase(this IServiceCollection services, string connectionString)
    {
        if (EntryExtends.IsInUnitTests())
        {
            Console.WriteLine("Unit test detected, using in-memory database.");
            services.AddAiurInMemoryDb<KahlaDbContext>();
        }
        else
        {
            Console.WriteLine("Production environment detected, using MySQL database.");
            
            // As tested, splitQuery: false has better performance.
            services.AddAiurMySqlWithCache<KahlaDbContext>(connectionString, splitQuery: false);
        }
        
        return services;
    }
}