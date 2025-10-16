using Aiursoft.AiurProtocol.Exceptions;
using Aiursoft.AiurProtocol.Models;
using Aiursoft.Kahla.SDK.Events;
using Aiursoft.Kahla.Server.Data;
using Aiursoft.Kahla.Tests.TestBase;

namespace Aiursoft.Kahla.Tests.SdkTests;

[TestClass]
public class ThreadsTests : KahlaTestBase
{
    [TestMethod]
    public async Task CreateAndListTest()
    {
        await RunUnderUser("user20", async () =>
        {
            await Sdk.CreateFromScratchAsync(
                "TestThread",
                false,
                false,
                false,
                false,
                false);

            // Search
            var searchResult = await Sdk.SearchThreadsAsync("Test");
            Assert.AreEqual(Code.ResultShown, searchResult.Code);
            Assert.HasCount(1, searchResult.KnownThreads);

            // Search Non-Existed
            var searchResult2 = await Sdk.SearchThreadsAsync("Non-Existed");
            Assert.AreEqual(Code.ResultShown, searchResult2.Code);
            Assert.IsEmpty(searchResult2.KnownThreads);

            // Search Excluding
            var searchResult3 = await Sdk.SearchThreadsAsync("Test", excluding: "est");
            Assert.AreEqual(Code.ResultShown, searchResult3.Code);
            Assert.IsEmpty(searchResult3.KnownThreads);
        });
    }

    [TestMethod]
    public async Task CreateAThreadWithNameTooLong()
    {
        await RunUnderUser("user19", async () =>
        {
            var nameTooLong = new string('a', 257);
            try
            {
                await Sdk.CreateFromScratchAsync(
                    nameTooLong,
                    false,
                    false,
                    false,
                    false,
                    false);
                Assert.Fail();
            }
            catch (AiurUnexpectedServerResponseException e)
            {
                Assert.AreEqual(Code.InvalidInput, e.Response.Code);
            }
        });
    }

    [TestMethod]
    public async Task UpdateAThreadWithNameTooLong()
    {
        await RunUnderUser("user19", async () =>
        {
            var nameTooLong = new string('a', 257);
            var threadDetails = await Sdk.CreateFromScratchAsync(
                "init test",
                false,
                false,
                false,
                false,
                false);
            try
            {
                await Sdk.UpdateThreadAsync(
                    threadDetails.NewThreadId,
                    name: nameTooLong);
                Assert.Fail();
            }
            catch (AiurUnexpectedServerResponseException e)
            {
                Assert.AreEqual(Code.InvalidInput, e.Response.Code);
            }

            var pushed = await RunAndGetEvent(async () =>
            {
                // Update to a valid name
                await Sdk.UpdateThreadAsync(
                    threadDetails.NewThreadId,
                    name: "valid name");
            });
            Assert.IsTrue(pushed is ThreadPropertyChangedEvent);
            Assert.AreEqual("valid name", ((ThreadPropertyChangedEvent)pushed).ThreadName);

            // Get the thread details
            var details = await Sdk.ThreadDetailsJoinedAsync(threadDetails.NewThreadId);
            Assert.AreEqual("valid name", details.Thread.Name);
        });
    }

    [TestMethod]
    public async Task SearchThreadByUserName()
    {
        var user1Id = "";
        await RunUnderUser("user1", async () => { user1Id = (await Sdk.MeAsync()).User.Id; });

        await RunUnderUser("user2", async () =>
        {
            // Create a thread
            var createdThread = await Sdk.HardInviteAsync(user1Id);

            // Set the name.
            await Sdk.UpdateThreadAsync(createdThread.NewThreadId, name: "Patched_name");

            // Search the thread by name
            var searchThread = await Sdk.SearchThreadsAsync("Patched_name");
            Assert.HasCount(1, searchThread.KnownThreads);
            Assert.AreEqual(createdThread.NewThreadId, searchThread.KnownThreads.First().Id);

            // Search the thread by username
            var searchThread2 = await Sdk.SearchThreadsAsync("user1");
            Assert.HasCount(1, searchThread2.KnownThreads);
            Assert.AreEqual(createdThread.NewThreadId, searchThread2.KnownThreads.First().Id);
        });
    }

