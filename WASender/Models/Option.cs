using System;
using System.Collections.Generic;

namespace WASender.Models;

public partial class Option
{
    public ulong Id { get; set; }

    public string Key { get; set; } = null!;

    public string Value { get; set; } = null!;

    public string Lang { get; set; } = null!;
}
