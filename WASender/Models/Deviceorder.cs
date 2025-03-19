using System;
using System.Collections.Generic;

namespace WASender.Models;

public partial class Deviceorder
{
    public ulong Id { get; set; }

    public string? Trx { get; set; }

    public double? Amount { get; set; }

    public ulong? UserId { get; set; }

    public int Phone { get; set; }

    public string Status { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual User? User { get; set; }
}