    [TestMethod]
    public async Task ListNewThreadOnlyMeAsMember()
    {
        await RunUnderUser("user23", async () =>
        {
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
            Assert.HasCount(1, members.Members);
        });
    }

    [TestMethod]
    public async Task HardInviteOnlyWeTwoAsMembers()
    {
        var user24Id = "";
        await RunUnderUser("user24", async () => { user24Id = (await Sdk.MeAsync()).User.Id; });

        await RunUnderUser("user25", async () =>
        {
            var myId = (await Sdk.MeAsync()).User.Id;
            var thread = await Sdk.HardInviteAsync(user24Id);

            // Members
            var members = await Sdk.ThreadMembersAsync(thread.NewThreadId);

            // Assert
            Assert.AreEqual(Code.ResultShown, members.Code);
            Assert.HasCount(2, members.Members);
            Assert.IsTrue(members.Members.Any(t => t.User.Id == user24Id));
            Assert.IsTrue(members.Members.Any(t => t.User.Id == myId));
        });
    }

    [TestMethod]
    public async Task HardInviteNotExists()
    {
        await RunUnderUser("user26", async () =>
        {
            try
            {
                await Sdk.HardInviteAsync("not-exists");
                Assert.Fail();
            }
            catch (AiurUnexpectedServerResponseException e)
            {
                Assert.AreEqual(Code.NotFound, e.Response.Code);
            }
        });
    }

    [TestMethod]
    public async Task HardInvitePrivateAccount()
    {
        var user27Id = "";
        await RunUnderUser("user27", async () =>
        {
            await Sdk.UpdateMeAsync(allowHardInvitation: false);
            user27Id = (await Sdk.MeAsync()).User.Id;
        });

        await RunUnderUser("user28", async () =>
        {
            try
            {
                await Sdk.HardInviteAsync(user27Id);
                Assert.Fail();
            }
            catch (AiurUnexpectedServerResponseException e)
            {
                Assert.AreEqual(Code.Unauthorized, e.Response.Code);
            }
        });
    }

    [TestMethod]
    public async Task HardInviteBlockedAccount()
    {
        var user28Id = "";
        await RunUnderUser("user28", async () => { user28Id = (await Sdk.MeAsync()).User.Id; });

        var user29Id = "";
        await RunUnderUser("user29", async () =>
        {
            await Sdk.BlockNewAsync(user28Id);
            user29Id = (await Sdk.MeAsync()).User.Id;
        });

        await RunUnderUser("user28", async () =>
        {
            try
            {
                await Sdk.HardInviteAsync(user29Id);
                Assert.Fail();
            }
            catch (AiurUnexpectedServerResponseException e)
            {
                Assert.AreEqual(Code.Conflict, e.Response.Code);
            }
        });
    }

    [TestMethod]
    public async Task ListMembersNotAllowed()
    {
        await RunUnderUser("user30", async () =>
        {
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
            Assert.HasCount(1, members.Members);

            // Set me as not admin
            await Sdk.PromoteAdminAsync(thread.NewThreadId, myId, false);

            // I cannot list members
            try
            {
                await Sdk.ThreadMembersAsync(thread.NewThreadId);
                Assert.Fail();
            }
            catch (AiurUnexpectedServerResponseException e)
            {
                Assert.AreEqual(Code.Unauthorized, e.Response.Code);
            }

            // Give me admin, allow members to enlist all members, then remove me as admin
            await Sdk.PromoteAdminAsync(thread.NewThreadId, myId, true);
            await Sdk.UpdateThreadAsync(thread.NewThreadId, allowMembersEnlistAllMembers: true);
            await Sdk.PromoteAdminAsync(thread.NewThreadId, myId, false);

            // I can list members
            var members2 = await Sdk.ThreadMembersAsync(thread.NewThreadId);
            Assert.AreEqual(Code.ResultShown, members2.Code);
        });
    }

