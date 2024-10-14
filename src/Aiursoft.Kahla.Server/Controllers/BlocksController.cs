using Aiursoft.AiurProtocol.Models;
using Aiursoft.AiurProtocol.Server;
using Aiursoft.AiurProtocol.Server.Attributes;
using Aiursoft.DocGenerator.Attributes;
using Aiursoft.Kahla.SDK.Models.AddressModels;
using Aiursoft.Kahla.SDK.Models.Entities;
using Aiursoft.Kahla.SDK.Models.ViewModels;
using Aiursoft.Kahla.Server.Attributes;
using Aiursoft.Kahla.Server.Data;
using Aiursoft.Kahla.Server.Services.AppService;
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
    KahlaDbContext dbContext,
    ILogger<BlocksController> logger) : ControllerBase
{
    // This lock is used to avoid blocking the same user multiple times.
    private static readonly SemaphoreSlim BlockUserLock = new(1, 1);
    
    [HttpGet]
    [Route("list")]
    [Produces<MyBlocksViewModel>]
    public async Task<IActionResult> List([FromQuery]int skip = 0, [FromQuery]int take = 20)
    {
        var currentUserId = User.GetUserId();
        logger.LogInformation("User with Id: {Id} is trying to get all his known blocks.", currentUserId);
        var (totalCount, knownBlocks) = await userAppService.GetMyBlocksPagedAsync(currentUserId, skip, take);
        logger.LogInformation("User with Id: {Id} successfully get all his known blocks with total {Count}.", currentUserId, knownBlocks.Count);
        return this.Protocol(new MyBlocksViewModel
        {
            Code = Code.ResultShown,
            Message = $"Successfully get your first {take} known blocks and skipped {skip} blocks.",
            KnownBlocks = knownBlocks,
            TotalKnownBlocks = totalCount
        });
    }
    
    [HttpPost]
    [Route("search")]
    [Produces<MyBlocksViewModel>]
    public async Task<IActionResult> Search([FromForm]SearchEverythingAddressModel model)
    {
        var currentUserId = User.GetUserId();
        logger.LogInformation("User with Id: {Id} is trying to search his blocks with keyword: {Search}.", currentUserId, model.SearchInput);
        
        var (totalCount, knownBlocks) = await userAppService.SearchMyBlocksPagedAsync(model.SearchInput, currentUserId, model.Skip, model.Take);
        logger.LogInformation("User with Id: {Id} successfully searched his blocks with keyword: {Search} with total {Count}.", currentUserId, model.SearchInput, knownBlocks.Count);
        return this.Protocol(new MyBlocksViewModel
        {
            Code = Code.ResultShown,
            Message = $"Successfully searched your first {model.Take} known blocks and skipped {model.Skip} blocks.",
            KnownBlocks = knownBlocks,
            TotalKnownBlocks = totalCount
        });
    }
    
    [HttpPost]
    [Route("block/{id}")]
    public async Task<IActionResult> BlockNew([FromRoute] string id)
    {
        var currentUserId = User.GetUserId();
        logger.LogInformation("User with Id: {Id} is trying to block a user with id: {TargetId}.", currentUserId, id);
        var target = await dbContext.Users.FindAsync(id);
        if (target == null)
        {
            logger.LogWarning("User with Id: {Id} is trying to block a user with id: {TargetId} but the target does not exist.", currentUserId, id);
            return this.Protocol(Code.NotFound, "The target user does not exist.");
        }
        
        logger.LogTrace("Waiting for the lock to block a user from id {SourceId} with id: {TargetId}.", currentUserId, target.Id);
        await BlockUserLock.WaitAsync();
        try
        {
            var duplicated =
                await dbContext.BlockRecords.AnyAsync(t => t.CreatorId == currentUserId && t.TargetId == target.Id);
            if (duplicated)
            {
                logger.LogWarning(
                    "User with Id: {Id} is trying to block a user with id: {TargetId} but the target is already blocked.", currentUserId, id);
                return this.Protocol(Code.Conflict, "The target user is already in your block list.");
            }

            var blockRecord = new BlockRecord
            {
                CreatorId = currentUserId,
                TargetId = target.Id,
                AddTime = DateTime.UtcNow
            };
            await dbContext.BlockRecords.AddAsync(blockRecord);
            await dbContext.SaveChangesAsync();
        }
        finally
        {
            BlockUserLock.Release();
            logger.LogTrace("Released the lock to block a user from id {SourceId} with id: {TargetId}.", currentUserId, id);
        }
        logger.LogInformation("User with Id: {Id} successfully blocked a user with id: {TargetId}.", currentUserId, id);
        return this.Protocol(Code.JobDone, "Successfully blocked the target user.");
    }
    
    [HttpPost]
    [Route("remove/{id}")]
    public async Task<IActionResult> RemoveBlock(string id)
    {
        var currentUserId = User.GetUserId();
        logger.LogInformation("User with Id: {Id} is trying to remove a block record with id: {TargetId}.", currentUserId, id);
        var blockRecord = await dbContext.BlockRecords.SingleOrDefaultAsync(t => t.CreatorId == currentUserId && t.TargetId == id);
        if (blockRecord == null)
        {
            logger.LogWarning("User with Id: {Id} is trying to remove a block record with id: {TargetId} but the target is not in the block list.", currentUserId, id);
            return this.Protocol(Code.NotFound, "The target user is not in your block list.");
        }
        dbContext.BlockRecords.Remove(blockRecord);
        await dbContext.SaveChangesAsync();
        logger.LogInformation("User with Id: {Id} successfully removed a block record with id: {TargetId}.", currentUserId, id);
        return this.Protocol(Code.JobDone, "Successfully removed the target user from your block list.");
    }
}