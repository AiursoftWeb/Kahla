using Aiursoft.AiurProtocol.Exceptions;
using Aiursoft.AiurProtocol.Models;
using Aiursoft.Kahla.SDK.Models.AddressModels;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;

namespace Aiursoft.Kahla.Server.Controllers;

public class FilesController(ILogger<FilesController> logger)
    : ControllerBase
{
    [Route("File/{ThreadId}/{**FolderNames}", Name = "File")]
    [Route("Open/{ThreadId}/{**FolderNames}", Name = "Open")]
    public async Task<IActionResult> Open(OpenAddressModel model)
    {
        // Ensure he joined this thread.
        // If not, return Unauthorized.

        var (folders, fileName) = _folderSplitter.SplitToFoldersAndFile(model.FolderNames);
        try
        {
            var siteRoot = await _folderRepo.GetFolderFromId(site.RootFolderId);
            var folder = await _folderRepo.GetFolderFromPath(folders, siteRoot, false);
            if (folder == null)
            {
                return NotFound();
            }

            var file = await _fileRepo.GetFileInFolder(folder, fileName);
            if (file == null)
            {
                return NotFound();
            }

            var path = _storageProvider.GetFilePath(file.HardwareId);
            var extension = _storageProvider.GetExtension(file.FileName);
            if (ControllerContext.ActionDescriptor.AttributeRouteInfo?.Name == "File")
            {
                return this.WebFile(path, "do-not-open");
            }

            if (file.FileName.IsStaticImage() && await IsValidImageAsync(path))
            {
                return await FileWithImageCompressor(path, extension);
            }

            return this.WebFile(path, extension);
        }
        catch (AiurUnexpectedServerResponseException e) when (e.Response.Code == Code.NotFound)
        {
            return NotFound();
        }
    }
    
    private async Task<bool> IsValidImageAsync(string imagePath)
    {
        try
        {
            _ = await Image.DetectFormatAsync(imagePath);
            logger.LogTrace("File with path {ImagePath} is an valid image", imagePath);
            return true;
        }
        catch (Exception e)
        {
            logger.LogWarning(e, "File with path {ImagePath} is not an valid image", imagePath);
            return false;
        }
    }
}