using Aiursoft.AiurProtocol.Exceptions;
using Aiursoft.Kahla.Tests.TestBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aiursoft.Kahla.Tests.SdkTests;

[TestClass]
public class AuthTests : KahlaTestBase
{
    [TestMethod]
    public async Task TestServerInfo()
    {
        var home = await Sdk.ServerInfoAsync();
        Assert.AreEqual("Your Server Name", home.ServerName);
    }

    [TestMethod]
    public async Task Register_Signout_SignIn()
    {
        await Sdk.RegisterAsync("user1@domain.com", "password");
        await Sdk.SignoutAsync();
        await Sdk.SignInAsync("user1@domain.com", "password");
    }

    [TestMethod]
    public async Task SignInInvalid()
    {
        await Sdk.RegisterAsync("userz@domain.com", "password");
        await Sdk.SignoutAsync();
        try
        {
            await Sdk.SignInAsync("userz@domain.com", "badzzzzzzz");
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
        await Sdk.RegisterAsync("user2@domain.com", "password");
        try
        {
            await Sdk.SignInAsync("zzzzzzzz@domain.com", "password");
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
        await Sdk.RegisterAsync("user11@domain.com", "password");
        try
        {
            await Sdk.ChangePasswordAsync("bad_password", "useless_string");
            Assert.Fail();
        }
        catch (AiurUnexpectedServerResponseException e)
        {
            Assert.AreEqual("Incorrect password.", e.Response.Message);
        }
        
        await Sdk.ChangePasswordAsync("password", "newpassword");
        await Sdk.SignoutAsync();
        try
        {
            await Sdk.SignInAsync("user11@domain.com", "password");
            Assert.Fail();
        }
        catch (AiurUnexpectedServerResponseException e)
        {
            Assert.AreEqual("Invalid login attempt! Please check your email and password.", e.Response.Message);
        }
        
        await Sdk.SignInAsync("user11@domain.com", "newpassword");
    }

    [TestMethod]
    public async Task DuplicateRegister()
    {
        await Sdk.RegisterAsync("anduin@aiursoft.com", "password");
        try
        {
            await Sdk.RegisterAsync("anduin@aiursoft.com", "password");
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
        await Sdk.RegisterAsync("user3@domain.com", "password");
        var me = await Sdk.MeAsync();
        Assert.AreEqual("user3", me.User.NickName);
        Assert.AreEqual("user3@domain.com", me.User.Email);
    }
    
    [TestMethod]
    public async Task GetMyInfoUnauthorized()
    {
        await Sdk.RegisterAsync("user4@domain.com", "password");
        await Sdk.SignoutAsync();
        
        try
        {
            await Sdk.MeAsync();
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
        await Sdk.RegisterAsync("user5@domain.com", "password");
        var me = await Sdk.MeAsync();
        Assert.AreEqual("user5", me.User.NickName);

        await Sdk.UpdateMeAsync(themeId: 1, listInSearchResult: false, nickName: "new nick name!");
        var me2 = await Sdk.MeAsync();
        Assert.AreEqual("new nick name!", me2.User.NickName);
        Assert.AreEqual(1, me2.PrivateSettings.ThemeId);
        Assert.IsFalse(me2.PrivateSettings.AllowSearchByName);
    }
}