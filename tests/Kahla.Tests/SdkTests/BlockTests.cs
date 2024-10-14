using Aiursoft.AiurProtocol.Exceptions;
using Aiursoft.AiurProtocol.Models;
using Aiursoft.Kahla.Tests.TestBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aiursoft.Kahla.Tests.SdkTests;

[TestClass]
public class BlockTests : KahlaTestBase
{
    [TestMethod]
    public async Task BlockAndUnblock()
    {
        await Sdk.RegisterAsync("user15@domain.com", "password");
        var myBlocks = await Sdk.ListBlocksAsync();
        Assert.AreEqual(0, myBlocks.TotalKnownBlocks);

        var me = await Sdk.MeAsync();
        var blockMySelf = await Sdk.BlockNewAsync(me.User.Id);
        Assert.AreEqual(Code.JobDone, blockMySelf.Code);
        
        var myBlocks2 = await Sdk.ListBlocksAsync();
        Assert.AreEqual(1, myBlocks2.TotalKnownBlocks);
        Assert.AreEqual(me.User.Id, myBlocks2.KnownBlocks.First().User.Id);

        try
        {
            await Sdk.BlockNewAsync(me.User.Id);
            Assert.Fail();
        }
        catch (AiurUnexpectedServerResponseException e)
        {
            Assert.AreEqual("The target user is already in your block list.", e.Response.Message);
        }
        
        var unblockResult = await Sdk.UnblockAsync(me.User.Id);
        Assert.AreEqual(Code.JobDone, unblockResult.Code);
        
        var myBlocks3 = await Sdk.ListBlocksAsync();
        Assert.AreEqual(0, myBlocks3.TotalKnownBlocks);
    }

    [TestMethod]
    public async Task BlockNonExist()
    {
        await Sdk.RegisterAsync("user16@domain.com", "password");
        try
        {
            await Sdk.BlockNewAsync("non-exist-user-id");
            Assert.Fail();
        }
        catch (AiurUnexpectedServerResponseException e)
        {
            Assert.AreEqual("The target user does not exist.", e.Response.Message);
        }
    }
    
    [TestMethod]
    public async Task RemoveBlockNonExist()
    {
        await Sdk.RegisterAsync("user17@domain.com", "password");
        try
        {
            await Sdk.UnblockAsync("non-exist-user-id");
            Assert.Fail();
        }
        catch (AiurUnexpectedServerResponseException e)
        {
            Assert.AreEqual("The target user is not in your block list.", e.Response.Message);
        }
    }
}