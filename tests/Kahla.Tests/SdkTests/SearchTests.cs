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
}