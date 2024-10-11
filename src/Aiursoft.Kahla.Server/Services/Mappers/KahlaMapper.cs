using Aiursoft.Kahla.Server.Data;
using Microsoft.Extensions.Caching.Memory;

namespace Aiursoft.Kahla.Server.Services.Mappers;

public class KahlaMapper(
    ILogger<KahlaMapper> logger,
    KahlaDbContext dbContext,
    IMemoryCache memoryCache)
{
   
}