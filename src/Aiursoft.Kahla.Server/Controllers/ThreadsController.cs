using System.ComponentModel.DataAnnotations;
using Aiursoft.AiurProtocol.Models;
using Aiursoft.AiurProtocol.Server;
using Aiursoft.AiurProtocol.Server.Attributes;
using Aiursoft.CSTools.Tools;
using Aiursoft.DocGenerator.Attributes;
using Aiursoft.Kahla.SDK.Events;
using Aiursoft.Kahla.SDK.Models;
using Aiursoft.Kahla.SDK.Models.AddressModels;
using Aiursoft.Kahla.SDK.Models.Mapped;
using Aiursoft.Kahla.SDK.Models.ViewModels;
using Aiursoft.Kahla.Server.Attributes;
using Aiursoft.Kahla.Server.Data;
using Aiursoft.Kahla.Server.Models.Entities;
using Aiursoft.Kahla.Server.Services;
using Aiursoft.Kahla.Server.Services.AppService;
using Aiursoft.WebTools.Attributes;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Aiursoft.Kahla.Server.Controllers;

[LimitPerMin]
[KahlaForceAuth]
[GenerateDoc]
[ApiExceptionHandler(
    PassthroughRemoteErrors = true,
    PassthroughAiurServerException = true)]
[ApiModelStateChecker]
[Route("api/threads")]
public class ThreadsController(
    BufferedKahlaPushService kahlaPushService,
    LocksInMemoryDb locksInMemoryDb,
    ArrayDbContext arrayDbContext,
    IDataProtectionProvider dataProtectionProvider,
    QuickMessageAccess quickMessageAccess,
    ILogger<ThreadsController> logger,
    ThreadJoinedViewAppService threadService,
    ThreadOthersViewAppService threadOthersViewAppService,
    UserInThreadViewAppService userAppService,
    KahlaRelationalDbContext relationalDbContext) : ControllerBase
{
    // TODO:
    // When thread-dissolved, you-direct-joined, you-been-kicked, you-leaved, someone-left, someone-direct-join, someone-soft-invite-finished, create-scratched, new-message, your-hard-invite-finished, you-was-hard-invited, you-completed-software-invited
    // Push to user's websocket so his thread list can be updated.
    [HttpGet]
    [Route("mine")]
    [Produces<MyThreadsViewModel>]
    public async Task<IActionResult> Mine([FromQuery] MyThreadsAddressModel model)
    {
        var currentUserId = User.GetUserId();
        logger.LogInformation("User with Id: {Id} is trying to list his threads.", currentUserId);
        var threads = await threadService.GetThreadsIJoinedAsync(currentUserId, model.SkipTillThreadId, model.Take);
        logger.LogInformation("User with Id: {Id} successfully list his threads with total {Count}.", currentUserId,
            threads.Count);
        return this.Protocol(new MyThreadsViewModel
        {
            Code = Code.ResultShown,
            Message = $"Successfully get your first {model.Take} threads sorted by last message time.",
            KnownThreads = threads,
            TotalCount = -1
        });
    }

    [HttpGet]
    [Route("search")]
    [Produces<MyThreadsViewModel>]
    public async Task<IActionResult> Search([FromQuery] SearchAddressModel model)
    {
        var currentUserId = User.GetUserId();
        logger.LogInformation("User with Id: {Id} is trying to search his threads with keyword: {Search}.",
            currentUserId, model.SearchInput);
        var (count, threads) = await threadService.SearchThreadsIJoinedAsync(model.SearchInput, model.Excluding,
            currentUserId, model.Skip, model.Take);
        logger.LogInformation(
            "User with Id: {Id} successfully searched his threads with keyword: {Search} with total {Count}.",
            currentUserId, model.SearchInput, threads.Count);
        return this.Protocol(new MyThreadsViewModel
        {
            Code = Code.ResultShown,
            Message =
                $"Successfully get your first {model.Take} threads from search result and skipped {model.Skip} threads.",
            KnownThreads = threads,
            TotalCount = count
        });
    }

    [HttpGet]
    [Route("members/{id:int}")]
    [Produces<ThreadMembersViewModel>]
    public async Task<IActionResult> Members([FromRoute] int id, [FromQuery] int skip = 0, [FromQuery] int take = 20)
    {
        var currentUserId = User.GetUserId();
        logger.LogInformation("User with Id: {Id} is trying to get the members of the thread. Thread ID: {ThreadID}.",
            currentUserId, id);
        var myRelation = await relationalDbContext.UserThreadRelations
            .AsSplitQuery()
            .Where(t => t.UserId == currentUserId)
            .Where(t => t.ThreadId == id)
            .Include(t => t.Thread)
            .FirstOrDefaultAsync();
        if (myRelation == null)
        {
            return this.Protocol(Code.Unauthorized, "You are not a member of this thread. So you can not get the members.");
        }

        if (myRelation.Thread.AllowMembersEnlistAllMembers == false &&
            myRelation.UserThreadRole != UserThreadRole.Admin)
        {
            return this.Protocol(Code.Unauthorized, "This thread does not allow members to enlist members. Please contact the admin for more information.");
        }

        var (count, members) = await userAppService.QueryMembersInThreadAsync(id, currentUserId, skip, take);
        logger.LogInformation(
            "User with Id: {Id} successfully get the members of the thread. Thread ID: {ThreadID}. Total members: {Count}.",
            currentUserId, id, members.Count);
        return this.Protocol(new ThreadMembersViewModel
        {
            Code = Code.ResultShown,
            Message = $"Successfully get the first {take} members of the thread and skipped {skip} members.",
            Members = members,
            TotalCount = count
        });
    }

    [HttpGet]
    [Route("details-anonymous/{id:int}")]
    [Produces<ThreadAnonymousViewModel>]
    public async Task<IActionResult> DetailsAnonymous([FromRoute] int id)
    {
        var currentUserId = User.GetUserId();
        logger.LogInformation(
            "User with Id: {Id} is trying to get the thread details anonymously. Thread ID: {ThreadID}.", currentUserId,
            id);

        var thread = await threadOthersViewAppService.GetThreadAsync(id, currentUserId);
        if (thread == null)
        {
            return this.Protocol(Code.NotFound, $"The thread with ID {id} does not exist. It might have been dissolved.");
        }

        logger.LogInformation(
            "User with Id: {Id} successfully get the thread details anonymously. Thread ID: {ThreadID}.", currentUserId,
            id);
        return this.Protocol(new ThreadAnonymousViewModel
        {
            Code = Code.ResultShown,
            Message = "Successfully get the thread details.",
            Thread = thread,
        });
    }

    [HttpGet]
    [Route("details-joined/{id:int}")]
    [Produces<ThreadDetailsViewModel>]
    public async Task<IActionResult> Details([FromRoute] int id)
    {
        var currentUserId = User.GetUserId();
        logger.LogInformation("User with Id: {Id} is trying to get the thread details. Thread ID: {ThreadID}.",
            currentUserId, id);
        var myRelation = await relationalDbContext.UserThreadRelations
            .Where(t => t.UserId == currentUserId)
            .Where(t => t.ThreadId == id)
            .FirstOrDefaultAsync();
        if (myRelation == null)
        {
            return this.Protocol(Code.Unauthorized, "You are not a member of this thread. So you can not get the details.");
        }

        var thread = await threadService.GetThreadIJoinedAsync(id, currentUserId);
        logger.LogInformation("User with Id: {Id} successfully get the thread details. Thread ID: {ThreadID}.",
            currentUserId, id);
        return this.Protocol(new ThreadDetailsViewModel
        {
            Code = Code.ResultShown,
            Message = "Successfully get the details of the thread.",
            Thread = thread,
        });
    }

    [HttpPatch]
    [Route("update-thread/{id:int}")]
    public async Task<IActionResult> UpdateThread([FromRoute] int id, [FromForm] UpdateThreadAddressModel model)
    {
        var currentUserId = User.GetUserId();
        logger.LogInformation("User with Id: {Id} is trying to update the thread's properties. Thread ID: {ThreadID}.",
            currentUserId, id);
        var thread = await relationalDbContext.ChatThreads.FindAsync(id);
        if (thread == null)
        {
            return this.Protocol(Code.NotFound, "The thread does not exist. It might have been dissolved.");
        }

        var myRelation = await relationalDbContext.UserThreadRelations
            .Where(t => t.UserId == currentUserId)
            .Where(t => t.ThreadId == id)
            .FirstOrDefaultAsync();
        if (myRelation == null)
        {
            return this.Protocol(Code.Unauthorized, "You are not a member of this thread. So you can not update it.");
        }

        if (myRelation.UserThreadRole != UserThreadRole.Admin)
        {
            return this.Protocol(Code.Unauthorized,
                "You are not the admin of this thread. Only the admin can update the thread's properties.");
        }

        var updatedProperties = new List<string>();
        if (model.Name != null)
        {
            thread.Name = model.Name;
            updatedProperties.Add(nameof(thread.Name));
        }

        if (model.IconFilePath != null)
        {
            thread.IconFilePath = model.IconFilePath;
            updatedProperties.Add(nameof(thread.IconFilePath));
        }

        if (model.AllowDirectJoinWithoutInvitation.HasValue)
        {
            thread.AllowDirectJoinWithoutInvitation = model.AllowDirectJoinWithoutInvitation == true;
            updatedProperties.Add(nameof(thread.AllowDirectJoinWithoutInvitation));
        }

        if (model.AllowMemberSoftInvitation.HasValue)
        {
            thread.AllowMemberSoftInvitation = model.AllowMemberSoftInvitation == true;
            updatedProperties.Add(nameof(thread.AllowMemberSoftInvitation));
        }

        if (model.AllowMembersSendMessages.HasValue)
        {
            thread.AllowMembersSendMessages = model.AllowMembersSendMessages == true;
            updatedProperties.Add(nameof(thread.AllowMembersSendMessages));
        }

        if (model.AllowMembersEnlistAllMembers.HasValue)
        {
            thread.AllowMembersEnlistAllMembers = model.AllowMembersEnlistAllMembers == true;
            updatedProperties.Add(nameof(thread.AllowMembersEnlistAllMembers));
        }

        if (model.AllowSearchByName.HasValue)
        {
            thread.AllowSearchByName = model.AllowSearchByName == true;
            updatedProperties.Add(nameof(thread.AllowSearchByName));
        }

        await relationalDbContext.SaveChangesAsync();
        var updatedPropertiesName = string.Join(", ", updatedProperties);
        logger.LogInformation("User with Id: {Id} updated the thread's properties: {Properties}.", currentUserId,
            updatedPropertiesName);
        return this.Protocol(Code.JobDone, $"Successfully updated the thread's properties: {updatedPropertiesName}.");
    }

    // Directly join a thread without an invitation. (Only when AllowDirectJoinWithoutInvitation is true)
    [HttpPost]
    [Route("direct-join/{id:int}")]
    public async Task<IActionResult> DirectJoin([FromRoute] int id)
    {
        var currentUserId = User.GetUserId();
        logger.LogInformation("User with Id: {Id} is trying to directly join a thread. Thread ID: {ThreadID}.",
            currentUserId, id);
        var thread = await relationalDbContext.ChatThreads.FindAsync(id);
        if (thread == null)
        {
            return this.Protocol(Code.NotFound, "The thread does not exist. It might have been dissolved.");
        }

        if (!thread.AllowDirectJoinWithoutInvitation)
        {
            return this.Protocol(Code.Unauthorized, "This thread does not allow joining without an invitation. Please try to contact the owner or an admin to get an invitation.");
        }

        var joinThreadLock = locksInMemoryDb.GetJoinThreadOperationLock(userId: currentUserId, threadId: id);
        await joinThreadLock.WaitAsync();
        try
        {
            var iMJoined = await relationalDbContext.UserThreadRelations
                .Where(t => t.UserId == currentUserId)
                .Where(t => t.ThreadId == id)
                .AnyAsync();
            if (iMJoined)
            {
                return this.Protocol(Code.Conflict, "You are already a member of this thread. No need to join again.");
            }

            var newRelation = new UserThreadRelation
            {
                UserId = currentUserId,
                ThreadId = id,
                UserThreadRole = UserThreadRole.Member
            };
            relationalDbContext.UserThreadRelations.Add(newRelation);
            await relationalDbContext.SaveChangesAsync();

            // Save to cache.
            quickMessageAccess.OnUserJoinedThread(id, currentUserId);
        }
        finally
        {
            joinThreadLock.Release();
        }
        
        // Push to the client
        kahlaPushService.QueuePushEventToUser(currentUserId, PushMode.OnlyWebSocket, new YouDirectJoinedEvent
        {
            Thread = await threadService.GetThreadIJoinedAsync(id, currentUserId)
        });
        var user = await relationalDbContext.Users.FirstAsync(t => t.Id == currentUserId);
        kahlaPushService.QueuePushEventsToThread(id, PushMode.OnlyWebSocket, new SomeOneDirectJoinEvent
        {
            User = new KahlaUserMappedPublicView
            {
                Id = user.Id,
                NickName = user.NickName,
                Bio = user.Bio,
                IconFilePath = user.IconFilePath,
                AccountCreateTime = user.AccountCreateTime,
                EmailConfirmed = user.EmailConfirmed,
                Email = user.Email
            },
            ThreadId = id
        });

        logger.LogInformation("User with Id: {Id} successfully directly joined a thread. Thread ID: {ThreadID}.",
            currentUserId, id);
        return this.Protocol(Code.JobDone, "Successfully joined the thread.");
    }

    // Transfer the ownership of the thread to another member. (Only the owner can do this)
    [HttpPost]
    [Route("transfer-ownership/{id:int}")]
    public async Task<IActionResult> TransferOwnership([FromRoute] int id, [FromForm] [Required] string targetUserId)
    {
        var currentUserId = User.GetUserId();
        logger.LogInformation(
            "User with Id: {Id} is trying to transfer the ownership of the thread. Thread ID: {ThreadID}.",
            currentUserId, id);
        var thread = await relationalDbContext.ChatThreads.FindAsync(id);
        if (thread == null)
        {
            return this.Protocol(Code.NotFound, "The thread does not exist. It might have been dissolved.");
        }

        var myRelation = await relationalDbContext.UserThreadRelations
            .Where(t => t.UserId == currentUserId)
            .Where(t => t.ThreadId == id)
            .FirstOrDefaultAsync();
        if (myRelation == null)
        {
            return this.Protocol(Code.Unauthorized, "You are not a member of this thread. So you can not transfer the ownership.");
        }

        if (thread.OwnerRelationId != myRelation.Id)
        {
            return this.Protocol(Code.Unauthorized,
                "You are not the owner of this thread. Only the owner can transfer the ownership.");
        }

        var targetUser = await relationalDbContext.Users.FindAsync(targetUserId);
        if (targetUser == null)
        {
            return this.Protocol(Code.NotFound, "The target user does not exist. You can not transfer the ownership.");
        }

        var targetRelation = await relationalDbContext.UserThreadRelations
            .Where(t => t.UserId == targetUserId)
            .Where(t => t.ThreadId == id)
            .FirstOrDefaultAsync();
        if (targetRelation == null)
        {
            return this.Protocol(Code.NotFound, "The target user is not a member of this thread. So you can not transfer the ownership.");
        }

        thread.OwnerRelationId = targetRelation.Id;
        // Also set the target user as an admin
        targetRelation.UserThreadRole = UserThreadRole.Admin;
        await relationalDbContext.SaveChangesAsync();
        logger.LogInformation(
            "User with Id: {Id} successfully transferred the ownership of the thread. Thread ID: {ThreadID}.",
            currentUserId, id);
        return this.Protocol(Code.JobDone, "Successfully transferred the ownership of the thread.");
    }

    // Promote a member as an admin. (Or demote an admin to a member) (Only the owner can do this)
    [HttpPost]
    [Route("promote-admin/{id:int}")]
    public async Task<IActionResult> PromoteAdmin(
        [FromRoute] int id,
        [FromForm] [Required] string targetUserId,
        [FromForm] [Required] bool promote)
    {
        var currentUserId = User.GetUserId();
        logger.LogInformation("User with Id: {Id} is trying to promote a member as an admin. Thread ID: {ThreadID}.",
            currentUserId, id);
        var thread = await relationalDbContext.ChatThreads.FindAsync(id);
        if (thread == null)
        {
            return this.Protocol(Code.NotFound, "The thread does not exist. It might have been dissolved.");
        }

        var myRelation = await relationalDbContext.UserThreadRelations
            .Where(t => t.UserId == currentUserId)
            .Where(t => t.ThreadId == id)
            .FirstOrDefaultAsync();
        if (myRelation == null)
        {
            return this.Protocol(Code.Unauthorized, "You are not a member of this thread. So you can not promote a member.");
        }

        if (thread.OwnerRelationId != myRelation.Id)
        {
            return this.Protocol(Code.Unauthorized,
                "You are not the owner of this thread. Only the owner can promote a member as an admin.");
        }

        var targetRelation = await relationalDbContext.UserThreadRelations
            .Where(t => t.UserId == targetUserId)
            .Where(t => t.ThreadId == id)
            .FirstOrDefaultAsync();
        if (targetRelation == null)
        {
            return this.Protocol(Code.NotFound, "The target user is not a member of this thread. So you can not promote him/her.");
        }

        targetRelation.UserThreadRole = promote ? UserThreadRole.Admin : UserThreadRole.Member;
        await relationalDbContext.SaveChangesAsync();
        logger.LogInformation(
            "User with Id: {Id} successfully changed a member's role. Thread ID: {ThreadID}. User ID: {UserId}. New role: {Role}.",
            currentUserId, id, targetUserId, targetRelation.UserThreadRole);
        return this.Protocol(Code.JobDone, $"Successfully set the user's role to {targetRelation.UserThreadRole}.");
    }

    // Kick a member from the thread. (Only the admin can do this) (Can not kick the owner)
    [HttpPost]
    [Route("kick-member/{id:int}")]
    public async Task<IActionResult> KickMember([FromRoute] int id, [FromForm] [Required] string targetUserId)
    {
        var currentUserId = User.GetUserId();
        logger.LogInformation("User with Id: {Id} is trying to kick a member from the thread. Thread ID: {ThreadID}.",
            currentUserId, id);
        var thread = await relationalDbContext.ChatThreads.FindAsync(id);
        if (thread == null)
        {
            return this.Protocol(Code.NotFound, "The thread does not exist. It might have been dissolved.");
        }

        var myRelation = await relationalDbContext.UserThreadRelations
            .Where(t => t.UserId == currentUserId)
            .Where(t => t.ThreadId == id)
            .FirstOrDefaultAsync();
        if (myRelation == null)
        {
            return this.Protocol(Code.Unauthorized, "You are not a member of this thread. So you can not kick a member.");
        }

        if (myRelation.UserThreadRole != UserThreadRole.Admin)
        {
            return this.Protocol(Code.Unauthorized,
                "You are not the admin of this thread. Only the admin can kick a member.");
        }

        var targetRelation = await relationalDbContext.UserThreadRelations
            .Where(t => t.UserId == targetUserId)
            .Where(t => t.ThreadId == id)
            .FirstOrDefaultAsync();
        if (targetRelation == null)
        {
            return this.Protocol(Code.NotFound, "The target user is not a member of this thread. So you can not kick him/her.");
        }

        if (thread.OwnerRelationId == targetRelation.Id)
        {
            return this.Protocol(Code.Unauthorized, "The owner of the thread can not be kicked.");
        }

        relationalDbContext.UserThreadRelations.Remove(targetRelation);
        await relationalDbContext.SaveChangesAsync();

        // Remove the thread from the cache.
        quickMessageAccess.OnUserLeftThread(id, targetUserId);
        logger.LogInformation("User with Id: {Id} successfully kicked a member from the thread. Thread ID: {ThreadID}.",
            currentUserId, id);
        return this.Protocol(Code.JobDone, "Successfully kicked the member from the thread.");
    }

    // Ban a member from the thread. (Or unban a member) (Only the admin can do this)

    // Leave the thread. (The owner can not leave the thread)
    [HttpPost]
    [Route("leave/{id:int}")]
    public async Task<IActionResult> LeaveThread([FromRoute] int id)
    {
        var currentUserId = User.GetUserId();
        logger.LogInformation("User with Id: {Id} is trying to leave the thread. Thread ID: {ThreadID}.", currentUserId,
            id);
        var thread = await relationalDbContext.ChatThreads.FindAsync(id);
        if (thread == null)
        {
            return this.Protocol(Code.NotFound, "The thread does not exist. It might have been dissolved.");
        }

        var myRelation = await relationalDbContext.UserThreadRelations
            .Where(t => t.UserId == currentUserId)
            .Where(t => t.ThreadId == id)
            .FirstOrDefaultAsync();
        if (myRelation == null)
        {
            return this.Protocol(Code.Unauthorized, "You are not a member of this thread. So you can not leave it.");
        }

        if (thread.OwnerRelationId == myRelation.Id)
        {
            return this.Protocol(Code.Conflict,
                "You are the owner of this thread. You can not leave the thread. If you don't want to own this thread anymore, please transfer the ownership to another member first. If you want to delete the thread, please dissolve the thread.");
        }

        relationalDbContext.UserThreadRelations.Remove(myRelation);
        await relationalDbContext.SaveChangesAsync();

        // Remove the thread from the cache.
        quickMessageAccess.OnUserLeftThread(id, currentUserId);
        logger.LogInformation("User with Id: {Id} successfully left the thread. Thread ID: {ThreadID}.", currentUserId,
            id);
        return this.Protocol(Code.JobDone, "Successfully left the thread.");
    }

    // Dissolve the thread. (Only the owner can do this)
    [HttpPost]
    [Route("dissolve/{id:int}")]
    public async Task<IActionResult> DissolveThread([FromRoute] int id)
    {
        var currentUserId = User.GetUserId();
        logger.LogInformation("User with Id: {Id} is trying to dissolve the thread. Thread ID: {ThreadID}.",
            currentUserId, id);
        var thread = await relationalDbContext.ChatThreads.FindAsync(id);
        if (thread == null)
        {
            return this.Protocol(Code.NotFound, "The thread does not exist. It might have been dissolved.");
        }

        var myRelation = await relationalDbContext.UserThreadRelations
            .Where(t => t.UserId == currentUserId)
            .Where(t => t.ThreadId == id)
            .FirstOrDefaultAsync();
        if (myRelation == null)
        {
            return this.Protocol(Code.Unauthorized, "You are not a member of this thread. So you can not dissolve it.");
        }

        if (thread.OwnerRelationId != myRelation.Id)
        {
            return this.Protocol(Code.Unauthorized,
                "You are not the owner of this thread. Only the owner can dissolve the thread.");
        }

        // Remove all the relations.
        relationalDbContext.UserThreadRelations.RemoveRange(
            relationalDbContext.UserThreadRelations.Where(t => t.ThreadId == id));
        await relationalDbContext.SaveChangesAsync();

        // Remove the thread.
        relationalDbContext.ChatThreads.Remove(thread);
        await relationalDbContext.SaveChangesAsync();

        // Remove the thread from the cache.
        quickMessageAccess.OnThreadDropped(id);

        // Remove the thread from the array database.
        await arrayDbContext.DeleteThreadAsync(id);

        logger.LogInformation("User with Id: {Id} successfully dissolved the thread. Thread ID: {ThreadID}.",
            currentUserId, id);
        return this.Protocol(Code.JobDone, "Successfully dissolved the thread.");
    }

    // Set muted (Or unmute) for current user. (Any member can do this)
    [HttpPost]
    [Route("set-mute/{id:int}")]
    public async Task<IActionResult> SetMute([FromRoute] int id, [FromForm] [Required] bool mute)
    {
        var currentUserId = User.GetUserId();
        logger.LogInformation("User with Id: {Id} is trying to set mute for the thread. Thread ID: {ThreadID}.",
            currentUserId, id);
        var thread = await relationalDbContext.ChatThreads.FindAsync(id);
        if (thread == null)
        {
            return this.Protocol(Code.NotFound, "The thread does not exist.");
        }

        var myRelation = await relationalDbContext.UserThreadRelations
            .Where(t => t.UserId == currentUserId)
            .Where(t => t.ThreadId == id)
            .FirstOrDefaultAsync();
        if (myRelation == null)
        {
            return this.Protocol(Code.Unauthorized, "You are not a member of this thread.");
        }

        myRelation.Muted = mute;
        await relationalDbContext.SaveChangesAsync();
        logger.LogInformation(
            "User with Id: {Id} successfully set mute as {Mute} for the thread. Thread ID: {ThreadID}.", currentUserId,
            mute, id);
        return this.Protocol(Code.JobDone, "Successfully set mute for the thread.");
    }

    [HttpPost]
    [Route("create-scratch")]
    [Produces<CreateNewThreadViewModel>]
    public async Task<IActionResult> CreateFromScratch([FromForm] CreateThreadAddressModel model)
    {
        var currentUserId = User.GetUserId();
        logger.LogInformation("User with Id: {Id} is trying to create a new thread from scratch.", currentUserId);
        var strategy = relationalDbContext.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await relationalDbContext.Database.BeginTransactionAsync();
            try
            {
                // Step 1: Create a new thread without setting OwnerRelationId initially.
                var thread = new ChatThread
                {
                    Name = model.Name,
                    AllowSearchByName = model.AllowSearchByName,
                    AllowDirectJoinWithoutInvitation = model.AllowDirectJoinWithoutInvitation,
                    AllowMemberSoftInvitation = model.AllowMemberSoftInvitation,
                    AllowMembersSendMessages = model.AllowMembersSendMessages,
                    AllowMembersEnlistAllMembers = model.AllowMembersEnlistAllMembers
                };
                relationalDbContext.ChatThreads.Add(thread);
                await relationalDbContext.SaveChangesAsync();

                // Step 2: Add myself to the thread with role Admin
                var myRelation = new UserThreadRelation
                {
                    UserId = currentUserId,
                    ThreadId = thread.Id,
                    UserThreadRole = UserThreadRole.Admin
                };
                relationalDbContext.UserThreadRelations.Add(myRelation);
                await relationalDbContext.SaveChangesAsync();

                // Step 3: Set the owner of the thread after myRelation is saved
                thread.OwnerRelationId = myRelation.Id;
                await relationalDbContext.SaveChangesAsync();

                // Commit the transaction if everything is successful
                await transaction.CommitAsync();

                // Save to cache.
                quickMessageAccess.OnNewThreadCreated(thread.Id, thread.CreateTime);
                quickMessageAccess.OnUserJoinedThread(thread.Id, currentUserId);

                // Save in array database.
                arrayDbContext.CreateNewThread(thread.Id);

                logger.LogInformation("User with Id: {Id} successfully created a new thread from scratch.",
                    currentUserId);
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

    [HttpPost]
    [Route("hard-invite/{id}")]
    [Produces<CreateNewThreadViewModel>]
    public async Task<IActionResult> HardInvite([FromRoute] string id)
    {
        var currentUserId = User.GetUserId();
        var targetUser = await relationalDbContext.Users.FindAsync(id);
        if (targetUser == null)
        {
            return this.Protocol(Code.NotFound, $"The target user with ID {id} does not exist.");
        }

        if (!targetUser.AllowHardInvitation)
        {
            return this.Protocol(Code.Unauthorized, "The target user does not allow others creating a thread with him/her.");
        }

        var targetUserBlockedMe = await relationalDbContext.BlockRecords
            .Where(t => t.CreatorId == targetUser.Id)
            .Where(t => t.TargetId == currentUserId)
            .AnyAsync();
        if (targetUserBlockedMe)
        {
            return this.Protocol(Code.Conflict,
                "The target user has blocked you so you can not create a thread with him/her.");
        }

        var strategy = relationalDbContext.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await relationalDbContext.Database.BeginTransactionAsync();
            try
            {
                // Step 1: Create a new thread without setting OwnerRelationId initially.
                var thread = new ChatThread();
                relationalDbContext.ChatThreads.Add(thread);
                logger.LogInformation("Creating a new thread...");
                await relationalDbContext.SaveChangesAsync(); // This will generate the thread's id

                // Step 2: Add myself to the thread with role Admin
                var myRelation = new UserThreadRelation
                {
                    UserId = currentUserId,
                    ThreadId = thread.Id,
                    UserThreadRole = UserThreadRole.Admin
                };
                relationalDbContext.UserThreadRelations.Add(myRelation);
                logger.LogInformation("Adding myself (ID is {ID}) to the thread...", currentUserId);
                await relationalDbContext.SaveChangesAsync(); // This will generate myRelation's id

                // Step 3: Set the owner of the thread after myRelation is saved
                thread.OwnerRelationId = myRelation.Id;
                relationalDbContext.ChatThreads.Update(thread);
                // Don't call SaveChangesAsync here for better performance.

                if (currentUserId != targetUser.Id)
                {
                    // Step 4: Add the target user to the thread
                    var targetRelation = new UserThreadRelation
                    {
                        UserId = targetUser.Id,
                        ThreadId = thread.Id,
                        UserThreadRole = UserThreadRole.Admin
                    };
                    relationalDbContext.UserThreadRelations.Add(targetRelation);
                    // Don't call SaveChangesAsync here for better performance.
                }
                else
                {
                    logger.LogWarning(
                        "The current user and the target user are the same. Skip adding the target user to the thread. This might because of the user creating a thread with himself/herself.");
                }

                // Commit the transaction if everything is successful
                logger.LogInformation(
                    "Setting the owner of the thread to myself (ID is {ID})... And adding the target user (ID is {ID}) to the thread...",
                    currentUserId, targetUser.Id);
                await relationalDbContext.SaveChangesAsync(); // Save the targetRelation
                await transaction.CommitAsync();

                // Save to cache.
                quickMessageAccess.OnNewThreadCreated(thread.Id, thread.CreateTime);
                quickMessageAccess.OnUserJoinedThread(thread.Id, currentUserId);
                if (currentUserId != targetUser.Id)
                {
                    quickMessageAccess.OnUserJoinedThread(thread.Id, targetUser.Id);
                }

                // Save in array database.
                arrayDbContext.CreateNewThread(thread.Id);

                logger.LogInformation("A new thread has been created successfully. Thread ID is {ID}.", thread.Id);
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

    // Soft Invite, allowing users to invite new users into an existing conversation.
    //
    // * Administrators can initiate a soft invite at any time. The invited user will become a regular member.
    // * Regular users can only initiate a soft invite in conversations where members are allowed to initiate soft invites. The invited user will become a regular member.
    //
    // In this case, the server needs to issue an invitation token, which will be sent to the target user. After the target user clicks on the token, they will automatically join the conversation.
    //
    // The token is a string divided by dots, with the first part being a JSON and the second part being the digital signature of the first part.
    //
    // The JSON in the first part contains:
    //
    // * The ID of the conversation the user is being invited to
    //     * The conversation name (considering that some conversations do not allow anonymous queries)
    // * The ID of the user being invited
    //     * Expiration time
    // Admin can always invite a user. Members can only invite a user if the thread allows member soft invitation.
    [HttpPost]
    [Route("soft-invite-init/{id:int}")]
    [Produces<CreateSoftInviteTokenViewModel>]
    public async Task<IActionResult> CreateSoftInviteToken([FromRoute] int id,
        [FromForm] [Required] string invitedUserId)
    {
        var currentUserId = User.GetUserId();
        logger.LogInformation(
            "User with Id: {Id} is trying to create a soft invite token for the thread. Thread ID: {ThreadID}.",
            currentUserId, id);
        var thread = await relationalDbContext.ChatThreads.FindAsync(id);
        if (thread == null)
        {
            return this.Protocol(Code.NotFound, "The thread does not exist. It might be deleted.");
        }

        var myRelation = await relationalDbContext.UserThreadRelations
            .Where(t => t.UserId == currentUserId)
            .Where(t => t.ThreadId == id)
            .FirstOrDefaultAsync();
        if (myRelation == null)
        {
            return this.Protocol(Code.Unauthorized, "You are not a member of this thread and can not invite a user.");
        }

        if (myRelation.UserThreadRole != UserThreadRole.Admin && !thread.AllowMemberSoftInvitation)
        {
            return this.Protocol(Code.Unauthorized,
                "You are not the admin of this thread. In this thread, only the admin can invite a user.");
        }

        var tokenObject = new SoftInviteToken
        {
            ThreadId = id,
            InviterId = currentUserId,
            InvitedUserId = invitedUserId,
            ExpireTime = DateTime.UtcNow.AddDays(5)
        };
        var tokenRaw = tokenObject.SerializeObject().StringToBase64();
        var encryptedToken = dataProtectionProvider.CreateProtector("SoftInvite").Protect(tokenRaw);
        var tokenFinal = $"{tokenRaw}.{encryptedToken}";
        logger.LogInformation(
            "User with Id: {Id} successfully created a soft invite token for the thread. Thread ID: {ThreadID}.",
            currentUserId, id);
        return this.Protocol(new CreateSoftInviteTokenViewModel
        {
            Code = Code.JobDone,
            Message = "Successfully created the soft invite token.",
            Token = tokenFinal
        });
    }

    // Complete the soft invite.
    // This will add the current user to the thread.
    [HttpPost]
    [Route("soft-invite-complete")]
    public async Task<IActionResult> CompleteSoftInvite([FromForm] string token)
    {
        var currentUserId = User.GetUserId();
        logger.LogInformation("User with Id: {Id} is trying to complete a soft invite.", currentUserId);
        var tokenParts = token.Split('.');
        if (tokenParts.Length != 2)
        {
            return this.Protocol(Code.Unauthorized,
                "Invalid token format. Valid token should be in the format of 'rawToken.encryptedToken'.");
        }

        var decryptedToken = dataProtectionProvider.CreateProtector("SoftInvite").Unprotect(tokenParts[1])
            .Base64ToString();
        var rawToken = tokenParts[0].Base64ToString();
        if (decryptedToken != rawToken)
        {
            return this.Protocol(Code.Unauthorized, "Invalid token! The token is tampered.");
        }

        var tokenObject = SoftInviteToken.DeserializeObject(rawToken);
        var threadId = tokenObject.ThreadId;
        var thread = await relationalDbContext.ChatThreads.FindAsync(threadId);
        if (thread == null)
        {
            return this.Protocol(Code.NotFound, "The thread does not exist. It might be deleted.");
        }

        if (tokenObject.InvitedUserId != currentUserId)
        {
            return this.Protocol(Code.Unauthorized, "You are not the invited user.");
        }

        if (tokenObject.ExpireTime < DateTime.UtcNow)
        {
            return this.Protocol(Code.Unauthorized, "The invitation was expired.");
        }

        logger.LogInformation("User with Id: {Id} passed a valid soft invite token. Thread ID: {ThreadID}.",
            currentUserId, threadId);

        var joinThreadLock = locksInMemoryDb.GetJoinThreadOperationLock(userId: currentUserId, threadId: threadId);
        await joinThreadLock.WaitAsync();
        try
        {
            // Ensure the user is not already a member of the thread.
            var myRelation = await relationalDbContext.UserThreadRelations
                .Where(t => t.UserId == currentUserId)
                .Where(t => t.ThreadId == threadId)
                .FirstOrDefaultAsync();
            if (myRelation != null)
            {
                return this.Protocol(Code.Conflict, "You are already a member of this thread.");
            }

            // Add the user to the thread.
            var newRelation = new UserThreadRelation
            {
                UserId = currentUserId,
                ThreadId = threadId,
                UserThreadRole = UserThreadRole.Member
            };
            relationalDbContext.UserThreadRelations.Add(newRelation);
            await relationalDbContext.SaveChangesAsync();

            // Save to cache.
            quickMessageAccess.OnUserJoinedThread(threadId, currentUserId);
        }
        finally
        {
            joinThreadLock.Release();
        }

        logger.LogInformation("User with Id: {Id} successfully completed the soft invite. Thread ID: {ThreadID}.",
            currentUserId, threadId);
        return this.Protocol(Code.JobDone, "Successfully joined the thread.");
    }
}