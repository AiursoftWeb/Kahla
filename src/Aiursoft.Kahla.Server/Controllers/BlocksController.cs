using Aiursoft.AiurProtocol.Models;
using Aiursoft.AiurProtocol.Server;
using Aiursoft.AiurProtocol.Server.Attributes;
using Aiursoft.DocGenerator.Attributes;
using Aiursoft.Kahla.SDK.Models;
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
[Route("api/blocks")]
public class BlocksController(
    UserManager<KahlaUser> userManager,
    KahlaDbContext dbContext,
    KahlaMapper kahlaMapper,
    ILogger<BlocksController> logger) : ControllerBase
{
    // This lock is used to prevent blocking the same user multiple times.
    private static readonly SemaphoreSlim BlockUserLock = new(1, 1);
    
    [HttpGet]
    [Route("list")]
    [Produces<MyBlocksViewModel>]
    public async Task<IActionResult> List([FromQuery]int take = 20)
    {
        var user = await this.GetCurrentUser(userManager);
        logger.LogInformation("User with email: {Email} is trying to get all his known blocks.", user.Email);
        var knownBlocks = await dbContext
            .BlockRecords
            .AsNoTracking()
            .Where(t => t.CreatorId == user.Id)
            .Select(t => t.Target)
            .OrderBy(t => t!.NickName)
            .Take(take)
            .ToListAsync();
        var mappedKnownBlocks = await knownBlocks
            .SelectAsListAsync(kahlaMapper.MapOtherUserViewAsync);
        logger.LogInformation("User with email: {Email} successfully get all his known blocks with total {Count}.", user.Email, knownBlocks.Count);
        return this.Protocol(new MyBlocksViewModel
        {
            Code = Code.ResultShown,
            Message = "Successfully get all your known blocks.",
            KnownBlocks = mappedKnownBlocks
        });
    }
    
    [HttpPost]
    [Route("block/{id}")]
    public async Task<IActionResult> BlockNew([FromRoute] string id)
    {
        var user = await this.GetCurrentUser(userManager);
        logger.LogInformation("User with email: {Email} is trying to block a user with id: {TargetId}.", user.Email, id);
        var target = await dbContext.Users.FindAsync(id);
        if (target == null)
        {
            logger.LogWarning("User with email: {Email} is trying to block a user with id: {TargetId} but the target does not exist.", user.Email, id);
            return this.Protocol(Code.NotFound, "The target user does not exist.");
        }
        
        logger.LogTrace("Waiting for the lock to block a user from id {SourceId} with id: {TargetId}.", user.Id, id);
        await BlockUserLock.WaitAsync();
        try
        {
            var duplicated =
                await dbContext.BlockRecords.AnyAsync(t => t.CreatorId == user.Id && t.TargetId == target.Id);
            if (duplicated)
            {
                logger.LogWarning(
                    "User with email: {Email} is trying to block a user with id: {TargetId} but the target is already blocked.", user.Email, id);
                return this.Protocol(Code.Conflict, "The target user is already in your block list.");
            }

            var blockRecord = new BlockRecord
            {
                CreatorId = user.Id,
                TargetId = target.Id,
                AddTime = DateTime.UtcNow
            };
            await dbContext.BlockRecords.AddAsync(blockRecord);
            await dbContext.SaveChangesAsync();
        }
        finally
        {
            BlockUserLock.Release();
            logger.LogTrace("Released the lock to block a user from id {SourceId} with id: {TargetId}.", user.Id, id);
        }
        logger.LogInformation("User with email: {Email} successfully blocked a user with id: {TargetId}.", user.Email, id);
        return this.Protocol(Code.JobDone, "Successfully blocked the target user. Please call the 'list' API to get the latest block list.");
    }
    
    [HttpPost]
    [Route("remove/{id}")]
    public async Task<IActionResult> RemoveBlock(string id)
    {
        var user = await this.GetCurrentUser(userManager);
        logger.LogInformation("User with email: {Email} is trying to remove a block record with id: {TargetId}.", user.Email, id);
        var blockRecord = await dbContext.BlockRecords.SingleOrDefaultAsync(t => t.CreatorId == user.Id && t.TargetId == id);
        if (blockRecord == null)
        {
            logger.LogWarning("User with email: {Email} is trying to remove a block record with id: {TargetId} but the target is not in the block list.", user.Email, id);
            return this.Protocol(Code.NotFound, "The target user is not in your block list.");
        }
        dbContext.BlockRecords.Remove(blockRecord);
        await dbContext.SaveChangesAsync();
        logger.LogInformation("User with email: {Email} successfully removed a block record with id: {TargetId}.", user.Email, id);
        return this.Protocol(Code.JobDone, "Successfully removed the target user from your block list. Please call the 'list' API to get the latest block list.");
    }
}