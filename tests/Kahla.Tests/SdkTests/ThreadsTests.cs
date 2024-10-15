// using System.ComponentModel.DataAnnotations;
// using Aiursoft.AiurProtocol.Models;
// using Aiursoft.AiurProtocol.Server;
// using Aiursoft.AiurProtocol.Server.Attributes;
// using Aiursoft.DocGenerator.Attributes;
// using Aiursoft.Kahla.SDK.Models;
// using Aiursoft.Kahla.SDK.Models.AddressModels;
// using Aiursoft.Kahla.SDK.Models.Entities;
// using Aiursoft.Kahla.SDK.Models.ViewModels;
// using Aiursoft.Kahla.Server.Attributes;
// using Aiursoft.Kahla.Server.Data;
// using Aiursoft.Kahla.Server.Services.AppService;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;
//
// namespace Aiursoft.Kahla.Server.Controllers;
//
// [KahlaForceAuth]
// [GenerateDoc]
// [ApiExceptionHandler(
//     PassthroughRemoteErrors = true,
//     PassthroughAiurServerException = true)]
// [ApiModelStateChecker]
// [Route("api/threads")]
// public class ThreadsController(
//     ILogger<ThreadsController> logger,
//     ThreadJoinedViewAppService threadService,
//     UserInThreadViewAppService userAppService,
//     KahlaDbContext dbContext) : ControllerBase
// {
//     [HttpGet]
//     [Route("list")]
//     [Produces<MyThreadsViewModel>]
//     public async Task<IActionResult> List([FromQuery]SearchAddressModel model)
//     {
//         var currentUserId = User.GetUserId();
//         logger.LogInformation("User with Id: {Id} is trying to search his threads with keyword: {Search}.", currentUserId, model.SearchInput);
//         var (count, threads) = await threadService.SearchThreadsIJoinedAsync(model.SearchInput, model.Excluding, currentUserId, model.Skip, model.Take);
//         logger.LogInformation("User with Id: {Id} successfully searched his threads with keyword: {Search} with total {Count}.", currentUserId, model.SearchInput, threads.Count);
//         return this.Protocol(new MyThreadsViewModel
//         {
//             Code = Code.ResultShown,
//             Message = $"Successfully get your first {model.Take} threads from search result and skipped {model.Skip} threads.",
//             KnownThreads = threads,
//             TotalCount = count
//         });
//     }
//     
//     [HttpGet]
//     [Route("members/{id:int}")]
//     [Produces<ThreadMembersViewModel>]
//     public async Task<IActionResult> Members([FromRoute]int id, [FromQuery]int skip = 0, [FromQuery]int take = 20)
//     {
//         var currentUserId = User.GetUserId();
//         logger.LogInformation("User with Id: {Id} is trying to get the members of the thread. Thread ID: {ThreadID}.", currentUserId, id);
//         var myRelation = await dbContext.UserThreadRelations
//             .Where(t => t.UserId == currentUserId)
//             .Where(t => t.ThreadId == id)
//             .Include(t => t.Thread)
//             .FirstOrDefaultAsync();
//         if (myRelation == null)
//         {
//             return this.Protocol(Code.Unauthorized, "You are not a member of this thread.");
//         }
//         if (myRelation.Thread.AllowMembersEnlistAllMembers == false && myRelation.UserThreadRole != UserThreadRole.Admin)
//         {
//             return this.Protocol(Code.Unauthorized, "This thread does not allow members to enlist members.");
//         }
//         var (count, members) = await userAppService.QueryMembersInThreadAsync(id, currentUserId, skip, take);
//         logger.LogInformation("User with Id: {Id} successfully get the members of the thread. Thread ID: {ThreadID}. Total members: {Count}.", currentUserId, id, members.Count);
//         return this.Protocol(new ThreadMembersViewModel
//         {
//             Code = Code.ResultShown,
//             Message = $"Successfully get the first {take} members of the thread and skipped {skip} members.",
//             Members = members,
//             TotalCount = count
//         });
//     }
//
//     [HttpGet]
//     [Route("details/{id:int}")]
//     [Produces<ThreadDetailsViewModel>]
//     public async Task<IActionResult> Details([FromRoute] int id)
//     {
//         var currentUserId = User.GetUserId();
//         logger.LogInformation("User with Id: {Id} is trying to get the thread details. Thread ID: {ThreadID}.", currentUserId, id);
//         var myRelation = await dbContext.UserThreadRelations
//             .Where(t => t.UserId == currentUserId)
//             .Where(t => t.ThreadId == id)
//             .Include(t => t.Thread)
//             .FirstOrDefaultAsync();
//         if (myRelation == null)
//         {
//             return this.Protocol(Code.Unauthorized, "You are not a member of this thread.");
//         }
//         var thread = await threadService.GetThreadAsync(id, currentUserId);
//         if (thread == null)
//         {
//             return this.Protocol(Code.NotFound, "The thread does not exist.");
//         }
//         logger.LogInformation("User with Id: {Id} successfully get the thread details. Thread ID: {ThreadID}.", currentUserId, id);
//         return this.Protocol(new ThreadDetailsViewModel
//         {
//             Code = Code.ResultShown,
//             Message = "Successfully get the thread details.",
//             Thread = thread,
//         });
//     }
//     
//     [HttpPatch]
//     [Route("update-thread/{id:int}")]
//     public async Task<IActionResult> UpdateThread([FromRoute]int id, [FromForm]UpdateThreadAddressModel model)
//     {
//         var currentUserId = User.GetUserId();
//         logger.LogInformation("User with Id: {Id} is trying to update the thread's properties. Thread ID: {ThreadID}.", currentUserId, id);
//         var thread = await dbContext.ChatThreads.FindAsync(id);
//         if (thread == null)
//         {
//             return this.Protocol(Code.NotFound, "The thread does not exist.");
//         }
//         var myRelation = await dbContext.UserThreadRelations
//             .Where(t => t.UserId == currentUserId)
//             .Where(t => t.ThreadId == id)
//             .Include(t => t.Thread)
//             .FirstOrDefaultAsync();
//         if (myRelation == null)
//         {
//             return this.Protocol(Code.Unauthorized, "You are not a member of this thread.");
//         }
//         if (myRelation.UserThreadRole != UserThreadRole.Admin)
//         {
//             return this.Protocol(Code.Unauthorized, "You are not the admin of this thread. Only the admin can update the thread's properties.");
//         }
//         var updatedProperties = new List<string>();
//         if (model.Name != null)
//         {
//             thread.Name = model.Name;
//             updatedProperties.Add(nameof(thread.Name));
//         }
//         if (model.IconFilePath != null)
//         {
//             thread.IconFilePath = model.IconFilePath;
//             updatedProperties.Add(nameof(thread.IconFilePath));
//         }
//         if (model.AllowDirectJoinWithoutInvitation.HasValue)
//         {
//             thread.AllowDirectJoinWithoutInvitation = model.AllowDirectJoinWithoutInvitation == true;
//             updatedProperties.Add(nameof(thread.AllowDirectJoinWithoutInvitation));
//         }
//         if (model.AllowMemberSoftInvitation.HasValue)
//         {
//             thread.AllowMemberSoftInvitation = model.AllowMemberSoftInvitation == true;
//             updatedProperties.Add(nameof(thread.AllowMemberSoftInvitation));
//         }
//         if (model.AllowMembersSendMessages.HasValue)
//         {
//             thread.AllowMembersSendMessages = model.AllowMembersSendMessages == true;
//             updatedProperties.Add(nameof(thread.AllowMembersSendMessages));
//         }
//         if (model.AllowMembersEnlistAllMembers.HasValue)
//         {
//             thread.AllowMembersEnlistAllMembers = model.AllowMembersEnlistAllMembers == true;
//             updatedProperties.Add(nameof(thread.AllowMembersEnlistAllMembers));
//         }
//         if (model.AllowSearchByName.HasValue)
//         {
//             thread.AllowSearchByName = model.AllowSearchByName == true;
//             updatedProperties.Add(nameof(thread.AllowSearchByName));
//         }
//         await dbContext.SaveChangesAsync();
//         var updatedPropertiesName = string.Join(", ", updatedProperties); 
//         logger.LogInformation("User with Id: {Id} updated the thread's properties: {Properties}.", currentUserId, updatedPropertiesName);
//         return this.Protocol(Code.JobDone, $"Successfully updated the thread's properties: {updatedPropertiesName}.");
//     }
//
//     // Directly join a thread without an invitation. (Only when AllowDirectJoinWithoutInvitation is true)
//     [HttpPost]
//     [Route("direct-join/{id:int}")]
//     public async Task<IActionResult> DirectJoin([FromRoute]int id)
//     {
//         var currentUserId = User.GetUserId();
//         logger.LogInformation("User with Id: {Id} is trying to directly join a thread. Thread ID: {ThreadID}.", currentUserId, id);
//         var thread = await dbContext.ChatThreads.FindAsync(id);
//         if (thread == null)
//         {
//             return this.Protocol(Code.NotFound, "The thread does not exist.");
//         }
//         if (!thread.AllowDirectJoinWithoutInvitation)
//         {
//             return this.Protocol(Code.Unauthorized, "This thread does not allow direct join without an invitation.");
//         }
//         var myRelation = await dbContext.UserThreadRelations
//             .Where(t => t.UserId == currentUserId)
//             .Where(t => t.ThreadId == id)
//             .FirstOrDefaultAsync();
//         if (myRelation != null)
//         {
//             return this.Protocol(Code.Conflict, "You are already a member of this thread.");
//         }
//         var newRelation = new UserThreadRelation
//         {
//             UserId = currentUserId,
//             ThreadId = id,
//             UserThreadRole = UserThreadRole.Member
//         };
//         dbContext.UserThreadRelations.Add(newRelation);
//         await dbContext.SaveChangesAsync();
//         logger.LogInformation("User with Id: {Id} successfully directly joined a thread. Thread ID: {ThreadID}.", currentUserId, id);
//         return this.Protocol(Code.JobDone, "Successfully joined the thread.");
//     }
//
//     // Transfer the ownership of the thread to another member. (Only the owner can do this)
//     [HttpPost]
//     [Route("transfer-ownership/{id:int}")]
//     public async Task<IActionResult> TransferOwnership([FromRoute]int id, [FromForm][Required]string targetUserId)
//     {
//         var currentUserId = User.GetUserId();
//         logger.LogInformation("User with Id: {Id} is trying to transfer the ownership of the thread. Thread ID: {ThreadID}.", currentUserId, id);
//         var thread = await dbContext.ChatThreads.FindAsync(id);
//         if (thread == null)
//         {
//             return this.Protocol(Code.NotFound, "The thread does not exist.");
//         }
//         var myRelation = await dbContext.UserThreadRelations
//             .Where(t => t.UserId == currentUserId)
//             .Where(t => t.ThreadId == id)
//             .Include(t => t.Thread)
//             .FirstOrDefaultAsync();
//         if (myRelation == null)
//         {
//             return this.Protocol(Code.Unauthorized, "You are not a member of this thread.");
//         }
//         if (thread.OwnerRelationId != myRelation.Id)
//         {
//             return this.Protocol(Code.Unauthorized, "You are not the owner of this thread. Only the owner can transfer the ownership.");
//         }
//         var targetUser = await dbContext.Users.FindAsync(targetUserId);
//         if (targetUser == null)
//         {
//             return this.Protocol(Code.NotFound, "The target user does not exist.");
//         }
//         var targetRelation = await dbContext.UserThreadRelations
//             .Where(t => t.UserId == targetUserId)
//             .Where(t => t.ThreadId == id)
//             .FirstOrDefaultAsync();
//         if (targetRelation == null)
//         {
//             return this.Protocol(Code.NotFound, "The target user is not a member of this thread.");
//         }
//         thread.OwnerRelationId = targetRelation.Id;
//         // Also set the target user as an admin
//         targetRelation.UserThreadRole = UserThreadRole.Admin;
//         await dbContext.SaveChangesAsync();
//         logger.LogInformation("User with Id: {Id} successfully transferred the ownership of the thread. Thread ID: {ThreadID}.", currentUserId, id);
//         return this.Protocol(Code.JobDone, "Successfully transferred the ownership of the thread.");
//     }
//
//     // Promote a member as an admin. (Or demote an admin to a member) (Only the owner can do this)
//     [HttpPost]
//     [Route("promote-admin/{id:int}")]
//     public async Task<IActionResult> PromoteAdmin(
//         [FromRoute]int id, 
//         [FromForm][Required]string targetUserId,
//         [FromForm][Required]bool promote)
//     {
//         var currentUserId = User.GetUserId();
//         logger.LogInformation("User with Id: {Id} is trying to promote a member as an admin. Thread ID: {ThreadID}.", currentUserId, id);
//         var thread = await dbContext.ChatThreads.FindAsync(id);
//         if (thread == null)
//         {
//             return this.Protocol(Code.NotFound, "The thread does not exist.");
//         }
//         var myRelation = await dbContext.UserThreadRelations
//             .Where(t => t.UserId == currentUserId)
//             .Where(t => t.ThreadId == id)
//             .Include(t => t.Thread)
//             .FirstOrDefaultAsync();
//         if (myRelation == null)
//         {
//             return this.Protocol(Code.Unauthorized, "You are not a member of this thread.");
//         }
//         if (thread.OwnerRelationId != myRelation.Id)
//         {
//             return this.Protocol(Code.Unauthorized, "You are not the owner of this thread. Only the owner can promote a member as an admin.");
//         }
//         var targetRelation = await dbContext.UserThreadRelations
//             .Where(t => t.UserId == targetUserId)
//             .Where(t => t.ThreadId == id)
//             .FirstOrDefaultAsync();
//         if (targetRelation == null)
//         {
//             return this.Protocol(Code.NotFound, "The target user is not a member of this thread.");
//         }
//         targetRelation.UserThreadRole = promote ? UserThreadRole.Admin : UserThreadRole.Member;
//         await dbContext.SaveChangesAsync();
//         logger.LogInformation("User with Id: {Id} successfully changed a member's role. Thread ID: {ThreadID}. User ID: {UserId}. New role: {Role}.", currentUserId, id, targetUserId, targetRelation.UserThreadRole);
//         return this.Protocol(Code.JobDone, $"Successfully set the user's role to {targetRelation.UserThreadRole}.");
//     }
//
//     // Kick a member from the thread. (Only the admin can do this) (Can not kick the owner)
//     [HttpPost]
//     [Route("kick-member/{id:int}")]
//     public async Task<IActionResult> KickMember([FromRoute]int id, [FromForm][Required]string targetUserId)
//     {
//         var currentUserId = User.GetUserId();
//         logger.LogInformation("User with Id: {Id} is trying to kick a member from the thread. Thread ID: {ThreadID}.", currentUserId, id);
//         var thread = await dbContext.ChatThreads.FindAsync(id);
//         if (thread == null)
//         {
//             return this.Protocol(Code.NotFound, "The thread does not exist.");
//         }
//         var myRelation = await dbContext.UserThreadRelations
//             .Where(t => t.UserId == currentUserId)
//             .Where(t => t.ThreadId == id)
//             .Include(t => t.Thread)
//             .FirstOrDefaultAsync();
//         if (myRelation == null)
//         {
//             return this.Protocol(Code.Unauthorized, "You are not a member of this thread.");
//         }
//         if (myRelation.UserThreadRole != UserThreadRole.Admin)
//         {
//             return this.Protocol(Code.Unauthorized, "You are not the admin of this thread. Only the admin can kick a member.");
//         }
//         var targetRelation = await dbContext.UserThreadRelations
//             .Where(t => t.UserId == targetUserId)
//             .Where(t => t.ThreadId == id)
//             .FirstOrDefaultAsync();
//         if (targetRelation == null)
//         {
//             return this.Protocol(Code.NotFound, "The target user is not a member of this thread.");
//         }
//         if (thread.OwnerRelationId == targetRelation.Id)
//         {
//             return this.Protocol(Code.Unauthorized, "The owner of the thread can not be kicked.");
//         }
//         dbContext.UserThreadRelations.Remove(targetRelation);
//         await dbContext.SaveChangesAsync();
//         logger.LogInformation("User with Id: {Id} successfully kicked a member from the thread. Thread ID: {ThreadID}.", currentUserId, id);
//         return this.Protocol(Code.JobDone, "Successfully kicked the member from the thread.");
//     }
//
//     // Ban a member from the thread. (Or unban a member) (Only the admin can do this)
//
//     // Leave the thread. (The owner can not leave the thread)
//     [HttpPost]
//     [Route("leave/{id:int}")]
//     public async Task<IActionResult> LeaveThread([FromRoute]int id)
//     {
//         var currentUserId = User.GetUserId();
//         logger.LogInformation("User with Id: {Id} is trying to leave the thread. Thread ID: {ThreadID}.", currentUserId, id);
//         var thread = await dbContext.ChatThreads.FindAsync(id);
//         if (thread == null)
//         {
//             return this.Protocol(Code.NotFound, "The thread does not exist.");
//         }
//         var myRelation = await dbContext.UserThreadRelations
//             .Where(t => t.UserId == currentUserId)
//             .Where(t => t.ThreadId == id)
//             .Include(t => t.Thread)
//             .FirstOrDefaultAsync();
//         if (myRelation == null)
//         {
//             return this.Protocol(Code.Unauthorized, "You are not a member of this thread.");
//         }
//         if (thread.OwnerRelationId == myRelation.Id)
//         {
//             return this.Protocol(Code.Conflict, "You are the owner of this thread. You can not leave the thread. If you don't want to own this thread anymore, please transfer the ownership to another member first. If you want to delete the thread, please dissolve the thread.");
//         }
//         dbContext.UserThreadRelations.Remove(myRelation);
//         await dbContext.SaveChangesAsync();
//         logger.LogInformation("User with Id: {Id} successfully left the thread. Thread ID: {ThreadID}.", currentUserId, id);
//         return this.Protocol(Code.JobDone, "Successfully left the thread.");
//     }
//
//     // Dissolve the thread. (Only the owner can do this)
//     [HttpPost]
//     [Route("dissolve/{id:int}")]
//     public async Task<IActionResult> DissolveThread([FromRoute]int id)
//     {
//         var currentUserId = User.GetUserId();
//         logger.LogInformation("User with Id: {Id} is trying to dissolve the thread. Thread ID: {ThreadID}.", currentUserId, id);
//         var thread = await dbContext.ChatThreads.FindAsync(id);
//         if (thread == null)
//         {
//             return this.Protocol(Code.NotFound, "The thread does not exist.");
//         }
//         var myRelation = await dbContext.UserThreadRelations
//             .Where(t => t.UserId == currentUserId)
//             .Where(t => t.ThreadId == id)
//             .Include(t => t.Thread)
//             .FirstOrDefaultAsync();
//         if (myRelation == null)
//         {
//             return this.Protocol(Code.Unauthorized, "You are not a member of this thread.");
//         }
//         if (thread.OwnerRelationId != myRelation.Id)
//         {
//             return this.Protocol(Code.Unauthorized, "You are not the owner of this thread. Only the owner can dissolve the thread.");
//         }
//         
//         // Remove all the relations.
//         dbContext.UserThreadRelations.RemoveRange(dbContext.UserThreadRelations.Where(t => t.ThreadId == id));
//         await dbContext.SaveChangesAsync();
//         
//         // Remove the thread.
//         dbContext.ChatThreads.Remove(thread);
//         await dbContext.SaveChangesAsync();
//         logger.LogInformation("User with Id: {Id} successfully dissolved the thread. Thread ID: {ThreadID}.", currentUserId, id);
//         return this.Protocol(Code.JobDone, "Successfully dissolved the thread.");
//     }
//     
//     // Set muted (Or unmuted) for current user. (Any member can do this)
//     [HttpPost]
//     [Route("set-mute/{id:int}")]
//     public async Task<IActionResult> SetMute([FromRoute]int id, [FromForm][Required]bool mute)
//     {
//         var currentUserId = User.GetUserId();
//         logger.LogInformation("User with Id: {Id} is trying to set mute for the thread. Thread ID: {ThreadID}.", currentUserId, id);
//         var thread = await dbContext.ChatThreads.FindAsync(id);
//         if (thread == null)
//         {
//             return this.Protocol(Code.NotFound, "The thread does not exist.");
//         }
//         var myRelation = await dbContext.UserThreadRelations
//             .Where(t => t.UserId == currentUserId)
//             .Where(t => t.ThreadId == id)
//             .Include(t => t.Thread)
//             .FirstOrDefaultAsync();
//         if (myRelation == null)
//         {
//             return this.Protocol(Code.Unauthorized, "You are not a member of this thread.");
//         }
//         myRelation.Muted = mute;
//         await dbContext.SaveChangesAsync();
//         logger.LogInformation("User with Id: {Id} successfully set mute as {Mute} for the thread. Thread ID: {ThreadID}.", currentUserId, mute, id);
//         return this.Protocol(Code.JobDone, "Successfully set mute for the thread.");
//     }
//
//     [HttpPost]
//     [Route("create-scratch")]
//     public async Task<IActionResult> CreateFromScratch([FromForm]CreateThreadAddressModel model)
//     {
//         var currentUserId = User.GetUserId();
//         logger.LogInformation("User with Id: {Id} is trying to create a new thread from scratch.", currentUserId);
//         var strategy = dbContext.Database.CreateExecutionStrategy();
//         return await strategy.ExecuteAsync(async () =>
//         {
//             await using var transaction = await dbContext.Database.BeginTransactionAsync();
//             try
//             {
//                 // Step 1: Create a new thread without setting OwnerRelationId initially.
//                 var thread = new ChatThread
//                 {
//                     Name = model.Name,
//                     AllowSearchByName = model.AllowSearchByName,
//                     AllowDirectJoinWithoutInvitation = model.AllowDirectJoinWithoutInvitation,
//                     AllowMemberSoftInvitation = model.AllowMemberSoftInvitation,
//                     AllowMembersSendMessages = model.AllowMembersSendMessages,
//                     AllowMembersEnlistAllMembers = model.AllowMembersEnlistAllMembers
//                 };
//                 dbContext.ChatThreads.Add(thread);
//                 await dbContext.SaveChangesAsync();
//                 
//                 // Step 2: Add myself to the thread with role Admin
//                 var myRelation = new UserThreadRelation
//                 {
//                     UserId = currentUserId,
//                     ThreadId = thread.Id,
//                     UserThreadRole = UserThreadRole.Admin
//                 };
//                 dbContext.UserThreadRelations.Add(myRelation);
//                 await dbContext.SaveChangesAsync();
//                 
//                 // Step 3: Set the owner of the thread after myRelation is saved
//                 thread.OwnerRelationId = myRelation.Id;
//                 await dbContext.SaveChangesAsync();
//                 
//                 // Commit the transaction if everything is successful
//                 await transaction.CommitAsync();
//                 
//                 logger.LogInformation("User with Id: {Id} successfully created a new thread from scratch.", currentUserId);
//                 return this.Protocol(new CreateNewThreadViewModel
//                 {
//                     NewThreadId = thread.Id,
//                     Code = Code.JobDone,
//                     Message = "The thread has been created successfully."
//                 });
//             }
//             catch (Exception e)
//             {
//                 logger.LogError(e, "Failed to create a thread.");
//                 await transaction.RollbackAsync();
//                 return this.Protocol(Code.UnknownError,
//                     "Failed to create the thread. Might because of a database error.");
//             }
//         });
//     }
//         
//     [HttpPost]
//     [Route("hard-invite/{id}")]
//     [Produces<CreateNewThreadViewModel>]
//     public async Task<IActionResult> HardInvite([FromRoute]string id)
//     {
//         var currentUserId = User.GetUserId();
//         var targetUser = await dbContext.Users.FindAsync(id);
//         if (targetUser == null)
//         {
//             return this.Protocol(Code.NotFound, $"The target user with ID {id} does not exist.");
//         }
//         if (!targetUser.AllowHardInvitation)
//         {
//             return this.Protocol(Code.Unauthorized, "The target user does not allow hard invitation.");
//         }
//         var targetUserBlockedMe = await dbContext.BlockRecords
//             .Where(t => t.CreatorId == targetUser.Id)
//             .Where(t => t.TargetId == currentUserId)
//             .AnyAsync();
//         if (targetUserBlockedMe)
//         {
//             return this.Protocol(Code.Conflict, "The target user has blocked you so you can not create a thread with him/her.");
//         }
//         
//         var strategy = dbContext.Database.CreateExecutionStrategy();
//         return await strategy.ExecuteAsync(async () =>
//         {
//             await using var transaction = await dbContext.Database.BeginTransactionAsync();
//             try
//             {
//                 // Step 1: Create a new thread without setting OwnerRelationId initially.
//                 var thread = new ChatThread();
//                 dbContext.ChatThreads.Add(thread);
//                 logger.LogInformation("Creating a new thread...");
//                 await dbContext.SaveChangesAsync(); // This will generate the thread's id
//
//                 // Step 2: Add myself to the thread with role Admin
//                 var myRelation = new UserThreadRelation
//                 {
//                     UserId = currentUserId,
//                     ThreadId = thread.Id,
//                     UserThreadRole = UserThreadRole.Admin
//                 };
//                 dbContext.UserThreadRelations.Add(myRelation);
//                 logger.LogInformation("Adding myself (ID is {ID}) to the thread...", currentUserId);
//                 await dbContext.SaveChangesAsync(); // This will generate myRelation's id
//
//                 // Step 3: Set the owner of the thread after myRelation is saved
//                 thread.OwnerRelationId = myRelation.Id;
//                 dbContext.ChatThreads.Update(thread);
//                 // Don't call SaveChangesAsync here for better performance.
//
//                 if (currentUserId != targetUser.Id)
//                 {
//                     // Step 4: Add the target user to the thread
//                     var targetRelation = new UserThreadRelation
//                     {
//                         UserId = targetUser.Id,
//                         ThreadId = thread.Id,
//                         UserThreadRole = UserThreadRole.Member
//                     };
//                     dbContext.UserThreadRelations.Add(targetRelation);
//                     // Don't call SaveChangesAsync here for better performance.
//                 }
//                 else
//                 {
//                     logger.LogWarning(
//                         "The current user and the target user are the same. Skip adding the target user to the thread. This might because of the user creating a thread with himself/herself.");
//                 }
//
//                 // Commit the transaction if everything is successful
//                 logger.LogInformation("Setting the owner of the thread to myself (ID is {ID})... And adding the target user (ID is {ID}) to the thread...", currentUserId, targetUser.Id);
//                 await dbContext.SaveChangesAsync(); // Save the targetRelation
//                 
//                 await transaction.CommitAsync();
//                 logger.LogInformation("A new thread has been created successfully. Thread ID is {ID}.", thread.Id);
//                 return this.Protocol(new CreateNewThreadViewModel
//                 {
//                     NewThreadId = thread.Id,
//                     Code = Code.JobDone,
//                     Message = "The thread has been created successfully."
//                 });
//             }
//             catch (Exception e)
//             {
//                 logger.LogError(e, "Failed to create a thread.");
//                 await transaction.RollbackAsync();
//                 return this.Protocol(Code.UnknownError,
//                     "Failed to create the thread. Might because of a database error.");
//             }
//         });
//     }
// }

