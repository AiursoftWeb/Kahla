using System.Security.Claims;
using Aiursoft.AiurProtocol.Exceptions;
using Aiursoft.AiurProtocol.Models;
using Aiursoft.Kahla.SDK.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Aiursoft.Kahla.Server;

public static class Extensions
{
    public static string? GetUserId(this ClaimsPrincipal user)
    {
        return user.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
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
    
    public static async Task<List<T2>> SelectAsListAsync<T1, T2>(this IEnumerable<T1> source, Func<T1, Task<T2>> selector)
    {
        var result = new List<T2>();
        foreach (var item in source)
        {
            result.Add(await selector(item));
        }
        return result;
    }
}