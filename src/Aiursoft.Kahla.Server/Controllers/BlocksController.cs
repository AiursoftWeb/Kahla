using Aiursoft.AiurProtocol.Models;
using Aiursoft.AiurProtocol.Server;
using Aiursoft.AiurProtocol.Server.Attributes;
using Aiursoft.DocGenerator.Attributes;
using Aiursoft.Kahla.SDK.Models;
using Aiursoft.Kahla.SDK.Models.ViewModels;
using Aiursoft.Kahla.Server.Attributes;
using Aiursoft.Kahla.Server.Data;
using Aiursoft.Kahla.Server.Services.AppService;
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
[Route("api/blocks")]
public class BlocksController(
    UserOthersViewAppService userAppService,
    UserManager<KahlaUser> userManager,
    KahlaDbContext dbContext,
    ILogger<BlocksController> logger) : ControllerBase
{
    // This lock is used to prevent blocking the same user multiple times.
    private static readonly SemaphoreSlim BlockUserLock = new(1, 1);
    
    [HttpGet]
    [Route("list")]
    [Produces<MyBlocksViewModel>]
    public async Task<IActionResult> List([FromQuery]int skip = 0, [FromQuery]int take = 20)
    {
        var currentUser = await this.GetCurrentUser(userManager);
        logger.LogInformation("User with email: {Email} is trying to get all his known blocks.", currentUser.Email);
        var (totalCount, knownBlocks) = await userAppService.GetMyBlocksPagedAsync(currentUser.Id, skip, take);
        logger.LogInformation("User with email: {Email} successfully get all his known blocks with total {Count}.", currentUser.Email, knownBlocks.Count);
        return this.Protocol(new MyBlocksViewModel
        {
            Code = Code.ResultShown,
            Message = "Successfully get all your known blocks.",
            KnownBlocks = knownBlocks,
            TotalKnownBlocks = totalCount
        });
    }
    
    [HttpPost]
    [Route("block/{id}")]
    public async Task<IActionResult> BlockNew([FromRoute] string id)
    {
        var currentUser = await this.GetCurrentUser(userManager);
        logger.LogInformation("User with email: {Email} is trying to block a user with id: {TargetId}.", currentUser.Email, id);
        var target = await dbContext.Users.FindAsync(id);
        if (target == null)
        {
            logger.LogWarning("User with email: {Email} is trying to block a user with id: {TargetId} but the target does not exist.", currentUser.Email, id);
            return this.Protocol(Code.NotFound, "The target user does not exist.");
        }
        
        logger.LogTrace("Waiting for the lock to block a user from id {SourceId} with id: {TargetId}.", currentUser.Id, target.Id);
        await BlockUserLock.WaitAsync();
        try
        {
            var duplicated =
                await dbContext.BlockRecords.AnyAsync(t => t.CreatorId == currentUser.Id && t.TargetId == target.Id);
            if (duplicated)
            {
                logger.LogWarning(
                    "User with email: {Email} is trying to block a user with id: {TargetId} but the target is already blocked.", currentUser.Email, id);
                return this.Protocol(Code.Conflict, "The target user is already in your block list.");
            }

            var blockRecord = new BlockRecord
            {
                CreatorId = currentUser.Id,
                TargetId = target.Id,
                AddTime = DateTime.UtcNow
            };
            await dbContext.BlockRecords.AddAsync(blockRecord);
            await dbContext.SaveChangesAsync();
        }
        finally
        {
            BlockUserLock.Release();
            logger.LogTrace("Released the lock to block a user from id {SourceId} with id: {TargetId}.", currentUser.Id, id);
        }
        logger.LogInformation("User with email: {Email} successfully blocked a user with id: {TargetId}.", currentUser.Email, id);
        return this.Protocol(Code.JobDone, "Successfully blocked the target user. Please call the 'list' API to get the latest block list.");
    }
    
    [HttpPost]
    [Route("remove/{id}")]
    public async Task<IActionResult> RemoveBlock(string id)
    {
        var currentUser = await this.GetCurrentUser(userManager);
        logger.LogInformation("User with email: {Email} is trying to remove a block record with id: {TargetId}.", currentUser.Email, id);
        var blockRecord = await dbContext.BlockRecords.SingleOrDefaultAsync(t => t.CreatorId == currentUser.Id && t.TargetId == id);
        if (blockRecord == null)
        {
            logger.LogWarning("User with email: {Email} is trying to remove a block record with id: {TargetId} but the target is not in the block list.", currentUser.Email, id);
            return this.Protocol(Code.NotFound, "The target user is not in your block list.");
        }
        dbContext.BlockRecords.Remove(blockRecord);
        await dbContext.SaveChangesAsync();
        logger.LogInformation("User with email: {Email} successfully removed a block record with id: {TargetId}.", currentUser.Email, id);
        return this.Protocol(Code.JobDone, "Successfully removed the target user from your block list. Please call the 'list' API to get the latest block list.");
    }
}