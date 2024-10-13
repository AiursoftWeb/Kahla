using Aiursoft.AiurProtocol.Models;
using Aiursoft.AiurProtocol.Server;
using Aiursoft.AiurProtocol.Server.Attributes;
using Aiursoft.DocGenerator.Attributes;
using Aiursoft.Kahla.SDK.Models;
using Aiursoft.Kahla.SDK.Models.AddressModels;
using Aiursoft.Kahla.SDK.Models.ViewModels;
using Aiursoft.Kahla.Server.Attributes;
using Aiursoft.Kahla.Server.Services.AppService;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Aiursoft.Kahla.Server.Controllers;

[KahlaForceAuth]
[GenerateDoc]
[ApiExceptionHandler(
    PassthroughRemoteErrors = true,
    PassthroughAiurServerException = true)]
[ApiModelStateChecker]
[Route("api/search")]
public class SearchController(
    UserOthersViewAppService usersAppAppService,
    ThreadOthersViewAppService threadsAppService,
    ILogger<ContactsController> logger,
    UserManager<KahlaUser> userManager) : ControllerBase
{
    [HttpPost]
    [Route("search-everything")]
    [Produces(typeof(SearchEverythingViewModel))]
    public async Task<IActionResult> SearchEverything(SearchEverythingAddressModel model)
    {
        var user = await this.GetCurrentUser(userManager);
        logger.LogInformation("User with email: {Email} is trying to search for {SearchInput}. Take: {Take}.", user.Email, model.SearchInput, model.Take);
        
        var (totalUsersCount, users) = await usersAppAppService.SearchUsersPagedAsync(model.SearchInput, user.Id, model.Skip, model.Take);
        logger.LogInformation("User with email: {Email} successfully got {Count} users from search result.", user.Email, users.Count);
        
        var (totalThreadsCount, threads) = await threadsAppService.SearchThreadsPagedAsync(model.SearchInput, user.Id, model.Skip, model.Take);
        logger.LogInformation("User with email: {Email} successfully got {Count} threads from search result.", user.Email, threads.Count);
    
        return this.Protocol(new SearchEverythingViewModel
        {
            TotalUsersCount = totalUsersCount,
            TotalThreadsCount = totalThreadsCount,
            Users = users,
            Threads = threads,
            Code = Code.ResultShown,
            Message = $"Search result is shown. Skip: {model.Skip}. Take: {model.Take}."
        });
    }
}