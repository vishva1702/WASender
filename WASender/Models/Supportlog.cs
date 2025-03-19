using System;
using System.Collections.Generic;

namespace WASender.Models;

public partial class Supportlog
{
    public ulong Id { get; set; }

    public int IsAdmin { get; set; }

    public int Seen { get; set; }

    public string Comment { get; set; } = null!;

    public ulong SupportId { get; set; }

    public ulong UserId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Support Support { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
