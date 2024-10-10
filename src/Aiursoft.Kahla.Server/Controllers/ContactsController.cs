using Aiursoft.AiurProtocol.Server.Attributes;
using Aiursoft.DocGenerator.Attributes;
using Aiursoft.Kahla.SDK.Models;
using Aiursoft.Kahla.Server.Attributes;
using Aiursoft.Kahla.Server.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Aiursoft.Kahla.Server.Controllers;

[KahlaForceAuth]
[GenerateDoc]
[ApiExceptionHandler(
    PassthroughRemoteErrors = true,
    PassthroughAiurServerException = true)]
[ApiModelStateChecker]
[Route("api/contacts")]
public class ContactsController(
#pragma warning disable CS9113 // Parameter is unread.
    KahlaDbContext dbContext,
    UserManager<KahlaUser> userManager) : ControllerBase
#pragma warning restore CS9113 // Parameter is unread.
{
    [HttpGet]
    [Route("mine")]
    public IActionResult Mine()
    {
        // var user = await this.GetCurrentUser(userManager);
        // var personalRelations = (await dbContext.PrivateConversations
        //         .AsNoTracking()
        //         .Where(t => t.RequesterId == user.Id || t.TargetId == user.Id)
        //         .Select(t => user.Id == t.RequesterId ? t.TargetUser : t.RequestUser)
        //         .ToListAsync())
        //     .Select(onlineJudger.BuildUserWithOnlineStatus)
        //     .OrderBy(t => t.User.NickName);
        // var groups = await dbContext.GroupConversations
        //     .AsNoTracking()
        //     .Where(t => t.Users.Any(p => p.UserId == user.Id))
        //     .OrderBy(t => t.GroupName)
        //     .ToListAsync();
        // return this.Protocol(new MineViewModel
        // {
        //     Code = Code.ResultShown,
        //     Message = "Successfully get all your groups and friends.",
        //     Users = personalRelations,
        //     Groups = SearchedGroup.Map(groups),
        // });
        throw new NotImplementedException();
    }
    
    // [Produces(typeof(SearchEverythingViewModel))]
    // public async Task<IActionResult> SearchEverything(SearchEverythingAddressModel model)
    // {
    //     var users = _dbContext
    //         .Users
    //         .AsNoTracking()
    //         .Where(t => t.ListInSearchResult || t.Id == model.SearchInput)
    //         .Where(t =>
    //             t.MarkEmailPublic && t.Email.Contains(model.SearchInput) ||
    //             t.NickName.Contains(model.SearchInput) ||
    //             t.Id == model.SearchInput);
    //
    //     var groups = _dbContext
    //         .GroupConversations
    //         .AsNoTracking()
    //         .Where(t => t.ListInSearchResult || t.Id.ToString() == model.SearchInput)
    //         .Where(t => t.GroupName.Contains(model.SearchInput));
    //
    //     var searched = SearchedGroup.Map(await groups.ToListAsync());
    //
    //     return this.Protocol(new SearchEverythingViewModel
    //     {
    //         UsersCount = await users.CountAsync(),
    //         GroupsCount = await groups.CountAsync(),
    //         Users = await users.Take(model.Take).ToListAsync(),
    //         Groups = searched,
    //         Code = Code.ResultShown,
    //         Message = "Search result is shown."
    //     });
    // }
}