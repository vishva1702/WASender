using System;
using System.Collections.Generic;

namespace WASender.Models;

public partial class Contact
{
    public ulong Id { get; set; }

    public ulong? UserId { get; set; }

    public string? Name { get; set; }

    public string? Phone { get; set; }

    public DateTime? CreatedAt { get; set; } 

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Schedulecontact> Schedulecontacts { get; set; } = new List<Schedulecontact>();

    public virtual User? User { get; set; }
}
