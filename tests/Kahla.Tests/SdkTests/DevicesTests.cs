using Aiursoft.AiurProtocol.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aiursoft.Kahla.Tests.SdkTests;

[TestClass]
public class DevicesTests : KahlaTestBase
{
    
    [TestMethod]
    public async Task GetMyDevices()
    {
        await _sdk.RegisterAsync("user6@domain.com", "password");
        var devices = await _sdk.MyDevicesAsync();
        Assert.AreEqual(0, devices.Items?.Count);
    }
    
    [TestMethod]
    public async Task AddAndGetMyDevices()
    {
        await _sdk.RegisterAsync("user7@domain.com", "password");
        var devices = await _sdk.MyDevicesAsync();
        Assert.AreEqual(0, devices.Items?.Count);
        
        await _sdk.AddDeviceAsync("device1", "auth", "endpoint://test_endpoint", "p256dh");
        var devices2 = await _sdk.MyDevicesAsync();
        Assert.AreEqual(1, devices2.Items?.Count);
        Assert.AreEqual("device1", devices2.Items?.First().Name);
    }

    [TestMethod]
    public async Task AddAndDropDevice()
    {
        await _sdk.RegisterAsync("user8@domain.com", "password");
        var devices = await _sdk.MyDevicesAsync();
        Assert.AreEqual(0, devices.Items?.Count);

        var addResponse = await _sdk.AddDeviceAsync("device1", "auth", "endpoint://test_endpoint", "p256dh");
        Assert.AreEqual(Code.JobDone, addResponse.Code);

        var devices2 = await _sdk.MyDevicesAsync();
        Assert.AreEqual(1, devices2.Items?.Count);

        var dropResponse = await _sdk.DropDeviceAsync(addResponse.Value);
        Assert.AreEqual(Code.JobDone, dropResponse.Code);

        var devices3 = await _sdk.MyDevicesAsync();
        Assert.AreEqual(0, devices3.Items?.Count);
    }

    [TestMethod]
    public async Task AddAndPatchDevice()
    {
        await _sdk.RegisterAsync("user9@domain.com", "password");
        var devices = await _sdk.MyDevicesAsync();
        Assert.AreEqual(0, devices.Items?.Count);

        var addResponse = await _sdk.AddDeviceAsync("device1", "auth", "endpoint://test_endpoint", "p256dh");
        Assert.AreEqual(Code.JobDone, addResponse.Code);

        var devices2 = await _sdk.MyDevicesAsync();
        Assert.AreEqual(1, devices2.Items?.Count);
        Assert.AreEqual("device1", devices2.Items?.First().Name);

        var patchResponse = await _sdk.UpdateDeviceAsync(addResponse.Value, "device2", "auth2",
            "endpoint://test_endpoint2", "p256dh2");
        Assert.AreEqual(Code.JobDone, patchResponse.Code);

        var devices3 = await _sdk.MyDevicesAsync();
        Assert.AreEqual(1, devices3.Items?.Count);
        Assert.AreEqual("device2", devices3.Items?.First().Name);
    }
    
    [TestMethod]
    public async Task PushTest()
    {
        await _sdk.RegisterAsync("user10@domain.com", "password");
        var addResponse = await _sdk.AddDeviceAsync("device1", "auth", "endpoint://test_endpoint", "p256dh");
        Assert.AreEqual(Code.JobDone, addResponse.Code);
        await _sdk.PushTestAsync();
    }
}