using Aiursoft.AiurProtocol.Exceptions;
using Aiursoft.AiurProtocol.Models;
using Aiursoft.Kahla.Tests.TestBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aiursoft.Kahla.Tests.SdkTests;

[TestClass]
public class ThreadsTests : KahlaTestBase
{
    [TestMethod]
    public async Task CreateAndListTest()
    {
        // Register
        await Sdk.RegisterAsync("user20@domain.com", "password");
        await Sdk.CreateFromScratchAsync(
            "TestThread", 
            false, 
            false,
            false,
            false,
            false);
        
        // Search
        var searchResult = await Sdk.ListThreadsAsync("Test");
        Assert.AreEqual(Code.ResultShown, searchResult.Code);
        Assert.AreEqual(1, searchResult.KnownThreads.Count);
        
        // Search Non-Existed
        var searchResult2 = await Sdk.ListThreadsAsync("Non-Existed");
        Assert.AreEqual(Code.ResultShown, searchResult2.Code);
        Assert.AreEqual(0, searchResult2.KnownThreads.Count);
        
        // Search Non-Existed
        var searchResult3 = await Sdk.ListThreadsAsync("Test", excluding: "est");
        Assert.AreEqual(Code.ResultShown, searchResult3.Code);
        Assert.AreEqual(0, searchResult3.KnownThreads.Count);
    }

