using System;
using System.Collections.Generic;

namespace WASender.Models;

public partial class Smstransaction
{
    public ulong Id { get; set; }

    public ulong? UserId { get; set; }

    public ulong? DeviceId { get; set; }

    public ulong? AppId { get; set; }

    public ulong? TemplateId { get; set; }

    public string? From { get; set; }

    public string? To { get; set; }

    public string? Type { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual App? App { get; set; }

    public virtual Device? Device { get; set; }

    public virtual Template? Template { get; set; }

    public virtual User? User { get; set; }
}
