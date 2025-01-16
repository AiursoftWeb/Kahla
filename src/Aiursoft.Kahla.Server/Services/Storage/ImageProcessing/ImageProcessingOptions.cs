using System.ComponentModel.DataAnnotations;

namespace Aiursoft.Kahla.Server.Services.Storage.ImageProcessing;

/// <summary>
/// Options for the image processing functionality. 
/// Typically bound from configuration (e.g., appsettings.json).
/// </summary>
public class ImageProcessingOptions
{
    /// <summary>
    /// The root folder where original user files are stored, e.g. "kahla-data/Workspace".
    /// </summary>
    [Required]
    public string WorkspaceFolder { get; set; } = default!;

    /// <summary>
    /// The root folder for storing processed images, e.g. "kahla-data/ImageCompressor".
    /// </summary>
    [Required]
    public string ProcessedFolder { get; set; } = default!;
}