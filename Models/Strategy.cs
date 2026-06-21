using System.Collections.Generic;

namespace ZapretUI.Models;

public sealed class Strategy
{
    public string Name { get; set; } = string.Empty;
    public List<string> Flags { get; set; } = [];
    public string Arguments { get; set; } = string.Empty;
}
