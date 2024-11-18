using System.ComponentModel.DataAnnotations;

namespace Aiursoft.Kahla.SDK.Models.AddressModels;

public class MyThreadsAddressModel
{
    [Range(0, int.MaxValue)] public int? SkipTillThreadId { get; init; }

    [Range(1, 50)] public int Take { get; init; } = 20;
}