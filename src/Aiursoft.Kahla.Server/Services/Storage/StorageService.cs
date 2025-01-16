namespace Aiursoft.Kahla.Server.Services.Storage;

/// <summary>
/// Represents a service for storing and retrieving files.
/// </summary>
public class StorageService
{
    private readonly string _workspaceFolder;

    // Async lock for creating unique file names if a file already exists.
    private readonly SemaphoreSlim _uniqueFileCreationLock = new(1, 1);

    public StorageService(IConfiguration configuration)
    {
        // Example: "Storage:Path" points to "kahla-data"
        // Then we place the workspace folder under that path, named "Workspace".
        var basePath = configuration["Storage:Path"]
                       ?? throw new InvalidDataException("Missing config 'Storage:Path'!");
        _workspaceFolder = Path.Combine(basePath, "Workspace");

        if (!Directory.Exists(_workspaceFolder))
        {
            Directory.CreateDirectory(_workspaceFolder);
        }
    }

    /// <summary>
    /// Saves a file to the storage.
    /// </summary>
    /// <param name="saveRelativePath">The relative path (relative to the workspace) the user wants to save.</param>
    /// <param name="file">The file to be saved.</param>
    /// <returns>The actual path (relative to the workspace) where the file is saved.</returns>
    public async Task<string> Save(string saveRelativePath, IFormFile file)
    {
        var finalFilePath = Path.Combine(_workspaceFolder, saveRelativePath);
        var finalFolder = Path.GetDirectoryName(finalFilePath);

        // Create the folder if it does not exist.
        if (!Directory.Exists(finalFolder))
        {
            Directory.CreateDirectory(finalFolder!);
        }

        // The problem is: what if the file already exists?
        await _uniqueFileCreationLock.WaitAsync();
        try
        {
            var expectedFileName = Path.GetFileName(finalFilePath);
            while (File.Exists(finalFilePath))
            {
                // If file exists, prepend an underscore to avoid overwriting.
                expectedFileName = "_" + expectedFileName;
                finalFilePath = Path.Combine(finalFolder!, expectedFileName);
            }

            // Create a new empty file, ensuring it won't be overwritten.
            File.Create(finalFilePath).Close();
        }
        finally
        {
            _uniqueFileCreationLock.Release();
        }

        // Write content to the newly created file.
        await using var fileStream = new FileStream(finalFilePath, FileMode.Create);
        await file.CopyToAsync(fileStream);
        fileStream.Close();

        // Return the path that is relative to the workspace.
        return Path.GetRelativePath(_workspaceFolder, finalFilePath);
    }

    /// <summary>
    /// Get the physical path on disk for a file stored relative to the workspace.
    /// </summary>
    public string GetFilePhysicalPath(string relativePath)
    {
        return Path.Combine(_workspaceFolder, relativePath);
    }

    /// <summary>
    /// Convert a relative path to a URL-friendly path string.
    /// </summary>
    public string RelativePathToUriPath(string relativePath)
    {
        var urlPath = Uri.EscapeDataString(relativePath)
            .Replace("%5C", "/")
            .Replace("%5c", "/")
            .Replace("%2F", "/")
            .Replace("%2f", "/")
            .TrimStart('/');
        return urlPath;
    }
}