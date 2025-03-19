using System;
using System.Collections.Generic;

namespace WASender.Models;

public partial class Category
{
    public ulong Id { get; set; }

    public string Title { get; set; } = null!;

    public string? Slug { get; set; }

    public string Type { get; set; } = null!;

    public int Status { get; set; }

    public int IsFeatured { get; set; }

    public string Lang { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Categorymeta> Categorymeta { get; set; } = new List<Categorymeta>();
}
