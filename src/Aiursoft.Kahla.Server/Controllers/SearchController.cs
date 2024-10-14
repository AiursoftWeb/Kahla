using Aiursoft.AiurProtocol.Models;
using Aiursoft.AiurProtocol.Server;
using Aiursoft.AiurProtocol.Server.Attributes;
using Aiursoft.DocGenerator.Attributes;
using Aiursoft.Kahla.SDK.Models.AddressModels;
using Aiursoft.Kahla.SDK.Models.ViewModels;
using Aiursoft.Kahla.Server.Attributes;
using Aiursoft.Kahla.Server.Services.AppService;
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
    ILogger<ContactsController> logger) : ControllerBase
{
    [HttpPost]
    [Route("search-server")]
    [Produces(typeof(SearchEverythingViewModel))]
    public async Task<IActionResult> SearchEverything([FromForm]SearchAddressModel model)
    {
        var currentUserId = User.GetUserId();
        logger.LogInformation("User with Id: {Id} is trying to search for {SearchInput}. Take: {Take}.", currentUserId, model.SearchInput, model.Take);
        
        var (totalUsersCount, users) = await usersAppAppService.SearchUsersPagedAsync(model.SearchInput, model.Excluding, currentUserId, model.Skip, model.Take);
        logger.LogInformation("User with Id: {Id} successfully got {Count} users from search result.", currentUserId, users.Count);
        
        var (totalThreadsCount, threads) = await threadsAppService.SearchThreadsPagedAsync(model.SearchInput, model.Excluding, currentUserId, model.Skip, model.Take);
        logger.LogInformation("User with Id: {Id} successfully got {Count} threads from search result.", currentUserId, threads.Count);
    
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