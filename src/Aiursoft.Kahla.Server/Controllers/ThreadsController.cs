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
[Route("api/threads")]
public class ThreadsController(
    ILogger<ThreadsController> logger,
    ThreadJoinedViewAppService threadService,
    KahlaDbContext dbContext,
    UserManager<KahlaUser> userManager) : ControllerBase
{
    [HttpGet]
    [Route("mine")]
    public async Task<IActionResult> Mine([FromQuery]int skip = 0, [FromQuery]int take = 20)
    {
        var currentUser = await this.GetCurrentUser(userManager);
        var (count, threads) = await threadService.QueryThreadsIJoinedAsync(currentUser.Id, skip, take);
        return this.Protocol(new MyThreadsViewModel
        {
            Code = Code.ResultShown,
            Message = $"Successfully get your first {take} joined threads and skipped {skip} threads.",
            KnownThreads = threads,
            TotalCount = count
        });
    }
    
    [HttpPost]
    [Route("hard-invite/{id}")]
    public async Task<IActionResult> HardInvite([FromRoute]string id)
    {
        var currentUser = await this.GetCurrentUser(userManager);
        var targetUser = await dbContext.Users.FindAsync(id);
        if (targetUser == null)
        {
            return this.Protocol(Code.NotFound, $"The target user with ID {id} does not exist.");
        }
        if (!targetUser.AllowHardInvitation)
        {
            return this.Protocol(Code.Unauthorized, "The target user does not allow hard invitation.");
        }
        var targetUserBlockedMe = await dbContext.BlockRecords
            .Where(t => t.CreatorId == targetUser.Id)
            .Where(t => t.TargetId == currentUser.Id)
            .AnyAsync();
        if (targetUserBlockedMe)
        {
            return this.Protocol(Code.Conflict, "The target user has blocked you so you can not create a thread with him/her.");
        }
        
        var strategy = dbContext.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await dbContext.Database.BeginTransactionAsync();
            try
            {
                // Step 1: Create a new thread without setting OwnerRelationId initially.
                var thread = new ChatThread();
                dbContext.ChatThreads.Add(thread);
                logger.LogInformation("Creating a new thread...");
                await dbContext.SaveChangesAsync(); // This will generate the thread's id

                // Step 2: Add myself to the thread with role Admin
                var myRelation = new UserThreadRelation
                {
                    UserId = currentUser.Id,
                    ThreadId = thread.Id,
                    UserThreadRole = UserThreadRole.Admin
                };
                dbContext.UserThreadRelations.Add(myRelation);
                logger.LogInformation("Adding myself (ID is {ID}) to the thread...", currentUser.Id);
                await dbContext.SaveChangesAsync(); // This will generate myRelation's id

                // Step 3: Set the owner of the thread after myRelation is saved
                thread.OwnerRelationId = myRelation.Id;
                dbContext.ChatThreads.Update(thread);
                // Don't call SaveChangesAsync here for better performance.

                if (currentUser.Id != targetUser.Id)
                {
                    // Step 4: Add the target user to the thread
                    var targetRelation = new UserThreadRelation
                    {
                        UserId = targetUser.Id,
                        ThreadId = thread.Id,
                        UserThreadRole = UserThreadRole.Member
                    };
                    dbContext.UserThreadRelations.Add(targetRelation);
                    // Don't call SaveChangesAsync here for better performance.
                }
                else
                {
                    logger.LogWarning(
                        "The current user and the target user are the same. Skip adding the target user to the thread. This might because of the user creating a thread with himself/herself.");
                }

                // Commit the transaction if everything is successful
                logger.LogInformation("Setting the owner of the thread to myself (ID is {ID})... And adding the target user (ID is {ID}) to the thread...", currentUser.Id, targetUser.Id);
                await dbContext.SaveChangesAsync(); // Save the targetRelation
                
                await transaction.CommitAsync();
                logger.LogInformation("The thread has been created successfully.");
                return this.Protocol(new CreateNewThreadViewModel
                {
                    NewThreadId = thread.Id,
                    Code = Code.JobDone,
                    Message = "The thread has been created successfully."
                });
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to create a thread.");
                await transaction.RollbackAsync();
                return this.Protocol(Code.UnknownError,
                    "Failed to create the thread. Might because of a database error.");
            }
        });
    }
}