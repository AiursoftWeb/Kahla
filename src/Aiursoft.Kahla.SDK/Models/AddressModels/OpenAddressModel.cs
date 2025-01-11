using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace Aiursoft.Kahla.SDK.Models.AddressModels;

public class OpenAddressModel
{
    [FromRoute] [Required] public int ThreadId { get; init; }

    [FromRoute] [Required] public required string FolderNames { get; init; }
}