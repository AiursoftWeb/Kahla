using InfluxDB.Client;

namespace Aiursoft.Kahla.Server.Data;

/// <summary>
/// A client to interact with InfluxDB.
/// </summary>
/// <param name="connectionString">The connection string to the InfluxDB server. The format is like: http://localhost:8086;influx_user;influx_password;kahla;messages</param>
public class InfluxDbClient(string connectionString)
{
    public string Host { get; } = connectionString.Split(';')[0];
    public string User { get; } = connectionString.Split(';')[1];
    public string Password { get; } = connectionString.Split(';')[2];
    public string Org { get; } = connectionString.Split(';')[3];
    public string Bucket { get; } = connectionString.Split(';')[4];
    
    private WriteApiAsync? _cachedWriteApi;
    private QueryApi? _cachedQueryApi;

    private async Task<WriteApiAsync> GetWriteApiInternal()
    {
        var client = new InfluxDBClient(Host, User, Password);
        var bucketsApi = client.GetBucketsApi();
        var bucketExists = await bucketsApi.FindBucketByNameAsync(Bucket);
        if (bucketExists == null)
        {
            var orgs = await client.GetOrganizationsApi().FindOrganizationsAsync(org: Org);
            await bucketsApi.CreateBucketAsync(Bucket, orgs.First().Id);
        }
        return client.GetWriteApiAsync();
    }

    public async Task<WriteApiAsync> GetWriteApi()
    {
        return _cachedWriteApi ??= await GetWriteApiInternal();
    }
    
    private Task<QueryApi> GetQueryApiInternal()
    {
        var client = new InfluxDBClient(Host, User, Password);
        return Task.FromResult(client.GetQueryApi());
    }
    
    public async Task<QueryApi> GetQueryApi()
    {
        return _cachedQueryApi ??= await GetQueryApiInternal();
    }
    
    public async Task EnsureDatabaseCreatedAsync()
    {
        await GetWriteApi();

        // Sample insert
        //for (int i = 0; i < 10000; i++)
        // {
        //     var entity = new MessageInfluxInsertingEntity
        //     {
        //         InnerContent = "Hello, world!",
        //         MessageId = Guid.NewGuid(),
        //         SenderId = Guid.NewGuid(),
        //         ThreadId = 123,
        //         SendTime = DateTime.UtcNow
        //     };
        //     var point = PointData.Measurement(nameof(MessageInfluxInsertingEntity))
        //         .Tag(nameof(MessageInfluxInsertingEntity.ThreadId), entity.ThreadId.ToString())
        //         .Tag(nameof(MessageInfluxInsertingEntity.SenderId), entity.SenderId.ToString())
        //         .Field(nameof(MessageInfluxReadingEntity.Content), entity.ToInfluxField())
        //         .Timestamp(entity.SendTime, WritePrecision.Ns);
        //     await writeApi.WritePointAsync(point, Bucket, Org);
        // }
        //
        // Sample query
        // var queryApi = await GetQueryApi();
        // var query = $"from(bucket: \"{Bucket}\") |> range(start: -1h) |> filter(fn: (r) => r._measurement == \"{nameof(MessageInfluxInsertingEntity)}\")";
        // var tables = await queryApi.QueryAsync<MessageInfluxReadingEntity>(query, Org);
    }
}