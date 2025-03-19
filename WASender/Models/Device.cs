using System;
using System.Collections.Generic;

namespace WASender.Models;

public partial class Device
{
    public ulong Id { get; set; }

    public Guid Uuid { get; set; }

    public ulong? UserId { get; set; }

    public string Name { get; set; } = null!;

    public string? Phone { get; set; }

    public string? UserName { get; set; }

    public string? Qr { get; set; }

    public string? Meta { get; set; }

    public string? HookUrl { get; set; }

    public int Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<App> Apps { get; set; } = new List<App>();

    public virtual ICollection<Reply> Replies { get; set; } = new List<Reply>();

    public virtual ICollection<Schedulemessage> Schedulemessages { get; set; } = new List<Schedulemessage>();

    public virtual ICollection<Smstesttransaction> Smstesttransactions { get; set; } = new List<Smstesttransaction>();

    public virtual ICollection<Smstransaction> Smstransactions { get; set; } = new List<Smstransaction>();

    public virtual User? User { get; set; }

    public virtual ICollection<Webhook> Webhooks { get; set; } = new List<Webhook>();
}
