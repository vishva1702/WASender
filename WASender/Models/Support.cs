using System;
using System.Collections.Generic;

namespace WASender.Models;

public partial class Support
{
    public ulong Id { get; set; }

    public int TicketNo { get; set; }

    public ulong? UserId { get; set; }

    public string? Subject { get; set; }

    public int Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Supportlog> Supportlogs { get; set; } = new List<Supportlog>();

    public virtual User? User { get; set; }
}
