using Aiursoft.AiurProtocol.Server.Attributes;
using Aiursoft.DocGenerator.Attributes;
using Aiursoft.Kahla.Server.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace Aiursoft.Kahla.Server.Controllers;

[KahlaForceAuth]
[GenerateDoc]
[ApiExceptionHandler(
    PassthroughRemoteErrors = true,
    PassthroughAiurServerException = true)]
[ApiModelStateChecker]
[Route("api/friendship")]
public class ContactsController : ControllerBase
{
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