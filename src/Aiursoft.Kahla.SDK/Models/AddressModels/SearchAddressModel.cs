﻿using System.ComponentModel.DataAnnotations;

namespace Aiursoft.Kahla.SDK.Models.AddressModels;

public class SearchAddressModel
{
    [Range(0, int.MaxValue)] public int Skip { get; init; }

    [Range(1, 50)] public int Take { get; init; } = 20;
    [MaxLength(50)] public string? SearchInput { get; init; }

    [MaxLength(50)] public string? Excluding { get; init; }
}