using System;
using System.Collections.Generic;

namespace WASender.Models;

public partial class Notification
{
    public ulong Id { get; set; }

    public ulong UserId { get; set; }

    public string? Title { get; set; }

    public string? Comment { get; set; }

    public string? Url { get; set; }

    public int Seen { get; set; }

    public int IsAdmin { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