    [TestMethod]
    public async Task TestSearchMemberInThread()
    {
        var threadId = 0;
        await RunUnderUser("user1", async () =>
        {
            var thread = await Sdk.CreateFromScratchAsync(
                "Test-Thread",
                true,
                true,
                true,
                true,
                true);
            threadId = thread.NewThreadId;
        });
        await RunUnderUser("user2", async () => { await Sdk.DirectJoinAsync(threadId); });
        await RunUnderUser("user12", async () =>
        {
            await Sdk.DirectJoinAsync(threadId);
            var searchResult = await Sdk.ThreadMembersAsync(threadId, "user1");
            Assert.AreEqual(Code.ResultShown, searchResult.Code);
            Assert.HasCount(2, searchResult.Members);

            var searchResult2 = await Sdk.ThreadMembersAsync(threadId, "user", "2");
            Assert.AreEqual(Code.ResultShown, searchResult2.Code);
            Assert.HasCount(1, searchResult2.Members);
            Assert.AreEqual("user1", searchResult2.Members.Single().User.NickName);
        });
    }

    [TestMethod]
    public async Task ListMembersAfterKicked()
    {
        var user31Id = "";
        await RunUnderUser("user31", async () => { user31Id = (await Sdk.MeAsync()).User.Id; });

        var threadId = 0;
        await RunUnderUser("user32", async () =>
        {
            var user32Id = (await Sdk.MeAsync()).User.Id;
            var thread = await Sdk.HardInviteAsync(user31Id);
            threadId = thread.NewThreadId;

            // Members
            var members = await Sdk.ThreadMembersAsync(thread.NewThreadId);

            // Assert
            Assert.AreEqual(Code.ResultShown, members.Code);
            Assert.HasCount(2, members.Members);
            Assert.IsTrue(members.Members.Any(t => t.User.Id == user31Id));
            Assert.IsTrue(members.Members.Any(t => t.User.Id == user32Id));

            // Kick user31
            await Sdk.KickMemberAsync(thread.NewThreadId, user31Id);

            // Only user32 left
            var membersOnly1 = await Sdk.ThreadMembersAsync(thread.NewThreadId);
            Assert.AreEqual(Code.ResultShown, membersOnly1.Code);
            Assert.HasCount(1, membersOnly1.Members);
            Assert.IsTrue(membersOnly1.Members.Any(t => t.User.Id == user32Id));
        });

        // From user31's view, he cannot list members
        await RunUnderUser("user31", async () =>
        {
            try
            {
                await Sdk.ThreadMembersAsync(threadId);
                Assert.Fail();
            }
            catch (AiurUnexpectedServerResponseException e)
            {
                Assert.AreEqual(Code.Unauthorized, e.Response.Code);
            }
        });
    }

    [TestMethod]
    public async Task GetThreadInfoAfterKicked()
    {
        var user33Id = "";
        var threadId = 0;

        await RunUnderUser("user33", async () => { user33Id = (await Sdk.MeAsync()).User.Id; });

        await RunUnderUser("user34", async () =>
        {
            var thread = await Sdk.HardInviteAsync(user33Id);
            threadId = thread.NewThreadId;

            // Details
            var details = await Sdk.ThreadDetailsJoinedAsync(thread.NewThreadId);

            // Assert
            Assert.AreEqual(Code.ResultShown, details.Code);
            Assert.IsFalse(details.Thread.AllowSearchByName);

            // Kick user33
            await Sdk.KickMemberAsync(thread.NewThreadId, user33Id);
        });

        await RunUnderUser("user33", async () =>
        {
            try
            {
                await Sdk.ThreadDetailsJoinedAsync(threadId);
                Assert.Fail();
            }
            catch (AiurUnexpectedServerResponseException e)
            {
                Assert.AreEqual(Code.Unauthorized, e.Response.Code);
            }

            // However, he can still get the thread details anonymously
            var threadAnonymous = await Sdk.ThreadDetailsAnonymousAsync(threadId);
            Assert.AreEqual(Code.ResultShown, threadAnonymous.Code);
            Assert.IsFalse(threadAnonymous.Thread.ImInIt);
        });
    }

    [TestMethod]
    public async Task GetThreadInfoNotFound()
    {
        await RunUnderUser("user35", async () =>
        {
            try
            {
                await Sdk.ThreadDetailsJoinedAsync(999);
                Assert.Fail();
            }
            catch (AiurUnexpectedServerResponseException e)
            {
                Assert.AreEqual(Code.Unauthorized, e.Response.Code);
            }
        });
    }

