using Aiursoft.AiurProtocol.Models;

namespace Aiursoft.Kahla.SDK.Models.ViewModels;

public class UploadViewModel : AiurResponse

{

    public required string FilePath { get; init; }

    public long FileSize { get; init; }

    public required string Krl { get; init; }

    public required string InternetOpenPath { get; init; }

    public required string InternetDownloadPath { get; init; }

}


