using Aiursoft.AiurProtocol.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aiursoft.Kahla.Tests.SdkTests;

[TestClass]
public class BlockTests : KahlaTestBase
{
    [TestMethod]
    public async Task BlockAndUnblock()
    {
        await _sdk.RegisterAsync("user15@domain.com", "password");
        var myBlocks = await _sdk.MyBlocksAsync();
        Assert.AreEqual(0, myBlocks.TotalKnownBlocks);

        var me = await _sdk.MeAsync();
        var blockMySelf = await _sdk.BlockNewAsync(me.User.Id);
        Assert.AreEqual(Code.JobDone, blockMySelf.Code);
        
        var myBlocks2 = await _sdk.MyBlocksAsync();
        Assert.AreEqual(1, myBlocks2.TotalKnownBlocks);
        Assert.AreEqual(me.User.Id, myBlocks2.KnownBlocks.First().User.Id);
    }
}