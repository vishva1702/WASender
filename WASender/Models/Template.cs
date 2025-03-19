using System;
using System.Collections.Generic;

namespace WASender.Models;

public partial class Template
{
    public ulong Id { get; set; }

    public Guid Uuid { get; set; }

    public ulong? UserId { get; set; }

    public string? Title { get; set; }

    public string? Body { get; set; }

    public string? Type { get; set; }

    public int Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Reply> Replies { get; set; } = new List<Reply>();

    public virtual ICollection<Schedulemessage> Schedulemessages { get; set; } = new List<Schedulemessage>();

    public virtual ICollection<Smstransaction> Smstransactions { get; set; } = new List<Smstransaction>();

    public virtual User? User { get; set; }
}