    [TestMethod]
    public async Task ListNewThreadOnlyMeAsMember()
    {
        await Sdk.RegisterAsync("user23@domain.com", "password");
        var thread = await Sdk.CreateFromScratchAsync(
            "TestThread2", 
            false, 
            false,
            false,
            false,
            false);
        
        // Members
        var members = await Sdk.ThreadMembersAsync(thread.NewThreadId);
        
        // Assert
        Assert.AreEqual(Code.ResultShown, members.Code);
        Assert.AreEqual(1, members.Members.Count);
    }
    
    [TestMethod]
    public async Task HardInviteOnlyWeTwoAsMembers()
    {
        await Sdk.RegisterAsync("user24@domain.com", "password");
        var user24Id = (await Sdk.MeAsync()).User.Id;
        await Sdk.SignoutAsync();
        await Sdk.RegisterAsync("user25@domain.com", "password");
        var myId = (await Sdk.MeAsync()).User.Id;
        var thread = await Sdk.HardInviteAsync(user24Id);
        
        // Members
        var members = await Sdk.ThreadMembersAsync(thread.NewThreadId);
        
        // Assert
        Assert.AreEqual(Code.ResultShown, members.Code);
        Assert.AreEqual(2, members.Members.Count);
        Assert.IsTrue(members.Members.Any(t => t.User.Id == user24Id));
        Assert.IsTrue(members.Members.Any(t => t.User.Id == myId));
    }
    
