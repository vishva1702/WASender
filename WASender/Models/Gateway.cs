using System;
using System.Collections.Generic;

namespace WASender.Models;

public partial class Gateway
{
    public ulong Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Currency { get; set; }

    public string? Logo { get; set; }

    public double Charge { get; set; }

    public double Multiply { get; set; }

    public string Namespace { get; set; } = null!;

    public double MinAmount { get; set; }

    public double MaxAmount { get; set; }

    public int IsAuto { get; set; }

    public int? ImageAccept { get; set; }

    public int TestMode { get; set; }

    public int Status { get; set; }

    public int PhoneRequired { get; set; }

    public string? Data { get; set; }

    public string? Comment { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
