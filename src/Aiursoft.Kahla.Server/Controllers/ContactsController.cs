using Aiursoft.AiurProtocol.Models;
using Aiursoft.AiurProtocol.Server;
using Aiursoft.AiurProtocol.Server.Attributes;
using Aiursoft.DocGenerator.Attributes;
using Aiursoft.Kahla.SDK.Models;
using Aiursoft.Kahla.SDK.Models.AddressModels;
using Aiursoft.Kahla.SDK.Models.ViewModels;
using Aiursoft.Kahla.Server.Attributes;
using Aiursoft.Kahla.Server.Data;
using Aiursoft.Kahla.Server.Services.Mappers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Aiursoft.Kahla.Server.Controllers;

[KahlaForceAuth]
[GenerateDoc]
[ApiExceptionHandler(
    PassthroughRemoteErrors = true,
    PassthroughAiurServerException = true)]
[ApiModelStateChecker]
[Route("api/contacts")]
public class ContactsController(
    KahlaThreadMapper kahlaThreadMapper,
    KahlaUserMapper kahlaUserMapper,
    KahlaDbContext dbContext,
    UserManager<KahlaUser> userManager) : ControllerBase
{
    [HttpGet]
    [Route("mine")]
    public async Task<IActionResult> Mine()
    {
        var user = await this.GetCurrentUser(userManager);
        await dbContext.Entry(user).Collection(t => t.KnownContacts).LoadAsync();
        var knownContacts = user.KnownContacts
            .Select(t => t.Target)
            .OrderBy(t => t?.NickName)
            .Select(kahlaUserMapper.MapOthersView);
        return this.Protocol(new MyContactsViewModel
        {
            Code = Code.ResultShown,
            Message = "Successfully get all your groups and friends.",
            Users = knownContacts
        });
    }
    
    [Produces(typeof(SearchEverythingViewModel))]
    public async Task<IActionResult> SearchEverything(SearchEverythingAddressModel model)
    {
        var usersQuery = dbContext
            .Users
            .AsNoTracking()
            .Where(t => t.AllowSearchByName || t.Id == model.SearchInput)
            .Where(t =>
                t.Email.Contains(model.SearchInput) ||
                t.NickName.Contains(model.SearchInput) ||
                t.Id == model.SearchInput);
        var usersEntities = await usersQuery
            .Take(model.Take)
            .ToListAsync();
        var usersView = usersEntities
            .Select(kahlaUserMapper.MapOthersView)
            .ToList();
        
        var threadsQuery = dbContext
            .ChatThreads
            .AsNoTracking()
            .Where(t => t.AllowSearchByName || t.Id.ToString() == model.SearchInput)
            .Where(t => 
                t.Name.Contains(model.SearchInput) ||
                t.Id.ToString() == model.SearchInput);
        var threadsEntities = await threadsQuery
            .Take(model.Take)
            .ToListAsync();
        var threadsView = threadsEntities
            .Select(kahlaThreadMapper.MapSearchedThread)
            .ToList();
    
        return this.Protocol(new SearchEverythingViewModel
        {
            TotalUsersCount = await usersQuery.CountAsync(),
            TotalThreadsCount = await threadsQuery.CountAsync(),
            Users = usersView,
            Threads = threadsView,
            Code = Code.ResultShown,
            Message = "Search result is shown."
        });
    }
}