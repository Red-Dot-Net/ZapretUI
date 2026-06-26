using System;
using System.Collections.Generic;

namespace ZapretUI.Models;

public class ExternalLibraryResources
{
    public string SelectedStrategyName { get; set; } = string.Empty;
    public string CurrentSourceVersion { get; set; } = string.Empty;
    public string BestStrategy { get; set; } = string.Empty;
    public List<Strategy> Strategies { get; set; } = [];
}
