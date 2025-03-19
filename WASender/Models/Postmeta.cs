using System;
using System.Collections.Generic;

namespace WASender.Models;

public partial class Postmeta
{
    public ulong Id { get; set; }

    public ulong PostId { get; set; }

    public string Key { get; set; } = null!;

    public string Value { get; set; } = null!;

    public virtual Post Post { get; set; } = null!;
}
