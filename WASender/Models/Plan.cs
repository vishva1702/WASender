using System;
using System.Collections.Generic;

namespace WASender.Models;

public partial class Plan
{
    public ulong Id { get; set; }

    public string? Title { get; set; }

    public string? Labelcolor { get; set; }

    public string? Iconname { get; set; }

    public double? Price { get; set; }

    public int? IsFeatured { get; set; } 

    public int? IsRecommended { get; set; }

    public int? IsTrial { get; set; }

    public int? Status { get; set; }

    public int? Days { get; set; }

    public int? TrialDays { get; set; }

    public string? Data { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}