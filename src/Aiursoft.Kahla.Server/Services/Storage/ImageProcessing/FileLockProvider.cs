using System.Collections.Concurrent;

namespace Aiursoft.Kahla.Server.Services.Storage.ImageProcessing;

/// <summary>
/// Provides a thread-safe mechanism to lock on certain file paths 
/// so that concurrent read/write operations do not clash.
/// In a single-instance scenario, this can help avoid race conditions.
/// </summary>
public class FileLockProvider
{
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _lockDictionary = new();

    /// <summary>
    /// Retrieves or creates a lock object for the specified path. 
    /// Use this object in a 'lock' statement to ensure exclusive access.
    /// </summary>
    public SemaphoreSlim GetLock(string path)
    {
        return _lockDictionary.GetOrAdd(path, _ => new SemaphoreSlim(1, 1));
    }
}