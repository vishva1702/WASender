using System;
using System.Collections.Generic;

namespace WASender.Models;

public partial class Group
{
    public ulong Id { get; set; }

    public ulong UserId { get; set; }

    public string? Name { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