    [TestMethod]
    public async Task GetThreadInfoAnonymousNotFound()
    {
        await RunUnderUser("user36", async () =>
        {
            try
            {
                await Sdk.ThreadDetailsAnonymousAsync(999);
                Assert.Fail();
            }
            catch (AiurUnexpectedServerResponseException e)
            {
                Assert.AreEqual(Code.NotFound, e.Response.Code);
            }
        });
    }

    [TestMethod]
    public async Task UpdateThreadNotFound()
    {
        await RunUnderUser("user37", async () =>
        {
            try
            {
                await Sdk.UpdateThreadAsync(999);
                Assert.Fail();
            }
            catch (AiurUnexpectedServerResponseException e)
            {
                Assert.AreEqual(Code.NotFound, e.Response.Code);
            }
        });
    }

    [TestMethod]
    public async Task UpdateThreadNotJoined()
    {
        var user39Id = "";
        await RunUnderUser("user39", async () => { user39Id = (await Sdk.MeAsync()).User.Id; });

        await RunUnderUser("user38", async () =>
        {
            var thread = await Sdk.HardInviteAsync(user39Id);

            // Update should be successful
            await Sdk.UpdateThreadAsync(thread.NewThreadId, allowSearchByName: true);

            // Transfer ownership to user39 (I'm still admin)
            await Sdk.TransferOwnershipAsync(thread.NewThreadId, user39Id);

            // Kick myself
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
        });
    }

    [TestMethod]
    public async Task UpdateThreadImNotAdmin()
    {
        var user40Id = "";
        await RunUnderUser("user40", async () => { user40Id = (await Sdk.MeAsync()).User.Id; });

        var threadId = 0;
        await RunUnderUser("user41", async () =>
        {
            var user41Id = (await Sdk.MeAsync()).User.Id;
            var thread = await Sdk.HardInviteAsync(user40Id);
            threadId = thread.NewThreadId;

            // Update should be successful
            await Sdk.UpdateThreadAsync(thread.NewThreadId, name: "New name");
            await Sdk.UpdateThreadAsync(thread.NewThreadId, iconFilePath: "New name");
            await Sdk.UpdateThreadAsync(thread.NewThreadId, allowDirectJoinWithoutInvitation: true);
            await Sdk.UpdateThreadAsync(thread.NewThreadId, allowMemberSoftInvitation: true);
            await Sdk.UpdateThreadAsync(thread.NewThreadId, allowMembersSendMessages: true);

            // Info should be updated
            var details = await Sdk.ThreadDetailsJoinedAsync(thread.NewThreadId);
            Assert.AreEqual("New name", details.Thread.Name);
            Assert.AreEqual("New name", details.Thread.ImagePath);
            Assert.IsTrue(details.Thread.AllowDirectJoinWithoutInvitation);
            Assert.IsTrue(details.Thread.AllowMemberSoftInvitation);
            Assert.IsTrue(details.Thread.AllowMembersSendMessages);

            // Demote user41
            await Sdk.PromoteAdminAsync(thread.NewThreadId, user41Id, false);
        });

        await RunUnderUser("user41", async () =>
        {
            // User41 cannot update thread
            try
            {
                await Sdk.UpdateThreadAsync(threadId, name: "New name 2");
                Assert.Fail();
            }
            catch (AiurUnexpectedServerResponseException e)
            {
                Assert.AreEqual(Code.Unauthorized, e.Response.Code);
            }
        });
    }

