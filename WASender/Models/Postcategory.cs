using System;
using System.Collections.Generic;

namespace WASender.Models;

public partial class Postcategory
{
    public ulong PostId { get; set; }

    public ulong CategoryId { get; set; }

    public virtual Category Category { get; set; } = null!;

    public virtual Post Post { get; set; } = null!;
}
