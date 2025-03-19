using System;
using System.Collections.Generic;

namespace WASender.Models;

public partial class Categorymeta
{
    public ulong Id { get; set; }

    public ulong CategoryId { get; set; }

    public string Type { get; set; } = null!;

    public string Value { get; set; } = null!;

    public virtual Category Category { get; set; } = null!;
}
