using Aiursoft.Kahla.SDK.Models.Mapped;
using Aiursoft.Kahla.Server.Models.Entities;

namespace Aiursoft.Kahla.Server.Services.Mappers;

public static class ObjectMappers
{
    public static KahlaMessageMappedSentView Map(this MessageInDatabaseEntity messageInDatabaseEntity, KahlaUser? sender)
    {
        return new KahlaMessageMappedSentView
        {
            Id = messageInDatabaseEntity.Id,
            ThreadId = messageInDatabaseEntity.ThreadId,
            Content = messageInDatabaseEntity.Content,
            SendTime = messageInDatabaseEntity.CreationTime,
            Sender = sender == null ? null : new KahlaUserMappedPublicView
            {
                Id = sender.Id,
                NickName = sender.NickName,
                Bio = sender.Bio,
                IconFilePath = sender.IconFilePath,
                AccountCreateTime = sender.AccountCreateTime,
                EmailConfirmed = sender.EmailConfirmed,
                Email = sender.Email
            }
        };
    }
}