    [TestMethod]
    public async Task TransferOwnershipTest()
    {
        var newUserId = "";
        await RunUnderUser("newUser", async () => { newUserId = (await Sdk.MeAsync()).User.Id; });

        var threadId = 0;
        await RunUnderUser("ownerUser", async () =>
        {
            var thread = await Sdk.CreateFromScratchAsync(
                "OwnershipTransferThread",
                false,
                true,
                false,
                false,
                false);
            threadId = thread.NewThreadId;
        });

        await RunUnderUser("newUser", async () => { await Sdk.DirectJoinAsync(threadId); });

        await RunUnderUser("ownerUser", async () =>
        {
            var transferResponse = await Sdk.TransferOwnershipAsync(threadId, newUserId);
            Assert.AreEqual(Code.JobDone, transferResponse.Code);
        });

        await RunUnderUser("newUser", async () =>
        {
            // Verify new ownership
            var threadDetails = await Sdk.ThreadDetailsJoinedAsync(threadId);
            Assert.AreEqual(newUserId, threadDetails.Thread.OwnerId);
        });
    }

    [TestMethod]
    public async Task TransferOwnershipInvalidUserTest()
    {
        await RunUnderUser("ownerUser2", async () =>
        {
            var thread = await Sdk.CreateFromScratchAsync(
                "InvalidTransferThread",
                false,
                false,
                false,
                false,
                false);

            try
            {
                await Sdk.TransferOwnershipAsync(thread.NewThreadId, "nonexistentUserId");
                Assert.Fail();
            }
            catch (AiurUnexpectedServerResponseException e)
            {
                Assert.AreEqual(Code.NotFound, e.Response.Code);
            }
        });
    }

    [TestMethod]
    public async Task TransferOwnershipNotAdminTest()
    {
        var adminUserId = "";
        await RunUnderUser("adminUser", async () => { adminUserId = (await Sdk.MeAsync()).User.Id; });

        var regularUserId = "";
        await RunUnderUser("regularUser", async () => { regularUserId = (await Sdk.MeAsync()).User.Id; });

        int threadId = 0;
        await RunUnderUser("adminUser", async () =>
        {
            var thread = await Sdk.CreateFromScratchAsync(
                "AdminTransferTest",
                false,
                false,
                false,
                false,
                false);
            threadId = thread.NewThreadId;

            await Sdk.HardInviteAsync(regularUserId);
        });

        await RunUnderUser("regularUser", async () =>
        {
            try
            {
                await Sdk.TransferOwnershipAsync(threadId, adminUserId);
                Assert.Fail();
            }
            catch (AiurUnexpectedServerResponseException e)
            {
                Assert.AreEqual(Code.Unauthorized, e.Response.Code);
            }
        });
    }

    [TestMethod]
    public async Task PromoteAdmin_ThreadNotFound()
    {
        await RunUnderUser("user42", async () =>
        {
            try
            {
                await Sdk.PromoteAdminAsync(999, "non-existent-user", true);
                Assert.Fail();
            }
            catch (AiurUnexpectedServerResponseException e)
            {
                Assert.AreEqual(Code.NotFound, e.Response.Code);
            }
        });
    }

    [TestMethod]
    public async Task PromoteAdmin_MyRelationNotFound()
    {
        var user43Id = "";
        await RunUnderUser("user43", async () => { user43Id = (await Sdk.MeAsync()).User.Id; });

        int threadId = 0;
        await RunUnderUser("user44", async () =>
        {
            var thread = await Sdk.HardInviteAsync(user43Id);
            threadId = thread.NewThreadId;
        });

        await RunUnderUser("user43", async () => { await Sdk.LeaveThreadAsync(threadId); });

        await RunUnderUser("user44", async () =>
        {
            try
            {
                await Sdk.PromoteAdminAsync(threadId, user43Id, true);
                Assert.Fail();
            }
            catch (AiurUnexpectedServerResponseException e)
            {
                Assert.AreEqual(Code.NotFound, e.Response.Code);
            }
        });
    }

    [TestMethod]
    public async Task PromoteAdmin_NotOwnerOfThread()
    {
        var user45Id = "";
        await RunUnderUser("user45", async () => { user45Id = (await Sdk.MeAsync()).User.Id; });

        int threadId = 0;
        await RunUnderUser("user46", async () =>
        {
            var thread = await Sdk.HardInviteAsync(user45Id);
            threadId = thread.NewThreadId;
        });

        await RunUnderUser("user45", async () =>
        {
            try
            {
                await Sdk.PromoteAdminAsync(threadId, user45Id, true);
                Assert.Fail();
            }
            catch (AiurUnexpectedServerResponseException e)
            {
                Assert.AreEqual(Code.Unauthorized, e.Response.Code);
            }
        });
    }

