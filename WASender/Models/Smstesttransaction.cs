using System;
using System.Collections.Generic;

namespace WASender.Models;

public partial class Smstesttransaction
{
    public ulong Id { get; set; }

    public ulong? UserId { get; set; }

    public ulong? AppId { get; set; }

    public ulong? DeviceId { get; set; }

    public string? Phone { get; set; }

    public string? Body { get; set; }

    public double? Charge { get; set; }

    public string? MessagingId { get; set; }

    public int? StatusCode { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual App? App { get; set; }

    public virtual Device? Device { get; set; }

    public virtual User? User { get; set; }
}
