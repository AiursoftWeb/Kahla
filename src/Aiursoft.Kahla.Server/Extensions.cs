using System.Linq.Expressions;
using System.Security.Claims;
using Aiursoft.AiurProtocol.Exceptions;
using Aiursoft.AiurProtocol.Models;
using Aiursoft.CSTools.Models;
using Aiursoft.CSTools.Tools;
using Aiursoft.DbTools.Switchable;
using Aiursoft.Kahla.Entities.Entities;
using Aiursoft.Kahla.InMemory;
using Aiursoft.Kahla.MySql;
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
        return string.IsNullOrWhiteSpace(condition) ? query : query.Where(predicate);
    }

    public static IServiceCollection AddRelationalDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var (connectionString, dbType, allowCache) = configuration.GetDbSettings();
        services.AddSwitchableRelationalDatabase(
            dbType: EntryExtends.IsInUnitTests() ? "InMemory": dbType,
            connectionString: connectionString,
            supportedDbs:
            [
                new MySqlSupportedDb(allowCache: allowCache, splitQuery: false),
                new InMemorySupportedDb()
            ]);
        return services;
    }

    public static int GetLimitedNumber(int min, int max, int suggested)
    {
        return Math.Max(min, Math.Min(max, suggested));
    }

    public static IEnumerable<T> SkipUntilEquals<T>(this IEnumerable<T> source, T? target) where T : struct
    {
        var shouldReturn = target == null;
        foreach (var item in source)
        {
            switch (shouldReturn)
            {
                case true:
                    yield return item;
                    break;
                case false:
                    shouldReturn = item.Equals(target);
                    break;
            }
        }
    }

    private static (string etag, long length) GetFileHttpProperties(string path)
    {
        var fileInfo = new FileInfo(path);

        // XOR the last write time and the file length to get a unique etag.
        var etagHash = fileInfo.LastWriteTime.ToUniversalTime().ToFileTime() ^ fileInfo.Length;
        var etag = Convert.ToString(etagHash, 16);
        return (etag, fileInfo.Length);
    }

    public static IActionResult WebFile(this ControllerBase controller, string path, string extension)
    {
        var (etag, length) = GetFileHttpProperties(path);

        // Handle etag
        controller.Response.Headers.Append("ETag", '\"' + etag + '\"');
        if (controller.Request.Headers.ContainsKey("If-None-Match"))
        {
            if (controller.Request.Headers["If-None-Match"].ToString().Trim('\"') == etag)
            {
                return new StatusCodeResult(304);
            }
        }

        // Return file result.
        controller.Response.Headers.Append("Content-Length", length.ToString());

        // Allow cache
        controller.Response.Headers.Append("Cache-Control", $"public, max-age={TimeSpan.FromDays(7).TotalSeconds}");
        return controller.PhysicalFile(path, Mime.GetContentType(extension), true);
    }
}
