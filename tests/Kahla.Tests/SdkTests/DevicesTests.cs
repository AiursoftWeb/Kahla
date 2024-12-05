using Aiursoft.AiurProtocol.Models;
using Aiursoft.Kahla.Tests.TestBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aiursoft.Kahla.Tests.SdkTests;

[TestClass]
public class DevicesTests : KahlaTestBase
{
    
    [TestMethod]
    public async Task GetMyDevices()
    {
        await Sdk.RegisterAsync("user6@domain.com", "password");
        var devices = await Sdk.MyDevicesAsync();
        Assert.AreEqual(0, devices.Items?.Count);
    }
    
    [TestMethod]
    public async Task AddAndGetMyDevices()
    {
        await Sdk.RegisterAsync("user7@domain.com", "password");
        var devices = await Sdk.MyDevicesAsync();
        Assert.AreEqual(0, devices.Items?.Count);
        
        await Sdk.AddDeviceAsync("device1", "auth", "endpoint://test_endpoint", "p256dh");
        var devices2 = await Sdk.MyDevicesAsync();
        Assert.AreEqual(1, devices2.Items?.Count);
        Assert.AreEqual("device1", devices2.Items?.First().Name);
    }

    [TestMethod]
    public async Task AddAndDropDevice()
    {
        await Sdk.RegisterAsync("user8@domain.com", "password");
        var devices = await Sdk.MyDevicesAsync();
        Assert.AreEqual(0, devices.Items?.Count);

        var addResponse = await Sdk.AddDeviceAsync("device1", "auth", "endpoint://test_endpoint", "p256dh");
        Assert.AreEqual(Code.JobDone, addResponse.Code);

        var devices2 = await Sdk.MyDevicesAsync();
        Assert.AreEqual(1, devices2.Items?.Count);

        var dropResponse = await Sdk.DropDeviceAsync(addResponse.Value);
        Assert.AreEqual(Code.JobDone, dropResponse.Code);

        var devices3 = await Sdk.MyDevicesAsync();
        Assert.AreEqual(0, devices3.Items?.Count);
    }

    [TestMethod]
    public async Task AddAndPatchDevice()
    {
        await Sdk.RegisterAsync("user9@domain.com", "password");
        var devices = await Sdk.MyDevicesAsync();
        Assert.AreEqual(0, devices.Items?.Count);

        var addResponse = await Sdk.AddDeviceAsync("device1", "auth", "endpoint://test_endpoint", "p256dh");
        Assert.AreEqual(Code.JobDone, addResponse.Code);

        var devices2 = await Sdk.MyDevicesAsync();
        Assert.AreEqual(1, devices2.Items?.Count);
        Assert.AreEqual("device1", devices2.Items?.First().Name);

        var patchResponse = await Sdk.UpdateDeviceAsync(addResponse.Value, "device2", "auth2",
            "endpoint://test_endpoint2", "p256dh2");
        Assert.AreEqual(Code.JobDone, patchResponse.Code);

        var devices3 = await Sdk.MyDevicesAsync();
        Assert.AreEqual(1, devices3.Items?.Count);
        Assert.AreEqual("device2", devices3.Items?.First().Name);
    }
    
    [TestMethod]
    public async Task PushTest()
    {
        await Sdk.RegisterAsync("user10@domain.com", "password");
        var addResponse = await Sdk.AddDeviceAsync("device1", "auth", "endpoint://test_endpoint", "p256dh");
        Assert.AreEqual(Code.JobDone, addResponse.Code);
        await Sdk.PushTestAsync();
        
        // My device is not a real device, so it will disappear after the test.
        var devices = await Sdk.MyDevicesAsync();
        Assert.AreEqual(0, devices.Items?.Count);
    }
}