    [TestMethod]
    public async Task PromoteAdmin_TargetRelationNotFound()
    {
        var user47Id = "";
        await RunUnderUser("user47", async () => { user47Id = (await Sdk.MeAsync()).User.Id; });

        await RunUnderUser("user48", async () =>
        {
            var thread = await Sdk.HardInviteAsync(user47Id);

            try
            {
                await Sdk.PromoteAdminAsync(thread.NewThreadId, "non-existent-user", true);
                Assert.Fail();
            }
            catch (AiurUnexpectedServerResponseException e)
            {
                Assert.AreEqual(Code.NotFound, e.Response.Code);
            }
        });
    }

    [TestMethod]
    public async Task CompleteSoftInviteTest()
    {
        var threadId = 0;
        var validToken = "";
        var user52Id = "";
        await RunUnderUser("user51", async () =>
        {
            var thread = await Sdk.CreateFromScratchAsync(
                "SoftInviteCompleteThread",
                false,
                true,
                true,
                true,
                false);
            threadId = thread.NewThreadId;
        });

        await RunUnderUser("user52", async () => { user52Id = (await Sdk.MeAsync()).User.Id; });

        await RunUnderUser("user51", async () =>
        {
            var tokenResponse = await Sdk.CreateSoftInviteTokenAsync(threadId, user52Id);
            validToken = tokenResponse.Token;
        });

        await RunUnderUser("user52", async () =>
        {
            var joinResponse = await Sdk.CompleteSoftInviteAsync(validToken);
            Assert.AreEqual(Code.JobDone, joinResponse.Code);

            // Ensure I have joined the thread
            var threadDetails = await Sdk.ThreadDetailsJoinedAsync(threadId);
            Assert.IsTrue(threadDetails.Thread.ImInIt);
        });
    }

    [TestMethod]
    public async Task DirectJoinNonExistentThread()
    {
        await RunUnderUser("user53", async () =>
        {
            try
            {
                await Sdk.DirectJoinAsync(999);
                Assert.Fail();
            }
            catch (AiurUnexpectedServerResponseException e)
            {
                Assert.AreEqual(Code.NotFound, e.Response.Code);
            }
        });
    }

    [TestMethod]
    public async Task EnsureCreateScratchEventPushed()
    {
        await RunUnderUser("user54", async () =>
        {
            var pushed = await RunAndGetEvent(async () =>
            {
                await Sdk.CreateFromScratchAsync("t", false, false, false, false, false);
            });

            Assert.IsTrue(pushed is CreateScratchedEvent);
            Assert.AreEqual("t", ((CreateScratchedEvent)pushed).Thread.Name);
        });
    }

    [TestMethod]
    public async Task EnsureDissolveEventPushed()
    {
        await RunUnderUser("user54", async () =>
        {
            var thread = await Sdk.CreateFromScratchAsync("bbbbb", false, false, false, false, false);
            var pushed = await RunAndGetEvent(async () => { await Sdk.DissolveThreadAsync(thread.NewThreadId); });

            Assert.IsTrue(pushed is ThreadDissolvedEvent);
            Assert.AreEqual("bbbbb", ((ThreadDissolvedEvent)pushed).ThreadName);
        });
    }

    [TestMethod]
    public async Task EnsureDirectJoinEventPushed()
    {
        var threadId = 0;
        await RunUnderUser("user100", async () =>
        {
            var thread = await Sdk.CreateFromScratchAsync(
                "t",
                true,
                true,
                false,
                false,
                false);
            threadId = thread.NewThreadId;
        });

        await RunUnderUser("user101", async () =>
        {
            var pushed = await RunAndGetEvent(async () => { await Sdk.DirectJoinAsync(threadId); });

            Assert.IsTrue(pushed is YouDirectJoinedEvent);
            Assert.AreEqual("t", ((YouDirectJoinedEvent)pushed).Thread.Name);
        });
    }

