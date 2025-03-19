using System;
using System.Collections.Generic;

namespace WASender.Models;

public partial class Webhook
{
    public ulong Id { get; set; }

    public ulong? DeviceId { get; set; }

    public ulong UserId { get; set; }

    public string? Payload { get; set; }

    public string? Hook { get; set; }

    public int Status { get; set; }

    public int? StatusCode { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Device? Device { get; set; }

    public virtual User User { get; set; } = null!;
}
