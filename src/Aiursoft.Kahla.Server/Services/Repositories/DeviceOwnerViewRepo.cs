using Aiursoft.Kahla.SDK.Models.Mapped;
using Aiursoft.Kahla.Server.Data;
using Aiursoft.Kahla.Server.Services.Mappers;
using Microsoft.EntityFrameworkCore;

namespace Aiursoft.Kahla.Server.Services.Repositories;

public class DeviceOwnerViewRepo(
    KahlaRelationalDbContext relationalDbContext)
{
    public IOrderedQueryable<DeviceMappedOwnerView> SearchDevicesIOwn(string userId)
    {
        return relationalDbContext
            .Devices
            .AsNoTracking()
            .Where(t => t.OwnerId == userId)
            .MapDevicesOwnedView()
            .OrderByDescending(t => t.AddTime);
    }
}