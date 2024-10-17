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

    public async Task<WriteApiAsync> GetWriteApiWithCache()
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
        await GetWriteApiWithCache();
    }
}