    [TestMethod]
    public async Task EnsureYouBeenKickedEventPushed()
    {
        // User 102 creates a thread
        var threadId = 0;
        await RunUnderUser("user102", async () =>
        {
            var thread = await Sdk.CreateFromScratchAsync(
                "t",
                true,
                true,
                false,
                false,
                false);
            threadId = thread.NewThreadId;
        });
        
        // User 103 joins the thread
        var user103Id = "";
        await RunUnderUser("user103", async () =>
        {
            user103Id = (await Sdk.MeAsync()).User.Id;
            await Sdk.DirectJoinAsync(threadId);
        });
        
        // User 102 will kick user 103 in 3 second
        _ = Task.Run(async () =>
        {
            await Task.Delay(3000);
            await Sdk.SignoutAsync();
            await RunUnderUser("user102", async () =>
            {
                await Sdk.KickMemberAsync(threadId, user103Id);
            });
        });
        
        // User 103 will receive a YouBeenKickedEvent
        await RunUnderUser("user103", async () =>
        {
            var pushed = await RunAndGetEvent(async () =>
            {
                await Task.CompletedTask;
            });

            Assert.IsTrue(pushed is YouBeenKickedEvent);
            Assert.AreEqual("t", ((YouBeenKickedEvent)pushed).ThreadName);
        }, autoSignOut: false);
    }

    [TestMethod]
    public async Task EnsureLeaveThreadEventPushed()
    {
        // User 104 creates a thread
        var threadId = 0;
        await RunUnderUser("user104", async () =>
        {
            var thread = await Sdk.CreateFromScratchAsync(
                "ttt",
                true,
                true,
                false,
                false,
                false);
            threadId = thread.NewThreadId;
        });
        await RunUnderUser("user105", async () =>
        {
            await Sdk.DirectJoinAsync(threadId);
            var pushed = await RunAndGetEvent(async () =>
            {
                await Sdk.LeaveThreadAsync(threadId);
            });
            
            Assert.IsTrue(pushed is YouLeftEvent);
            Assert.AreEqual("ttt", ((YouLeftEvent)pushed).ThreadName);
        });
    }

    [TestMethod]
    public async Task EnsureThreadDissolvedEventPushed()
    {
        // User 126 creates a thread
        await RunUnderUser("user126", async () =>
        {
            var thread = await Sdk.CreateFromScratchAsync(
                "tt",
                true,
                true,
                false,
                false,
                false);

            var pushed = await RunAndGetEvent(async () =>
            {
                await Sdk.DissolveThreadAsync(thread.NewThreadId);
            });
            Assert.IsTrue(pushed is ThreadDissolvedEvent);
            Assert.AreEqual("tt", ((ThreadDissolvedEvent)pushed).ThreadName);
        });
    }

    [TestMethod]
    public async Task EnsureYourHardInviteFinishedEventPushed()
    {
        var user107Id = "";
        await RunUnderUser("user107", async () => { user107Id = (await Sdk.MeAsync()).User.Id; });
        await RunUnderUser("user108", async () =>
        {
            var pushed = await RunAndGetEvent(async () =>
            {
                await Sdk.HardInviteAsync(user107Id);
            });
            Assert.IsTrue(pushed is YourHardInviteFinishedEvent);
            Assert.AreEqual("{THE OTHER USER}", ((YourHardInviteFinishedEvent)pushed).Thread.Name);
        });
    }

    [TestMethod]
    public async Task EnsureYouWasHardInvitedEventPushed()
    {
        var user109Id = "";
        await RunUnderUser("user109", async () => { user109Id = (await Sdk.MeAsync()).User.Id; });
        
        // User 110 will hard invite user 109 in 3 second
        _ = Task.Run(async () =>
        {
            await Task.Delay(3000);
            await Sdk.SignoutAsync();
            await RunUnderUser("user110", async () =>
            {
                await Sdk.HardInviteAsync(user109Id);
            }, autoSignOut: false);
        });
        
        await RunUnderUser("user109", async () =>
        {
            var pushed = await RunAndGetEvent(async () =>
            {
                await Task.CompletedTask;
            });

            Assert.IsTrue(pushed is YouWasHardInvitedEvent);
            Assert.AreEqual("{THE OTHER USER}", ((YouWasHardInvitedEvent)pushed).Thread.Name);
        }, autoSignOut: false);
    }

