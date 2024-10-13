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
        var myBlocks = await Sdk.MyBlocksAsync();
        Assert.AreEqual(0, myBlocks.TotalKnownBlocks);

        var me = await Sdk.MeAsync();
        var blockMySelf = await Sdk.BlockNewAsync(me.User.Id);
        Assert.AreEqual(Code.JobDone, blockMySelf.Code);
        
        var myBlocks2 = await Sdk.MyBlocksAsync();
        Assert.AreEqual(1, myBlocks2.TotalKnownBlocks);
        Assert.AreEqual(me.User.Id, myBlocks2.KnownBlocks.First().User.Id);
    }
}