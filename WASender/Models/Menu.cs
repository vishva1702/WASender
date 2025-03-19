using System;
using System.Collections.Generic;

namespace WASender.Models;

public partial class Menu
{
    public ulong Id { get; set; }

    public string Name { get; set; } = null!;

    public string Position { get; set; } = null!;

    public string Data { get; set; } = null!;

    public string Lang { get; set; } = null!;

    public string Status { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
