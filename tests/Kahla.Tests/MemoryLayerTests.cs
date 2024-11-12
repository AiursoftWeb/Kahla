using Aiursoft.CSTools.Tools;
using Aiursoft.DbTools;
using Aiursoft.Kahla.Server;
using Aiursoft.Kahla.Server.Data;
using Aiursoft.Kahla.Server.Models.Entities;
using Aiursoft.WebTools;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aiursoft.Kahla.Tests;

[TestClass]
public class MemoryLayerTests
{
    [TestMethod]
    public async Task LoadQuickMessagesTest()
    {
        var port = Network.GetAvailablePort();
        var server = await Extends.AppAsync<Startup>([], port: port);
        await server.UpdateDbAsync<KahlaRelationalDbContext>(UpdateMode.RecreateThenUse);
        var dbContext = server
            .Services
            .GetRequiredService<IServiceScopeFactory>()
            .CreateScope()
            .ServiceProvider
            .GetRequiredService<KahlaRelationalDbContext>();
        
        var arrayDbContext = server
            .Services
            .GetRequiredService<ArrayDbContext>();
        // Add a user.
        var user = new KahlaUser
        {
            Email = "test@domain.com",
            NickName = "Test",
        };
        await dbContext.Users.AddAsync(user);
        await dbContext.SaveChangesAsync();
        
        // Add a chat thread.
        var thread = new ChatThread
        {
            Name = "Test",
        };
        await dbContext.ChatThreads.AddAsync(thread);
        await dbContext.SaveChangesAsync();
        
        // Add the user to the thread.
        await dbContext.UserThreadRelations.AddAsync(new UserThreadRelation
        {
            UserId = user.Id,
            ThreadId = thread.Id,
        });

        arrayDbContext.AddMessage(new MessageInDatabaseEntity
        {
            MessageId = Guid.NewGuid(),
            ThreadId = thread.Id,
            SenderId = user.Id,
            Content = "Test",
            SendTime = DateTime.UtcNow
        });
        await dbContext.SaveChangesAsync();
        await server.Services.GetRequiredService<QuickMessageAccess>().LoadAsync();
    }
}