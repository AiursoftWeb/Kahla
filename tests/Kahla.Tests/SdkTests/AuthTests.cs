using Aiursoft.AiurProtocol.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aiursoft.Kahla.Tests.SdkTests;

[TestClass]
public class AuthTests : KahlaTestBase
{
    [TestMethod]
    public async Task TestServerInfo()
    {
        var home = await _sdk.ServerInfoAsync();
        Assert.AreEqual("Your Server Name", home.ServerName);
    }

    [TestMethod]
    public async Task Register_Signout_SignIn()
    {
        await _sdk.RegisterAsync("user1@domain.com", "password");
        await _sdk.SignoutAsync();
        await _sdk.SignInAsync("user1@domain.com", "password");
    }

    [TestMethod]
    public async Task SignInInvalid()
    {
        await _sdk.RegisterAsync("userz@domain.com", "password");
        await _sdk.SignoutAsync();
        try
        {
            await _sdk.SignInAsync("userz@domain.com", "badzzzzzzz");
            Assert.Fail();
        }
        catch (AiurUnexpectedServerResponseException e)
        {
            Assert.AreEqual("Invalid login attempt! Please check your email and password.", e.Response.Message);
        }
    }

    [TestMethod]
    public async Task SignInWhileSignedIn()
    {
        await _sdk.RegisterAsync("user2@domain.com", "password");
        try
        {
            await _sdk.SignInAsync("zzzzzzzz@domain.com", "password");
            Assert.Fail();
        }
        catch (AiurUnexpectedServerResponseException e)
        {
            Assert.AreEqual("You are already signed in!", e.Response.Message);
        }
    }
    
    [TestMethod]
    public async Task SignIn_ChangePassword_SignIn()
    {
        await _sdk.RegisterAsync("user11@domain.com", "password");
        try
        {
            await _sdk.ChangePasswordAsync("bad_password", "useless_string");
            Assert.Fail();
        }
        catch (AiurUnexpectedServerResponseException e)
        {
            Assert.AreEqual("Incorrect password.", e.Response.Message);
        }
        
        await _sdk.ChangePasswordAsync("password", "newpassword");
        await _sdk.SignoutAsync();
        try
        {
            await _sdk.SignInAsync("user11@domain.com", "password");
            Assert.Fail();
        }
        catch (AiurUnexpectedServerResponseException e)
        {
            Assert.AreEqual("Invalid login attempt! Please check your email and password.", e.Response.Message);
        }
        
        await _sdk.SignInAsync("user11@domain.com", "newpassword");
    }

    [TestMethod]
    public async Task DuplicateRegister()
    {
        await _sdk.RegisterAsync("anduin@aiursoft.com", "password");
        try
        {
            await _sdk.RegisterAsync("anduin@aiursoft.com", "password");
            Assert.Fail();
        }
        catch (AiurUnexpectedServerResponseException e)
        {
            Assert.AreEqual("Username 'anduin@aiursoft.com' is already taken.", e.Response.Message);
        }
    }

    [TestMethod]
    public async Task GetMyInfo()
    {
        await _sdk.RegisterAsync("user3@domain.com", "password");
        var me = await _sdk.MeAsync();
        Assert.AreEqual("user3", me.User.NickName);
        Assert.AreEqual("user3@domain.com", me.User.Email);
    }
    
    [TestMethod]
    public async Task GetMyInfoUnauthorized()
    {
        await _sdk.RegisterAsync("user4@domain.com", "password");
        await _sdk.SignoutAsync();
        
        try
        {
            await _sdk.MeAsync();
            Assert.Fail();
        }
        catch (AiurUnexpectedServerResponseException e)
        {
            Assert.IsTrue(e.Message.StartsWith("You are unauthorized to access this API."));
        }
    }

    [TestMethod]
    public async Task PatchMyInfo()
    {
        await _sdk.RegisterAsync("user5@domain.com", "password");
        var me = await _sdk.MeAsync();
        Assert.AreEqual("user5", me.User.NickName);

        await _sdk.UpdateMeAsync(themeId: 1, listInSearchResult: false, nickName: "new nick name!");
        var me2 = await _sdk.MeAsync();
        Assert.AreEqual("new nick name!", me2.User.NickName);
        Assert.AreEqual(1, me2.PrivateSettings.ThemeId);
        Assert.IsFalse(me2.PrivateSettings.AllowSearchByName);
    }
}