// using System.Collections.Concurrent;
// using Aiursoft.Kahla.Server.Data;
// using Aiursoft.Kahla.Server.Data.Models;
// using InfluxDB.Client.Api.Domain;
// using InfluxDB.Client.Writes;
//
// namespace Aiursoft.Kahla.Server.Services.Repositories;
//
// public class MessagesRepository(
//     ILogger<MessagesRepository> logger,
//     InfluxDbClient influxDbClient)
// {
//     private readonly ConcurrentQueue<MessageInfluxInsertingEntity> _insertQueue = new();
//     private Task _engine = Task.CompletedTask;
//
//     // Sample query
//     // var queryApi = await GetQueryApi();
//     // var query = $"from(bucket: \"{Bucket}\") |> range(start: -1h) |> filter(fn: (r) => r._measurement == \"{nameof(MessageInfluxInsertingEntity)}\")";
//     // var tables = await queryApi.QueryAsync<MessageInfluxReadingEntity>(query, Org);
//     public async Task<List<T>> QueryAsync<T>(string query)
//     {
//         var queryApi = await influxDbClient.GetQueryApi();
//         var tables = await queryApi.QueryAsync<T>(query, influxDbClient.Org);
//         return tables.ToList();
//     }
//     
//     public void InsertNewMessage(MessageInfluxInsertingEntity entity, bool startTheEngine = true, int maxDegreeOfBulk = 128)
//     {
//         _insertQueue.Enqueue(entity);
//         if (!startTheEngine)
//         {
//             return;
//         }
//
//         lock (this)
//         {
//             if (!_engine.IsCompleted)
//             {
//                 return;
//             }
//
//             _engine = BulkInsertInQueue(maxDegreeOfBulk);
//             logger.LogDebug("Engine is sleeping. Trying to wake it up...");
//         }
//     }
//
//     private async Task BulkInsertInQueue(int maxDegreeOfBulk)
//     {
//         try
//         {
//             var writeApi = await influxDbClient.GetWriteApiWithCache();
//             while (!_insertQueue.IsEmpty)
//             {
//                 var batch = new List<MessageInfluxInsertingEntity>();
//                 for (int i = 0; i < maxDegreeOfBulk && _insertQueue.TryDequeue(out var entity); i++)
//                 {
//                     batch.Add(entity);
//                 }
//
//                 var bulkPoints = batch.Select(entity =>
//                 {
//                     var point = PointData.Measurement(nameof(MessageInfluxInsertingEntity))
//                         .Tag(nameof(MessageInfluxInsertingEntity.ThreadId), entity.ThreadId.ToString())
//                         .Tag(nameof(MessageInfluxInsertingEntity.SenderId), entity.SenderId.ToString())
//                         .Field(nameof(MessageInfluxReadingEntity.Content), entity.ToInfluxField())
//                         .Timestamp(entity.SendTime, WritePrecision.Ns);
//                     return point;
//                 }).ToList();
//
//                 if (bulkPoints.Any())
//                 {
//                     logger.LogInformation("Bulk inserting {Count} messages to InfluxDB.", bulkPoints.Count);
//                     await writeApi.WritePointsAsync(bulkPoints, influxDbClient.Bucket, influxDbClient.Org);
//                 }
//             }
//
//             logger.LogInformation("All messages inserted to InfluxDB. Engine is sleeping.");
//         }
//         catch (Exception e)
//         {
//             logger.LogCritical(e, "An error occurred when inserting messages to InfluxDB.");
//         }
//     }
// }