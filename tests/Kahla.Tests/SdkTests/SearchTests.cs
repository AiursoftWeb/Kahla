using Aiursoft.Kahla.Tests.TestBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aiursoft.Kahla.Tests.SdkTests;

[TestClass]
public class SearchTests : KahlaTestBase
{
    [TestMethod]
    public async Task SearchContactsAsync()
    {
        // Prepare user21
        await Sdk.RegisterAsync("user21@domain.com", "password");
        await Sdk.UpdateMeAsync(listInSearchResult: false);
        var user21Id = (await Sdk.MeAsync()).User.Id;
        await Sdk.SignoutAsync();
        
        // Login as user21
        await Sdk.RegisterAsync("user22@domain.com", "password");
        
        // Can search him by id
        var search21ById = await Sdk.SearchUsersGloballyAsync(user21Id);
        Assert.AreEqual(1, search21ById.Users.Count);
        Assert.AreEqual(user21Id, search21ById.Users.First().User.Id);
        
        // Can not search him by Email
        var search21ByEmail = await Sdk.SearchUsersGloballyAsync("user21");
        Assert.AreEqual(0, search21ByEmail.Users.Count);
        
        // Login user21
        await Sdk.SignoutAsync();
        await Sdk.SignInAsync("user21@domain.com", "password");
        await Sdk.UpdateMeAsync(listInSearchResult: true);
        await Sdk.SignoutAsync();
        
        // Login as user22
        await Sdk.SignInAsync("user22@domain.com", "password");

        // Can search him by id
        search21ById = await Sdk.SearchUsersGloballyAsync(user21Id);
        Assert.AreEqual(1, search21ById.Users.Count);
        Assert.AreEqual(user21Id, search21ById.Users.First().User.Id);
        
        // Can search him by Email
        search21ByEmail = await Sdk.SearchUsersGloballyAsync("user21");
        Assert.AreEqual(1, search21ByEmail.Users.Count);
        Assert.AreEqual(user21Id, search21ByEmail.Users.First().User.Id);
    }

    [TestMethod]
    public async Task SearchThreadsAsync()
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

        // Search the thread (Not found, because by default, the thread is private)
        var searchThread = await Sdk.SearchThreadsGloballyAsync("Patched_name");
        Assert.AreEqual(0, searchThread.Threads.Count);

        // Make the thread public
        await Sdk.UpdateThreadAsync(createdThread.NewThreadId, allowSearchByName: true);
        var searchThread2 = await Sdk.SearchThreadsGloballyAsync("Patched_name");
        Assert.AreEqual(1, searchThread2.Threads.Count);
        Assert.AreEqual(createdThread.NewThreadId, searchThread2.Threads.First().Id);
    }
}