    [TestMethod]
    public async Task HardInviteNotExists()
    {
        await Sdk.RegisterAsync("user26@domain.com", "password");
        try
        {
            await Sdk.HardInviteAsync("not-exists");
            Assert.Fail();
        }
        catch (AiurUnexpectedServerResponseException e)
        {
            Assert.AreEqual(Code.NotFound, e.Response.Code);
        }
    }
    
    [TestMethod]
    public async Task HardInvitePrivateAccount()
    {
        await Sdk.RegisterAsync("user27@domain.com", "password");
        await Sdk.UpdateMeAsync(allowHardInvitation: false);
        var user27Id = (await Sdk.MeAsync()).User.Id;
        await Sdk.SignoutAsync();
        
        await Sdk.RegisterAsync("user28@domain.com", "password");
        try
        {
            await Sdk.HardInviteAsync(user27Id);
            Assert.Fail();
        }
        catch (AiurUnexpectedServerResponseException e)
        {
            Assert.AreEqual(Code.Unauthorized, e.Response.Code);
        }
    }
    
    [TestMethod]
    public async Task HardInviteBlockedAccount()
    {
        // Register user 28
        await Sdk.RegisterAsync("user28@domain.com", "password");
        var user28Id = (await Sdk.MeAsync()).User.Id;
        await Sdk.SignoutAsync();
        
        // Register user 29. Block user 28
        await Sdk.RegisterAsync("user29@domain.com", "password");
        await Sdk.BlockNewAsync(user28Id);
        var user29Id = (await Sdk.MeAsync()).User.Id;
        await Sdk.SignoutAsync();
        
        // User 28 hard invite user 29
        await Sdk.SignInAsync("user28@domain.com", "password");
        try
        {
            await Sdk.HardInviteAsync(user29Id);
            Assert.Fail();
        }
        catch (AiurUnexpectedServerResponseException e)
        {
            Assert.AreEqual(Code.Conflict, e.Response.Code);
        }
    }

