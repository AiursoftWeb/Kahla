using Aiursoft.AiurProtocol.Exceptions;
using Aiursoft.AiurProtocol.Models;
using Aiursoft.Kahla.Tests.TestBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aiursoft.Kahla.Tests.SdkTests;

[TestClass]
public class ContactsTests : KahlaTestBase
{
    [TestMethod]
    public async Task AddMySelfAsContactTest()
    {
        // Register
        await Sdk.RegisterAsync("user12@domain.com", "password");
        
        // No contacts.
        var myContacts = await Sdk.ListContactsAsync(take: 2);
        Assert.AreEqual(0, myContacts.KnownContacts.Count);

        // Search me.
        var searchResult = await Sdk.SearchUsersGloballyAsync("user12", excluding: null);
        Assert.AreEqual(Code.ResultShown, searchResult.Code);
        Assert.AreEqual(1, searchResult.Users.Count);
        Assert.AreEqual("user12", searchResult.Users.First().User.NickName);
        Assert.AreEqual(false, searchResult.Users.First().IsKnownContact);
        
        // Add me as a contact.
        var addResult = await Sdk.AddContactAsync(searchResult.Users.First().User.Id);
        Assert.AreEqual(Code.JobDone, addResult.Code);

        // Add again will fail.
        try
        {
            await Sdk.AddContactAsync(searchResult.Users.First().User.Id);
            Assert.Fail();
        }
        catch
        {
            // ignored
        }

        // I should have one contact now.
        var myContacts2 = await Sdk.ListContactsAsync(take: 2);
        Assert.AreEqual(1, myContacts2.KnownContacts.Count);
        Assert.AreEqual("user12", myContacts2.KnownContacts.First().User.NickName);
        Assert.AreEqual(true, myContacts2.KnownContacts.First().IsKnownContact);

        // Remove me as a contact.
        var removeResult = await Sdk.RemoveContactAsync(searchResult.Users.First().User.Id);
        Assert.AreEqual(Code.JobDone, removeResult.Code);
        
        // Remove again will fail.
        try
        {
            await Sdk.RemoveContactAsync(searchResult.Users.First().User.Id);
            Assert.Fail();
        }
        catch
        {
            // ignored
        }
        
        // I should have no contact now.
        var myContacts3 = await Sdk.ListContactsAsync(take: 2);
        Assert.AreEqual(0, myContacts3.KnownContacts.Count);
    }

    [TestMethod]
    public async Task GetMyDetailsTest()
    {
        // Register
        await Sdk.RegisterAsync("user13@domain.com", "password");
        var searchResult = await Sdk.SearchUsersGloballyAsync("user13", excluding: null);
        Assert.AreEqual(Code.ResultShown, searchResult.Code);
        Assert.AreEqual(1, searchResult.Users.Count);
        
        var details = await Sdk.UserDetailAsync(searchResult.Users.First().User.Id);
        Assert.AreEqual("user13", details.SearchedUser.User.NickName);
    }

    [TestMethod]
    public async Task ReportTwiceTest()
    {
        // Register bad guy.
        await Sdk.RegisterAsync("bad@domain.com", "password");
        
        // Register
        await Sdk.RegisterAsync("user14@domain.com", "password");
        
        // Search bad guy.
        var searchResult = await Sdk.SearchUsersGloballyAsync("bad", excluding: null);
        
        // Report
        var reportResult = await Sdk.ReportUserAsync(searchResult.Users.First().User.Id, "reason1");
        Assert.AreEqual(Code.JobDone, reportResult.Code);
        
        // Report again should fail.
        try
        {
            await Sdk.ReportUserAsync(searchResult.Users.First().User.Id, "reason2");
            Assert.Fail();
        }
        catch
        {
            // ignored
        }
    }

    [TestMethod]
    public async Task AddNonExistTest()
    {
        await Sdk.RegisterAsync("user18@domain.com", "password");
        try
        {
            await Sdk.AddContactAsync("non-exist-user-id");
            Assert.Fail();
        }
        catch (AiurUnexpectedServerResponseException e)
        {
            Assert.AreEqual("The target user does not exist.", e.Response.Message);
        }
    }

    [TestMethod]
    public async Task DetailsNonExistTest()
    {
        await Sdk.RegisterAsync("user19@domain.com", "password");
        try
        {
            await Sdk.UserDetailAsync("non-exist-user-id");
            Assert.Fail();
        }
        catch (AiurUnexpectedServerResponseException e)
        {
            Assert.AreEqual("The target user with id `non-exist-user-id` does not exist.", e.Response.Message);
        }
    }
}