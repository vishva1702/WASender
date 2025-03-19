using System;
using System.Collections.Generic;

namespace WASender.Models;

public partial class Reply
{
    public ulong Id { get; set; }

    public ulong UserId { get; set; }

    public ulong DeviceId { get; set; }

    public ulong? TemplateId { get; set; }

    public string? Keyword { get; set; }

    public string? Reply1 { get; set; }

    public string MatchType { get; set; } = null!;

    public string ReplyType { get; set; } = null!;

    public string? ApiKey { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Device Device { get; set; } = null!;

    public virtual Template? Template { get; set; }

    public virtual User User { get; set; } = null!;
}
