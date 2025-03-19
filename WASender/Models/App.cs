using System;
using System.Collections.Generic;

namespace WASender.Models;

public partial class App
{
    public ulong Id { get; set; }

    public Guid Uuid { get; set; }

    public string Key { get; set; } = null!;

    public ulong? UserId { get; set; }

    public ulong? DeviceId { get; set; }

    public string? Title { get; set; }

    public string? Website { get; set; }

    public int Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Device? Device { get; set; }

    public virtual ICollection<Smstesttransaction> Smstesttransactions { get; set; } = new List<Smstesttransaction>();

    public virtual ICollection<Smstransaction> Smstransactions { get; set; } = new List<Smstransaction>();

    public virtual User? User { get; set; }
}
