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
}