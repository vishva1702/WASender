using System;
using System.Collections.Generic;

namespace WASender.Models;

public partial class Post
{
    public ulong Id { get; set; }

    public string Title { get; set; } = null!;

    public string? Slug { get; set; }

    public string Type { get; set; } = null!;

    public int Status { get; set; }

    public int Featured { get; set; }

    public string Lang { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Postmeta> Postmetas { get; set; } = new List<Postmeta>();
}