    [TestMethod]
    public async Task ListMembersNotAllowed()
    {
        await Sdk.RegisterAsync("user30@domain.com", "password");
        var myId = (await Sdk.MeAsync()).User.Id;
        var thread = await Sdk.CreateFromScratchAsync(
            "TestThread2", 
            false, 
            false,
            false,
            false,
            false);
        
        // Members
        var members = await Sdk.ThreadMembersAsync(thread.NewThreadId);
        
        // I can enlist members because I'm the admin
        Assert.AreEqual(Code.ResultShown, members.Code);
        Assert.AreEqual(1, members.Members.Count);
        
        // Set me as not admin
        await Sdk.PromoteAdminAsync(thread.NewThreadId, myId, false);
        
        // I can not list members
        try
        {
            await Sdk.ThreadMembersAsync(thread.NewThreadId);
            Assert.Fail();
        }
        catch (AiurUnexpectedServerResponseException e)
        {
            Assert.AreEqual(Code.Unauthorized, e.Response.Code);
        }

        // Give me admin, allow members enlist all members, then remove me as admin
        await Sdk.PromoteAdminAsync(thread.NewThreadId, myId, true);
        await Sdk.UpdateThreadAsync(thread.NewThreadId, allowMembersEnlistAllMembers: true);
        await Sdk.PromoteAdminAsync(thread.NewThreadId, myId, false);
        
        // I can list members
        var members2 = await Sdk.ThreadMembersAsync(thread.NewThreadId);
        Assert.AreEqual(Code.ResultShown, members2.Code);
    }

