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
    public async Task SearchThreadByUserName()
    {
        // Prepare user1
        await Sdk.RegisterAsync("user1@domain.com", "password");
        var user1Id = (await Sdk.MeAsync()).User.Id;
        await Sdk.SignoutAsync();

        // Prepare user2
        await Sdk.RegisterAsync("user2@domain.com", "password");

        // Create a thread
        var createdThread = await Sdk.HardInviteAsync(user1Id);

        // Set the name.
        await Sdk.UpdateThreadAsync(createdThread.NewThreadId, name: "Patched_name");

        // Search the thread by name
        var searchThread = await Sdk.ListThreadsAsync("Patched_name");
        Assert.AreEqual(1, searchThread.KnownThreads.Count);
        Assert.AreEqual(createdThread.NewThreadId, searchThread.KnownThreads.First().Id);

        // Search the thread by username
        var searchThread2 = await Sdk.ListThreadsAsync("user1");
        Assert.AreEqual(1, searchThread2.KnownThreads.Count);
        Assert.AreEqual(createdThread.NewThreadId, searchThread2.KnownThreads.First().Id);
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
        var details = await Sdk.ThreadDetailsJoinedAsync(thread.NewThreadId);

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
            await Sdk.ThreadDetailsJoinedAsync(thread.NewThreadId);
            Assert.Fail();
        }
        catch (AiurUnexpectedServerResponseException e)
        {
            Assert.AreEqual(Code.Unauthorized, e.Response.Code);
        }

        // However, he can still the thread details anonymously.
        var threadAnonymous = await Sdk.ThreadDetailsAnonymousAsync(thread.NewThreadId);
        Assert.AreEqual(Code.ResultShown, threadAnonymous.Code);
        Assert.AreEqual(false, threadAnonymous.Thread.ImInIt);
    }

    [TestMethod]
    public async Task GetThreadInfoNotFound()
    {
        await Sdk.RegisterAsync("user35@domain.com", "password");
        try
        {
            await Sdk.ThreadDetailsJoinedAsync(999);
            Assert.Fail();
        }
        catch (AiurUnexpectedServerResponseException e)
        {
            Assert.AreEqual(Code.Unauthorized, e.Response.Code);
        }
    }

    [TestMethod]
    public async Task GetThreadInfoAnonymousNotFound()
    {
        await Sdk.RegisterAsync("user36@domain.com", "password");
        try
        {
            await Sdk.ThreadDetailsAnonymousAsync(999);
            Assert.Fail();
        }
        catch (AiurUnexpectedServerResponseException e)
        {
            Assert.AreEqual(Code.NotFound, e.Response.Code);
        }
    }

    [TestMethod]
    public async Task UpdateThreadNotFound()
    {
        await Sdk.RegisterAsync("user37@domain.com", "password");
        try
        {
            await Sdk.UpdateThreadAsync(999);
            Assert.Fail();
        }
        catch (AiurUnexpectedServerResponseException e)
        {
            Assert.AreEqual(Code.NotFound, e.Response.Code);
        }
    }

    [TestMethod]
    public async Task UpdateThreadNotJoined()
    {
        await Sdk.RegisterAsync("user39@domain.com", "password");
        var user39Id = (await Sdk.MeAsync()).User.Id;
        await Sdk.SignoutAsync();
        await Sdk.RegisterAsync("user38@domain.com", "password");
        var thread = await Sdk.HardInviteAsync(user39Id);

        // Update should be successful.
        await Sdk.UpdateThreadAsync(thread.NewThreadId, allowSearchByName: true);

        // Transfer ownership to user 38 (I'm still admin)
        await Sdk.TransferOwnershipAsync(thread.NewThreadId, user39Id);

        // Kick me
        await Sdk.KickMemberAsync(thread.NewThreadId, (await Sdk.MeAsync()).User.Id);
        try
        {
            await Sdk.UpdateThreadAsync(thread.NewThreadId, allowSearchByName: false);
            Assert.Fail();
        }
        catch (AiurUnexpectedServerResponseException e)
        {
            Assert.AreEqual(Code.Unauthorized, e.Response.Code);
        }
    }

    [TestMethod]
    public async Task UpdateThreadImNotAdmin()
    {
        await Sdk.RegisterAsync("user40@domain.com", "password");
        var user40Id = (await Sdk.MeAsync()).User.Id;
        await Sdk.SignoutAsync();

        await Sdk.RegisterAsync("user41@domain.com", "password");
        var user41Id = (await Sdk.MeAsync()).User.Id;
        var thread = await Sdk.HardInviteAsync(user40Id);

        // Update should be successful.
        await Sdk.UpdateThreadAsync(thread.NewThreadId, name: "New name");
        await Sdk.UpdateThreadAsync(thread.NewThreadId, iconFilePath: "New name");
        await Sdk.UpdateThreadAsync(thread.NewThreadId, allowDirectJoinWithoutInvitation: true);
        await Sdk.UpdateThreadAsync(thread.NewThreadId, allowMemberSoftInvitation: true);
        await Sdk.UpdateThreadAsync(thread.NewThreadId, allowMembersSendMessages: true);

        // info should be updated.
        var details = await Sdk.ThreadDetailsJoinedAsync(thread.NewThreadId);
        Assert.AreEqual("New name", details.Thread.Name);
        Assert.AreEqual("New name", details.Thread.ImagePath);
        Assert.AreEqual(true, details.Thread.AllowDirectJoinWithoutInvitation);
        Assert.AreEqual(true, details.Thread.AllowMemberSoftInvitation);
        Assert.AreEqual(true, details.Thread.AllowMembersSendMessages);

        // Demote user 41
        await Sdk.PromoteAdminAsync(thread.NewThreadId, user41Id, false);

        // User 41 can not update thread
        try
        {
            await Sdk.UpdateThreadAsync(thread.NewThreadId, name: "New name 2");
            Assert.Fail();
        }
        catch (AiurUnexpectedServerResponseException e)
        {
            Assert.AreEqual(Code.Unauthorized, e.Response.Code);
        }
    }

    [TestMethod]
    public async Task TransferOwnershipTest()
    {
        // Step 1: Owner
        await Sdk.RegisterAsync("ownerUser@domain.com", "password");
        await Sdk.SignoutAsync();

        // Step 2: Register a new user
        await Sdk.RegisterAsync("newUser@domain.com", "password");
        var newUserId = (await Sdk.MeAsync()).User.Id;
        await Sdk.SignoutAsync();
        
        // Step 3: Create a new thread by ownerUser
        await Sdk.SignInAsync("ownerUser@domain.com", "password");
        var thread = await Sdk.CreateFromScratchAsync(
            "OwnershipTransferThread",
            false,
            true,
            false,
            false,
            false);
        await Sdk.SignoutAsync();
        
        // Step 4: Add the new user to the thread
        await Sdk.SignInAsync("newUser@domain.com", "password");
        await Sdk.DirectJoinAsync(thread.NewThreadId);
        await Sdk.SignoutAsync();

        // Step 5: Transfer ownership from ownerUser to newUser
        await Sdk.SignInAsync("ownerUser@domain.com", "password");
        var transferResponse = await Sdk.TransferOwnershipAsync(thread.NewThreadId, newUserId);
        Assert.AreEqual(Code.JobDone, transferResponse.Code);

        // Step 6: Verify new ownership
        var threadDetails = await Sdk.ThreadDetailsJoinedAsync(thread.NewThreadId);
        Assert.AreEqual(newUserId, threadDetails.Thread.OwnerId);
    }

    [TestMethod]
    public async Task TransferOwnershipInvalidUserTest()
    {
        // Step 1: Register a user and create a thread
        await Sdk.RegisterAsync("ownerUser2@domain.com", "password");
        var thread = await Sdk.CreateFromScratchAsync(
            "InvalidTransferThread",
            false,
            false,
            false,
            false,
            false);

        // Step 2: Attempt to transfer ownership to an invalid user ID
        try
        {
            await Sdk.TransferOwnershipAsync(thread.NewThreadId, "nonexistentUserId");
            Assert.Fail();
        }
        catch (AiurUnexpectedServerResponseException e)
        {
            Assert.AreEqual(Code.NotFound, e.Response.Code);
        }
    }

    [TestMethod]
    public async Task TransferOwnershipNotAdminTest()
    {
        // Step 1: Register two users
        await Sdk.RegisterAsync("adminUser@domain.com", "password");
        var adminUserId = (await Sdk.MeAsync()).User.Id;

        await Sdk.SignoutAsync();
        await Sdk.RegisterAsync("regularUser@domain.com", "password");
        var regularUserId = (await Sdk.MeAsync()).User.Id;

        // Step 2: Create a thread as adminUser
        await Sdk.SignoutAsync();
        await Sdk.SignInAsync("adminUser@domain.com", "password");
        var thread = await Sdk.CreateFromScratchAsync(
            "AdminTransferTest",
            false,
            false,
            false,
            false,
            false);

        // Step 3: Add regularUser to the thread
        await Sdk.HardInviteAsync(regularUserId);

        // Step 4: Switch to regularUser and attempt to transfer ownership
        await Sdk.SignoutAsync();
        await Sdk.SignInAsync("regularUser@domain.com", "password");
        try
        {
            await Sdk.TransferOwnershipAsync(thread.NewThreadId, adminUserId);
            Assert.Fail();
        }
        catch (AiurUnexpectedServerResponseException e)
        {
            Assert.AreEqual(Code.Unauthorized, e.Response.Code);
        }
    }
    
        [TestMethod]
    public async Task PromoteAdmin_ThreadNotFound()
    {
        await Sdk.RegisterAsync("user42@domain.com", "password");
        try
        {
            await Sdk.PromoteAdminAsync(999, "non-existent-user", true);
            Assert.Fail();
        }
        catch (AiurUnexpectedServerResponseException e)
        {
            Assert.AreEqual(Code.NotFound, e.Response.Code);
        }
    }

    [TestMethod]
    public async Task PromoteAdmin_MyRelationNotFound()
    {
        await Sdk.RegisterAsync("user43@domain.com", "password");
        var user43Id = (await Sdk.MeAsync()).User.Id;
        await Sdk.SignoutAsync();
        await Sdk.RegisterAsync("user44@domain.com", "password");
        var thread = await Sdk.HardInviteAsync(user43Id);

        // User 43 leaves the thread
        await Sdk.SignoutAsync();
        await Sdk.SignInAsync("user43@domain.com", "password");
        await Sdk.LeaveThreadAsync(thread.NewThreadId);
        await Sdk.SignoutAsync();

        // Attempt to promote admin without membership
        await Sdk.SignInAsync("user44@domain.com", "password");
        try
        {
            await Sdk.PromoteAdminAsync(thread.NewThreadId, user43Id, true);
            Assert.Fail();
        }
        catch (AiurUnexpectedServerResponseException e)
        {
            Assert.AreEqual(Code.NotFound, e.Response.Code);
        }
    }

    [TestMethod]
    public async Task PromoteAdmin_NotOwnerOfThread()
    {
        await Sdk.RegisterAsync("user45@domain.com", "password");
        var user45Id = (await Sdk.MeAsync()).User.Id;
        await Sdk.SignoutAsync();
        await Sdk.RegisterAsync("user46@domain.com", "password");
        var thread = await Sdk.HardInviteAsync(user45Id);

        await Sdk.SignoutAsync();
        await Sdk.SignInAsync("user45@domain.com", "password");
        // User 46 is not the owner, attempt to promote
        try
        {
            await Sdk.PromoteAdminAsync(thread.NewThreadId, user45Id, true);
            Assert.Fail();
        }
        catch (AiurUnexpectedServerResponseException e)
        {
            Assert.AreEqual(Code.Unauthorized, e.Response.Code);
        }
    }

    [TestMethod]
    public async Task PromoteAdmin_TargetRelationNotFound()
    {
        await Sdk.RegisterAsync("user47@domain.com", "password");
        var user47Id = (await Sdk.MeAsync()).User.Id;
        await Sdk.SignoutAsync();
        await Sdk.RegisterAsync("user48@domain.com", "password");
        var thread = await Sdk.HardInviteAsync(user47Id);

        // Attempt to promote a user not in the thread
        try
        {
            await Sdk.PromoteAdminAsync(thread.NewThreadId, "non-existent-user", true);
            Assert.Fail();
        }
        catch (AiurUnexpectedServerResponseException e)
        {
            Assert.AreEqual(Code.NotFound, e.Response.Code);
        }
    }
    
    [TestMethod]
    public async Task CompleteSoftInviteTest()
    {
        // Register a user and create a thread
        await Sdk.RegisterAsync("user51@domain.com", "password");
        var thread = await Sdk.CreateFromScratchAsync(
            "SoftInviteCompleteThread",
            false,
            true,
            true,
            true,
            false);
        await Sdk.SignoutAsync();
        
        // Register another user and attempt to complete the soft invite
        await Sdk.RegisterAsync("user52@domain.com", "password");
        var user52Id = (await Sdk.MeAsync()).User.Id;
        await Sdk.SignoutAsync();
        
        // Generate a valid soft invite token
        await Sdk.SignInAsync("user51@domain.com", "password");
        var tokenResponse = await Sdk.CreateSoftInviteTokenAsync(thread.NewThreadId, user52Id);
        var validToken = tokenResponse.Token;
        await Sdk.SignoutAsync();
        
        // Complete the soft invite with the valid token
        await Sdk.SignInAsync("user52@domain.com", "password");
        var joinResponse = await Sdk.CompleteSoftInviteAsync(validToken);
        Assert.AreEqual(Code.JobDone, joinResponse.Code);
        
        // Ensure I have joined the thread
        var threadDetails = await Sdk.ThreadDetailsJoinedAsync(thread.NewThreadId);
        Assert.IsTrue(threadDetails.Thread.ImInIt);
    }
}