    [TestMethod]
    public async Task EnsureYouCompletedSoftInvitedEventPushed()
    {
        var threadId = 0;
        var validToken = "";
        var user52Id = "";
        await RunUnderUser("user51", async () =>
        {
            var thread = await Sdk.CreateFromScratchAsync(
                "SoftInviteCompleteThread",
                false,
                true,
                true,
                true,
                false);
            threadId = thread.NewThreadId;
        });

        await RunUnderUser("user52", async () => { user52Id = (await Sdk.MeAsync()).User.Id; });

        await RunUnderUser("user51", async () =>
        {
            var tokenResponse = await Sdk.CreateSoftInviteTokenAsync(threadId, user52Id);
            validToken = tokenResponse.Token;
        });

        await RunUnderUser("user52", async () =>
        {
            var pushed = await RunAndGetEvent(async () => { await Sdk.CompleteSoftInviteAsync(validToken); });
            
            Assert.IsTrue(pushed is YouCompletedSoftInvitedEvent);
            Assert.AreEqual("SoftInviteCompleteThread", ((YouCompletedSoftInvitedEvent)pushed).Thread.Name);
        });
    }

    [TestMethod]
    public async Task EnsureThreadPropertyChangedEventPushed()
    {
        await RunUnderUser("user111", async () =>
        {
            await Sdk.CreateFromScratchAsync("t", true, true, false, false, false);
            
            var pushed = await RunAndGetEvent(async () =>
            {
                await Sdk.UpdateThreadAsync(1, name: "new name");
            });
            
            Assert.IsTrue(pushed is ThreadPropertyChangedEvent);
            Assert.AreEqual("new name", ((ThreadPropertyChangedEvent)pushed).ThreadName);
        });
    }

    [TestMethod]
    public async Task DirectJoinAThreadNotAllowingDirectJoin()
    {
        var threadId = 0;
        await RunUnderUser("user54", async () =>
        {
            var createdThread = await Sdk.CreateFromScratchAsync("t", false, false, false, false, false);
            threadId = createdThread.NewThreadId;
        });

        await RunUnderUser("user55", async () =>
        {
            try
            {
                await Sdk.DirectJoinAsync(threadId);
                Assert.Fail();
            }
            catch (AiurUnexpectedServerResponseException e)
            {
                Assert.AreEqual(Code.Unauthorized, e.Response.Code);
            }
        });
    }

    [TestMethod]
    public async Task DirectJoinImAlreadyIn()
    {
        await RunUnderUser("user56", async () =>
        {
            var createdThread = await Sdk.CreateFromScratchAsync("t", false, true, false, false, false);
            try
            {
                await Sdk.DirectJoinAsync(createdThread.NewThreadId);
                Assert.Fail();
            }
            catch (AiurUnexpectedServerResponseException e)
            {
                Assert.AreEqual(Code.Conflict, e.Response.Code);
            }
        });
    }

    [TestMethod]
    public async Task TestSetMute()
    {
        var threadId = 0;
        await RunUnderUser("user57", async () =>
        {
            var thread = await Sdk.CreateFromScratchAsync("t", false, true, false, false, false);
            threadId = thread.NewThreadId;
            await Sdk.SetMuteAsync(threadId, true);
            var details = await Sdk.ThreadDetailsJoinedAsync(threadId);
            Assert.IsTrue(details.Thread.Muted);
        });

        await Server!.Services.GetRequiredService<QuickMessageAccess>().LoadAsync();

        await RunUnderUser("user57", async () =>
        {
            var details = await Sdk.ThreadDetailsJoinedAsync(threadId);
            Assert.IsTrue(details.Thread.Muted);
            await Sdk.SetMuteAsync(threadId, false);
            var details2 = await Sdk.ThreadDetailsJoinedAsync(threadId);
            Assert.IsFalse(details2.Thread.Muted);
        });

        await Server!.Services.GetRequiredService<QuickMessageAccess>().LoadAsync();

        await RunUnderUser("user57", async () =>
        {
            var details = await Sdk.ThreadDetailsJoinedAsync(threadId);
            Assert.IsFalse(details.Thread.Muted);
        });
    }
}