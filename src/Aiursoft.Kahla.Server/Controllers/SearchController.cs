using Aiursoft.AiurProtocol.Models;
using Aiursoft.AiurProtocol.Server;
using Aiursoft.AiurProtocol.Server.Attributes;
using Aiursoft.DocGenerator.Attributes;
using Aiursoft.Kahla.SDK.Models.AddressModels;
using Aiursoft.Kahla.SDK.Models.ViewModels;
using Aiursoft.Kahla.Server.Attributes;
using Aiursoft.Kahla.Server.Services.AppService;
using Aiursoft.WebTools.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace Aiursoft.Kahla.Server.Controllers;

[LimitPerMin]
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
    [HttpGet]
    [Route("search-users")]
    [Produces(typeof(SearchUsersViewModel))]
    public async Task<IActionResult> SearchUsers([FromQuery]SearchAddressModel model)
    {
        var currentUserId = User.GetUserId();
        logger.LogInformation("User with Id: {Id} is trying to search for users with '{SearchInput}' globally. Take: {Take}.", currentUserId, model.SearchInput, model.Take);
        
        var (totalUsersCount, users) = await usersAppAppService.SearchUsersPagedAsync(model.SearchInput, model.Excluding, currentUserId, model.Skip, model.Take);
        logger.LogInformation("User with Id: {Id} successfully got {Count} users from global search result.", currentUserId, users.Count);
        
        return this.Protocol(new SearchUsersViewModel
        {
            TotalUsersCount = totalUsersCount,
            Users = users,
            Code = Code.ResultShown,
            Message = $"Search result is shown. Skip: {model.Skip}. Take: {model.Take}."
        });
    }
    
    [HttpGet]
    [Route("search-threads")]
    [Produces(typeof(SearchThreadsViewModel))]
    public async Task<IActionResult> SearchThreads([FromQuery]SearchAddressModel model)
    {
        var currentUserId = User.GetUserId();
        logger.LogInformation("User with Id: {Id} is trying to search for threads with '{SearchInput}' globally. Take: {Take}.", currentUserId, model.SearchInput, model.Take);
        
        var (totalThreadsCount, threads) = await threadsAppService.SearchThreadsPagedAsync(model.SearchInput, model.Excluding, currentUserId, model.Skip, model.Take);
        logger.LogInformation("User with Id: {Id} successfully got {Count} threads from global search result.", currentUserId, threads.Count);
    
        return this.Protocol(new SearchThreadsViewModel
        {
            TotalThreadsCount = totalThreadsCount,
            Threads = threads,
            Code = Code.ResultShown,
            Message = $"Search result is shown. Skip: {model.Skip}. Take: {model.Take}."
        });
    }
}