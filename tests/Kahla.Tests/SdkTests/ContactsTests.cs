using Aiursoft.AiurProtocol.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aiursoft.Kahla.Tests.SdkTests;

[TestClass]
public class ContactsTests : KahlaTestBase
{
    [TestMethod]
    public async Task AddMySelfAsContactTest()
    {
        // Register
        await _sdk.RegisterAsync("user12@domain.com", "password");
        
        // No contacts.
        var myContacts = await _sdk.MineAsync(take: 2);
        Assert.AreEqual(0, myContacts.KnownContacts.Count);

        // Search me.
        var searchResult = await _sdk.SearchEverythingAsync("user12");
        Assert.AreEqual(Code.ResultShown, searchResult.Code);
        Assert.AreEqual(1, searchResult.Users.Count);
        Assert.AreEqual("user12", searchResult.Users.First().User.NickName);
        Assert.AreEqual(false, searchResult.Users.First().IsKnownContact);
        
        // Add me as a contact.
        var addResult = await _sdk.AddContactAsync(searchResult.Users.First().User.Id);
        Assert.AreEqual(Code.JobDone, addResult.Code);

        // Add again will fail.
        try
        {
            await _sdk.AddContactAsync(searchResult.Users.First().User.Id);
            Assert.Fail();
        }
        catch
        {
            // ignored
        }

        // I should have one contact now.
        var myContacts2 = await _sdk.MineAsync(take: 2);
        Assert.AreEqual(1, myContacts2.KnownContacts.Count);
        Assert.AreEqual("user12", myContacts2.KnownContacts.First().User.NickName);
        Assert.AreEqual(true, myContacts2.KnownContacts.First().IsKnownContact);

        // Remove me as a contact.
        var removeResult = await _sdk.RemoveContactAsync(searchResult.Users.First().User.Id);
        Assert.AreEqual(Code.JobDone, removeResult.Code);
        
        // Remove again will fail.
        try
        {
            await _sdk.RemoveContactAsync(searchResult.Users.First().User.Id);
            Assert.Fail();
        }
        catch
        {
            // ignored
        }
        
        // I should have no contact now.
        var myContacts3 = await _sdk.MineAsync(take: 2);
        Assert.AreEqual(0, myContacts3.KnownContacts.Count);
    }

    [TestMethod]
    public async Task GetMyDetailsTest()
    {
        // Register
        await _sdk.RegisterAsync("user13@domain.com", "password");
        var searchResult = await _sdk.SearchEverythingAsync("user13");
        Assert.AreEqual(Code.ResultShown, searchResult.Code);
        Assert.AreEqual(1, searchResult.Users.Count);
        
        var details = await _sdk.UserDetailAsync(searchResult.Users.First().User.Id);
        Assert.AreEqual("user13", details.SearchedUser.User.NickName);
    }

    [TestMethod]
    public async Task ReportTwiceTest()
    {
        // Register bad guy.
        await _sdk.RegisterAsync("bad@domain.com", "password");
        
        // Register
        await _sdk.RegisterAsync("user14@domain.com", "password");
        
        // Search bad guy.
        var searchResult = await _sdk.SearchEverythingAsync("bad");
        
        // Report
        var reportResult = await _sdk.ReportUserAsync(searchResult.Users.First().User.Id, "reason1");
        Assert.AreEqual(Code.JobDone, reportResult.Code);
        
        // Report again should fail.
        try
        {
            await _sdk.ReportUserAsync(searchResult.Users.First().User.Id, "reason2");
            Assert.Fail();
        }
        catch
        {
            // ignored
        }
    }
    
}