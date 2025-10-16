using Aiursoft.Kahla.Tests.TestBase;

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
        Assert.HasCount(1, search21ById.Users);
        Assert.AreEqual(user21Id, search21ById.Users.First().User.Id);
        
        // Can not search him by Email
        var search21ByEmail = await Sdk.SearchUsersGloballyAsync("user21");
        Assert.IsEmpty(search21ByEmail.Users);
        
        // Login user21
        await Sdk.SignoutAsync();
        await Sdk.SignInAsync("user21@domain.com", "password");
        await Sdk.UpdateMeAsync(listInSearchResult: true);
        await Sdk.SignoutAsync();
        
        // Login as user22
        await Sdk.SignInAsync("user22@domain.com", "password");

        // Can search him by id
        search21ById = await Sdk.SearchUsersGloballyAsync(user21Id);
        Assert.HasCount(1, search21ById.Users);
        Assert.AreEqual(user21Id, search21ById.Users.First().User.Id);
        
        // Can search him by Email
        search21ByEmail = await Sdk.SearchUsersGloballyAsync("user21");
        Assert.HasCount(1, search21ByEmail.Users);
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
        Assert.IsEmpty(searchThread.Threads);

        // Make the thread public
        await Sdk.UpdateThreadAsync(createdThread.NewThreadId, allowSearchByName: true);
        var searchThread2 = await Sdk.SearchThreadsGloballyAsync("Patched_name");
        Assert.HasCount(1, searchThread2.Threads);
        Assert.AreEqual(createdThread.NewThreadId, searchThread2.Threads.First().Id);
    }
}