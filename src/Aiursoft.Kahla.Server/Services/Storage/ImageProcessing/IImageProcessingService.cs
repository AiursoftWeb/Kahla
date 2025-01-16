namespace Aiursoft.Kahla.Server.Services.Storage.ImageProcessing;

/// <summary>
/// Defines the contract for an image processing service, 
/// including EXIF clearing and optional resizing.
/// </summary>
public interface IImageProcessingService
{
    /// <summary>
    /// Clears the EXIF data from the specified image and writes the output to a 
    /// "ClearedEXIF" subdirectory (while retaining the same subfolder structure).
    /// </summary>
    /// <param name="relativePath">
    /// A path relative to the WorkspaceFolder (e.g., "thread-files/1/2025/01/15/foo.jpg").
    /// </param>
    /// <returns>
    /// The path *relative to the ProcessedFolder* for the newly created EXIF-cleared image.
    /// </returns>
    Task<string> ClearExifAsync(string relativePath);

    /// <summary>
    /// Compresses an image by resizing it to the specified width and/or height, and writes
    /// the output to a "Compressed" subdirectory (while retaining the same subfolder structure).
    /// Also clears EXIF data automatically.
    /// </summary>
    /// <param name="relativePath">
    /// A path relative to the WorkspaceFolder (e.g., "thread-files/1/2025/01/15/foo.jpg").
    /// </param>
    /// <param name="width">Target width (0 means no constraint on width).</param>
    /// <param name="height">Target height (0 means no constraint on height).</param>
    /// <returns>
    /// The path *relative to the ProcessedFolder* for the newly created compressed image.
    /// </returns>
    Task<string> CompressAsync(string relativePath, int width, int height);
}