    [TestMethod]
    public async Task ListMembersAfterKicked()
    {
        await Sdk.RegisterAsync("user31@domain.com", "password");
        var user31Id = (await Sdk.MeAsync()).User.Id;
        await Sdk.SignoutAsync();
        await Sdk.RegisterAsync("user32@domain.com", "password");
        var user32Id = (await Sdk.MeAsync()).User.Id;
        var thread = await Sdk.HardInviteAsync(user31Id);
        
        // Members
        var members = await Sdk.ThreadMembersAsync(thread.NewThreadId);
        
        // Assert
        Assert.AreEqual(Code.ResultShown, members.Code);
        Assert.AreEqual(2, members.Members.Count);
        Assert.IsTrue(members.Members.Any(t => t.User.Id == user31Id));
        Assert.IsTrue(members.Members.Any(t => t.User.Id == user32Id));
        
        // Kick user 31.
        await Sdk.KickMemberAsync(thread.NewThreadId, user31Id);
        
        // Only user 32 left.
        var membersOnly1 = await Sdk.ThreadMembersAsync(thread.NewThreadId);
        Assert.AreEqual(Code.ResultShown, membersOnly1.Code);
        Assert.AreEqual(1, membersOnly1.Members.Count);
        Assert.IsTrue(membersOnly1.Members.Any(t => t.User.Id == user32Id));
        
        // From user 31 view, he can not list members.
        await Sdk.SignoutAsync();
        await Sdk.SignInAsync("user31@domain.com", "password");
        try
        {
            await Sdk.ThreadMembersAsync(thread.NewThreadId);
            Assert.Fail();
        }
        catch (AiurUnexpectedServerResponseException e)
        {
            Assert.AreEqual(Code.Unauthorized, e.Response.Code);
        }
    }
    
    [TestMethod]
    public async Task GetThreadInfoAfterKicked()
    {
        await Sdk.RegisterAsync("user33@domain.com", "password");
        var user33Id = (await Sdk.MeAsync()).User.Id;
        await Sdk.SignoutAsync();
        await Sdk.RegisterAsync("user34@domain.com", "password");
        var thread = await Sdk.HardInviteAsync(user33Id);
        
        // Details
        var details = await Sdk.ThreadDetailsAsync(thread.NewThreadId);
        
        // Assert
        Assert.AreEqual(Code.ResultShown, details.Code);
        Assert.AreEqual(false, details.Thread.AllowSearchByName);
        
        // Kick user 33.
        await Sdk.KickMemberAsync(thread.NewThreadId, user33Id);
        
        // From user 33 view, he can not get thread details.
        await Sdk.SignoutAsync();
        await Sdk.SignInAsync("user33@domain.com", "password");
        try
        {
            await Sdk.ThreadDetailsAsync(thread.NewThreadId);
            Assert.Fail();
        }
        catch (AiurUnexpectedServerResponseException e)
        {
            Assert.AreEqual(Code.Unauthorized, e.Response.Code);
        }
    }

    [TestMethod]
    public async Task GetThreadInfoNotFound()
    {
        await Sdk.RegisterAsync("user35@domain.com", "password");
        try
        {
            await Sdk.ThreadDetailsAsync(999);
            Assert.Fail();
        }
        catch (AiurUnexpectedServerResponseException e)
        {
            Assert.AreEqual(Code.Unauthorized, e.Response.Code);
        }
    }
}